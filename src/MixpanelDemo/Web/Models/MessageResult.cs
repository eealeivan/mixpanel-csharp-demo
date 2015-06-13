namespace Web.Models
{
    public class MessageResult
    {
        public string SentJson { get; set; }
        public string Error { get; set; }
        public bool? MixpanelResponse { get; set; }
    }
}