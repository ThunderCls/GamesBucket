using Microsoft.AspNetCore.Diagnostics;

namespace GamesBucket.App.Models
{
    public class ErrorViewModel
    {
        public string RequestId { get; set; }
        public IExceptionHandlerFeature ExceptionHandlerFeature { get; set; }
        
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}