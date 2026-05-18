using AngleSharp;
using TideScraper.Api.Models;

namespace TideScraper.Api.Services;

internal class TideScraperService(ILogger<TideScraperService> logger) : ITideScraperService
{
    public async Task<Result<Tide[], TideScraperErrorEnum>> GetTidesAsync()
    {
        logger.LogInformation("Getting tides");

        Tide[] tides = await ScrapeTidesFromDigimap();

        // return Result<Tide[], TideScraperErrorEnum>.Failure(TideScraperErrorEnum.Unknown, "Could not query tides");

        return Result<Tide[], TideScraperErrorEnum>.Success(tides);
    }

    private async Task<Tide[]> ScrapeTidesFromDigimap()
    {
        // 1. Setup the configuration with a default loader (required for web requests)
        var config = Configuration.Default.WithDefaultLoader();

        // 2. Create a new browsing context
        using var context = BrowsingContext.New(config);

        // 3. Fetch the document from a URL
        var url = "https://tides.digimap.gg/";
        using var document = await context.OpenAsync(url);

        // 4. Parse the document using CSS selectors
        var title = document.Title;
        var firstParagraph = document.QuerySelector("p")?.TextContent;
        var allLinks = document.QuerySelectorAll("a");

        logger.LogInformation("Title: {title}", title);
        foreach (var link in allLinks) logger.LogInformation($"Link: {link.GetAttribute("href")}");

        return [
            new Tide
            {
                TideType = TideType.High,
                Time = DateTime.Now,
                Height = 10
            }
        ];
    }
}