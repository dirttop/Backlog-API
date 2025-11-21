using System;
using System.Threading.Tasks;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

namespace BacklogAPI.Helpers
{
    public class KVHelper
    {
        private readonly SecretClient _client;

        public KVHelper()
        {
            string kvName = Environment.GetEnvironmentVariable("KEY_VAULT_NAME");
            if (string.IsNullOrEmpty(kvName))
            {
                throw new InvalidOperationException("Environment variable 'KEY_VAULT_NAME' is not set.");
            }

            var kvUri = $"https://{kvName}.vault.azure.net/";

            var managedIdentityClientId = Environment.GetEnvironmentVariable("MANAGED_IDENTITY_CLIENT_ID");
            
            DefaultAzureCredentialOptions options = null;

            if (!string.IsNullOrEmpty(managedIdentityClientId))
            {
                options = new DefaultAzureCredentialOptions
                {
                    ManagedIdentityClientId = managedIdentityClientId
                };
            }

            var credential = (options == null)
                ? new DefaultAzureCredential()
                : new DefaultAzureCredential(options);

            _client = new SecretClient(new Uri(kvUri), credential);
        }
        public async Task<string> GetSecretAsync(string secretName)
        {
            try
            {
                KeyVaultSecret secret = await _client.GetSecretAsync(secretName);
                return secret.Value;
            }
            catch (Azure.RequestFailedException ex) when (ex.Status == 404)
            {
                Console.WriteLine($"Secret '{secretName}' not found.");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving secret: {ex.Message}");
                throw;
            }
        }
    }
}