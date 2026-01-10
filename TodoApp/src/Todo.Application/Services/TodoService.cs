using Todo.Application.Abstractions;
using Todo.Domain.Entities;
using Todo.Domain.Exceptions;

namespace Todo.Application.Services;

public sealed class TodoService
{
    private readonly IListRepository _lists;
    private readonly ITaskRepository _tasks;
    private readonly IClock _clock;

    public TodoService(IListRepository lists, ITaskRepository tasks, IClock clock)
    {
        _lists = lists;
        _tasks = tasks;
        _clock = clock;
    }

    public async Task<TaskList> CreateListAsync(string name, CancellationToken ct = default)
    {
        var list = new TaskList(name);
        await _lists.AddAsync(list, ct);
        await _lists.SaveChangesAsync(ct);
        return list;
    }

    public async Task<TodoTask> CreateTaskAsync(Guid listId, string title, CancellationToken ct = default)
    {
        var list = await _lists.GetByIdAsync(listId, ct);
        if (list is null)
            throw new NotFoundException($"List '{listId}' was not found.");

        var task = new TodoTask(listId, title);
        await _tasks.AddAsync(task, ct);
        await _tasks.SaveChangesAsync(ct);
        return task;
    }

    public async Task UpdateTaskTitleAsync(Guid taskId, string newTitle, CancellationToken ct = default)
    {
        var task = await _tasks.GetByIdAsync(taskId, ct);
        if (task is null)
            throw new NotFoundException($"Task '{taskId}' was not found.");

        task.UpdateTitle(newTitle);
        await _tasks.SaveChangesAsync(ct);
    }

    public async Task SetTaskDeadlineAsync(Guid taskId, DateTimeOffset? deadline, CancellationToken ct = default)
    {
        var task = await _tasks.GetByIdAsync(taskId, ct);
        if (task is null)
            throw new NotFoundException($"Task '{taskId}' was not found.");

        if (deadline is not null && deadline.Value < _clock.UtcNow)
            throw new ValidationException("Deadline cannot be in the past.");

        task.SetDeadline(deadline);
        await _tasks.SaveChangesAsync(ct);
    }

    public async Task MarkTaskCompletedAsync(Guid taskId, CancellationToken ct = default)
    {
        var task = await _tasks.GetByIdAsync(taskId, ct);
        if (task is null)
            throw new NotFoundException($"Task '{taskId}' was not found.");

        task.MarkCompleted();
        await _tasks.SaveChangesAsync(ct);
    }

    public async Task MarkTaskInProgressAsync(Guid taskId, CancellationToken ct = default)
    {
        var task = await _tasks.GetByIdAsync(taskId, ct);
        if (task is null)
            throw new NotFoundException($"Task '{taskId}' was not found.");

        task.MarkInProgress();
        await _tasks.SaveChangesAsync(ct);
    }

    public async Task MoveTaskToListAsync(Guid taskId, Guid newListId, CancellationToken ct = default)
    {
        var task = await _tasks.GetByIdAsync(taskId, ct);
        if (task is null)
            throw new NotFoundException($"Task '{taskId}' was not found.");

        var list = await _lists.GetByIdAsync(newListId, ct);
        if (list is null)
            throw new NotFoundException($"List '{newListId}' was not found.");

        task.MoveToList(newListId);
        await _tasks.SaveChangesAsync(ct);
    }

    public async Task DeleteTaskAsync(Guid taskId, CancellationToken ct = default)
    {
        var task = await _tasks.GetByIdAsync(taskId, ct);
        if (task is null)
            return; // idempotent delete is fine (also simplifies API)

        await _tasks.DeleteAsync(task, ct);
        await _tasks.SaveChangesAsync(ct);
    }
}
