using Todo.Application.Abstractions;
using Todo.Domain.Entities;

namespace Todo.UnitTests.Spies;

internal sealed class SpyListRepository : IListRepository
{
    private readonly Dictionary<Guid, TaskList> _lists = new();

    public int AddCalls { get; private set; }
    public int SaveChangesCalls { get; private set; }

    public Task<TaskList?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        _lists.TryGetValue(id, out var list);
        return Task.FromResult(list);
    }

    public Task AddAsync(TaskList list, CancellationToken ct = default)
    {
        AddCalls++;
        _lists[list.Id] = list;
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken ct = default)
    {
        SaveChangesCalls++;
        return Task.CompletedTask;
    }

    public void Seed(TaskList list) => _lists[list.Id] = list;
}
