using Duende.IdentityModel;
using Duende.IdentityServer.Models;

namespace IdentityProvider;

public static class Config
{
    public static IEnumerable<IdentityResource> IdentityResources =>
    [
        new IdentityResources.OpenId(),
        new IdentityResources.Profile(),
    ];

    public static IEnumerable<ApiScope> ApiScopes =>
    [
        new ApiScope("shopclientscope")
    ];

    public static IEnumerable<Client> Clients() =>
    [
        // represents the client that is delegating the access token
        new Client
        {
            ClientId = "tokenexchangeclientid",
            ClientSecrets = { new Secret("--in-user-secrets--".Sha256()) },

            AllowedGrantTypes = { OidcConstants.GrantTypes.TokenExchange },
            AllowedScopes = { "shopclientscope" }
        }
    ];
}
