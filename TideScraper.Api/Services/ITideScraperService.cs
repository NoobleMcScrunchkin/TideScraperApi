using TideScraper.Api.Models;

namespace TideScraper.Api.Services;

public interface ITideScraperService
{
  public Task<Result<Tide[], TideScraperErrorEnum>> GetTidesAsync();
}