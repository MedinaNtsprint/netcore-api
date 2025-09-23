# NET 7 API

[![Continuous Integration](https://github.com/techsavyntsprint/netcore-api/workflows/CI/badge.svg)](https://github.com/techsavyntsprint/netcore-api/actions)

The solution includes the API to be reused. 
````markdown
# APICore (.NET)

This repository contains an ASP.NET Core Web API (APICore) and related projects (Data, Services, Tests). The codebase is currently targeting .NET 8.

Badge(s)
- CI badge may be present in the original upstream repository.

## Quick start (Windows / PowerShell)

Prerequisites
- .NET SDK 8.x installed. Verify with:

```powershell
dotnet --version
```

- A valid `appsettings.json` in the `APICore/APICore.API` project root (see the sample below).

Build

From the repository root or `APICore` folder run:

```powershell
dotnet restore
dotnet build "APICore.sln" -c Debug
```

`````markdown
# NET 7 API / APICore (.NET)

[![Continuous Integration](https://github.com/techsavyntsprint/netcore-api/workflows/CI/badge.svg)](https://github.com/techsavyntsprint/netcore-api/actions)

This repository contains an ASP.NET Core Web API (APICore) and related projects (Data, Services, Tests).

Note: the original project referenced .NET 7; this workspace is currently stabilized to build and run on .NET 8. The samples below include the original appsettings fields that are still used by the project (Blob settings, SendGrid, detailed BearerTokens entries, etc.).

## Quick start (Windows / PowerShell)

Prerequisites
- .NET SDK 8.x installed. Verify with:

```powershell
dotnet --version
```

- A valid `appsettings.json` in the `APICore/APICore.API` project root (see the sample below).

Build

From the repository root or `APICore` folder run:

```powershell
dotnet restore
dotnet build "APICore.sln" -c Debug
```

Run tests

```powershell
dotnet test "APICore.sln" -c Debug
```

Run the API locally (from `APICore.API` folder):

```powershell
cd APICore.API
dotnet run
```

This will start Kestrel on the configured port(s) defined in `Properties/launchSettings.json` or the environment.

## APICore sample_appsettings.json (full fields)

Is important to note: in the case of Azure, you can use developer tools instead of production environments directly from Azure on your developer machine. To accomplish this you can install the Azure Storage Emulator (or use Azurite). After installing the emulator, replace the Azure connection string with the emulator one shown below.

Install Azure Storage Emulator (or Azurite) and use the development account settings when testing blobs locally.

Example (copy `sample_appsettings.json` to `appsettings.json` and update values):

```json
{
  "ConnectionStrings": {
    "AlBoteConnection": "Server=[YOUR_SERVER];Database=[YOUR_DATABASE];User Id=[YOUR_USERNAME];Password=[YOUR_PASWORD];",
    "Azure": "DefaultEndpointsProtocol=https;AccountName=[YOUR_ACCOUNT_NAME];AccountKey=[YOUR_ACCOUNT_KEY];BlobEndpoint=[ROOT_PATH];"
  },

  "BearerTokens": {
    "Key": "YOUR_SECRET_KEY",
    "Issuer": "API_HOST",
    "Audience": "Any",
    "AccessTokenExpirationMinutes": ACCESS_TOKEN_EXPIRATION_TIME,
    "RefreshTokenExpirationMinutes": REFRESH_TOKEN_EXPIRATION_TIME,
    "AllowMultipleLoginsFromTheSameUser": IF_YOU_MAY_ACCEPT_MULTIPLE_LOGINS,
    "AllowSignoutAllUserActiveClients": true
  },

  "Blobs": {
    "ImagesRootPath": "[ROOT_PATH]/[CONTAINER_NAME]",
    "ImagesContainer": "[CONTAINER_NAME]"
  },
  "SendGrid": {
    "SendGridKey": "[SENDGRID_KEY]",
    "SendGridUser": "[SENDGRID_USER]",
    "UseSandbox": "true"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "AllowedHosts": "*"
}
```

Azure Storage Emulator (developer storage) example connection string

Replace the Azure connection string with the development emulator's string when using the emulator (example):

```
DefaultEndpointsProtocol=https;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;BlobEndpoint=[ROOT_PATH];
```

## appsettings notes (important)

- `BearerTokens:Key` — secret used to sign JWT tokens. IMPORTANT: when using HS256, the key must be long enough (recommended at least 32 bytes / 256 bits). If you use a short string here tests or token generation may fail with a key-length error.
- `AccessTokenExpirationMinutes` vs `AccessTokenExpirationHours`: some sample files use minutes, others use hours; the implementation in the code expects the values used in the project's configuration (check `Program.cs` and `ServicesExtensions.cs`). Adapt the sample values accordingly.

## Migrations / Database

EF Core is used for migrations. In the original README we referenced EF Core CLI for .NET 7; use the corresponding EF Core tools compatible with your SDK. From the `APICore.Data` project folder you can add a migration and apply it to the database using the API project as the startup project:

```powershell
cd APICore.Data
dotnet ef migrations add NameOfTheMigration -s ..\APICore.API
dotnet ef database update -s ..\APICore.API
```

Make sure the `APICore.API/appsettings.json` `ConnectionStrings` entry points to a reachable database before running migrations.

## Changes applied while stabilizing the repo

- Project targets updated to .NET 8 in the source tree.
- NuGet package versions were aligned to avoid package-downgrade/restore errors (MySql packages, AutoMapper, Serilog adjustments were made where necessary).
- Replaced obsolete SHA256CryptoServiceProvider usages with `System.Security.Cryptography.SHA256.Create()` to remove SYSLIB0021 warnings.
- Replaced `HttpContext.Response.Headers.Add(...)` with indexer assignments to satisfy ASP.NET analyzers (ASP0019) and avoid duplicate-key issues.
- Updated integration tests to use async/await patterns and to assert concrete service exception types. Also ensured test JWT keys are long enough for HS256.

## Troubleshooting

- If the language server / IDE shows NuGet restore or package-related warnings, run `dotnet restore` in the `APICore` folder and restart the IDE window.
- If JWT token creation throws an ArgumentOutOfRangeException referring to HS256 key size, ensure `BearerTokens:Key` is at least 32 bytes long.

## Contributing

If you add or change packages, please keep versions consistent across projects to avoid downgrade warnings. Run the test suite after changes.

````

