using System.Text.Json;
using AngleSharp;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using TideScraper.Api.Configuration;
using TideScraper.Api.Models;

namespace TideScraper.Api.Services;

public class TideScraperService : ITideScraperService
{
    private readonly ILogger<TideScraperService> _logger;
    private readonly IDatabase _redisCache;

    public TideScraperService(ILogger<TideScraperService> logger, IOptions<RedisConfig> redisConfig)
    {
        _logger = logger;

        var muxer = ConnectionMultiplexer.Connect(redisConfig.Value.ConnectionString);
        _redisCache = muxer.GetDatabase();
    }

    public async Task<Result<Tide[], TideScraperErrorEnum>> GetTidesAsync(CancellationToken cancellationToken = default)
    {
        Tide[] tides;

        String? tidesJson = _redisCache.StringGet("tides");

        if (tidesJson != null)
        {
            try
            {
                Tide[]? cachedTides = JsonSerializer.Deserialize<Tide[]>(tidesJson);

                if (cachedTides != null)
                {
                    return Result<Tide[], TideScraperErrorEnum>.Success(cachedTides);    
                }
            }
            catch
            {
                await _redisCache.KeyDeleteAsync("tides");
            }
        }
        
        try
        {
            tides = await ScrapeTidesFromDigimap(cancellationToken);

            TimeSpan diff = DateTime.Today.AddDays(1) - DateTime.Now;
            
            await _redisCache.StringSetAsync("tides", JsonSerializer.Serialize(tides), diff);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to get tide boundaries");
            
            return Result<Tide[], TideScraperErrorEnum>.Failure(TideScraperErrorEnum.Unknown, "Could not query tides");   
        }

        return Result<Tide[], TideScraperErrorEnum>.Success(tides);
    }
    
    public async Task<Result<TideBoundary[], TideScraperErrorEnum>> GetTideBoundariesAsync(CancellationToken cancellationToken = default)
    {
        TideBoundary[] tideBoundaries;
        
        String? tidesJson = _redisCache.StringGet("tidesBoundaries");

        if (tidesJson != null)
        {
            try
            {
                TideBoundary[]? cachedTides = JsonSerializer.Deserialize<TideBoundary[]>(tidesJson);

                if (cachedTides != null)
                {
                    return Result<TideBoundary[], TideScraperErrorEnum>.Success(cachedTides);    
                }
            }
            catch
            {
                await _redisCache.KeyDeleteAsync("tidesBoundaries");
            }
        }

        
        try
        {
            tideBoundaries = await ScrapeTideBoundariesFromDigimap(cancellationToken);

            TimeSpan diff = DateTime.Today.AddDays(1) - DateTime.Now;
            
            await _redisCache.StringSetAsync("tidesBoundaries", JsonSerializer.Serialize(tideBoundaries), diff);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to get tide boundaries");
            
            return Result<TideBoundary[], TideScraperErrorEnum>.Failure(TideScraperErrorEnum.Unknown, "Could not query tides");   
        }

        return Result<TideBoundary[], TideScraperErrorEnum>.Success(tideBoundaries);
    }

    private async Task<Tide[]> ScrapeTidesFromDigimap(CancellationToken cancellationToken = default)
    {
        var config = AngleSharp.Configuration.Default.WithDefaultLoader();

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
                
                DateTime time = DateTime.SpecifyKind(DateTime.Parse(timeElement.TextContent.Trim()), DateTimeKind.Local);
                string height = heightElement.TextContent;
                
                tides.Add(new Tide
                {
                    Time = time,
                    Height = decimal.Parse(height.Trim())
                });
            }
            
        }

        return tides.ToArray();
    }

    private async Task<TideBoundary[]> ScrapeTideBoundariesFromDigimap(CancellationToken cancellationToken = default)
    {
        var config = AngleSharp.Configuration.Default.WithDefaultLoader();

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
            DateTime time = DateTime.SpecifyKind(DateTime.Parse(timeElement.TextContent.Trim()), DateTimeKind.Local);
            decimal height = decimal.Parse(heightElement.TextContent.Trim());

            return new TideBoundary()
            {
                TideType = type,
                Height = height,
                Time = time
            };
        });
        
        return tideBoundaries.ToArray();
    }
}