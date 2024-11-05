using System;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;

public class PlaywrightTests : PageTest
{
    [Fact]
    public async Task CheckHomePageTitle()
    {
        await Page.GotoAsync("http://localhost:5000");
        Assert.Equal("Home - Tailwind Traders", await Page.TitleAsync());
    }

    [Fact]
    public async Task CheckButtonExists()
    {
        await Page.GotoAsync("http://localhost:5000");
        var button = await Page.QuerySelectorAsync("button");
        Assert.NotNull(button);
    }

    [Fact]
    public async Task CheckFormExists()
    {
        await Page.GotoAsync("http://localhost:5000");
        var form = await Page.QuerySelectorAsync("form");
        Assert.NotNull(form);
    }

    [Fact]
    public async Task CheckLinkExists()
    {
        await Page.GotoAsync("http://localhost:5000");
        var link = await Page.QuerySelectorAsync("a");
        Assert.NotNull(link);
    }
}
