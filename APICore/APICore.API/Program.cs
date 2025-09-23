using APICore.API.Filters;
using APICore.API.Middlewares;
using APICore.API.Utils;
using APICore.Data.Repository;
using APICore.Data.UoW;
using APICore.Services;
using APICore.Services.Impls;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Newtonsoft.Json;
using Serilog;
using System.Globalization;
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using AutoMapper;
using Microsoft.Extensions.Hosting;
using APICore.API;
using Microsoft.Extensions.Configuration;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.File("logs/apicore-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

// Add Serilog
builder.Host.UseSerilog();

// Configure Services
builder.Services.ConfigureI18N();
builder.Services.ConfigureCors();
builder.Services.AddControllers(config =>
{
    config.Filters.Add(typeof(ApiValidationFilterAttribute));
}).AddNewtonsoftJson(opt => opt.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore);

builder.Services.ConfigureHsts();
builder.Services.ConfigureDbContext(builder.Configuration);
builder.Services.ConfigureSwagger();
builder.Services.ConfigureTokenAuth(builder.Configuration);
builder.Services.ConfigurePerformance();
builder.Services.ConfigureHealthChecks(builder.Configuration);
builder.Services.ConfigureDetection();

builder.Services.AddHttpContextAccessor();
builder.Services.AddAutoMapper(typeof(Program));

// Register services with DI patterns for .NET 8
builder.Services.AddTransient<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped(x => new BlobServiceClient(builder.Configuration.GetConnectionString("Azure")));
builder.Services.AddTransient<IAccountService, AccountService>();
builder.Services.AddTransient<ISettingService, SettingService>();
builder.Services.AddTransient<IEmailService, EmailService>();
builder.Services.AddTransient<ILogService, LogService>();
builder.Services.AddTransient<IStorageService, StorageService>();

var app = builder.Build();

// Configure Pipeline
app.UseDetection();
app.UseCors();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "API Core V1");
    });
    // Swagger UI is enabled for development
}
else
{
    app.UseHsts();
}

// Localization
var supportedCultures = new List<CultureInfo>
{
    new CultureInfo("en-US")
};

var localizationOptions = new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(culture: "en-US", uiCulture: "en-US"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
};

app.UseRequestLocalization(localizationOptions);

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<ErrorWrappingMiddleware>();
app.UseResponseCompression();

app.MapHealthChecks("/health", new HealthCheckOptions()
{
    ResultStatusCodes =
    {
        [HealthStatus.Healthy] = StatusCodes.Status200OK,
        [HealthStatus.Degraded] = StatusCodes.Status200OK,
        [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
    }
});

app.MapControllers();

// Seed Database with improved async pattern
using (var scope = app.Services.CreateScope())
{
    await DatabaseSeed.SeedDatabaseAsync(scope.ServiceProvider);
}

await app.RunAsync();