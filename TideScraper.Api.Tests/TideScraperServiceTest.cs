using TideScraper.Api.Models;
using TideScraper.Api.Services;
using TideScraper.Api.Tests.Setup;

namespace TideScraper.Api.Tests;

[TestFixture]
public class TideScraperServiceTest : TestBase
{
    private ITideScraperService TideScraperService { get; set; }
    
    [SetUp]
    public void Setup()
    {
        TideScraperService = GetService<ITideScraperService>();
    }

    [Test]
    public async Task TideScraper_ReturnsTides()
    {
        var result = await TideScraperService.GetTidesAsync();
        
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Value, Is.TypeOf<Tide[]>());
    }
}