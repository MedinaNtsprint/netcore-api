# Copilot Instructions for APICore .NET 8 Project

## Architecture Overview

This is a clean architecture .NET 8 Web API with these key layers:
- **APICore.API**: Web API controllers, middleware, DI configuration
- **APICore.Services**: Business logic interfaces and implementations  
- **APICore.Data**: EF Core entities, DbContext, repository pattern, migrations
- **APICore.Common**: DTOs for requests/responses
- **APICore.Test**: xUnit integration and unit tests

## Key Patterns & Conventions

### Service Registration Pattern
Services use conditional registration in `Program.cs`. Example with Azure Blob Storage:
```csharp
// Only register real Azure service when connection string is properly configured
if (!string.IsNullOrWhiteSpace(azureConnection) && !azureConnection.Contains("[YOUR_ACCOUNT_NAME]"))
{
    builder.Services.AddScoped(x => new BlobServiceClient(azureConnection));
    builder.Services.AddTransient<IStorageService, StorageService>();
}
else
{
    // No-op implementation for local development
    builder.Services.AddTransient<IStorageService, NoOpStorageService>();
}
```

### Entity Structure
All entities inherit from `BaseEntity` with `Id`, `CreatedAt`, `ModifiedAt`. Example:
```csharp
public class User : BaseEntity
{
    public string Identity { get; set; }
    public string Email { get; set; }
    // ... other properties
    public virtual ICollection<UserToken> UserTokens { get; set; }
}
```

### Repository Pattern
Uses generic repository with UoW pattern:
- `IGenericRepository<T>` for standard CRUD operations
- `IUnitOfWork` for transaction management
- Controllers inject services, services inject repositories

### Controller Response Pattern
Controllers return standardized API responses using classes in `BasicResponses/`:
- `ApiOkResponse<T>` for successful responses
- `ApiBadRequestResponse` for validation errors
- `ApiServiceUnavailableResponse` for system errors

### Mapping Strategy
Uses **Riok.Mapperly** (source generators) instead of AutoMapper for better performance:
- `IAppMapper` interface with `Map()` methods
- `AppMapper` class uses `[Mapper]` attribute with partial implementation
- Registered as singleton in DI container

## Configuration Requirements

### appsettings.json Critical Settings
1. **JWT Key Length**: `BearerTokens.Key` must be ≥32 bytes for HS256 algorithm
2. **Database**: PostgreSQL with EF Core, connection in `ConnectionStrings.ApiConnection`  
3. **Azure Blob**: Optional, falls back to no-op service when not configured
4. **SendGrid**: Email service configuration

Use `sample_appsettings.json` as template - copy to `appsettings.json` and update placeholders.

## Development Workflows

### Build & Test Commands
```powershell
# Use helper script for common tasks
./dev.ps1 -RestoreBuild    # Restore packages and build
./dev.ps1 -RunTests        # Run test suite  
./dev.ps1 -RunApi          # Start API locally
./dev.ps1 -NewTestJwtKey   # Generate secure JWT key
```

### Database Migrations
EF Core migrations from `APICore.Data` folder using API as startup project:
```powershell
cd APICore.Data
dotnet ef migrations add MigrationName -s ..\APICore.API
dotnet ef database update -s ..\APICore.API
```

### Testing Structure
- **Integration tests**: Use in-memory SQLite/EF Core
- **Unit tests**: Mock dependencies with Moq
- Test JWT keys must be ≥32 bytes (same HS256 requirement)

## Technology Stack Specifics

- **Logging**: Serilog with file and console sinks (`logs/` folder)
- **Health Checks**: `/health` endpoint with PostgreSQL and system checks
- **API Documentation**: Scalar UI (not Swagger UI) in development
- **Authentication**: JWT Bearer tokens with refresh token support
- **Validation**: `ApiValidationFilterAttribute` on all controllers
- **Error Handling**: `ErrorWrappingMiddleware` for global exception handling

## Common Gotchas

1. **JWT Key Size**: Short keys cause runtime exceptions - use `./dev.ps1 -NewTestJwtKey`
2. **PostgreSQL Version**: Project uses PostgreSQL with EF Core in `ServicesExtensions.ConfigureDbContext()`
3. **Async Patterns**: Database seeding uses `await DatabaseSeed.SeedDatabaseAsync()` in `Program.cs`
4. **File Templates**: HTML email templates in `Templates/` folder are copied to output
5. **Azure Storage**: Local development should use Azurite emulator, not production Azure

## Adding New Features

1. **New Entity**: Inherit from `BaseEntity`, add to `CoreDbContext`, create migration
2. **New Service**: Create interface in `Services/`, implementation in `Services/Impls/`, register in `Program.cs`
3. **New Controller**: Inject required services, use API response classes, add validation filter
4. **New DTO**: Add to `Common/DTO/Request|Response/`, update mapper interface/implementation