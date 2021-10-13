using System;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Reflection;
using System.Threading.Tasks;
using FluentEmail.Core;
using FluentEmail.Razor;
using FluentEmail.Smtp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GamesBucket.App.Services.EmailService
{
    public class EmailService : IEmailService
    {
        private readonly ILogger _logger;
        private string EmailServer { get; set; }
        private int EmailPort { get; set; }
        private string EmailUser { get; set; }
        private string EmailPassword { get; set; }

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _logger = logger;
            EmailServer = configuration.GetValue<string>("EmailConfig:server");
            EmailPort = configuration.GetValue<int>("EmailConfig:port");
            EmailUser = configuration.GetValue<string>("EmailConfig:username");
            EmailPassword = configuration.GetValue<string>("EmailConfig:password");
            //bool authentication = _configuration.GetValue<bool>("EmailConfig:authentication");
        }

        public async Task SendEmail<T>(EmailMessage<T> message)
        {
            var sender = new SmtpSender(() => new SmtpClient(EmailServer, EmailPort)
            {
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Credentials = new NetworkCredential(EmailUser, EmailPassword)
            });

            Email.DefaultSender = sender;
            Email.DefaultRenderer = new RazorRenderer();

            try
            {
                string templateResource = Assembly.GetExecutingAssembly().GetManifestResourceNames()
                    .Single(str => str.EndsWith(message.TemplateFile));
            
                var email = await Email
                    .From(EmailUser, "GamesBucket Team")
                    .To(message.ReceiverUser.Email, $"{message.ReceiverUser.FirstName} {message.ReceiverUser.LastName}")
                    .Subject(message.Subject)
                    .UsingTemplateFromEmbedded(templateResource, message.TemplateModel, this.GetType().GetTypeInfo().Assembly)
                    //.UsingTemplateFromFile(message.TemplateFile, message.TemplateModel)
                    .SendAsync();

                if (!email.Successful)
                {
                    _logger.LogError("Error sending email");

                    string errors = string.Empty;
                    foreach (var emailErrorMessage in email.ErrorMessages)
                    {
                        errors += emailErrorMessage + "\n";
                    }

                    _logger.LogError(errors);
                }
            }
            catch (Exception e)
            {
                _logger.LogError($"{e.Message}\n{e.StackTrace}");
            }
        }
    }
}
