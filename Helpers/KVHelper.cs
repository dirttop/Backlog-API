using System;
using System.Threading.Tasks;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Logging;
using Azure;

namespace BacklogAPI.Helpers
{
    public interface IKVHelper
    {
        Task<string> GetSecretAsync(string secretName);
    }

    public class KVHelper : IKVHelper
    {
        private readonly SecretClient _client;
        private readonly ILogger<KVHelper> _logger;
        public KVHelper(SecretClient client, ILogger<KVHelper> logger)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<string> GetSecretAsync(string secretName)
        {
            try
            {
                KeyVaultSecret secret = await _client.GetSecretAsync(secretName);
                return secret.Value;
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                _logger.LogWarning("Secret '{SecretName}' not found in Key Vault.", secretName);
                return string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving secret '{SecretName}'.", secretName);
                throw;
            }
        }
    }
}