using Serilog;

/// <summary>
/// Extension methods for configuring the HTTP request pipeline
/// </summary>
public static class WebApplicationBuilderExtensions
{
    public static WebApplicationBuilder AddLogConfiguration(this WebApplicationBuilder builder, IConfiguration configuration)
    {
        // Configure Serilog
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .CreateLogger();

        builder.Host.UseSerilog();

        return builder;
    }

}