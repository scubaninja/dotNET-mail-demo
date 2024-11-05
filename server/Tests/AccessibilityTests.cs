using System;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using AxeCore.NET;

public class AccessibilityTests : PageTest
{
    [Fact]
    public async Task CheckAccessibility()
    {
        await Page.GotoAsync("http://localhost:5000");

        var axeBuilder = new AxeBuilder(Page);
        var results = await axeBuilder.AnalyzeAsync();

        Assert.Empty(results.Violations);
    }

    [Fact]
    public async Task CheckButtonAccessibility()
    {
        await Page.GotoAsync("http://localhost:5000");

        var button = await Page.QuerySelectorAsync("button");
        var axeBuilder = new AxeBuilder(button);
        var results = await axeBuilder.AnalyzeAsync();

        Assert.Empty(results.Violations);
    }

    [Fact]
    public async Task CheckFormAccessibility()
    {
        await Page.GotoAsync("http://localhost:5000");

        var form = await Page.QuerySelectorAsync("form");
        var axeBuilder = new AxeBuilder(form);
        var results = await axeBuilder.AnalyzeAsync();

        Assert.Empty(results.Violations);
    }
}
