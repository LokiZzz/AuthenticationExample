using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace MvcClient.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public HomeController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public IActionResult Index()
        {
            return View();
        }

        [Authorize]
        public async Task<IActionResult> Secret()
        {
            string accsessToken = await HttpContext.GetTokenAsync("access_token");
            string idToken = await HttpContext.GetTokenAsync("id_token");
            string refreshToken = await HttpContext.GetTokenAsync("refresh_token");

            List<Claim> claims = User.Claims.ToList();
            JwtSecurityToken _accessToken = new JwtSecurityTokenHandler().ReadJwtToken(accsessToken);
            JwtSecurityToken _idToken = new JwtSecurityTokenHandler().ReadJwtToken(idToken);

            var result = await GetSecret(accsessToken);

            await RefreshAccessToken();

            return View();
        }

        public async Task<IActionResult> Logout()
        {
            return SignOut("Cookie", "oidc");
        }

        public async Task<string> GetSecret(string accessToken)
        {
            HttpClient apiClient = _httpClientFactory.CreateClient();
            apiClient.SetBearerToken(accessToken);
            HttpResponseMessage response = await apiClient.GetAsync("https://localhost:44384/secret");
            string content = await response.Content.ReadAsStringAsync();

            return content;
        }

        private async Task RefreshAccessToken()
        {
            HttpClient serverClient = _httpClientFactory.CreateClient();
            DiscoveryDocumentResponse discoveryDocument =
                await serverClient.GetDiscoveryDocumentAsync("https://localhost:44385/");

            var accessToken = await HttpContext.GetTokenAsync("access_token");
            var idToken = await HttpContext.GetTokenAsync("id_token");
            var refreshToken = await HttpContext.GetTokenAsync("refresh_token");
            var refreshTokenClient = _httpClientFactory.CreateClient();

            TokenResponse tokenResponse = await refreshTokenClient.RequestRefreshTokenAsync(new RefreshTokenRequest 
            { 
                Address = discoveryDocument.TokenEndpoint,
                RefreshToken = refreshToken,
                ClientId = "client_id_mvc",
                ClientSecret = "client_secret_mvc",
            });

            AuthenticateResult authInfo = await HttpContext.AuthenticateAsync("Cookie");

            authInfo.Properties.UpdateTokenValue("access_token", tokenResponse.AccessToken);
            authInfo.Properties.UpdateTokenValue("id_token", tokenResponse.IdentityToken);
            authInfo.Properties.UpdateTokenValue("refresh_token", tokenResponse.RefreshToken);

            await HttpContext.SignInAsync("Cookie", authInfo.Principal, authInfo.Properties);

            bool accessTokenDifferent = !accessToken.Equals(tokenResponse.AccessToken);
            bool idTokenDifferent = !idToken.Equals(tokenResponse.IdentityToken);
            bool refreshTokenDifferent = !refreshToken.Equals(tokenResponse.RefreshToken);
        }
    }
}
