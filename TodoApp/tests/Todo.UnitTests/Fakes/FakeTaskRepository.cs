using Todo.Application.Abstractions;
using Todo.Domain.Entities;

namespace Todo.UnitTests.Fakes;

internal sealed class FakeTaskRepository : ITaskRepository
{
    private readonly Dictionary<Guid, TodoTask> _tasks = new();

    public Task<TodoTask?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        _tasks.TryGetValue(id, out var task);
        return Task.FromResult(task);
    }

    public Task AddAsync(TodoTask task, CancellationToken ct = default)
    {
        _tasks[task.Id] = task;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(TodoTask task, CancellationToken ct = default)
    {
        _tasks.Remove(task.Id);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken ct = default) => Task.CompletedTask;

    // Convenience for tests
    public void Seed(TodoTask task) => _tasks[task.Id] = task;
    public bool Contains(Guid id) => _tasks.ContainsKey(id);
}
