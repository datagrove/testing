using System.Linq;
using System.Text.RegularExpressions;
using Datagrove.Testing.Selenium;
using Playwright.WebDriver.WaitExtension.WaitTypeSelections;

namespace Datagrove.Testing.Selenium
{
    public class ClassWaitConditions : WaitConditionsBase, IClassWaitConditions
    {
        private readonly IWebElement _webelement;

        public ClassWaitConditions(IWebElement webelement, int delayMs) : base(delayMs)
        {
            _webelement = webelement;
        }

        public bool ToContain(string className)
        {
            return WaitFor(() => GetClasses().Contains(className));
        }

        private string[] GetClasses()
        {
            return _webelement.GetAttribute("class").Split(' ');
        }

        public bool ToContainMatch(string regexPattern)
        {
            var regex = new Regex(regexPattern);
            return WaitFor(() => GetClasses().Any( cn => regex.Match(cn).Success), ClassesString());
        }

        public bool ToNotContain(string className)
        {
            return WaitFor(() => !ToContain(className), ClassesString());
        }
        public bool ToNotContainMatch(string regexPattern)
        {
            var regex = new Regex(regexPattern);
            return WaitFor(() => GetClasses().All(cn => !regex.Match(cn).Success), ClassesString());
        }

        private string ClassesString()
        {
            return "classes;\n   " + _webelement.GetAttribute("class");
        }

    }
}