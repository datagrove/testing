namespace pepin_simple;
using System;
using TechTalk.SpecFlow;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

// This a wrapper around the test runner context; you can store anything you want here. Steps may reference context itself or any member. Each test will initialize a TestState when it starts, and provide it to each of the steps used by the test. Be sure code is thread safe so that you can run all your tests in parallel.
public class StepState : IAsyncDisposable
{
    public TestContext context;

    public StepState(TestContext context)
    {
        this.context = context;
    }

    public async ValueTask DisposeAsync()
    {
        await Task.CompletedTask;
    }

}

// note that the name doen't matter, all steps are effectively global for a compilation. If you are constrained to be backwards compatible with specflow, you can take the name of the
[Binding] 
public class CalculatorSteps : IAsyncDisposable
{
    StepState state;
    // note that a step class is initialized once per test. It is not shared.
    int sum = 0;

    // Note that you can "inject" state variable on any step. The construct or will be called at the beginning of the test (scenario) and the dispose at the end.
    public CalculatorSteps(StepState state)
    {
        this.state = state;
    }

    public async ValueTask DisposeAsync()
    {
        await Task.CompletedTask;
    }

    // note that we can have a mix of async and sync steps. 

    [Given(@"I have a calculator")]
    public void I_have_a_calculator()
    {
        sum = 0;
    }

    [Given(@"I have (.*) and (.*) as input")]
    public async Task I_have_and_as_input(int p0, int p1)
    {
        sum = p0 + p1;
        await Task.CompletedTask;
    }
    [Given(@"I add more numbers")]
    public async Task I_add_more_numbers(Table table)
    {
        foreach (var row in table.Rows)
        {
            sum += int.Parse(row[0]);
        }
        await Task.CompletedTask;
    }

    [Then(@"I should get an output of (.*)")]
    public void I_should_get_an_output_of(int p0)
    {
        Assert.AreEqual(p0, sum);
    }

}
