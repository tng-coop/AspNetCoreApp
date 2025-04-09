using Bunit;
using BlazorWebApp.Components.Pages; // Ensure this matches your project's namespace

public class HomeComponentTests : TestContext
{
    [Fact]
    public void DefaultHomePageDisplaysExpectedContent()
    {
        // Arrange & Act: Render the default Home component
        var cut = RenderComponent<Home>();

        // Assert: Check that the rendered markup includes some expected text
        // Replace "Welcome" with a snippet of text that you know appears in Home.razor
        Assert.Contains("Welcome", cut.Markup);
    }
}
