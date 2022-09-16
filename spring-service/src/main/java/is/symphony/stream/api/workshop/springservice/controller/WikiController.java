package is.symphony.stream.api.workshop.springservice.controller;

import is.symphony.stream.api.workshop.springservice.services.WikiService;
import org.springframework.http.MediaType;
import org.springframework.http.codec.ServerSentEvent;
import org.springframework.web.bind.annotation.PathVariable;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RequestMethod;
import org.springframework.web.bind.annotation.RestController;
import reactor.core.publisher.Flux;
import reactor.core.publisher.Mono;

import java.time.Duration;
import java.util.List;

@RestController
public class WikiController {

    private final WikiService wikiService;

    public WikiController(WikiService wikiService) {
        this.wikiService = wikiService;
    }

    @RequestMapping(value = "wikis", produces = MediaType.TEXT_EVENT_STREAM_VALUE, method = RequestMethod.GET)
    public Flux<ServerSentEvent<String>> getWikis() {
        return Flux.merge(getPingEvents(), wikiService.getWikis().distinct()
                .map(wiki -> ServerSentEvent.builder(wiki).event("data").build()));
    }

    @RequestMapping(value = "titles", produces = MediaType.TEXT_EVENT_STREAM_VALUE, method = RequestMethod.GET)
    public Flux<ServerSentEvent<String>> getTitles() {
        return Flux.merge(getPingEvents(), wikiService.getTitles()
                .map(title -> ServerSentEvent.builder(title).event("data").build()));
    }

    @RequestMapping(value = "titles/current", produces = MediaType.APPLICATION_JSON_VALUE, method = RequestMethod.GET)
    public Mono<List<String>> getCurrentTitles() {
        return wikiService.getTitles().take(100).collectList();
    }

    @RequestMapping(value = "titles/{wiki}", produces = MediaType.TEXT_EVENT_STREAM_VALUE, method = RequestMethod.GET)
    public Flux<ServerSentEvent<String>> getTitlesForWiki(@PathVariable String wiki) {
        return Flux.merge(getPingEvents(), wikiService.getTitles(wiki)
                .map(title -> ServerSentEvent.builder(title).event("data").build()));
    }

    @RequestMapping(value = "titles/filter/{word}", produces = MediaType.TEXT_EVENT_STREAM_VALUE, method = RequestMethod.GET)
    public Flux<ServerSentEvent<String>> getTitlesFilteredByWord(@PathVariable String word) {
        return Flux.merge(getPingEvents(), wikiService.getTitlesFiltered(word)
                .map(title -> ServerSentEvent.builder(title).event("data").build()));
    }

    public Flux<ServerSentEvent<String>> getPingEvents() {
        return Flux.interval(Duration.ofSeconds(10)).map(i -> ServerSentEvent.builder((String)null).event("ping").build());
    }
}