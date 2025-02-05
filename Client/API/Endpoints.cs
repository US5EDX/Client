using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Client.API
{
    public class Endpoints
    {
        private readonly IConfigurationSection _endpoints;
        private readonly HttpClient _client;

        public Endpoints(HttpClient client, IConfigurationSection endpoints)
        {
            _endpoints = endpoints;
            _client = client;
            _client.Timeout = TimeSpan.FromSeconds(1000);
            _client.DefaultRequestHeaders.Accept.Clear();
        }

        public async Task<HttpResponseMessage?> PostCall(string nav, string endpoint, object newObject, string? accessToken = null)
        {
            try
            {
                string apiUrl = $"{_endpoints["Base"]}{_endpoints[nav]}{endpoint}";

                if (accessToken is not null)
                {
                    _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                }

                var jsonContent = JsonSerializer.Serialize(newObject);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                return await _client.PostAsync(apiUrl, content);
            }
            catch
            {
                return null;
            }
        }
    }
}
