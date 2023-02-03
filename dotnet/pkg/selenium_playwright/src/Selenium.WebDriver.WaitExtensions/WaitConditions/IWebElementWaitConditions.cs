using Datagrove.Playwright;

namespace Playwright.WebDriver.WaitExtension.WaitConditions
{
    public interface IWebElementWaitConditions
    {
        IWebElement ToExist();
        void ToNotExist();
    }
}