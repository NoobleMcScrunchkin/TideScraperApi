using Microsoft.Extensions.Logging;

namespace TideScraper.Api.Tests.Setup;

public class NUnitLoggerProvider : ILoggerProvider
{
    public ILogger CreateLogger(string categoryName) => new NUnitLogger(categoryName);
    public void Dispose() { }
}