# ASP.NET Core delegated OAuth 2.0 Token Exchange RFC 8693 access token management

ASP.NET Core implementation of the OAuth 2.0 Token Exchange RFC 8693 standard using Entra ID and Duende IdentityServer.

[![.NET](https://github.com/damienbod/token-mgmt-ui-delegated-token-exchange/actions/workflows/dotnet.yml/badge.svg)](https://github.com/damienbod/token-mgmt-ui-delegated-token-exchange/actions/workflows/dotnet.yml)

## Setup

The solution implements an ASP.NET Core web application which authenticates using Microsoft Entra ID. The web application uses an API protected with a Microsoft Entra ID access token. This API uses another downstream API protected with Duende IdentityServer. The API exchanges the Microsoft Entra ID access token for a new Duende IdentityServer access token using the OAuth 2.0 Token Exchange standard. Both APIs use a user delegated access token. The tokens are persisted on the trusted backend using the IDistributedCache implementation. This can be an in-memory cache or a persistent cache. When using this cache, it is important to automatically renew the access token, if it is missing or invalid.

![ASP.NET Core access token management](https://github.com/damienbod/token-mgmt-ui-delegated-token-exchange/blob/main/images/context.png)

## Blogs in this series

- [ASP.NET Core user delegated access token management](https://damienbod.com/2025/01/15/asp-net-core-user-delegated-access-token-management/)
- [ASP.NET Core user application access token management](https://damienbod.com/2025/01/20/asp-net-core-user-application-access-token-management/)
- [ASP.NET Core delegated OAuth token exchange access token management](https://damienbod.com/2025/02/10/asp-net-core-delegated-oauth-token-exchange-access-token-management/)
- [ASP.NET Core delegated Microsoft OBO access token management (Entra only)](https://damienbod.com/2025/03/25/asp-net-core-delegated-microsoft-obo-access-token-management-entra-only/)

## Migrations

```
Add-Migration "InitIdentityNew" -c ApplicationDbContext
```

```
Update-Database
```

## Further examples of OAuth 2.0 Token Exchange RFC 8693

OAuth 2.0 Token Exchange delegated implementation with Microsoft Entra ID and OpenIddict (RFC 8693)

https://github.com/damienbod/OAuthGrantExchangeOidcDownstreamApi

[OAuth 2.0 Token Exchange delegated implementation with Microsoft Entra ID and OpenIddict](https://damienbod.com/2023/01/09/implement-the-oauth-2-0-token-exchange-delegated-flow-between-an-azure-ad-api-and-an-api-protected-using-openiddict/)

## History

- 2025-08-01 Update packages
- 2025-02-07 Update packages
- 2025-02-01 Initial version

## Links

https://damienbod.com/2023/01/09/implement-the-oauth-2-0-token-exchange-delegated-flow-between-an-azure-ad-api-and-an-api-protected-using-openiddict/

https://github.com/damienbod/OAuthGrantExchangeOidcDownstreamApi

https://docs.duendesoftware.com/identityserver/v7/tokens/extension_grants/token_exchange/

https://datatracker.ietf.org/doc/html/rfc8693

https://www.youtube.com/watch?v=Ue8HKBGkIJY&t=

https://github.com/damienbod/OnBehalfFlowOidcDownstreamApi

https://www.rfc-editor.org/rfc/rfc6749#section-5.2

https://github.com/blowdart/idunno.Authentication/tree/dev/src/idunno.Authentication.Basic

## Standards

[JSON Web Token (JWT)](https://datatracker.ietf.org/doc/html/rfc7519)

[Best Current Practice for OAuth 2.0 Security](https://datatracker.ietf.org/doc/rfc9700/)

[The OAuth 2.0 Authorization Framework](https://datatracker.ietf.org/doc/html/rfc6749)

[OAuth 2.0 Demonstrating Proof of Possession DPoP](https://datatracker.ietf.org/doc/html/rfc9449)

[OAuth 2.0 JWT-Secured Authorization Request (JAR) RFC 9101](https://datatracker.ietf.org/doc/rfc9101/)

[OAuth 2.0 Mutual-TLS Client Authentication and Certificate-Bound Access Tokens](https://datatracker.ietf.org/doc/html/rfc8705)

[OpenID Connect 1.0](https://openid.net/specs/openid-connect-core-1_0-final.html)

[Microsoft identity platform and OAuth 2.0 On-Behalf-Of flow](/azure/active-directory/develop/v2-oauth2-on-behalf-of-flow)

[OAuth 2.0 Token Exchange RFC 8693](https://datatracker.ietf.org/doc/html/rfc8693)

[JSON Web Token (JWT) Profile for OAuth 2.0 Access Tokens](https://datatracker.ietf.org/doc/html/rfc9068)

[HTTP Semantics RFC 9110](https://datatracker.ietf.org/doc/html/rfc9110#section-15.5.2)
