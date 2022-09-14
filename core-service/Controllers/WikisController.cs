using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent ;

namespace CoreWorkshop.Api.Controllers;

[ApiController]
[Route("/wikis")]
public class WikisController : ControllerBase
{
    [HttpGet(Name = "Wikis")]
    public async Task Get()
    {
        setupResponse();

        WikiRecord lastRecord = null;

        var duplicationCheck = new HashSet<string>();

        while (true) {
            var records = WikiService.GetNewWikiRecords(lastRecord);

            foreach (WikiRecord record in records) {
                if (duplicationCheck.Contains(record.wiki)) {
                    continue;
                }

                duplicationCheck.Add(record.wiki);

                await Response.WriteAsync($"data: {record.wiki}\r\r");

                lastRecord = record;
            }

            await Task.Delay(100);
        }
    }

    private void setupResponse() {
        Response.Headers.Add("Cache-Control", "no-cache");
        Response.Headers.Add("Content-Type", "text/event-stream");
        Response.Headers.Add("Connection", "keep-alive");
    }
}
