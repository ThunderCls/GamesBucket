namespace GamesBucket.App.Models
{
    public class MessageViewModel
    {
        public enum ResultType
        {
            None,
            Info,
            Success,
            Error
        };
        
        public string Title { get; set; }
        public string Content { get; set; }
        public ResultType Result { get; set; } = ResultType.None;
    }
}