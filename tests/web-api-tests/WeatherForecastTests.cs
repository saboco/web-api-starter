namespace web_api_tests;

using System.Net.Http.Json;
using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;


public class WeatherForecastTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task GetWeatherForecast_ReturnsSuccessAndForecasts()
    {
        var response = await _client.GetAsync("/weatherforecast");
        response.EnsureSuccessStatusCode();

        var forecasts = await response.Content.ReadFromJsonAsync<WeatherForecast[]>();
        Assert.NotNull(forecasts);
        Assert.Equal(5, forecasts!.Length);
    }
}
