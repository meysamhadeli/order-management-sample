using Duende.IdentityServer;
using Duende.IdentityServer.Models;
using IdentityModel;
using OrderManagement.Identities.Constants;

namespace IdentityServer;

public static class Config
{
    public static IEnumerable<IdentityResource> IdentityResources =>
        new List<IdentityResource>
        {
            new IdentityResources.OpenId(),
            new IdentityResources.Profile(),
            new IdentityResources.Email()
        };

    public static IEnumerable<ApiScope> ApiScopes =>
        new List<ApiScope>
        {
            new(IdentityConstant.StandardScopes.OrderManagementApi),
            new(JwtClaimTypes.Role, new List<string> {"role"})
        };

    public static IList<ApiResource> ApiResources =>
        new List<ApiResource>
        {
            new(IdentityConstant.StandardScopes.OrderManagementApi)
        };

    public static IEnumerable<Client> Clients =>
        new List<Client>
        {
            // Password flow client (replaces client credentials for user access)
            new()
            {
                ClientId = "client",
                AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
                ClientSecrets =
                {
                    new Secret("secret".Sha256())
                },
                AllowedScopes =
                {
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    IdentityConstant.StandardScopes.OrderManagementApi,
                    JwtClaimTypes.Role // Include roles scope
                },
                AccessTokenLifetime = 3600, // authorize the client to access protected resources
                IdentityTokenLifetime = 3600, // authenticate the user
                AlwaysIncludeUserClaimsInIdToken = true // Include claims in ID token
            },

            // interactive client using code flow + pkce
            new Client
            {
                ClientId = "interactive",
                ClientSecrets =
                {
                    new Secret("secret".Sha256())
                },
                AllowedGrantTypes = GrantTypes.Code,
                RequirePkce = true, // Enforce PKCE for better security
                RequireClientSecret = true,

                RedirectUris = { "https://localhost:4200/signin-oidc" },
                FrontChannelLogoutUri = "https://localhost:4200/signout-oidc",
                PostLogoutRedirectUris = { "https://localhost:4200/signout-callback-oidc" },
                AllowOfflineAccess = true,
                AllowedScopes =
                {
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    IdentityConstant.StandardScopes.OrderManagementApi,
                    JwtClaimTypes.Role // Include roles scope
                },
                // Recommended settings for SPA/public clients
                AccessTokenLifetime = 3600,
                IdentityTokenLifetime = 3600,
                RefreshTokenExpiration = TokenExpiration.Sliding,
                SlidingRefreshTokenLifetime = 86400,
                UpdateAccessTokenClaimsOnRefresh = true
            }
        };
}
