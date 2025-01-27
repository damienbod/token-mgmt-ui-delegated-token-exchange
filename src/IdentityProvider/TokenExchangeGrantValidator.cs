using System.Security.Claims;
using System.Text.Json;
using Duende.IdentityModel;
using Duende.IdentityServer;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Validation;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Protocols;
using OAuthGrantExchangeIntegration.Server;
using Microsoft.Extensions.Options;
using OAuthGrantExchangeIntegration;

namespace IdentityProvider;

public class TokenExchangeGrantValidator : IExtensionGrantValidator
{
    private readonly ITokenValidator _validator;
    private readonly OauthTokenExchangeConfiguration _oauthTokenExchangeConfiguration;

    public TokenExchangeGrantValidator(ITokenValidator validator,
        IOptions<OauthTokenExchangeConfiguration> oauthTokenExchangeConfiguration)
    {
        _validator = validator;
        _oauthTokenExchangeConfiguration = oauthTokenExchangeConfiguration.Value;
    }

    public async Task ValidateAsync(ExtensionGrantValidationContext context)
    {
        // defaults
        context.Result = new GrantValidationResult(TokenRequestErrors.InvalidRequest);
        var customResponse = new Dictionary<string, object>
        {
            {OidcConstants.TokenResponse.IssuedTokenType, OidcConstants.TokenTypeIdentifiers.AccessToken}
        };

        var subjectToken = context.Request.Raw.Get(OidcConstants.TokenRequest.SubjectToken);
        var subjectTokenType = context.Request.Raw.Get(OidcConstants.TokenRequest.SubjectTokenType);
        var oauthTokenExchangePayload = new OauthTokenExchangePayload
        {
            subject_token = subjectToken!,
            subject_token_type = subjectTokenType!,
            audience = context.Request.Raw.Get(OidcConstants.TokenRequest.Audience),
            grant_type = context.Request.Raw.Get(OidcConstants.TokenRequest.GrantType)!,
            scope = context.Request.Raw.Get(OidcConstants.TokenRequest.Scope),
        };
        // mandatory parameters
        if (string.IsNullOrWhiteSpace(subjectToken))
        {
            return;
        }

        if (!string.Equals(subjectTokenType, OidcConstants.TokenTypeIdentifiers.AccessToken))
        {
            return;
        }

        /////////
        // TODO Validate Entra ID token

        var (Valid, Reason, Error) = ValidateOauthTokenExchangeRequestPayload
           .IsValid(oauthTokenExchangePayload, _oauthTokenExchangeConfiguration);

        if (!Valid)
        {
            return; // UnauthorizedValidationParametersFailed(oauthTokenExchangePayload, Reason, Error);
        }

        // get well known endpoints and validate access token sent in the assertion
        var configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
            _oauthTokenExchangeConfiguration.AccessTokenMetadataAddress,
            new OpenIdConnectConfigurationRetriever());

        var wellKnownEndpoints = await configurationManager.GetConfigurationAsync();

        var accessTokenValidationResult = await ValidateOauthTokenExchangeRequestPayload.ValidateTokenAndSignature(
            subjectToken,
            _oauthTokenExchangeConfiguration,
            wellKnownEndpoints.SigningKeys);

        if (!accessTokenValidationResult.Valid)
        {
            return; // UnauthorizedValidationTokenAndSignatureFailed(oauthTokenExchangePayload, accessTokenValidationResult);
        }

        // get claims from Microsoft Entra ID token and re use in OpenIddict token
        var claimsIdentity = accessTokenValidationResult.ClaimsIdentity;
        if (claimsIdentity == null)
        {
            return;
        }

        var isDelegatedToken = ValidateOauthTokenExchangeRequestPayload
            .IsDelegatedAadAccessToken(claimsIdentity);

        if (!isDelegatedToken)
        {
            return; // UnauthorizedValidationRequireDelegatedTokenFailed();
        }

        var name = ValidateOauthTokenExchangeRequestPayload.GetPreferredUserName(claimsIdentity);

        var isNameAndEmail = ValidateOauthTokenExchangeRequestPayload.IsEmailValid(name);
        if (!isNameAndEmail)
        {
            return; // UnauthorizedValidationPrefferedUserNameFailed();
        }

        // validate user exists TODO
        //var user = await _userManager.FindByNameAsync(name);
        //if (user == null)
        //{
        //    return UnauthorizedValidationNoUserExistsFailed();
        //}

        /////////

        var sub = claimsIdentity.Claims!.First(c => c.Type == JwtClaimTypes.Subject).Value;

        var style = context.Request.Raw.Get("exchange_style");

        if (style == "impersonation")
        {
            // set token client_id to original id
            context.Request.ClientId = oauthTokenExchangePayload.audience!;

            context.Result = new GrantValidationResult(
                subject: sub,
                authenticationMethod: GrantType,
                customResponse: customResponse);
        }
        else if (style == "delegation")
        {
            // set token client_id to original id
            context.Request.ClientId = oauthTokenExchangePayload.audience!;

            var actor = new
            {
                client_id = context.Request.Client.ClientId
            };

            var actClaim = new Claim(JwtClaimTypes.Actor, JsonSerializer.Serialize(actor), 
                IdentityServerConstants.ClaimValueTypes.Json);

            context.Result = new GrantValidationResult(
                subject: sub,
                authenticationMethod: GrantType,
                claims: [actClaim],
                customResponse: customResponse);
        }
        else if (style == "custom")
        {
            context.Result = new GrantValidationResult(
                subject: sub,
                authenticationMethod: GrantType,
                customResponse: customResponse);
        }
    }

    public string GrantType => OidcConstants.GrantTypes.TokenExchange;

}