namespace Datagrove.Playwright;
#nullable enable
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using Microsoft.Playwright;
using Microsoft.Playwright.MSTest;
using Datagrove.Playwright;
using Datagrove.Playwright.Interactions;

// assumes that all IWebDrivers will call from the same thread. add a mutex if this
public class BrowserProxy
{
    public PlaywrightOptions options;
    public IPlaywright playwright;
    public IBrowser browser;
    public IBrowserContext context;

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

    public BrowserProxy(PlaywrightDriver root, PlaywrightOptions? options = null)
    {
        this.options = options ?? new PlaywrightOptions();
        Task.Run(async () => await ThreadProc(this, root));
        reply.WaitOne();
    }


    public static async Task ThreadProc(BrowserProxy p, PlaywrightDriver root)
    {
        var playwright = await Playwright.CreateAsync();
        await playwright.Firefox.LaunchAsync();
        IBrowser? browser = null;

        var opt = p.options.options;
        switch (p.options.browserType)
        {
            case PlaywrightOptions.Browser.Chrome:
                browser = await playwright.Chromium.LaunchAsync(opt);
                break;
            case PlaywrightOptions.Browser.Edge:
                // channel msedge
                browser = await playwright.Chromium.LaunchAsync(opt);
                break;
            case PlaywrightOptions.Browser.Safari:
                browser = await playwright.Webkit.LaunchAsync(opt);
                break;
            case PlaywrightOptions.Browser.Firefox:
                browser = await playwright.Firefox.LaunchAsync(opt);
                break;
        }
        if (browser == null)
        {
            throw new Exception("Browser not found");
        }

        p.context = await browser.NewContextAsync(p.options.contextOptions);
       // p.context.SetDefaultTimeout(10000);
        root.page = await p.context.NewPageAsync();

        await p.context.Tracing.StartAsync(new TracingStartOptions
        {
            Screenshots = true,
            Snapshots = true,
        });

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
        await p.context.Tracing.StopAsync(new()
        {
            Path = p.options.trace
        });

        await p.context.CloseAsync();
        await browser.DisposeAsync();
        playwright.Dispose();
        p.reply.Release();
    }


    public void stop()
    {
        quit = true;
        rpc.Release();
        reply.WaitOne();
    }


}
