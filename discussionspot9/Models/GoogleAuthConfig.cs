namespace discussionspot9.Models
{
    public class GoogleAuthConfig
    {
        public GoogleAuthWeb? Web { get; set; }
    }

    public class GoogleAuthWeb
    {
        public string? ClientId { get; set; }
        public string? ProjectId { get; set; }
        public string? AuthUri { get; set; }
        public string? TokenUri { get; set; }
        public string? AuthProviderX509CertUrl { get; set; }
        public string? ClientSecret { get; set; }
        public List<string>? RedirectUris { get; set; }
    }
}

