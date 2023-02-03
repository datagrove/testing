namespace pepin.steps;
using System;
using TechTalk.SpecFlow;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// This a wrapper around the test runner context; you can store anything you want here. Steps may reference context itself or any member. Each test will initialize a TestState when it starts, and provide it to each of the steps used by the test. Be sure code is thread safe so that you can run all your tests in parallel.
public class StepState : IAsyncDisposable {
    public TestContext context;

    public StepState(TestContext context) {
        this.context = context;
    }

    public async ValueTask DisposeAsync() {
        await Task.CompletedTask;
    }

}

// note that the name doen't matter, all steps are effectively global for a compilation. If you are constrained to be backwards compatible with specflow, you can take the name of the 
public class CalculatorSteps : IAsyncDisposable
{
    StepState state;

     // Note that you can "inject" state variable on any step. The construct or will be called at the beginning of the test (scenario) and the dispose at the end.
   public  CalculatorSteps(StepState state){
        this.state = state;
    }

    public async ValueTask DisposeAsync()
    {
        await Task.CompletedTask;
    }

    // steps can be sync or async
    [Given(@"I have entered (.*) into the calculator")]
    public void GivenIHaveEnteredIntoTheCalculator(int p0)
    {
        //ScenarioContext.Current.Pending();
    }
}
