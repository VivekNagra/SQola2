using Microsoft.EntityFrameworkCore;
using Todo.Application.Abstractions;
using Todo.Domain.Entities;
using Todo.Infrastructure.Persistence;

namespace Todo.Infrastructure.Repositories;

public sealed class ListRepository : IListRepository
{
    private readonly TodoDbContext _db;

    public ListRepository(TodoDbContext db) => _db = db;

    public Task<TaskList?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        _db.Lists.FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task AddAsync(TaskList list, CancellationToken ct = default)
    {
        await _db.Lists.AddAsync(list, ct);
    }

    public Task SaveChangesAsync(CancellationToken ct = default) =>
        _db.SaveChangesAsync(ct);
}
