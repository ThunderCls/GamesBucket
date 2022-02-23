using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using GamesBucket.App.Models;
using GamesBucket.App.Services.Account;
using GamesBucket.DataAccess;
using GamesBucket.DataAccess.Models;
using GamesBucket.DataAccess.Services.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GamesBucket.App.Controllers
{
    public class AccountController : Controller
    {
        private readonly IMapper _mapper;
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IUserService _userService;
        private readonly IAccountService _accountService;

        public AccountController(IMapper mapper, UserManager<AppUser> userManager, 
            SignInManager<AppUser> signInManager, IUserService userService,
            IAccountService accountService)
        {
            _mapper = mapper;
            _userManager = userManager;
            _signInManager = signInManager;
            _userService = userService;
            _accountService = accountService;
        }
        
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }
        
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Register(UserRegisterViewModel userRegistration)
        {
            if (ModelState.IsValid)
            {
                var userIdentity = _mapper.Map<AppUser>(userRegistration);
                userIdentity.UserName = userRegistration.Email;
                var result = await _userManager.CreateAsync(userIdentity, userRegistration.Password);
                if (result.Succeeded)
                {
                    await _accountService.RequestConfirmationEmail(userIdentity, HttpContext);
                    var confirmEmailViewModel = new MessageViewModel
                    {
                        Result = MessageViewModel.ResultType.Info,
                        Title = "Confirmation required",
                        Content = "Your email needs to be confirmed before you can log into your account<br/>" +
                                  $"A message has been sent to: <u>{userIdentity.Email}</u><br/><br/>Please check your email for further instructions"
                    };
                    return View("Message", confirmEmailViewModel);
                }
                
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(userRegistration);
        }
        
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(UserLoginViewModel userLoginViewModel, [FromServices] AppDbContext dbContext)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(userLoginViewModel.Email);
                if (user != null)
                {
                    if (await _userManager.CheckPasswordAsync(user, userLoginViewModel.Password) &&
                        !user.EmailConfirmed)
                    {
                        ModelState.AddModelError(string.Empty, 
                            "The email for this account has not been confirmed yet");
                        return View(userLoginViewModel);
                    }
                    
                    var result = await _signInManager.PasswordSignInAsync(userLoginViewModel.Email, 
                        userLoginViewModel.Password, userLoginViewModel.Remember, false);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("Index", "Home");
                    }
                }

                ModelState.AddModelError(string.Empty, "Incorrect login attempt");
            }
            
            return View(userLoginViewModel);
        }
        
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return Ok();
        }
        
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Forgot()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Forgot(string email)
        {
            if (!string.IsNullOrEmpty(email))
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user != null)
                {
                    await _accountService.RequestPasswordReset(user, HttpContext);
                }
            }

            var confirmEmailView = new MessageViewModel
            {
                Result = MessageViewModel.ResultType.Info,
                Title = "Information sent",
                Content = "Please check your email and follow the steps to reset your password"
            };
            return View("Message", confirmEmailView);
        }

        [AllowAnonymous]
        public IActionResult Activate()
        {
            return View();
        }
        
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Activate(string email)
        {
            if (!string.IsNullOrEmpty(email))
            {
                await _accountService.ResendConfirmationEmail(email, HttpContext);
            }

            var confirmEmailView = new MessageViewModel
            {
                Result = MessageViewModel.ResultType.Info,
                Title = "Information sent",
                Content = "Please check your email and follow the steps to activate your account"
            };
            return View("Message", confirmEmailView);
        }
        
        [HttpPost]
        public async Task<IActionResult> Delete()
        {
            var loggedUser = await _userService.GetLoggedUser(User);
            if (loggedUser == null) return RedirectToAction("Error", "Home");
            
            var result = await _userManager.DeleteAsync(loggedUser);
            if (result.Succeeded)
            {
                await _signInManager.SignOutAsync();
                return Ok();
            }
            
            return BadRequest("error deleting account");
        }
        
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            var confirmEmailViewModel = new MessageViewModel();

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
            {
                confirmEmailViewModel.Result = MessageViewModel.ResultType.Error;
                confirmEmailViewModel.Title = "Invalid Request";
                confirmEmailViewModel.Content = "Your email could not be confirmed. Your request was invalid";
                return View("Message", confirmEmailViewModel);
            }
            
            var user = await _userManager.FindByIdAsync(userId);
            var confirmResult = await _userManager.ConfirmEmailAsync(user, token);
            if (confirmResult.Succeeded)
            {
                confirmEmailViewModel.Result = MessageViewModel.ResultType.Success;
                confirmEmailViewModel.Title = "Email Confirmed";
                confirmEmailViewModel.Content = "Your email is confirmed. You can log into your account";
                return View("Message", confirmEmailViewModel);
            }
            
            StringBuilder message = new StringBuilder();
            foreach (var identityError in confirmResult.Errors)
            {
                message.AppendLine(identityError.Description);
            }

            confirmEmailViewModel.Result = MessageViewModel.ResultType.Error;
            confirmEmailViewModel.Title = "Invalid Request";
            confirmEmailViewModel.Content = message.ToString();
            return View("Message", confirmEmailViewModel);
        }
        
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword(string userId, string token)
        {
            if (!string.IsNullOrEmpty(userId) && !string.IsNullOrEmpty(token))
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user != null)
                {
                    var resetPasswordModel = new ResetPasswordViewModel()
                    {
                        UserId = userId, Token = token
                    };
                    return View("ChangePassword", resetPasswordModel);
                }
            }

            var confirmEmailViewModel = new MessageViewModel
            {
                Result = MessageViewModel.ResultType.Error,
                Title = "Invalid Request",
                Content = "Your password could not be reset. Your request was invalid"
            };
            
            return View("Message", confirmEmailViewModel);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel resetPasswordViewModel)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByIdAsync(resetPasswordViewModel.UserId);
                if (user != null)
                {
                    var result = await _userManager.ResetPasswordAsync(user,
                        resetPasswordViewModel.Token, resetPasswordViewModel.NewPassword);
                    if (result.Succeeded)
                    {
                        var messageModel = new MessageViewModel()
                        {
                            Result = MessageViewModel.ResultType.Success,
                            Title = "Success",
                            Content = "Your password has been successfully reset. You can log into your account"
                        };

                        return View("Message", messageModel);
                    }
                    
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }

            return View("ChangePassword", resetPasswordViewModel);
        }
    }
}