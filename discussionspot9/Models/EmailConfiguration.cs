namespace discussionspot9.Models
{
    public class EmailConfiguration
    {
        public string SmtpHost { get; set; } = "smtp.gmail.com";
        public int SmtpPort { get; set; } = 587;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FromEmail { get; set; } = "no-reply@discussionspot.com";
        public string FromName { get; set; } = "DiscussionSpot";
        public bool EnableSsl { get; set; } = true;
        public bool EnableEmails { get; set; } = true;
        public bool TestMode { get; set; } = false;
        public string AdminEmail { get; set; } = string.Empty;
        public bool SendWelcomeEmail { get; set; } = true;
        public bool SendDigestEmails { get; set; } = true;
        public string DigestTime { get; set; } = "09:00";
    }
}

