namespace CielaDocs.Shared.Services
{
    public class SendGridMailerSettings
    {
        public string SendGridApiKey { get; set; }
        public string OutgoingEmail { get; set; }
        public string FeedbackEmail { get; set; }
    }
}
