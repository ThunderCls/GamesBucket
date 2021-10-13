using System.Threading.Tasks;

namespace GamesBucket.App.Services.EmailService
{
    public interface IEmailService
    {
        Task SendEmail<T>(EmailMessage<T> message);
    }
}
