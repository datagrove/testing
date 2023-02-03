This is a generated sample project that uses Pepinillo to compile 
Cucumber/Gherkin into c# tests using microsoft test.

To run the tests 
```
dotnet test
```

After adding or changing feature files you must run 
```
pepin build
```

Note there are two  projects: The outer project  holds the generated code for tests and the inner project has the definitions of steps for those tests. Have two projects allows pepin to rebuild the step project even if there are errors in the generated code (mostly due to configuration issues). The outer project references the inner one.

Pepin build will first build the c# project, then search it for cucumber steps. If a step is not found pepin will generate a shell in needed_steps.cs.

