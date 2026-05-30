using TideScraper.Api.Models;

namespace TideScraper.Api.Services;

public interface ITideScraperService
{
  public Task<Result<Tide[], TideScraperErrorEnum>> GetTidesAsync(CancellationToken cancellationToken = default);
  
  public Task<Result<TideBoundary[], TideScraperErrorEnum>> GetTideBoundariesAsync(CancellationToken cancellationToken = default);
}