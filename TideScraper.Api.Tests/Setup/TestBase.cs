using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace TideScraper.Api.Tests.Setup;

public class TestBase
{
    protected WebApplicationFactory<Program> Factory { get; }
    protected IConfiguration Configuration { get; }
    
    public TestBase()
    {
        Factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureLogging(logging =>
            {
                logging.ClearProviders(); // Removes default console/debug loggers
                logging.AddProvider(new NUnitLoggerProvider());
            });
        });
        
        Configuration = Factory.Services.GetRequiredService<IConfiguration>();
    }
    
    public TService GetService<TService>()  where TService : class
    {
        return Factory.Services.GetRequiredService<TService>();
    }
}