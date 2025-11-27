using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using BacklogAPI.Data;

using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using BacklogAPI.Helpers;
using System;

var builder = FunctionsApplication.CreateBuilder(args);

string? kvName = Environment.GetEnvironmentVariable("KEY_VAULT_NAME");
if (string.IsNullOrEmpty(kvName))
{
    throw new InvalidOperationException("Environment variable 'KEY_VAULT_NAME' is not set.");
}
var kvUri = new Uri($"https://{kvName}.vault.azure.net/");

var managedIdentityClientId = Environment.GetEnvironmentVariable("MANAGED_IDENTITY_CLIENT_ID");
DefaultAzureCredentialOptions? options = null;
if (!string.IsNullOrEmpty(managedIdentityClientId))
{
    options = new DefaultAzureCredentialOptions { ManagedIdentityClientId = managedIdentityClientId };
}
var credential = (options == null) 
    ? new DefaultAzureCredential() 
    : new DefaultAzureCredential(options);


builder.Configuration.AddAzureKeyVault(kvUri, credential);

builder.ConfigureFunctionsWebApplication();

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

var connectionString = builder.Configuration.GetValue<string>("DefaultConnection");
if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("Could not retrieve 'DefaultConnection' from configuration (Key Vault).");
}

var apiKey = builder.Configuration.GetValue<string>("ApiKey");
if (string.IsNullOrEmpty(apiKey))
{
    throw new InvalidOperationException("Could not retrieve 'ApiKey' from configuration (Key Vault).");
}

builder.Services.AddDbContext<ApplicationDbContext>(opt =>
{
    opt.UseSqlServer(connectionString);
});

builder.Services.AddSingleton(new ApiKeySettings { ApiKey = apiKey });

builder.Services.AddSingleton(provider =>
{
    return new SecretClient(kvUri, credential);
});
builder.Services.AddSingleton<IKVHelper, KVHelper>();


builder.Build().Run();