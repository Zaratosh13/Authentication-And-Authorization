using Microsoft.AspNetCore.Identity;

namespace Authentication_And_Authorization.Models.Domain
{
    public class ApplicationUser : IdentityUser
    {
        public string? Name { get; set; }
    }
}
