using TideScraper.Api.Models;

namespace TideScraper.Api.Services;

public interface ITideScraperService
{
  public Task<Result<Tide[], TideScraperErrorEnum>> GetTidesAsync(DateTime? providedDay = null, CancellationToken cancellationToken = default);
  
  public Task<Result<TideBoundary[], TideScraperErrorEnum>> GetTideBoundariesAsync(DateTime? providedDay = null, CancellationToken cancellationToken = default);
}