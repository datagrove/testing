namespace Datagrove.Pep;

using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics;

// these are tests for the gherkin compiler. It is more conventional in C# to put these in their own project, so these probably go away at some point.

[TestClass]
[TestCategory("unit")]
public class PepinTest
{
    string rootDir()
    {
        return Directory.GetCurrentDirectory() + "/../../../../../template/pepinillo";
    }
    [TestMethod]
    public async Task TestInit()
    {
        await Pepin.build(rootDir());
    }
}