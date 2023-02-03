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

Pepin build will first build the c# project, then search it for cucumber steps. If a step is not found pepin will generate a shell in needed_steps.cs.

