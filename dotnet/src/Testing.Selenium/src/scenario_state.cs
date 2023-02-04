namespace Datagrove.Testing.Selenium;
#nullable enable
using System.Collections.Generic;
using Microsoft.Playwright;

// the purpose of this is so that steps have an easy global state they can share
// this is at odds with have a test that uses multiple contexts, but how could you do this in gherkin anyway? 

// This is a statically typed dependency injection container, if you care about that sort of thing

public class PlaywrightScenarioState : IAsyncDisposable
{

    public PlaywrightOptions options;

    public IPlaywright? playwright;

    public IAPIRequestContext? _api;

    private PlaywrightDriver? _driver;
    public IBrowser? browser;
    public IBrowserContext? browserContext;

    public IPage? _page;

    public PlaywrightScenarioState( PlaywrightOptions options)
    {
        this.options = options;
    }

    public async ValueTask<IPlaywright> pw() {
        if (playwright==null) playwright =  await Playwright.CreateAsync();
        return playwright;
    }
    public async ValueTask<IPage> page() {
        if (_page==null) {
            browser = await (await pw()).Chromium.LaunchAsync(options.options);
            browserContext = await browser.NewContextAsync(options.contextOptions);
            _page = await browserContext.NewPageAsync();
        }
        return _page;
    }

    public async ValueTask<PlaywrightDriver> driver() {
        var p = await page();
        return _driver =  new PlaywrightDriver(browserContext!,p);
    }

    public async ValueTask<IAPIRequestContext> api() => _api = await (await pw()).APIRequest.NewContextAsync();
    
    public async ValueTask DisposeAsync()
    {
        if (_api!=null) await _api.DisposeAsync();
        if (browserContext!=null) await browserContext.DisposeAsync();
        if (browser!=null) await browser.DisposeAsync();
        _driver?.Dispose();
        playwright?.Dispose();
    }
}

