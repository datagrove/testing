using Datagrove.Testing.Selenium;
using Playwright.WebDriver.WaitExtension.WaitConditions;

namespace Playwright.WebDriver.WaitExtension.WaitTypeSelections
{
    public interface IWaitTypeSelection
    {
        IWebElementWaitConditions ForElement(By @by);
        IWebPageWaitConditions ForPage();
    }
}