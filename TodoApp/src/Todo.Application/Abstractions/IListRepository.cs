using Todo.Domain.Entities;

namespace Todo.Application.Abstractions;

public interface IListRepository
{
    Task<TaskList?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(TaskList list, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
