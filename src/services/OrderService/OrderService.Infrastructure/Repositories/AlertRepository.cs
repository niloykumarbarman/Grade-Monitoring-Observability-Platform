// Repositories/AlertRepository.cs
using Microsoft.EntityFrameworkCore;
using OrderService.Application.Common.Interfaces;
using OrderService.Domain.Entities;
using OrderService.Infrastructure.Persistence;

namespace OrderService.Infrastructure.Repositories;

public class AlertRepository : Repository<Alert>, IAlertRepository
{
    public AlertRepository(ApplicationDbContext db) : base(db) { }

    public async Task<IReadOnlyList<Alert>> GetActiveAsync(CancellationToken ct = default)
    {
        return await Db.Alerts.AsNoTracking()
            .Where(a => a.Status == OrderService.Domain.Enums.AlertStatus.Firing)
            .OrderByDescending(a => a.FiredAt)
            .ToListAsync(ct);
    }
}
