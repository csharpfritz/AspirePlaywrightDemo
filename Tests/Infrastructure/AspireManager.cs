using Aspire.Hosting;

namespace Tests.Infrastructure;

/// <summary>
/// Startup and configure the Aspire application for testing.
/// </summary>
public class AspireManager : IAsyncLifetime
{

	internal PlaywrightManager PlaywrightManager { get; } = new();

	internal DistributedApplication? App { get; private set; }

	public async Task<DistributedApplication> ConfigureAsync<TEntryPoint>(
			string[]? args = null,
			Action<IDistributedApplicationTestingBuilder>? configureBuilder = null) where TEntryPoint : class
	{

		if (App is not null) return App;

		var builder = await DistributedApplicationTestingBuilder.CreateAsync<TEntryPoint>(
				args: args ?? [],
				configureBuilder: static (options, _) =>
				{
					options.DisableDashboard = false;
				});

		builder.Configuration["ASPIRE_ALLOW_UNSECURED_TRANSPORT"] = "true";

		configureBuilder?.Invoke(builder);

		App = await builder.BuildAsync();

		await App.StartAsync();

		return App;
	}


	public async Task InitializeAsync()
	{
		// Initialization logic here
		await PlaywrightManager.InitializeAsync();
	}
	public async Task DisposeAsync()
	{
		await PlaywrightManager.DisposeAsync();

		await (App?.DisposeAsync() ?? ValueTask.CompletedTask);
	}
}
