using InvalidateCookie.WebApplication.Data.DbContext;
using InvalidateCookie.WebApplication.Data.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace InvalidateCookie.WebApplication.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddCustomAuthentication(this IServiceCollection services)
        {
            services.AddAuthorization();
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            }).AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
             {
                 options.AccessDeniedPath = "/Auth/SignIn";
                 options.Cookie.HttpOnly = true;
                 options.ExpireTimeSpan = TimeSpan.FromDays(15);
                 options.LoginPath = "/Auth/SignIn";
                 options.ReturnUrlParameter = "returnUrl";
                 options.SlidingExpiration = true;
                 options.Cookie.IsEssential = true;// ignore GDPR
                 options.Cookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.Always;
                 options.Events = new CookieAuthenticationEvents
                 {
                     OnValidatePrincipal = ValidateAsync
                 };
             });
            services.Configure<SecurityStampValidatorOptions>(options =>
            {
                options.ValidationInterval = TimeSpan.Zero;
            });
            services.AddIdentity<Users, Roles>(identityOptions =>
            {
                #region Password
                //Password Settings       
                //identityOptions.Password.RequireDigit = settings.PasswordRequireDigit;
                //identityOptions.Password.RequiredLength = settings.PasswordRequiredLength;
                //identityOptions.Password.RequireNonAlphanumeric = settings.PasswordRequireNonAlphanumic; //#@!
                //identityOptions.Password.RequireUppercase = settings.PasswordRequireUppercase;
                //identityOptions.Password.RequireLowercase = settings.PasswordRequireLowercase;
                #endregion
                #region Username
                //identityOptions.User.RequireUniqueEmail = settings.RequireUniqueEmail;
                #endregion
                #region SignIn
                ////Singin Settings
                //identityOptions.SignIn.RequireConfirmedEmail = false;
                ////identityOptions.SignIn.RequireConfirmedPhoneNumber = false;
                #endregion
                #region LockOut
                ////Lockout Settings
                //identityOptions.Lockout.MaxFailedAccessAttempts = 5;
                //identityOptions.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromDays(1);
                //identityOptions.Lockout.AllowedForNewUsers = false;
                #endregion
            })
            .AddDefaultTokenProviders()
            .AddEntityFrameworkStores<ApplicationDbContext>();

        }
        private static async Task ValidateAsync(CookieValidatePrincipalContext context)
        {
            context = context ?? throw new ArgumentNullException(nameof(context));
            var claimsIdentity = context.Principal.Identity as ClaimsIdentity;
            if (claimsIdentity?.Claims == null || !claimsIdentity.Claims.Any())
            {
                await RejectPrincipal();
                return;
            }
            UserManager<Users> userManager = context.HttpContext.RequestServices.GetRequiredService<UserManager<Users>>();
            var user = await userManager.FindByNameAsync(context.Principal.FindFirstValue(ClaimTypes.NameIdentifier));
            if (user == null || user.SecurityStamp != context.Principal.FindFirst(new ClaimsIdentityOptions().SecurityStampClaimType)?.Value)
            {
                await RejectPrincipal();
                return;
            }
            async Task RejectPrincipal()
            {
                context.RejectPrincipal();
                await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            }
        }
    }
}
