using InvalidateCookie.WebApplication.Data.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace InvalidateCookie.WebApplication.Data.DbContext
{
    public class ApplicationDbContext : IdentityDbContext<Users, Roles, int>
    {

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
    }
}
