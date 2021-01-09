using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Client.Conttrollers
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
            HttpResponseMessage serverResponse = await AccessTokenRefreshWrapper(
                () => SecuredGetRequest("https://localhost:44364/secret/index")
            );

            HttpResponseMessage apiResponse = await AccessTokenRefreshWrapper(
                () => SecuredGetRequest("https://localhost:44384/secret/index")
            );

            return View();
        }

        private async Task<HttpResponseMessage> SecuredGetRequest(string url)
        {
            string token = await HttpContext.GetTokenAsync("access_token");
            HttpClient client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
            return await client.GetAsync(url);
        }

        public async Task<HttpResponseMessage> AccessTokenRefreshWrapper(Func<Task<HttpResponseMessage>> initialRequest)
        {
            HttpResponseMessage response = await initialRequest();
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                await RefreshAccessToken();
                response = await initialRequest();
            }

            return response;
        }

        private async Task RefreshAccessToken()
        {
            string refreshToken = await HttpContext.GetTokenAsync("refresh_token");

            HttpClient refreshTokenClient = _httpClientFactory.CreateClient();
            Dictionary<string, string> requestData = new Dictionary<string, string>()
            {
                ["grant_type"] = "refresh_token",
                ["refresh_token"] = refreshToken,
            };
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, @"https://localhost:44364/oauth/token")
            {
                Content = new FormUrlEncodedContent(requestData),
            };
            string basicCredential = "username:password";
            byte[] encodedCredentials = Encoding.UTF8.GetBytes(basicCredential);
            string base64Credential = Convert.ToBase64String(encodedCredentials);

            request.Headers.Add("Authorization", $"Basic {base64Credential}");

            HttpResponseMessage response = await refreshTokenClient.SendAsync(request);

            string responseString = await response.Content.ReadAsStringAsync();
            Dictionary<string, string> responseData = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseString);

            string newAccessToken = responseData.GetValueOrDefault("access_token");
            string newRefreshToken = responseData.GetValueOrDefault("refresh_token");

            AuthenticateResult authInfo = await HttpContext.AuthenticateAsync("ClientCookie");
            authInfo.Properties.UpdateTokenValue("access_token", newAccessToken);
            authInfo.Properties.UpdateTokenValue("refresh_token", newRefreshToken);

            await HttpContext.SignInAsync("ClientCookie", authInfo.Principal, authInfo.Properties);
        }
    }
}
