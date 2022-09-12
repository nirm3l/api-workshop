package is.symphony.stream.api.workshop.springservice.services;

import is.symphony.stream.api.workshop.springservice.model.WikiRecord;
import org.springframework.boot.context.event.ApplicationReadyEvent;
import org.springframework.context.event.EventListener;
import org.springframework.core.ParameterizedTypeReference;
import org.springframework.http.codec.ServerSentEvent;
import org.springframework.stereotype.Service;
import org.springframework.web.reactive.function.client.WebClient;
import reactor.core.publisher.Flux;
import reactor.core.publisher.Sinks;
import reactor.core.scheduler.Schedulers;
import reactor.util.retry.Retry;

import java.util.Map;
import java.util.concurrent.ConcurrentHashMap;
import java.util.regex.Pattern;

@Service
public class WikiService {

    private final WebClient wikiClient;

    private final Sinks.Many<String> wikis;

    private final Map<String, Sinks.Many<String>> titlesMap = new ConcurrentHashMap<>();

    public WikiService(WebClient wikiClient) {
        this.wikiClient = wikiClient;
        wikis = Sinks.many().multicast().directBestEffort();

        titlesMap.put("all", Sinks.many().multicast().directBestEffort());
    }

    @EventListener(ApplicationReadyEvent.class)
    public void doSomethingAfterStartup() {
        getEvents()
                .filter(event -> event.getWiki() != null && event.getTitle() != null)
                .doOnNext(event -> {
                    wikis.tryEmitNext(event.getWiki());
                    titlesMap.get("all").tryEmitNext(event.getTitle());
                })
                .doOnNext(event -> {
                    if(!titlesMap.containsKey(event.getWiki())) {
                        titlesMap.put(event.getWiki(), Sinks.many().multicast().directBestEffort());
                    }

                    titlesMap.get(event.getWiki()).tryEmitNext(event.getTitle());
                })
                .subscribeOn(Schedulers.boundedElastic()).subscribe();
    }

    public Flux<String> getWikis() {
        return wikis.asFlux().distinct();
    }

    public Flux<String> getTitles(String wiki) {
        if(!titlesMap.containsKey(wiki)) {
            titlesMap.put(wiki, Sinks.many().multicast().directBestEffort());
        }

        return titlesMap.get(wiki).asFlux();
    }

    public Flux<String> getTitlesFiltered(String word) {
        Pattern pattern = Pattern.compile(Pattern.quote(word), Pattern.CASE_INSENSITIVE);

        return titlesMap.get("all").asFlux().filter(title -> pattern.matcher(title).find());
    }

    public Flux<WikiRecord> getEvents() {
        return wikiClient.get().uri("/mediawiki.recentchange")
                .retrieve().bodyToFlux(new ParameterizedTypeReference<ServerSentEvent<WikiRecord>>() {})
                .mapNotNull(ServerSentEvent::data)
                .retryWhen(Retry.indefinitely());
    }
}