using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using InvalidateCookie.WebApplication.Data.Identity;
using InvalidateCookie.WebApplication.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace InvalidateCookie.WebApplication.Controllers
{
    [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
    public class AuthController : Controller
    {
        private readonly UserManager<Users> _userManager;
        private readonly SignInManager<Users> _signInManager;

        public AuthController(UserManager<Users> userManager, SignInManager<Users> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }
        [AllowAnonymous]

        public IActionResult SignIn()
        {
            return View();
        }
        [AllowAnonymous]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> SignIn(SignInViewModel model, string returnUrl = null)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByNameAsync(model.Username);
                if (user != null)
                {
                    var result = await _signInManager.PasswordSignInAsync(user, model.Password, true, true);
                    if (result.Succeeded)
                    {
                        returnUrl = Url.IsLocalUrl(returnUrl) ? returnUrl : "/Home";
                        var cliams = new List<Claim>
                        {
                            new Claim(ClaimTypes.NameIdentifier, model.Username),
                            new Claim(ClaimTypes.Name, model.Username),
                            new Claim(new ClaimsIdentityOptions().SecurityStampClaimType, user.SecurityStamp)
                        };
                        var claimsIdentity = new ClaimsIdentity(cliams, CookieAuthenticationDefaults.AuthenticationScheme);
                        AuthenticationProperties authenticationProperties = new AuthenticationProperties()
                        {
                            IsPersistent = true,
                            RedirectUri = returnUrl,
                            ExpiresUtc = DateTime.Now.AddDays(15)
                        };
                        await _signInManager.SignInAsync(user, authenticationProperties);   
                        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authenticationProperties);
                        return Redirect(returnUrl);
                    }
                    else
                    {
                        ModelState.AddModelError(nameof(model.Username), "Username or password is incorrect");
                        return View(model);
                    }
                }
                ModelState.AddModelError(nameof(model.Username), "Username or password is incorrect");
                return View(model);
            }
            return View(model);
        }
        public IActionResult ChangePassword()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> SignOut()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Redirect(nameof(SignIn));
        }
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            ViewBag.IsSuccess = false;
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await _userManager.FindByNameAsync(User.Identity.Name);
            var checkPassword = await _userManager.CheckPasswordAsync(user, model.CurrentPassword);
            if (checkPassword)
            {
                var resutl = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.Password);
                if (resutl.Succeeded)
                {
                    await _userManager.UpdateSecurityStampAsync(user);
                    ViewBag.IsSuccess = true;
                    return View();
                }
                else
                {
                    return View(model);
                }
            }
            ModelState.AddModelError(nameof(model.CurrentPassword), "کلمه عبور فعلی اشتباه است");
            return View(model);
        }        
    }
}
