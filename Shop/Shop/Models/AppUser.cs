using Microsoft.AspNetCore.Identity;

namespace Shop.Models
{
    public class AppUser : IdentityUser
    {
        public string FullName{ get; set; }
    }
}
