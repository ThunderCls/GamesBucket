using System;
using System.Threading.Tasks;
using GamesBucket.App.Models;
using GamesBucket.App.Services.EmailService;
using GamesBucket.DataAccess.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;

namespace GamesBucket.App.Services.Account
{
    public class AccountService : ControllerBase, IAccountService
    {
        private readonly IEmailService _emailService;
        private readonly UserManager<AppUser> _userManager;
        private readonly IUrlHelperFactory _urlHelperFactory;

        public AccountService(IEmailService emailService, UserManager<AppUser> userManager, 
            IUrlHelperFactory urlHelperFactory)
        {
            _emailService = emailService;
            _userManager = userManager;
            _urlHelperFactory = urlHelperFactory;
        }

        public async Task RequestPasswordReset(AppUser userIdentity, HttpContext httpContext)
        {
            var urlHelper = _urlHelperFactory
                .GetUrlHelper(new ActionContext(httpContext, httpContext.GetRouteData(), new ActionDescriptor()));
            var token = await _userManager.GeneratePasswordResetTokenAsync(userIdentity);
            string url = urlHelper.Action("ResetPassword", "Account", 
                new {userId = userIdentity.Id, token = token}, httpContext.Request.Scheme);

            await SendResetPasswordEmail(userIdentity, url);
        }

        public async Task ResendConfirmationEmail(string email, HttpContext httpContext)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user != null)
            {
                await RequestConfirmationEmail(user, httpContext);
            }
        }
        
        public async Task RequestConfirmationEmail(AppUser userIdentity, HttpContext httpContext)
        {
            var urlHelper = _urlHelperFactory
                .GetUrlHelper(new ActionContext(httpContext, httpContext.GetRouteData(), new ActionDescriptor()));
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(userIdentity);
            string url = urlHelper.Action("ConfirmEmail", "Account", 
                new {userId = userIdentity.Id, token = token}, httpContext.Request.Scheme);

            await SendConfirmationEmail(userIdentity, url);
        }

        private async Task SendConfirmationEmail(AppUser userIdentity, string url)
        {
            var message = new EmailMessage<ConfirmEmailMessageViewModel>
            {
                ReceiverUser = userIdentity,
                Subject = "Account Activation!",
                //TemplateFile = $"{Directory.GetCurrentDirectory()}/Views/Templates/_ConfirmationEmailTemplate.cshtml",
                TemplateFile = "_ConfirmationEmailTemplate.cshtml",
                TemplateModel = new ConfirmEmailMessageViewModel
                {
                    FirstName = userIdentity.FirstName,
                    LastName = userIdentity.LastName,
                    Url = url,
                    BaseUrl = new Uri(url).GetLeftPart(UriPartial.Authority)
                }
            };

            await _emailService.SendEmail(message);
        }
        
        private async Task SendResetPasswordEmail(AppUser user, string url)
        {
            var message = new EmailMessage<ResetPasswordMessageViewModel>
            {
                ReceiverUser = user,
                Subject = "Change Password!",
                //TemplateFile = $"{Directory.GetCurrentDirectory()}/Views/Templates/_ResetPasswordEmailTemplate.cshtml",
                TemplateFile = "_ResetPasswordEmailTemplate.cshtml",
                TemplateModel = new ResetPasswordMessageViewModel
                {
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Url = url,
                    BaseUrl = new Uri(url).GetLeftPart(UriPartial.Authority)
                }
            };

            await _emailService.SendEmail(message);
        }
    }
}