using Microsoft.Playwright;
namespace Datagrove.Playwright;


public class PlaywrightOptions
{
    public enum Browser
    {
        Chrome,
        Edge,
        Safari,
        Firefox
    }

    public Browser browserType;
    public BrowserTypeLaunchOptions options;
    public BrowserNewContextOptions contextOptions;

    public string trace {get;set;} = "";
    public PlaywrightOptions()
    {
        this.options =  new BrowserTypeLaunchOptions();
        this.contextOptions = contextOptions ?? new BrowserNewContextOptions
        {
            ViewportSize = new ViewportSize
            {
                Width = 1920,
                Height = 1080
            },
            RecordVideoDir = "videos"
        };
        this.browserType = Browser.Chrome;
    }

}





