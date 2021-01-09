using IdentityExample.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NETCore.MailKit.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace IdentityExample.Controllers
{
    public class IdentityExampleController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IEmailService _emailService;

        public IdentityExampleController(UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            IEmailService emailService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailService = emailService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [Authorize]
        public IActionResult Secret()
        {
            return View();
        }

        public async Task<IActionResult> Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            var user = await _userManager.FindByNameAsync(username);

            if(user != null)
            {
                await _signInManager.PasswordSignInAsync(user, password, false, false);
            }

            return RedirectToAction("Index");
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(string username, string password)
        {
            IdentityUser user = new IdentityUser
            {
                UserName = username,
                Email = "email@email",
            };

            IdentityResult result = await _userManager.CreateAsync(user, password);

            if(result.Succeeded)
            {
                //generation of email token
                string code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                string link = Url.Action(nameof(VerifyEmail), 
                    "Home", 
                    new { userId = user.Id, code }, 
                    Request.Scheme, 
                    Request.Host.ToString());

                try
                {
                    await _emailService.SendAsync("test@test.com", "email verify", $"<a href=\"{link}\">Verify email</a>", true);
                }
                catch(Exception ex)
                {

                }

                return RedirectToAction("EmailVerification");
            }

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> VerifyEmail(string userId, string code)
        {
            IdentityUser user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return BadRequest();
            }

            IdentityResult result = await _userManager.ConfirmEmailAsync(user, code);

            if(result.Succeeded)
            {
                return View();
            }

            return BadRequest();
        }

        public IActionResult EmailVerification() => View();

        public async Task<IActionResult> LogOut()
        {
            await _signInManager.SignOutAsync();

            return RedirectToAction("Index");
        }
    }
}
