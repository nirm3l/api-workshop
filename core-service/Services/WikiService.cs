using System.Collections.Concurrent ;

public class WikiService {

    private static FixedSizedQueue<WikiRecord> WIKI_RECORDS = new FixedSizedQueue<WikiRecord>{Limit=100};
    
    public static ICollection<WikiRecord> GetNewWikiRecords(WikiRecord lastWikiRecord) {

        var wikiRecords = new LinkedList<WikiRecord>();

        if (WIKI_RECORDS.GetQueue().Last() != lastWikiRecord) {
            WikiRecord[] allRecords = WIKI_RECORDS.GetQueue().ToArray();
            
            foreach (var wikiRecord in allRecords.Reverse()) {
                if (wikiRecord == lastWikiRecord) {
                    break;
                }

                wikiRecords.AddFirst(wikiRecord);
            }
        }

        return wikiRecords;
    }

    public static FixedSizedQueue<WikiRecord> GetWikiRecordsQueue() {
        return WIKI_RECORDS;
    }
}
