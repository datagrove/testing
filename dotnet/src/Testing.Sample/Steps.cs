namespace pepin_simple;
using System;
using TechTalk.SpecFlow;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using Datagrove.Testing.Selenium;
using Microsoft.Playwright;
using System.Text.RegularExpressions;
using Datagrove.Testing.MsTest;

// steps are global; we could easily have one step class but instead we break them apart here to make it easier to see what's going on

// Holds the state for a single example of a single scenario. Steps may reference context itself or any member. Each test will initialize a TestState when it starts, and provide it to each of the steps used by the test. Be sure code is thread safe so that you can run all your tests in parallel (not hard because each thread will have its own instance).

// the Datagrove.Testing.Selenium.ScenarioState gives you most of the things you want: browser, api, selenium webdriver, direct playwright access. but add service here that you want your steps to have access to.
public class ScenarioState : ScenarioBase {

    // there's nothing special about the members provided here
    // returns a playwright page without selenium compatibility
  
    PlaywrightState? api_;
    public async ValueTask<Datagrove.Testing.Selenium.PlaywrightState> state()
    {
        if (state_ == null)
        {
            state_ = await Datagrove.Testing.Selenium.PlaywrightState.createAsync(new PlaywrightOptions());
        }
        return state_;
    }

    public async Task<IPage> page() => (await state()).page;

    static public async ValueTask create(TextContext context) {
        var r = new ScenarioState();
        await r.create(context);
    }

    static public async Task<ScenarioState> begin(TestContext context)
    {
        await Task.CompletedTask;
        return new ScenarioState(context);
    }


    // note that the name doen't matter, all steps are effectively global for a compilation. If you are constrained to be backwards compatible with specflow, you can take the name of the

    public ValueTask DisposeAsync()
    {
        throw new NotImplementedException();
    }
}
[Binding]
class CalculatorSteps : IAsyncDisposable
{
    //StepState state;
    // note that a step class is initialized once per test. It is not shared.
    int sum = 0;

    // Note that you can "inject" state variable on any step. The construct or will be called at the beginning of the test (scenario) and the dispose at the end.
    public CalculatorSteps()
    {
        //this.state = state;
    }

    public async ValueTask DisposeAsync()
    {
        await Task.CompletedTask;
    }

    // note that we can have a mix of async and sync steps. 

    [Given(@"I have a calculator")]
    public void I_have_a_calculator()
    {
        sum = 0;
    }

    [Given(@"I have numbers (.*) and (.*) as input")]
    public async Task I_have_and_as_input(int p0, int p1)
    {
        sum = p0 + p1;
        await Task.CompletedTask;
    }
    [Given(@"I add more numbers")]
    public async Task I_add_more_numbers(Table table)
    {
        foreach (var row in table.Rows)
        {
            sum += int.Parse(row[0]);
        }
        await Task.CompletedTask;
    }

    [Then(@"I should get an output of (.*)")]
    public void I_should_get_an_output_of(int p0)
    {
        Assert.AreEqual(p0, sum);
    }

}
[Binding]
public class RestSteps
{
    static string API_TOKEN = Environment.GetEnvironmentVariable("GITHUB_API_TOKEN") ?? "";
    private IAPIRequestContext Request = null;

    public string getDoc() => @"https://dog.ceo/api/breeds/image/random";
    public string getImage() => @"https://images.dog.ceo/breeds/schipperke/n02104365_9489.jpg";

    public async Task SetUpAPITesting()
    {
        await CreateAPIRequestContext();
    }

    private async Task CreateAPIRequestContext()
    {
        Request = await this.Playwright.APIRequest.NewContextAsync(new()
        {
            BaseURL = "https://api.github.com",
        });
    }


}
[Binding]
public class BoaSteps
{

}
[Binding]
public class PlaywrightSteps : PageTest // worth overhead to get expect?
{
    IPage page;
    PlaywrightSteps(IPage page)
    {
        Page.GotoAsync("https://playwright.dev/");
        this.page = page;
    }

    [When(@"on (.*) page")]
    public async Task OnHomePage(string url)
    {
        await page.WaitForURLAsync(url);
    }

    [Then(@"the page should have a title of (.*)")]
    public async Task ThePageShouldHaveATitleOf(string title)
    {

        // Expect a title "to contain" a substring.
        await Expect(page).ToHaveTitleAsync(new Regex(title));
    }

    [Then(@"the url should be (.*)")]
    public async Task Urlshouldbe(string title)
    {
        // Expect a title "to contain" a substring.
        await Expect(page).ToHaveTitleAsync(new Regex(title));
    }

    [When(@"I click link (.*)")]
    public async Task ClickLink(string title)
    {
        // create a locator
        var getStarted = page.GetByRole(AriaRole.Link, new() { Name = "Get started" });

        // Expect an attribute "to be strictly equal" to the value.
        await Expect(getStarted).ToHaveAttributeAsync("href", "/docs/intro");

        // Click the get started link.
        await getStarted.ClickAsync();

        // Expects the URL to contain intro.
        await Expect(Page).ToHaveURLAsync(new Regex(".*intro"));
    }
}

