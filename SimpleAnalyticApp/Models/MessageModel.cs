namespace SimpleAnalyticApp.Models
{
    public class MessageModel<T>
    {
        public string MessageId { get; set; }
        public string Instruction { get; set; }
        public T Message { get; set; }
    }
}
