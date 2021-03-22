# Accessioning

This project was created with [Craftsman](https://github.com/pdevito3/craftsman).

## Get Started

Go to your solution directory:

```shell
cd Accessioning
```

Run your solution:

```shell
dotnet run --project webapi
```

## Running Integration Tests
To run integration tests:

1. Ensure that you have docker installed.
2. Go to your src directory: `cd C:\Users\Paul\Documents\testoutput\LimsLiteFast2\Accessioning\src`
3. Set an environment. It doesn't matter what that environment name is for these purposes.
    - Powershell: `$Env:ASPNETCORE_ENVIRONMENT = "IntegrationTesting"`
    - Bash: export `ASPNETCORE_ENVIRONMENT = IntegrationTesting`
4. Run a Migration (necessary to set up the database) `dotnet ef migrations add "InitialMigration" --project Accessioning.Infrastructure --startup-project Accessioning.WebApi --output-dir Migrations`
5. Run the tests. They will take some time on the first run in the last 24 hours in order to set up the docker configuration.
