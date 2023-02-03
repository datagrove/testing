using Datagrove.Playwright;
using Playwright.WebDriver.WaitExtension.WaitTypeSelections;

namespace Playwright.WebDriver.WaitExtension
{
    public static class WebElementExtensions
    {
        public static IElementWaitTypeSelection Wait(this IWebElement webelement , int ms = 500)
        {
            return new ElementWaitTypeSelection(webelement, ms);
        }

    }
}