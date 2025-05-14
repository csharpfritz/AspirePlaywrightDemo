using Microsoft.Playwright;

namespace Tests;

public class WeatherHubTests : BasePlaywrightTests
{
	public WeatherHubTests(AspireManager aspireManager) : base(aspireManager) { }

	[Fact]
	public async Task TestWebAppHomePage()
	{


		await ConfigureAsync<Projects.AppHost>();

		await InteractWithPageAsync("myweatherhub", async page =>
			{
				await page.GotoAsync("/");

				var title = await page.TitleAsync();
				Assert.Equal("My Weather Hub", title);

			});
	}

	[Theory]
	[InlineData("phila", "Philadelphia")]
	[InlineData("spokane", "Spokane Area")]
	[InlineData("manhattan", "New York (Manhattan)")]
	[InlineData("fairbanks", "Fairbanks Metro Area")]
	public async Task SearchForCity(string searchText, string locationText)
	{

		await ConfigureAsync<Projects.AppHost>();

		await InteractWithPageAsync("myweatherhub", async page =>
		{

			await page.GotoAsync("/");

			await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

			await page.GetByRole(AriaRole.Button, new() { Name = "Column options" }).First.ClickAsync();
			await page.GetByRole(AriaRole.Searchbox, new() { Name = "Name..." }).ClickAsync();
			await page.GetByRole(AriaRole.Searchbox, new() { Name = "Name..." }).FillAsync(searchText);
			await page.GetByText("Not all forecast zones will").ClickAsync();
			await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

			await page.GetByText(locationText, new() { Exact = true }).ClickAsync();

			var weatherTitle = await page.Locator("h3").TextContentAsync();
			Assert.Contains($"Weather for {locationText}", weatherTitle);

		});

	}

}
