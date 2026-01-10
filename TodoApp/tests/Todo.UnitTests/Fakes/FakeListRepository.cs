using Todo.Application.Abstractions;
using Todo.Domain.Entities;

namespace Todo.UnitTests.Fakes;

internal sealed class FakeListRepository : IListRepository
{
    private readonly Dictionary<Guid, TaskList> _lists = new();

    public Task<TaskList?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        _lists.TryGetValue(id, out var list);
        return Task.FromResult(list);
    }

    public Task AddAsync(TaskList list, CancellationToken ct = default)
    {
        _lists[list.Id] = list;
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken ct = default) => Task.CompletedTask;

    // Convenience for tests
    public void Seed(TaskList list) => _lists[list.Id] = list;
}
