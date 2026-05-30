using Microsoft.AspNetCore.Mvc;
using TideScraper.Api.Models;
using TideScraper.Api.Services;

namespace TideScraper.Api.Controllers;

[ApiController]
[Route("/")]
public class TidesController(ITideScraperService tideScraperService) : ControllerBase
{
    [HttpGet("Tides", Name = "GetTides")]
    [ProducesResponseType<Tide[]>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IResult> GetTides(CancellationToken cancellationToken = default)
    {
        var tides = await tideScraperService.GetTidesAsync(cancellationToken);

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
    public async Task<IResult> GetTideBoundaries(CancellationToken cancellationToken = default)
    {
        var tideBoundaries = await tideScraperService.GetTideBoundariesAsync(cancellationToken);

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