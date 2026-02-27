using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Pdfpeaks.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Pdfpeaks.Services.Auth;

/// <summary>
/// JWT Authentication service with token generation and validation
/// </summary>
public class JwtTokenService
{
    private readonly ILogger<JwtTokenService> _logger;
    private readonly IConfiguration _configuration;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly byte[] _key;

    public JwtTokenService(
        ILogger<JwtTokenService> logger,
        IConfiguration configuration,
        UserManager<ApplicationUser> userManager)
    {
        _logger = logger;
        _configuration = configuration;
        _userManager = userManager;
        
        var secretKey = configuration["Jwt:SecretKey"];
        
        // Validate key is present and strong enough (minimum 32 chars for HMAC-SHA256)
        if (string.IsNullOrWhiteSpace(secretKey))
        {
            if (configuration["ASPNETCORE_ENVIRONMENT"] == "Production")
            {
                throw new InvalidOperationException(
                    "JWT SecretKey is not configured. Set the JWT__SecretKey environment variable in production.");
            }
            // Development fallback — never use in production
            secretKey = "LocalDevSecretKey_ChangeInProduction_AtLeast32Chars!";
            logger.LogWarning("JWT SecretKey not configured — using development fallback. Set Jwt:SecretKey in appsettings.");
        }
        else if (secretKey.Length < 32)
        {
            logger.LogWarning("JWT SecretKey is shorter than 32 characters. Use a longer key for better security.");
        }
        
        _key = Encoding.UTF8.GetBytes(secretKey);
    }

    /// <summary>
    /// Generate JWT token for user
    /// </summary>
    public async Task<JwtAuthResult> GenerateTokenAsync(ApplicationUser user)
    {
        _logger.LogInformation("Generating JWT token for user: {UserId}", user.Id);

        var roles = await _userManager.GetRolesAsync(user);
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.UserName ?? ""),
            new(ClaimTypes.Email, user.Email ?? ""),
            new(ClaimTypes.GivenName, user.FirstName ?? ""),
            new(ClaimTypes.Surname, user.LastName ?? ""),
            new("subscription_tier", user.SubscriptionTier.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var tokenHandler = new JwtSecurityTokenHandler();
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(24),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(_key),
                SecurityAlgorithms.HmacSha256Signature),
            Issuer = _configuration["Jwt:Issuer"] ?? "Pdfpeaks",
            Audience = _configuration["Jwt:Audience"] ?? "PdfpeaksUsers"
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        var accessToken = tokenHandler.WriteToken(token);

        // Generate refresh token
        var refreshToken = GenerateRefreshToken();

        return new JwtAuthResult
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresIn = 86400, // 24 hours in seconds
            TokenType = "Bearer",
            UserId = user.Id,
            Email = user.Email ?? "",
            Roles = roles.ToList()
        };
    }

    /// <summary>
    /// Validate JWT token
    /// </summary>
    public ClaimsPrincipal? ValidateToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(_key),
                ValidateIssuer = true,
                ValidIssuer = _configuration["Jwt:Issuer"] ?? "Pdfpeaks",
                ValidateAudience = true,
                ValidAudience = _configuration["Jwt:Audience"] ?? "PdfpeaksUsers",
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            return tokenHandler.ValidateToken(token, validationParameters, out _);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Token validation failed");
            return null;
        }
    }

    /// <summary>
    /// Refresh access token
    /// </summary>
    public async Task<JwtAuthResult?> RefreshTokenAsync(string refreshToken)
    {
        // In production, validate refresh token against database
        // For now, decode the token to get user info
        var principal = ValidateToken(refreshToken);
        if (principal == null)
            return null;

        var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return null;

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null || !user.IsActive)
            return null;

        return await GenerateTokenAsync(user);
    }

    /// <summary>
    /// Get user ID from token
    /// </summary>
    public string? GetUserIdFromToken(string token)
    {
        var principal = ValidateToken(token);
        return principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }

    private string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }
}

/// <summary>
/// JWT authentication result
/// </summary>
public class JwtAuthResult
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public int ExpiresIn { get; set; }
    public string TokenType { get; set; } = "Bearer";
    public string UserId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
}
