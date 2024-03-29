﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Proge.Teams.Edu.Abstraction;

namespace Proge.Teams.Edu.TeamsDashboard
{
    public interface IAzureADJwtBearerValidation
    {
        string GetPreferredUserName();
        Task<ClaimsPrincipal> ValidateTokenAsync(string authorizationHeader, CancellationToken cancellationToken = default);
    }

    public class AzureADJwtBearerValidation : IAzureADJwtBearerValidation
    {
        private readonly AuthenticationConfig _authenticationConfig;
        private readonly UniSettings uniSettings;
        private readonly ILogger<AzureADJwtBearerValidation> _logger;
        private const string scopeType = @"appid";
        private ConfigurationManager<OpenIdConnectConfiguration> _configurationManager;
        private ClaimsPrincipal _claimsPrincipal;

        private readonly string _wellKnownEndpoint = string.Empty;
        private readonly string _tenantId = string.Empty;
        private readonly string _tenant = string.Empty;
        private readonly List<string> _audience = null;
        private readonly List<string> _issuer = null;
        private readonly string _instance = string.Empty;
        private readonly string _requiredScope = string.Empty;

        public AzureADJwtBearerValidation(IOptions<AuthenticationConfig> authCfg, IOptions<UniSettings> uniCfg, ILogger<AzureADJwtBearerValidation> logger)
        {
            _logger = logger;
            _authenticationConfig = authCfg.Value;
            uniSettings = uniCfg.Value;
            _tenantId = _authenticationConfig.TenantId;
            _audience = new List<string>() { uniSettings.AppAudience, uniSettings.AppClientId };
            _instance = _authenticationConfig.Instance;
            _tenant = uniSettings.AppTenant;
            _requiredScope = uniSettings.AppClientId;
            _wellKnownEndpoint = $"{String.Format(CultureInfo.InvariantCulture, _instance, _tenantId)}/v2.0/.well-known/openid-configuration";
            _issuer = new List<string>()
                    {
                        $"https://login.microsoftonline.com/{_tenant}/",
                        $"https://login.microsoftonline.com/{_tenant}/v2.0",
                        $"https://login.windows.net/{_tenant}/",
                        $"https://login.microsoft.com/{_tenant}/",
                        $"https://sts.windows.net/{_tenantId}/"
                    };
        }

        public async Task<ClaimsPrincipal> ValidateTokenAsync(string authorizationHeader, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(authorizationHeader))
            {
                return null;
            }

            if (!authorizationHeader.Contains("Bearer"))
            {
                return null;
            }

            var accessToken = authorizationHeader.Substring("Bearer ".Length);

            var oidcWellknownEndpoints = await GetOIDCWellknownConfiguration(cancellationToken);

            var tokenValidator = new JwtSecurityTokenHandler();

            var validationParameters = new TokenValidationParameters
            {
                RequireSignedTokens = true,
                ValidAudiences = _audience,
                ValidateAudience = true,
                ValidateIssuer = true,
                ValidateIssuerSigningKey = true,
                ValidateLifetime = true,
                IssuerSigningKeys = oidcWellknownEndpoints.SigningKeys,
                ValidIssuers = _issuer
            };

            try
            {
                _claimsPrincipal = tokenValidator.ValidateToken(accessToken, validationParameters, out SecurityToken securityToken);

                if (IsScopeValid(_requiredScope))
                {
                    return _claimsPrincipal;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating token");
                if (ex is SecurityTokenExpiredException)
                    throw;
            }

            return null;
        }

        public string GetPreferredUserName()
        {
            string preferredUsername = string.Empty;
            var preferred_username = _claimsPrincipal.Claims.FirstOrDefault(t => t.Type == "preferred_username");
            if (preferred_username != null)
            {
                preferredUsername = preferred_username.Value;
            }

            return preferredUsername;
        }

        private async Task<OpenIdConnectConfiguration> GetOIDCWellknownConfiguration(CancellationToken cancellationToken)
        {
            _logger.LogDebug($"Get OIDC well known endpoints {_wellKnownEndpoint}");
            _configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
                 _wellKnownEndpoint, new OpenIdConnectConfigurationRetriever());

            return await _configurationManager.GetConfigurationAsync(cancellationToken);
        }

        private bool IsScopeValid(string scopeName)
        {
            if (_claimsPrincipal == null)
            {
                _logger.LogWarning($"Scope invalid {scopeName}");
                return false;
            }

            var scopeClaim = _claimsPrincipal.HasClaim(x => x.Type == scopeType)
                ? _claimsPrincipal.Claims.First(x => x.Type == scopeType).Value
                : string.Empty;

            if (string.IsNullOrEmpty(scopeClaim))
            {
                _logger.LogWarning($"Scope invalid {scopeName}");
                return false;
            }

            if (!scopeClaim.Equals(scopeName, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning($"Scope invalid {scopeName}");
                return false;
            }

            _logger.LogDebug($"Scope valid {scopeName}");
            return true;
        }
    }
}
