namespace Datagrove.Playwright;
#nullable enable
using System.Collections.Generic;
using Microsoft.Playwright;

public class PlaywrightState : IAsyncDisposable
{
    public PlaywrightOptions options;
    public IPlaywright playwright;
    public IBrowser browser;
    public IBrowserContext context;

    public IPage page;

    PlaywrightState(PlaywrightOptions options, IPlaywright playwright, IBrowser browser, IBrowserContext context, IPage page)
    {
        this.options = options;
        this.playwright = playwright;
        this.browser = browser;
        this.context = context;
        this.page = page;
    }

    static public async ValueTask<PlaywrightState> createAsync(PlaywrightOptions opt)
    {
        var playwright = await Playwright.CreateAsync();
        IBrowser? browser = null;

        switch (opt.browserType)
        {
            case PlaywrightOptions.Browser.Chrome:
                browser = await playwright.Chromium.LaunchAsync(opt.options);
                break;
            case PlaywrightOptions.Browser.Edge:
                // channel msedge
                browser = await playwright.Chromium.LaunchAsync(opt.options);
                break;
            case PlaywrightOptions.Browser.Safari:
                browser = await playwright.Webkit.LaunchAsync(opt.options);
                break;
            case PlaywrightOptions.Browser.Firefox:
                browser = await playwright.Firefox.LaunchAsync(opt.options);
                break;
        }

        if (browser == null)
        {
            throw new Exception("Browser not found");
        }

        var context = await browser.NewContextAsync(opt.contextOptions);
        // p.context.SetDefaultTimeout(10000);
        var page = await context.NewPageAsync();
        await context.Tracing.StartAsync(new TracingStartOptions
        {
            Screenshots = true,
            Snapshots = true,
        });
        return new PlaywrightState(opt, playwright, browser, context, page);
    }

    public async ValueTask DisposeAsync()
    {
        await context.Tracing.StopAsync(new()
        {
            Path = options.trace
        });

        await context.CloseAsync();
        await browser.DisposeAsync();
        playwright.Dispose();
    }
}

// assumes that all IWebDrivers will call from the same thread. add a mutex if this
public class PlaywrightProxy
{
    public PlaywrightState state;

    Semaphore rpc = new Semaphore(0, 1);
    Semaphore reply = new Semaphore(0, 1);

    PlaywrightDriver driver;

    List<Func<PlaywrightDriver, Task<object>>> fn = new List<Func<PlaywrightDriver, Task<object>>>();
    public bool quit = false;
    Exception? e;
    public T exec<T>(PlaywrightDriver driver, Func<PlaywrightDriver, Task<object>> fn)
    {
        this.driver = driver;
        e = null;
        this.fn.Clear();
        this.fn.Add(fn);
        rpc.Release();
        reply.WaitOne();
        if (e != null)
        {
            // we should snapshot here,  
            throw e;
        }
        return (T)driver!.rvalue!;
    }
    public void Perform(List<Func<PlaywrightDriver, Task<object>>> fn)
    {
        e = null;
        this.fn = fn;
        rpc.Release();
        reply.WaitOne();
        if (e != null)
        {
            throw e;
        }
    }

    public PlaywrightProxy(PlaywrightDriver root, PlaywrightState state)
    {
        this.state = state;
        this.driver = root;
        Task.Run(async () => await ThreadProc(this, root));
        reply.WaitOne();
    }

    public static async Task ThreadProc(PlaywrightProxy p, PlaywrightDriver root)
    {
        while (!p.quit)
        {
            p.reply.Release();
            p.rpc.WaitOne();
            try
            {
                foreach (var e in p.fn)
                {
                    p.driver.rvalue = await e(p.driver);
                }
            }
            catch (Exception o)
            {
                p.e = o;
            }
        }

        p.reply.Release();
    }

    public void stop()
    {
        quit = true;
        rpc.Release();
        reply.WaitOne();
    }
}
