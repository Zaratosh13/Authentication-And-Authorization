using Authentication_And_Authorization.Models.Domain;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Authentication_And_Authorization.Data
{
    public class AuThDbContext : IdentityDbContext<ApplicationUser>
    {
        public AuThDbContext(DbContextOptions<AuThDbContext> options) : base(options)
        {

        }

        public DbSet<TokenInfo> TokenInfo { get; set; }
    }
}
