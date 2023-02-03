namespace Playwright.WebDriver.WaitExtension.WaitConditions
{
    public interface IElementWaitConditions
    {
        void ToBeVisible();
        void ToBeInvisible();
        void ToBeDisabled();
        void ToBeEnabled();
        void ToBeSelected();
        void ToNotBeSelected();
    }
}