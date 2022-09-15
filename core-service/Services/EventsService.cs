public class EventsService {

    private HttpResponse response;

    public EventsService(HttpResponse response) {
        this.response = response;

        response.Headers.Add("Cache-Control", "no-cache");
        response.Headers.Add("Content-Type", "text/event-stream");
        response.Headers.Add("Connection", "keep-alive");
    }

    public async Task WriteNewEvents(Func<WikiRecord, Task> recordHanlder) {
        WikiRecord? lastRecord = null;

        int counter = 0;

        while (true) {
            var records = WikiService.GetNewWikiRecords(lastRecord);

            foreach (WikiRecord record in records) {
                await recordHanlder(record);

                lastRecord = record;
            }

            await Task.Delay(100);

            counter++;

            if (counter > 100) {
                counter = 0;

                try {
                    await Task.Delay(1, response.HttpContext.RequestAborted);
                } 
                catch (TaskCanceledException) {
                    return;
                }
            }
        }
    }
}
