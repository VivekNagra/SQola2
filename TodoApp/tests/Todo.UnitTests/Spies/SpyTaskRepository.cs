using Todo.Application.Abstractions;
using Todo.Domain.Entities;

namespace Todo.UnitTests.Spies;

internal sealed class SpyTaskRepository : ITaskRepository
{
    private readonly Dictionary<Guid, TodoTask> _tasks = new();

    public int AddCalls { get; private set; }
    public int DeleteCalls { get; private set; }
    public int SaveChangesCalls { get; private set; }

    public Task<TodoTask?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        _tasks.TryGetValue(id, out var task);
        return Task.FromResult(task);
    }

    public Task AddAsync(TodoTask task, CancellationToken ct = default)
    {
        AddCalls++;
        _tasks[task.Id] = task;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(TodoTask task, CancellationToken ct = default)
    {
        DeleteCalls++;
        _tasks.Remove(task.Id);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken ct = default)
    {
        SaveChangesCalls++;
        return Task.CompletedTask;
    }

    public void Seed(TodoTask task) => _tasks[task.Id] = task;
}
