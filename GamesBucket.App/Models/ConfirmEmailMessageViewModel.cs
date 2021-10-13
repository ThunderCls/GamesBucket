namespace GamesBucket.App.Models
{
    public class ConfirmEmailMessageViewModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Url { get; set; }
        public string BaseUrl { get; set; }
        public string FullName()
        {
            return $"{FirstName} {LastName}";
        }
    }
}