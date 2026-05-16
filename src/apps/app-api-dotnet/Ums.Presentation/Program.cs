using Ums.Globalization;
using Ums.Globalization.Access;
using Ums.Presentation.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddScoped<ILocalizationService, LocalizationService>();

var app = builder.Build();

app.UseCulture();

app.UseHttpsRedirection();

app.MapGet("/health", () => Results.Ok(new
{
    Status = "Healthy",
    Service = "UMS API",
    Language = CultureContext.Current,
    Timestamp = DateTimeOffset.UtcNow
}))
.WithName("GetHealth")
.WithTags("Platform");

app.MapGet("/health/{language}", (string language) =>
{
    using (CultureContext.Set(language))
    {
        var localizer = new LocalizationService();
        return Results.Ok(new
        {
            Status = "Healthy",
            Service = "UMS API",
            Message = localizer.T("system.internal_error"),
            Language = language,
            Timestamp = DateTimeOffset.UtcNow
        });
    }
})
.WithName("GetHealthLocalized")
.WithTags("Platform");

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5)
        .Select(index => new WeatherForecast(
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]))
        .ToArray();

    return Results.Ok(forecast);
})
.WithName("GetWeatherForecast")
.WithTags("Diagnostics");

app.Run();

internal sealed record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
