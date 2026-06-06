using Microsoft.AspNetCore.Mvc;
using TideScraper.Api.Models;
using TideScraper.Api.Services;

namespace TideScraper.Api.Controllers;

[ApiController]
[Route("/")]
public class TidesController(ITideScraperService tideScraperService) : ControllerBase
{
    [HttpGet("CurrentTide", Name = "GetCurrentTides")]
    [ProducesResponseType<Tide>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IResult> GetCurrentTide(CancellationToken cancellationToken = default)
    {
        var tides = await tideScraperService.GetTidesAsync(DateTime.Today, cancellationToken);

        if (!tides.IsSuccess || tides.Value is null)
        {
            ProblemDetails problemDetails = new()
            {
                Title = tides.Error.ToString(),
                Detail = "There was an error retrieving the tides.",
                Status = 500
            };

            return Results.InternalServerError(problemDetails);
        }
        
        DateTime currentTime = DateTime.Now;
        
        Tide closestTideValue = tides.Value.OrderBy(t => Math.Abs((t.Time - currentTime).Ticks)).First();

        return Results.Ok(closestTideValue);
    }
    
    [HttpGet("Tides", Name = "GetTides")]
    [ProducesResponseType<Tide[]>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IResult> GetTides([FromQuery(Name = "Year")] int? year, [FromQuery(Name = "DayOfYear")] int? dayOfYear, CancellationToken cancellationToken = default)
    {
        DateTime day;

        if (dayOfYear is null && year is null)
        {
            day = DateTime.Today;
        }
        else
        {
            day = new DateTime(year ?? DateTime.Today.DayOfYear, 1, 1).AddDays((dayOfYear ?? 1) - 1);
        }
        
        var tides = await tideScraperService.GetTidesAsync(day, cancellationToken);

        if (!tides.IsSuccess || tides.Value is null)
        {
            ProblemDetails problemDetails = new()
            {
                Title = tides.Error.ToString(),
                Detail = "There was an error retrieving the tides.",
                Status = 500
            };

            return Results.InternalServerError(problemDetails);
        }

        return Results.Ok(tides.Value);
    }
    
    [HttpGet("TideBoundaries", Name = "GetTideBoundaries")]
    [ProducesResponseType<TideBoundary[]>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IResult> GetTideBoundaries([FromQuery(Name = "Year")] int? year, [FromQuery(Name = "DayOfYear")] int? dayOfYear, CancellationToken cancellationToken = default)
    {
        DateTime day;

        if (dayOfYear is null && year is null)
        {
            day = DateTime.Today;
        }
        else
        {
            day = new DateTime(year ?? DateTime.Today.DayOfYear, 1, 1).AddDays((dayOfYear ?? 1) - 1);
        }
        
        var tideBoundaries = await tideScraperService.GetTideBoundariesAsync(day, cancellationToken);

        if (!tideBoundaries.IsSuccess || tideBoundaries.Value is null)
        {
            ProblemDetails problemDetails = new()
            {
                Title = tideBoundaries.Error.ToString(),
                Detail = "There was an error retrieving the tides.",
                Status = 500
            };

            return Results.InternalServerError(problemDetails);
        }

        return Results.Ok(tideBoundaries.Value);
    }
}