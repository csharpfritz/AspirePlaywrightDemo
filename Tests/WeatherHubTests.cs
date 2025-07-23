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

	[Fact]
    public async Task SearchForNonExistentLocation()
    {
        await ConfigureAsync<Projects.AppHost>();

        await InteractWithPageAsync("myweatherhub", async page =>
        {
            await page.GotoAsync("/");
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Open the search and enter a non-existent location
            await page.GetByRole(AriaRole.Button, new() { Name = "Column options" }).First.ClickAsync();
            await page.GetByRole(AriaRole.Searchbox, new() { Name = "Name..." }).ClickAsync();
            await page.GetByRole(AriaRole.Searchbox, new() { Name = "Name..." }).FillAsync("NonExistentCity123");
            
            // Wait for the search results to load
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Verify that no results are found
            // The text about forecast zones should still be visible
            await page.GetByText("Not all forecast zones will").IsVisibleAsync();

            // Verify that our non-existent city text is not found in the results
            var nonExistentLocationElement = await page.GetByText("NonExistentCity123", new() { Exact = true }).CountAsync();
            Assert.Equal(0, nonExistentLocationElement);
        });
    }

}
