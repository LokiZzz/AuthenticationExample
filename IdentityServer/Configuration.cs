using IdentityModel;
using IdentityServer4.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdentityServer
{
    static public class Configuration
    {
        public static IEnumerable<IdentityResource> GetIdentityResources() =>
            new List<IdentityResource>
            {
                new IdentityResources.OpenId(),
                //new IdentityResources.Profile(),
                new IdentityResource
                {
                    Name = "rc.scope",
                    UserClaims =
                    {
                        "rc.grandma"
                    }
                }
            };

        public static IEnumerable<ApiResource> GetApis() =>
            new List<ApiResource> 
            { 
                new ApiResource("ApiOne", new string[] { "claim_for_api" }),
                new ApiResource("ApiTwo"),
            };

        public static IEnumerable<ApiScope> GetApiScopes() =>
           new List<ApiScope>
           {
                new ApiScope("ApiOne", new string[] { "claim_for_api" }),
                new ApiScope("ApiTwo"),
           };

        public static IEnumerable<Client> GetClients() =>
            new List<Client>
            {
                new Client
                {
                    ClientId = "client_id",
                    ClientSecrets = { new Secret("client_secret".ToSha256()) },

                    AllowedGrantTypes = GrantTypes.ClientCredentials,

                    AllowedScopes = { "ApiOne" }
                },
                new Client
                {
                    ClientId = "client_id_mvc",
                    ClientSecrets = { new Secret("client_secret_mvc".ToSha256()) },

                    AllowedGrantTypes = GrantTypes.Code,

                    RedirectUris = { "https://localhost:44398/signin-oidc" },
                    PostLogoutRedirectUris = { "https://localhost:44398/Home/Index" },

                    AllowedScopes = 
                    { 
                        "ApiOne", 
                        "ApiTwo", 
                        IdentityServer4.IdentityServerConstants.StandardScopes.OpenId,
                        //IdentityServer4.IdentityServerConstants.StandardScopes.Profile,
                        "rc.scope"
                    },
                    
                    //AlwaysIncludeUserClaimsInIdToken = true,

                    AllowOfflineAccess = true,

                    RequireConsent = false
                }
            };

       
    }
}
