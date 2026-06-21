// Repositories/UserRepository.cs
using Microsoft.EntityFrameworkCore;
using OrderService.Application.Common.Interfaces;
using OrderService.Domain.Entities;
using OrderService.Infrastructure.Persistence;

namespace OrderService.Infrastructure.Repositories;

public class UserRepository : Repository<ApplicationUser, Guid>, IUserRepository
{
    public UserRepository(ApplicationDbContext db) : base(db) { }

    public async Task<ApplicationUser?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await Db.Users.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }
}
