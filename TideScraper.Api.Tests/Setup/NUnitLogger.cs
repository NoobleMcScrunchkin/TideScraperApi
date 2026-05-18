using Microsoft.Extensions.Logging;

namespace TideScraper.Api.Tests.Setup;

public class NUnitLogger : ILogger
{
    private readonly string _categoryName;
    public NUnitLogger(string categoryName) => _categoryName = categoryName;

    public IDisposable BeginScope<TState>(TState state) => null;
    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
    {
        // NUnit captures anything written to Out or Progress
        TestContext.Progress.WriteLine($"[{logLevel}] {_categoryName}: {formatter(state, exception)}");
    }
}