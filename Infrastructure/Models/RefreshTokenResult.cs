using Microsoft.AspNetCore.Http;

namespace Infrastructure.Models;

public class RefreshTokenResult
{
    public int? StatusCode { get; set; }
    public string? Token { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public CookieOptions? cookieOptions { get; set; }
    public string? Error { get; set; }
        
}
