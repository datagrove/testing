// File generated by Pepinillo, If you edit this file, rename it and/or delete the .feature file that generates it.

namespace pepin;
#nullable enable
using Microsoft.VisualStudio.TestTools.UnitTesting;
using pepin.steps;

// /Users/jim/dev/datagrove/testing/dotnet/template/pepinillo/feature/Calc.feature
[TestClass()]
[TestCategory("pepin")]
public class Calculator 
{
    public TestContext? TestContext { get; set; }
    
    public class Steps {

        public Steps(StepState context)
        {
            var step=new CalculatorSteps(context);
        }
    }

    [TestMethod()]
    public async Task Add_two_numbers()
    {
        await using (var context = new StepState(TestContext!)){
            var step = new Steps(context);
            }
    }
    [TestMethod()]
    public async Task Add_two_numbers__1()
    {
        await using (var context = new StepState(TestContext!)){
        var step = new Steps(context);
        }
    }

    
}
