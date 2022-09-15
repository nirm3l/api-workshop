using System.Text.Json;

public class WorkerHostedService : BackgroundService
{
    private static string URL = "https://stream.wikimedia.org/v2/stream/recentchange";
    
    protected override async Task ExecuteAsync(CancellationToken stopToken) {
        await LoadWikiRecords(stopToken);
    }

    private async Task LoadWikiRecords(CancellationToken stopToken) {
        HttpClient client = new HttpClient();

        while (!stopToken.IsCancellationRequested) {
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
            catch (Exception) {
                await Task.Delay(TimeSpan.FromSeconds(1));
            }
	    }
    }
}
