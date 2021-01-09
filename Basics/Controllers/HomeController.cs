using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Basics.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [Authorize]
        public IActionResult Secret()
        {
            return View();
        }

        [Authorize(Policy = "Claim.DoB")]
        public IActionResult SecretPolicy()
        {
            return View("Secret");
        }

        [Authorize(Roles = "Admin")]
        public IActionResult SecretRole()
        {
            return View("Secret");
        }

        [AllowAnonymous]
        public async Task<IActionResult> Authenticate()
        {
            List<Claim> grandmaClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, "Bob"),
                new Claim(ClaimTypes.Email, "asdf@mail.ru"),
                new Claim("GrandmaSays", "Very nice boi!"),
                new Claim(ClaimTypes.Role, "Admin"),
                new Claim(ClaimTypes.DateOfBirth, "12-12-2020"),
            };

            var licenseClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, "Bob K Foo"),
                new Claim("DriverLicense", "A+")
            };

            var grandmaIdentity = new ClaimsIdentity(grandmaClaims, "Grandma Identity");
            var licenseIdentity = new ClaimsIdentity(licenseClaims, "Government");

            var userPrinciple = new ClaimsPrincipal(new[] { grandmaIdentity, licenseIdentity });

            await HttpContext.SignInAsync(userPrinciple);

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> DoStuff([FromServices] IAuthorizationService svcAuth)
        {
            //we are doing stuff

            AuthorizationPolicyBuilder builder = new AuthorizationPolicyBuilder("Schema");
            AuthorizationPolicy customPolicy = builder.RequireClaim("Hello").Build();

            AuthorizationResult result = await svcAuth.AuthorizeAsync(User, customPolicy);

            if(result.Succeeded)
            {
                return View("Index");
            }

            return View("Index");
        }
    }
}
