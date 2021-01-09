using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Api.AuthRequirement
{
    public class JwtRequirement : IAuthorizationRequirement
    {

    }

    public class JwtRequirementHandler : AuthorizationHandler<JwtRequirement>
    {
        private HttpClient _http;
        private HttpContext _httpContext;

        public JwtRequirementHandler(
            IHttpClientFactory httpFactory,
            IHttpContextAccessor httpContextAccessor)
        {
            _http = httpFactory.CreateClient();
            _httpContext = httpContextAccessor.HttpContext;
        }

        protected async override Task HandleRequirementAsync(
            AuthorizationHandlerContext context, 
            JwtRequirement requirement)
        {
            if(_httpContext.Request.Headers.TryGetValue("Authorization", out var authHeader))
            {
                var accessToken = authHeader.ToString().Split(' ')[1];

                HttpResponseMessage serverResponse = await _http.GetAsync(
                    $"https://localhost:44364/oauth/validate?access_token={accessToken}"
                );

                if(serverResponse.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    context.Succeed(requirement);
                }
            }
        }
    }
}
