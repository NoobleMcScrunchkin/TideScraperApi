using AngleSharp;
using AngleSharp.Dom;
using TideScraper.Api.Models;

namespace TideScraper.Api.Services;

public class TideScraperService(ILogger<TideScraperService> logger) : ITideScraperService
{
    public async Task<Result<Tide[], TideScraperErrorEnum>> GetTidesAsync(CancellationToken cancellationToken = default)
    {
        Tide[] tides;
        
        try
        {
            tides = await ScrapeTidesFromDigimap(cancellationToken);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to get tide boundaries");
            
            return Result<Tide[], TideScraperErrorEnum>.Failure(TideScraperErrorEnum.Unknown, "Could not query tides");   
        }

        return Result<Tide[], TideScraperErrorEnum>.Success(tides);
    }
    
    public async Task<Result<TideBoundary[], TideScraperErrorEnum>> GetTideBoundariesAsync(CancellationToken cancellationToken = default)
    {
        TideBoundary[] tideBoundaries;
            
        try
        {
            tideBoundaries = await ScrapeTideBoundariesFromDigimap(cancellationToken);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to get tide boundaries");
            
            return Result<TideBoundary[], TideScraperErrorEnum>.Failure(TideScraperErrorEnum.Unknown, "Could not query tides");   
        }

        return Result<TideBoundary[], TideScraperErrorEnum>.Success(tideBoundaries);
    }

    private async Task<Tide[]> ScrapeTidesFromDigimap(CancellationToken cancellationToken = default)
    {
        var config = Configuration.Default.WithDefaultLoader();

        using var context = BrowsingContext.New(config);

        var url = "https://tides.digimap.gg/";
        
        using var document = await context.OpenAsync(url, cancellationToken);

        var tables = document.QuerySelectorAll("table.float-left.table-condensed.table-bordered");

        if (tables.Length != 4)
        {
            throw new Exception($"No tables found. Expecting 4 tables, found {tables.Length}");
        }

        List<Tide> tides = [];

        foreach (var table in tables)
        {
            var rows = table.QuerySelectorAll("tr").Skip(1);

            foreach (var row in rows)
            {
                var cells = row.QuerySelectorAll("td");
                
                var timeElement = cells.FirstOrDefault();
                var heightElement = cells.LastOrDefault();
                
                if (timeElement == null || heightElement == null)
                {
                    throw new Exception("Failed to get time and height cells");
                }
                
                string time = timeElement.TextContent;
                string height = heightElement.TextContent;

                tides.Add(new Tide
                {
                    Time = DateTime.Parse(time.Trim()),
                    Height = decimal.Parse(height.Trim())
                });
            }
            
        }

        return tides.ToArray();
    }

    private async Task<TideBoundary[]> ScrapeTideBoundariesFromDigimap(CancellationToken cancellationToken = default)
    {
        var config = Configuration.Default.WithDefaultLoader();

        using var context = BrowsingContext.New(config);

        var url = "https://tides.digimap.gg/";
        
        using var document = await context.OpenAsync(url, cancellationToken);

        var tables = document.QuerySelectorAll("table.table-condensed.table-bordered");

        var table = tables.ElementAtOrDefault(1);
        
        if (table == null)
        {
            throw new Exception("Tide boundary table not found.");
        }
        
        var rows = table.QuerySelectorAll("tr").Skip(1);
        
        IEnumerable<TideBoundary> tideBoundaries = rows.Select(row =>
        {
            var cells = row.QuerySelectorAll("td");
                
            var boundaryType = cells.FirstOrDefault();
            var timeElement = cells.ElementAtOrDefault(1);
            var heightElement = cells.LastOrDefault();
                
            if (boundaryType == null || timeElement == null || heightElement == null)
            {
                throw new Exception("Failed to get boundary type, time and height cells");
            }
            
            TideType type = boundaryType.TextContent.Trim() == "Low" ? TideType.Low : TideType.High;
            DateTime time = DateTime.Parse(timeElement.TextContent.Trim());
            decimal height = decimal.Parse(heightElement.TextContent.Trim());

            return new TideBoundary()
            {
                Type = type,
                Height = height,
                Time = time
            };
        });
        
        return tideBoundaries.ToArray();
    }
}