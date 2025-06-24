using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;
using Moq;
using Moq.Protected;
using MyWeatherHub;
using Xunit;

public class NwsManagerTests
{
    [Fact]
    public async Task GetZonesAsync_ReturnsZones()
    {
        var zonesJson = "[{\"Key\":\"1\",\"Name\":\"Zone1\",\"State\":\"WA\"}]";
        var client = CreateMockClient(zonesJson, "zones");
        var manager = new NwsManager(client);

        var result = await manager.GetZonesAsync();

        Assert.Single(result);
        Assert.Equal("Zone1", result[0].Name);
    }

    [Fact]
    public async Task GetZonesAsync_ReturnsEmptyArray_WhenResponseIsNull()
    {
        // Use empty JSON array to simulate no zones
        var client = CreateMockClient("[]", "zones");
        var manager = new NwsManager(client);

        var result = await manager.GetZonesAsync();

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetForecastByZoneAsync_ReturnsForecast()
    {
        var forecastJson = "[{\"Name\":\"Today\",\"DetailedForecast\":\"Sunny\"}]";
        var client = CreateMockClient(forecastJson, "forecast/zone1");
        var manager = new NwsManager(client);

        var result = await manager.GetForecastByZoneAsync("zone1");

        Assert.Single(result);
        Assert.Equal("Today", result[0].Name);
    }

    [Fact]
    public async Task GetForecastByZoneAsync_ReturnsEmptyArray_WhenResponseIsNull()
    {
        // Use empty JSON array to simulate no forecast
        var client = CreateMockClient("[]", "forecast/zone1");
        var manager = new NwsManager(client);

        var result = await manager.GetForecastByZoneAsync("zone1");

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetForecastByZoneAsync_EncodesZoneId()
    {
        string zoneId = "zone/with special";
        string encoded = System.Web.HttpUtility.UrlEncode(zoneId);
        var forecastJson = "[{\"Name\":\"Today\",\"DetailedForecast\":\"Sunny\"}]";
        var client = CreateMockClient(forecastJson, $"forecast/{encoded}");
        var manager = new NwsManager(client);

        var result = await manager.GetForecastByZoneAsync(zoneId);

        Assert.Single(result);
    }

    [Fact]
    public async Task GetZonesAsync_ThrowsException_OnHttpError()
    {
        var handler = new Mock<HttpMessageHandler>();
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Network error"));

        var client = new HttpClient(handler.Object)
        {
            BaseAddress = new System.Uri("http://localhost/")
        };
        var manager = new NwsManager(client);

        await Assert.ThrowsAsync<HttpRequestException>(() => manager.GetZonesAsync());
    }

    private HttpClient CreateMockClient(string? json, string expectedPath)
    {
        var handler = new Mock<HttpMessageHandler>();
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri!.PathAndQuery.Contains(expectedPath)),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = json != null ? new StringContent(json) : null
            });

        return new HttpClient(handler.Object)
        {
            BaseAddress = new System.Uri("http://localhost/")
        };
    }
}