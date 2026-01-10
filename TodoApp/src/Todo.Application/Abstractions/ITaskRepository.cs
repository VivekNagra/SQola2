using Todo.Domain.Entities;

namespace Todo.Application.Abstractions;

public interface ITaskRepository
{
    Task<TodoTask?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(TodoTask task, CancellationToken ct = default);
    Task DeleteAsync(TodoTask task, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
