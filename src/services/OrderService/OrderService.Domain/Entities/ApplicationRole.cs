using Microsoft.AspNetCore.Identity;

namespace OrderService.Domain.Entities;

public class ApplicationRole : IdentityRole<Guid>
{
    public string? Description { get; set; }
}
