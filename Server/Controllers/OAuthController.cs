using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Server.Controllers
{
    public class OAuthController : Controller
    {
        [HttpGet]
        public IActionResult Authorize(
            string response_type, // authorization flow type
            string client_id,
            string redirect_uri, 
            string scope,
            string state)
        {
            QueryBuilder query = new QueryBuilder();
            query.Add("redirectUri", redirect_uri);
            query.Add("state", state);
            
            return View(model: query.ToString());
        }

        [HttpPost]
        public IActionResult Authorize(
            string username,
            string redirectUri,
            string state)
        {
            const string code = "opan'ki";

            QueryBuilder query = new QueryBuilder();
            query.Add("code", code);
            query.Add("state", state);

            return Redirect($"{redirectUri}{query}");
        }

        public async Task<IActionResult> Token(
            string grant_type,
            string code,
            string redirect_uri,
            string client_id,
            string refresh_token)
        {
            Claim[] claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, "some_id"),
                new Claim("granny", "cookie")

            };

            byte[] secretBytes = Encoding.UTF8.GetBytes(Constants.Secret);
            SymmetricSecurityKey key = new SymmetricSecurityKey(secretBytes);
            SigningCredentials signInCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            JwtSecurityToken token = new JwtSecurityToken(
                Constants.Issuer,
                Constants.Audiance,
                claims,
                notBefore: DateTime.Now,
                expires: grant_type == "refresh_token"
                    ? DateTime.Now.AddMinutes(5)
                    : DateTime.Now.AddMilliseconds(1),
                signInCredentials
            );

            string access_token = new JwtSecurityTokenHandler().WriteToken(token);
            
            string json = JsonConvert.SerializeObject(new
            {
                access_token,
                token_type = "Bearer",
                raw_claim = "oauthTest",
                refresh_token = "refresh_token_value"
            });
            byte[] responseBytes = Encoding.UTF8.GetBytes(json);

            await Response.Body.WriteAsync(responseBytes, 0, responseBytes.Length);


            return Redirect(redirect_uri);
        }

        [Authorize]
        public IActionResult Validate()
        {
            if (HttpContext.Request.Query.TryGetValue("access_token", out var accessToken))
            {
                return Ok();
            }

            return BadRequest();
        }
    }
}
