using System.Net.Http.Json;

namespace MyWeatherHub.Tests;


public class IntegrationTests
{
	[Fact]
	public async Task TestApiGetZones()
	{
		// Arrange
		var appHost = await DistributedApplicationTestingBuilder
				.CreateAsync<Projects.AppHost>();

		appHost.Services.ConfigureHttpClientDefaults(clientBuilder =>
		{
			clientBuilder.AddStandardResilienceHandler();
		});

		await using var app = await appHost.BuildAsync();

		var resourceNotificationService = app.Services
				.GetRequiredService<ResourceNotificationService>();

		await app.StartAsync();

		// Act
		var httpClient = app.CreateHttpClient("api");

		await resourceNotificationService.WaitForResourceAsync(
						"api",
						KnownResourceStates.Running
				)
				.WaitAsync(TimeSpan.FromSeconds(30));

		var response = await httpClient.GetAsync("/zones");

		// Assert
		response.EnsureSuccessStatusCode();
		var zones = await response.Content.ReadFromJsonAsync<Zone[]>();
		Assert.NotNull(zones);
		Assert.True(zones.Length > 0);
	}

	[Fact]
	public async Task TestWebAppHomePage()
	{
		// Arrange
		var appHost = await DistributedApplicationTestingBuilder
				.CreateAsync<Projects.AppHost>();

		appHost.Services.ConfigureHttpClientDefaults(clientBuilder =>
		{
			clientBuilder.AddStandardResilienceHandler();
		});

		await using var app = await appHost.BuildAsync();

		var resourceNotificationService = app.Services
				.GetRequiredService<ResourceNotificationService>();

		await app.StartAsync();

		// Act
		var httpClient = app.CreateHttpClient("myweatherhub");

		await resourceNotificationService.WaitForResourceAsync(
						"myweatherhub",
						KnownResourceStates.Running
				)
				.WaitAsync(TimeSpan.FromSeconds(30));

		var response = await httpClient.GetAsync("/");

		// Assert
		response.EnsureSuccessStatusCode();
		var content = await response.Content.ReadAsStringAsync();
		Assert.Contains("MyWeatherHub", content);
	}
}

public record Zone(string Key, string Name, string State);
