# ExecutivesCompensation service

This repo contains an implementation of the `ExecutivesCompensation` service. The service finds all executives on the ASX stock exchange which have a compensation at least 10% above average for their industry.

## Cloning the repo

To clone, run `git clone https://github.com/BoardOutlook/benjamin-tech-test.git`

## Building and running

Building the repo requires the .NET 8 SDK, available at https://dotnet.microsoft.com/en-us/download/dotnet/8.0.

### Configuring the API key

The backend at https://fn-techtest-ase.azurewebsites.net/api/swagger/ui requires an API key, which is not stored in the repo.

To configure the secret locally, run the following command from the top-level directory, replacing `<value>` with the actual API key:

```shell
dotnet user-secrets --project ExecutivesCompensation set "ServiceApiKey" "<value>"
```

### Running the service

You can open the solution in Visual Studio (untested) or VSCode, or run the following from the top-level directory:

```shell
dotnet build
dotnet run --project ExecutivesCompensation
```

You can run tests with `dotnet test`.

### Debugging with Swagger

Once up and running, the swagger UI should be accessible at http://localhost:5091/swagger.

The first request may take some time. However, intermediate results are cached so subsequent requests should be faster.

## Future work

*   (P0) End-to-end tests: Currently only unit tests
*   (P1) Incoming QoS logging
*   (P1) Outgoing QoS logging
*   (P1) Build pipeline
*   (P1) Deployment pipeline
*   (P2) See if we can auto-generate the client models instead of manually writing them
*   (P2) Better exception handling
*   (P2) Add jitter to the cache expiry
*   (P3) Filtering data if the year is too old
