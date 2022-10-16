const express = require('express');
const actuator = require('express-actuator');
const EventSource = require('eventsource');
const Denque = require("denque");

const app = express();
app.use(actuator({
    basePath: '/actuator'
}));

const PORT = 8084;

const URL = 'https://stream.wikimedia.org/v2/stream/recentchange';
const eventSource = new EventSource(URL);

eventSource.onopen = () => {
    console.info('Opened connection.');
};

eventSource.onerror = (event) => {
    console.error('Encountered error', event);
};

const wikiRecords = new Denque();

eventSource.onmessage = (event) => {
    const data = JSON.parse(event.data);

    wikiRecords.push({
        wiki: data.wiki,
        title: data.title
    });

    if (wikiRecords.size() >= 100) {
        wikiRecords.shift()
    }
};

const getNewRecords = (wikiRecords, lastEvent) => {
    const recordsCopy = wikiRecords.toArray();

    const records = [];

    for (let i = recordsCopy.length - 1; i >= 0 && recordsCopy[i] !== lastEvent; i--) {
        records.push(recordsCopy[i])
    }

    records.reverse();

    return records;
}

const writeEvents = async (res, f, distinct=false, condition) => {
    let lastEvent = null;

    const duplicationCheck = new Set()

    let counter = 0;

    while (true) {
        if (wikiRecords.length && lastEvent !== wikiRecords.peekBack()) {
            for (const data of getNewRecords(wikiRecords, lastEvent)) {
                if (!condition || condition(data)) {
                    const value = f(data);

                    if (distinct) {
                        if (duplicationCheck.has(value)) {
                            continue;
                        }

                        duplicationCheck.add(value);
                    }

                    if(!await res.write(`event: data\ndata: ${value}\n\n`)) {
                        res.end();

                        return;
                    }
                }

                lastEvent = data;
            }
        }

        await new Promise(resolve => setTimeout(resolve, 100));

        counter += 1;

        if (counter > 100) {
            counter = 0;

            if(!await res.write(`type: 'ping'\n\n`)) {
                res.end();

                return;
            }
        }
    }
}

const setupResponse = (res) => {
    res.set({
        'Cache-Control': 'no-cache',
        'Content-Type': 'text/event-stream',
        'Connection': 'keep-alive'
    });

    res.flushHeaders();
}

app.get('/titles', async function(req, res) {
    setupResponse(res);

    res.on("finish", () => {
        console.log('response finished!');
    });

    return writeEvents(res,(value) => value.title);
});

app.get('/titles/current', async function(req, res) {
    return res.send(wikiRecords.toArray()
        .map(record => record.title));
});

app.get('/titles/:wiki', async function(req, res) {
    setupResponse(res);

    return writeEvents(res,
        (value) => value.title, false, (value) => value.wiki === req.params.wiki);
});

app.get('/titles/filter/:word', async function(req, res) {
    setupResponse(res);

    return writeEvents(res,(value) => value.title, false,
        (value) => value.title.match(new RegExp(req.params.word, 'i')));
});

app.get('/wikis', async function(req, res) {
    setupResponse(res);

    return writeEvents(res,
        (value) => value.wiki, true);
});

app.listen(PORT, function() {
  console.log('Server listening on http://localhost:' + PORT);
});
