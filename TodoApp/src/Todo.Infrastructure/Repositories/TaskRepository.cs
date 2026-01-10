using Microsoft.EntityFrameworkCore;
using Todo.Application.Abstractions;
using Todo.Domain.Entities;
using Todo.Infrastructure.Persistence;

namespace Todo.Infrastructure.Repositories;

public sealed class TaskRepository : ITaskRepository
{
    private readonly TodoDbContext _db;

    public TaskRepository(TodoDbContext db) => _db = db;

    public Task<TodoTask?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        _db.Tasks.FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task AddAsync(TodoTask task, CancellationToken ct = default)
    {
        await _db.Tasks.AddAsync(task, ct);
    }

    public Task DeleteAsync(TodoTask task, CancellationToken ct = default)
    {
        _db.Tasks.Remove(task);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken ct = default) =>
        _db.SaveChangesAsync(ct);
}
