namespace Playwright.WebDriver.WaitExtension.WaitConditions
{
    public interface IWebPageWaitConditions
    {
        void TitleToEqual(string title);
        void TitleToContain(string title);

        void UrlToEqual(string url);
        void UrlToContain(string url);
        void UrlToMatch(string regex);
        void ReadyStateComplete();
    }
}