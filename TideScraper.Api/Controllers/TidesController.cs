using Microsoft.AspNetCore.Mvc;
using TideScraper.Api.Models;
using TideScraper.Api.Services;

namespace TideScraper.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class TidesController(ITideScraperService tideScraperService) : ControllerBase
{
    [HttpGet(Name = "GetTides")]
    [ProducesResponseType<Tide[]>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IResult> Get()
    {
        var tides = await tideScraperService.GetTidesAsync();

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
}