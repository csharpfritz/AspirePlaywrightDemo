using Aspire.Hosting;

namespace MyWeatherHub.Tests;


public class EnvVarTests
{
	[Fact]
	public async Task WebResourceEnvVarsResolveToApiService()
	{
		// Arrange
		var appHost = await DistributedApplicationTestingBuilder
				.CreateAsync<Projects.AppHost>();

		var frontend = (IResourceWithEnvironment)appHost.Resources
				.Single(static r => r.Name == "myweatherhub");

		// Act
		var envVars = await frontend.GetEnvironmentVariableValuesAsync(
				DistributedApplicationOperation.Publish);

		// Assert
		//new()
		//{
		//	["services__api__https__0"] = "{api.bindings.https.url}",
		//}
		Assert.Contains(
			new KeyValuePair<string, string>("services__api__https__0", "{api.bindings.https.url}"),
			envVars);

	}
}