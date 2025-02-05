using Client.Stores;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Client.Handlers
{
    public class TokenRefreshHandler : DelegatingHandler
    {
        private readonly UserStore _userStore;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _endpoints;

        public TokenRefreshHandler(UserStore userStore, IHttpClientFactory httpClientFactory, IConfigurationSection endpoints)
        {
            _userStore = userStore;
            _httpClientFactory = httpClientFactory;
            _endpoints = endpoints;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = await base.SendAsync(request, cancellationToken);

            if (_userStore.UserId != null && response.StatusCode == HttpStatusCode.Unauthorized)
            {
                if (await RefreshTokenAsync())
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _userStore.AccessToken);
                    response = await base.SendAsync(request, cancellationToken);
                }
            }

            return response;
        }

        private async Task<bool> RefreshTokenAsync()
        {
            var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(1000);

            var refreshRequest = new HttpRequestMessage(HttpMethod.Post, $"{_endpoints["Base"]}{_endpoints["Auth"]}refresh")
            {
                Content = new StringContent(JsonSerializer.Serialize(_userStore.RefreshToken),
                                            Encoding.UTF8, "application/json")
            };

            var response = await client.SendAsync(refreshRequest);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var jsonResponse = JsonSerializer.Deserialize<JsonObject>(responseContent);

                if (jsonResponse != null)
                {
                    _userStore.AccessToken = jsonResponse["accessToken"].ToString();
                    _userStore.RefreshToken = jsonResponse["refreshToken"].ToString();
                    return true;
                }
            }

            return false;
        }
    }
}