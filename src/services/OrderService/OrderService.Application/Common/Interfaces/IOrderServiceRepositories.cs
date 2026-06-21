using CommonInterfaces = Common.Interfaces;
using OrderService.Domain.Entities;

namespace OrderService.Application.Common.Interfaces;

public interface IUserRepository : CommonInterfaces.IRepository<ApplicationUser, Guid>
{
    Task<ApplicationUser?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
}

public interface IAlertRepository : CommonInterfaces.IRepository<Alert, Guid>
{
    Task<IReadOnlyList<Alert>> GetActiveAsync(CancellationToken cancellationToken = default);
}

public interface IUnitOfWork : CommonInterfaces.IUnitOfWork
{
    IUserRepository Users { get; }
    IAlertRepository Alerts { get; }
}
