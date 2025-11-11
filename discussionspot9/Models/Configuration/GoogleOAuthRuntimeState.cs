namespace discussionspot9.Models.Configuration
{
    /// <summary>
    /// Captures the resolved Google OAuth credentials at startup so that other services (health checks, diagnostics)
    /// can evaluate whether authentication is correctly configured.
    /// </summary>
    public sealed class GoogleOAuthRuntimeState
    {
        public GoogleOAuthRuntimeState(string? clientId, string? clientSecret, string? credentialsPathHint)
        {
            ClientId = clientId;
            ClientSecret = clientSecret;
            CredentialsPathHint = credentialsPathHint;
        }

        public string? ClientId { get; }

        public string? ClientSecret { get; }

        /// <summary>
        /// Provides a hint about where credentials were expected (for diagnostic messages).
        /// </summary>
        public string? CredentialsPathHint { get; }
    }
}





