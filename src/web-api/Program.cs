using System.Net;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using web_api;

var builder = WebApplication.CreateBuilder(args);
var otlpEndpoint = new Uri(builder.Configuration.GetValue<string>("Otlp:Endpoint")!);

builder.Services
    .AddOpenTelemetry()
    .ConfigureResource(r => r.AddService(builder.Environment.ApplicationName))
    .WithTracing(traceBuilder =>
        traceBuilder
            .AddAspNetCoreInstrumentation(o =>
            {
                o.EnrichWithHttpRequest = (activity, httpRequest) => { activity.SetTag("requestProtocol", httpRequest.Protocol); };
                o.EnrichWithHttpResponse = (activity, httpResponse) => { activity.SetTag("responseLength", httpResponse.ContentLength); };
                o.EnrichWithException = (activity, exception) =>
                {
                    if (exception.Source != null)
                    {
                        activity.SetTag("exception.source", exception.Source);
                    }
                };
            })
            //.AddConsoleExporter()
            .AddOtlpExporter(otelpExporter => otelpExporter.Endpoint = otlpEndpoint)
    )
    .WithMetrics(meterBuilder =>
        meterBuilder
            .AddAspNetCoreInstrumentation()
            .AddView(
                instrumentName: "http.server.request.duration",
                new Base2ExponentialBucketHistogramConfiguration())
            .AddView(
                instrumentName: "kestrel.connection.duration",
                new Base2ExponentialBucketHistogramConfiguration())
            // .AddRuntimeInstrumentation()
            // .AddProcessInstrumentation()
            //.AddConsoleExporter()
            .AddOtlpExporter(otelpExporter => otelpExporter.Endpoint = otlpEndpoint)
    )
    .WithLogging(loggerBuilder =>
        loggerBuilder
            //.AddConsoleExporter()
            .AddOtlpExporter(otelpExporter => otelpExporter.Endpoint = otlpEndpoint)
    );

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services
    .AddHttpClient<AHttpClient>(opts => opts.BaseAddress = new Uri(builder.Configuration.GetValue<string>("ExternalService:Endpoint")!))
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
    {
        Proxy = new WebProxy
        {
            Address = new Uri("http://localhost:8888") // Fiddler
        },
        UseProxy = false, // enable proxy
        ServerCertificateCustomValidationCallback = (_, _, _, _) => true
    });

builder.Services.AddSingleton(new Db());

builder.Services.AddSingleton(new QuestionsStore());

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    // app.UseExceptionHandler(exceptionHandlerApp =>
    // {
    //     exceptionHandlerApp.Run(async context =>
    //     {
    //
    //     });
    // });

}
// app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", async (ILogger<Program> logger, AHttpClient ipconfig, Db db) =>
    {
        var country = await ipconfig.GetSomething("/country");
        var forecast = Enumerable.Range(1, 5).Select(index =>
                new WeatherForecast
                (
                    DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    Random.Shared.Next(-20, 55),
                    country.Trim(),
                    summaries[Random.Shared.Next(summaries.Length)]
                ))
            .ToArray();

        foreach (var item in forecast)
        {
            db.InsertWeatherForecast(item);
            logger.GetWeatherForecast(item);
        }

        return forecast;
    })
    .WithName("GetWeatherForecast");

app.MapGet("/weatherforecast_db", (ILogger<Program> logger, Db db) =>
    {
        var forecast = db.GetWeatherForecast();
        return forecast;
    })
    .WithName("GetWeatherForecastDb");


app.MapPost("/questions", async (HttpContext context, QuestionsStore questionsStore) =>
{
    var question = await context.Request.ReadFromJsonAsync<Question>();
    questionsStore.Add(question);

});

app.MapGet("/questions", async (ILogger<Program> logger, QuestionsStore questionsStore) =>
    {
        return questionsStore.GetQuestions();
    }
);

app.MapPut("/vote", async (HttpContext context, QuestionsStore questionsStore) =>
{
    var vote =  await context.Request.ReadFromJsonAsync<Vote>();
    questionsStore.Vote(vote);
});



app.Run();

public partial class Program
{
}

public record WeatherForecast(DateOnly Date, long TemperatureC, string Country, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

internal static partial class LoggerExtensions
{
    [LoggerMessage(LogLevel.Information, "Starting the app...")]
    public static partial void StartingApp(this ILogger logger);

    [LoggerMessage(LogLevel.Information, "Weather forecast is `{forecast}`")]
    public static partial void GetWeatherForecast(this ILogger logger, [LogProperties(OmitReferenceName = true)] WeatherForecast forecast);
}