using Microsoft.AspNetCore.Identity;

namespace OrderService.Domain.Entities;

public class ApplicationUser : IdentityUser<Guid>
{
    public string? FullName { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTimeOffset? LastLoginAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public string? RefreshToken { get; set; }
    public DateTimeOffset? RefreshTokenExpiry { get; set; }
}
