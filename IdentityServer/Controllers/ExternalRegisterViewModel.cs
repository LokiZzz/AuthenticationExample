using System.ComponentModel.DataAnnotations;

namespace IdentityServer.Controllers
{
    public class ExternalRegisterViewModel
    {
        [Required]
        public string Username { get; set; }
        public string ReturnUrl { get; set; }
    }
}