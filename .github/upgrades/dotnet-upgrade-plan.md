# .NET9.0 Upgrade Plan

## Execution Steps

Execute steps below sequentially one by one in the order they are listed.

1. Validate that an .NET9.0 SDK required for this upgrade is installed on the machine and if not, help to get it installed.
2. Ensure that the SDK version specified in global.json files is compatible with the .NET9.0 upgrade.
3. Upgrade LoanApp.Domain\LoanApp.Domain.csproj
4. Upgrade LoanApp.Application\LoanApp.Application.csproj
5. Upgrade LoanApp.Infrastructure\LoanApp.Infrastructure.csproj
6. Upgrade LoanApp.Api\LoanApp.Api.csproj

## Settings

This section contains settings and data used by execution steps.

### Excluded projects

| Project name | Description |
|:------------|:-----------:|

### Aggregate NuGet packages modifications across all projects

| Package Name | Current Version | New Version | Description |
|:------------------------------|:---------------:|:-----------:|:------------|
| Microsoft.Extensions.Options |8.0.0 | | Replace with Microsoft.Extensions.Options9.0.11 |
| Microsoft.Extensions.Options | |9.0.11 | Replacement for Microsoft.Extensions.Options |

### Project upgrade details

#### LoanApp.Domain modifications

Project properties changes:
- Target framework should be changed from `net8.0` to `net9.0`

#### LoanApp.Application modifications

Project properties changes:
- Target framework should be changed from `net8.0` to `net9.0`

#### LoanApp.Infrastructure modifications

Project properties changes:
- Target framework should be changed from `net8.0` to `net9.0`

NuGet packages changes:
- Microsoft.Extensions.Options should be updated from `8.0.0` to `9.0.11` (*recommended for .NET9.0*)

#### LoanApp.Api modifications

Project properties changes:
- Target framework should be changed from `net8.0` to `net9.0`
