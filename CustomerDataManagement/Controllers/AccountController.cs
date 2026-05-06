using CustomerDataManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MimeKit;
using System.Text;
using MailKit.Net.Smtp;
namespace CustomerDataManagement.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> userManager;
        private readonly SignInManager<IdentityUser> signInManager;
        public AccountController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
        }
        #region Register
        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Register(UserViewModel userModel)
        {
            if (ModelState.IsValid)
            {
                IdentityUser identityUser = new IdentityUser
                {
                    UserName = userModel.Name,
                    Email = userModel.Email,
                    PhoneNumber = userModel.Mobile
                };
                var result = await userManager.CreateAsync(identityUser, userModel.Password);
                
                if (result.Succeeded)
                {
                    await signInManager.SignInAsync(identityUser, false);
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    foreach (var Error in result.Errors)
                    {
                        ModelState.AddModelError("", Error.Description);
                    }
                }

            }
            return View(userModel);
        }
        #endregion
        #region Login and Logout
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel loginModel)
        {
            if (ModelState.IsValid)
            {
                var user = await userManager.FindByNameAsync(loginModel.Name);
                if (user != null && (await userManager.CheckPasswordAsync(user, loginModel.Password)) && user.EmailConfirmed == false)
                {
                    ModelState.AddModelError("", "Your email not confirmed.");
                    return View(loginModel);
                }
                var result = await signInManager.PasswordSignInAsync(loginModel.Name, loginModel.Password,
                loginModel.RememberMe, false);
                if (result.Succeeded)
                {
                    if (string.IsNullOrEmpty(loginModel.ReturnUrl))
                        return RedirectToAction("Index", "Home");
                    else
                        return LocalRedirect(loginModel.ReturnUrl);
                }
                else
                {
                    ModelState.AddModelError("", "Invalid login credentials.");
                }
            }
            return View(loginModel);
        }
        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }
        #endregion
        
        #region Confirm Email
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (userId != null && token != null)
            {
                var User = await userManager.FindByNameAsync(userId);
                if (User != null)
                {
                    var result = await userManager.ConfirmEmailAsync(User, token);
                    if (result.Succeeded)
                    {
                        TempData["Title"] = "Email Confirmation Success.";
                        TempData["Message"] = "Email confirmation is completed.You can now login into the application.";
                        return View("DisplayMessage");
                    }
                    else
                    {
                        StringBuilder Errors=new StringBuilder();
                        foreach(var Error in result.Errors)
                        {
                            Errors.Append(Error.Description);
                        }
                        TempData["Title"] = "Confirmation Email Failure";
                        TempData["Message"] = Errors.ToString();
                        return View("DisplayMessages");
                    }
                }
                else
                {
                    TempData["Title"] = "Invalid User Id.";
                    TempData["Message"] = "User Id which is present in confirm email link is in-valid.";
                    return View("DisplayMessages");
                }
            }
            else
            {
                TempData["Title"] = "Invalid Email Confirmation Link.";
                TempData["Message"] = "Email confirmation link is invalid, either missing the User Id or Confirmation Token.";
                return View("DisplayMessages");
            
             }
        

        }
        #endregion
        public IActionResult ForgotPassword()
        {
            return View();
        }
    }
}
