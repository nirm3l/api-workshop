using System.Collections.Generic;
using System.Text.Json;
using System.Text.RegularExpressions;


public class WorkerHostedService : BackgroundService
{
    private static string URL = "https://stream.wikimedia.org/v2/stream/recentchange";
    
    protected override async Task ExecuteAsync(CancellationToken stopToken) {
        while (!stopToken.IsCancellationRequested) {
            await LoadWikiRecords();
        }
    }

    private async Task LoadWikiRecords() {
        HttpClient client = new HttpClient();

        while (true) {
            try {
                using (var streamReader = new StreamReader(await client.GetStreamAsync(URL))) {
                    while (!streamReader.EndOfStream) {
                        var message = await streamReader.ReadLineAsync();

                        if (message != null && message.StartsWith("data: ")) {
                            WikiService.GetWikiRecordsQueue().Enqueue(JsonSerializer.Deserialize<WikiRecord>(message.Substring(6)));
                        }
                    }
                }
            } 
            catch (Exception ex) {
                await Task.Delay(TimeSpan.FromSeconds(1));
            }
	    }
    }
}
