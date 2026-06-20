// Repositories/UnitOfWork.cs
using OrderService.Application.Common.Interfaces;
using OrderService.Infrastructure.Persistence;

namespace OrderService.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _db;
    public IUserRepository Users { get; }
    public IAlertRepository Alerts { get; }

    public UnitOfWork(ApplicationDbContext db, IUserRepository users, IAlertRepository alerts)
    {
        _db = db;
        Users = users;
        Alerts = alerts;
    }

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        return await _db.SaveChangesAsync(ct);
    }
}
