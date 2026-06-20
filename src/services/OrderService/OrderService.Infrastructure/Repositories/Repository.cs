// Repositories/Repository.cs
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using OrderService.Application.Common.Interfaces;
using OrderService.Infrastructure.Persistence;

namespace OrderService.Infrastructure.Repositories;

public class Repository<T> : IRepository<T> where T : class
{
    protected readonly ApplicationDbContext Db;
    protected readonly DbSet<T> Set;

    public Repository(ApplicationDbContext db)
    {
        Db = db;
        Set = db.Set<T>();
    }

    public async Task<T?> GetByIdAsync(object id, CancellationToken ct = default)
    {
        return await Set.FindAsync(new[] { id }, ct);
    }

    public async Task<IReadOnlyList<T>> ListAllAsync(CancellationToken ct = default)
    {
        return await Set.AsNoTracking().ToListAsync(ct);
    }

    public async Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
    {
        return await Set.AsNoTracking().Where(predicate).ToListAsync(ct);
    }

    public async Task AddAsync(T entity, CancellationToken ct = default)
    {
        await Set.AddAsync(entity, ct);
    }

    public void Update(T entity) => Set.Update(entity);
    public void Remove(T entity) => Set.Remove(entity);
}
