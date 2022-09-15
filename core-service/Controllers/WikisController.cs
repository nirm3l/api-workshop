using Microsoft.AspNetCore.Mvc;

namespace CoreWorkshop.Api.Controllers;

[ApiController]
[Route("/wikis")]
public class WikisController : ControllerBase
{
    [HttpGet(Name = "Wikis")]
    public async Task Get()
    {
        var duplicationCheck = new HashSet<string>();

        await new EventsService(Response).WriteNewEvents(async record => {
            if (record.wiki != null && !duplicationCheck.Contains(record.wiki)) {
                duplicationCheck.Add(record.wiki);

                await Response.WriteAsync($"type: data\ndata: {record.wiki}\n\n");
            }
        });
    }
}
