using IdentityModel.Client;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace WebApiEntraId.WebApiDuende;

public class WebApiDuendeService
{
    private readonly IOptions<WebApiDuendeConfig> _webApiDuendeConfig;
    private readonly IHttpClientFactory _clientFactory;
    private readonly ApiTokenCacheClient _apiTokenClient;

    public WebApiDuendeService(
        IOptions<WebApiDuendeConfig> webApiDuendeConfig,
        IHttpClientFactory clientFactory,
        ApiTokenCacheClient apiTokenClient)
    {
        _webApiDuendeConfig = webApiDuendeConfig;
        _clientFactory = clientFactory;
        _apiTokenClient = apiTokenClient;
    }

    public async Task<string> GetWebApiDuendeDataAsync(string entraIdAccessToken)
    {
        try
        {
            var client = _clientFactory.CreateClient();

            client.BaseAddress = new Uri(_webApiDuendeConfig.Value.ApiBaseAddress);

            var accessToken = await _apiTokenClient.GetApiTokenOauthGrantTokenExchange
            (
                _webApiDuendeConfig.Value.ClientId,
                _webApiDuendeConfig.Value.Audience,
                _webApiDuendeConfig.Value.ScopeForAccessToken,
                _webApiDuendeConfig.Value.ClientSecret,
                entraIdAccessToken
            );

            client.SetBearerToken(accessToken);

            var response = await client.GetAsync("api/profiles/photo");
            if (response.IsSuccessStatusCode)
            {
                var data = await JsonSerializer.DeserializeAsync<string>(
                    await response.Content.ReadAsStreamAsync());

                if (data != null)
                {
                    return data;
                }

                return string.Empty;
            }

            throw new ApplicationException($"Status code: {response.StatusCode}, Error: {response.ReasonPhrase}");
        }
        catch (Exception e)
        {
            throw new ApplicationException($"Exception {e}");
        }
    }
}