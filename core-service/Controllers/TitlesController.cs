using Microsoft.AspNetCore.Mvc;

namespace CoreWorkshop.Api.Controllers;

[ApiController]
[Route("")]
public class TitlesController : ControllerBase
{
    [HttpGet(Name = "Titles")]
    [Route("/titles")]
    public async Task GetTitles()
    {
        await new EventsService(Response).WriteNewEvents(async record => {
            await Response.WriteAsync($"type: data\ndata: {record.title}\n\n");
        });
    }

    [HttpGet(Name = "WikiTitles")]
    [Route("/titles/{wiki}")]
    public async Task GetWikiTitles(string wiki)
    {
        await new EventsService(Response).WriteNewEvents(async record => {
            if (record.wiki != null && record.wiki.Equals(wiki)) {
                await Response.WriteAsync($"type: data\ndata: {record.title}\n\n");
            }
        });
    }

    [HttpGet(Name = "FilterWikiTitles")]
    [Route("/titles/filter/{word}")]
    public async Task GetWikiTitlesFiltered(string word)
    {
        await new EventsService(Response).WriteNewEvents(async record => {
            if (record.title != null && record.title.IndexOf(word, 0, StringComparison.OrdinalIgnoreCase) != -1) {
                await Response.WriteAsync($"type: data\ndata: {record.title}\n\n");
            }
        });
    }
}
