// Application/Common/Interfaces/IRepository.cs
using System.Linq.Expressions;
using OrderService.Domain.Entities;

namespace OrderService.Application.Common.Interfaces;

public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(object id, CancellationToken ct = default);
    Task<IReadOnlyList<T>> ListAllAsync(CancellationToken ct = default);
    Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);
    Task AddAsync(T entity, CancellationToken ct = default);
    void Update(T entity);
    void Remove(T entity);
}

public interface IUserRepository : IRepository<ApplicationUser>
{
    Task<ApplicationUser?> GetByEmailAsync(string email, CancellationToken ct = default);
}

public interface IAlertRepository : IRepository<Alert>
{
    Task<IReadOnlyList<Alert>> GetActiveAsync(CancellationToken ct = default);
}

public interface IUnitOfWork
{
    IUserRepository Users { get; }
    IAlertRepository Alerts { get; }
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
