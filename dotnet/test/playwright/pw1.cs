
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Playwright.MSTest;
using System.Threading.Tasks;

[TestClass]
public class ScreenplayWebUiTest
{

    [TestMethod]
    public async Task TestMethod1()
    {
        var playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        var browser = await playwright.Chromium.LaunchAsync();
        var context = await browser.NewContextAsync();
        var page = await context.NewPageAsync();
        var x = await page.EvaluateAsync("() => 2+2");
        browser.CloseAsync().Wait();
    }
}