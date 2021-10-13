using GamesBucket.DataAccess.Models;

namespace GamesBucket.App.Services.EmailService
{
    public class EmailMessage<T>
    {
        public AppUser ReceiverUser { get; set; }
        public string Subject { get; set; }
        public string TemplateFile { get; set; }
        public T TemplateModel { get; set; }
    }
}
