using InvalidateCookie.WebApplication.Data.DbContext;
using InvalidateCookie.WebApplication.Data.Identity;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InvalidateCookie.WebApplication.Extensions
{
    public static class ApplicationBuilderExtensions
    {
        public static void IntializeDatabase(this IApplicationBuilder app)
        {
            using (var scope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>(); //Service locator
                dbContext.Database.Migrate();
                var userManager = scope.ServiceProvider.GetService<UserManager<Users>>();
                if (!userManager.Users.Any())
                {
                    userManager.CreateAsync(new Users { UserName = "farhad", Email = "admin@gmail.com" }, "Aa_123456").GetAwaiter().GetResult();
                }
            }
        }
    }
}
