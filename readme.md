## Create project structure
1. Create solution `dotnet new sln`
2. Create lib `dotnet new classlib -o src/Aggregator`
3. Create test project `dotnet new xunit -o test/AggregatorTest` 
4. add to the solution `dotnet sln add src/Aggregator/Aggregator.csproj` 
5. add to the solution `dotnet sln add test/AggregatorTest/AggregatorTest.csproj` 
7. add reference `dotnet add ./test/AggregatorTest/AggregatorTest.csproj reference ./src/Aggregator/Aggregator.csproj` 
8. add gitignore `dotnet new gitignore`


## Run tests

`dotnet test --logger:"console;verbosity=normal"`



## Resources
https://learn.microsoft.com/en-us/dotnet/core/tutorials/libraries 