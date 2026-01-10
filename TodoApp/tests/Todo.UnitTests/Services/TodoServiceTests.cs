using Todo.Application.Services;
using Todo.Domain.Entities;
using Todo.Domain.Exceptions;
using Todo.UnitTests.Fakes;
using Todo.UnitTests.Stubs;
using Xunit;

namespace Todo.UnitTests.Services;

public sealed class TodoServiceTests
{
    private static (TodoService sut, FakeListRepository lists, FakeTaskRepository tasks, StubClock clock)
        CreateSut()
    {
        var lists = new FakeListRepository();
        var tasks = new FakeTaskRepository();
        var clock = new StubClock { UtcNow = new DateTimeOffset(2026, 01, 10, 12, 0, 0, TimeSpan.Zero) };

        var sut = new TodoService(lists, tasks, clock);
        return (sut, lists, tasks, clock);
    }

    [Fact]
    public async Task CreateTaskAsync_WhenListExists_CreatesTaskWithExpectedDefaults()
    {
        // Unit: TodoService.CreateTaskAsync
        // Behavior: creates a task for an existing list with defaults (not completed, no deadline)
        // Repro: run this test; if it fails, see assertions

        var (sut, lists, _, _) = CreateSut();
        var list = new TaskList("School");
        lists.Seed(list);

        var task = await sut.CreateTaskAsync(list.Id, "Finish report");

        Assert.Equal(list.Id, task.ListId);
        Assert.Equal("Finish report", task.Title);
        Assert.False(task.IsCompleted);
        Assert.Null(task.Deadline);
        Assert.NotEqual(Guid.Empty, task.Id);
    }

    [Fact]
    public async Task CreateTaskAsync_WhenListDoesNotExist_ThrowsNotFoundException()
    {
        // Unit: TodoService.CreateTaskAsync
        // Behavior: should reject creating tasks in non-existing lists

        var (sut, _, _, _) = CreateSut();
        var missingListId = Guid.NewGuid();

        var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
            sut.CreateTaskAsync(missingListId, "Task title"));

        Assert.Contains(missingListId.ToString(), ex.Message);
    }

    [Fact]
    public async Task UpdateTaskTitleAsync_WhenTaskExists_UpdatesTitle()
    {
        // Unit: TodoService.UpdateTaskTitleAsync
        // Behavior: updates the title and trims whitespace through domain normalization

        var (sut, lists, tasks, _) = CreateSut();
        var list = new TaskList("Home");
        lists.Seed(list);

        var task = new TodoTask(list.Id, "Old title");
        tasks.Seed(task);

        await sut.UpdateTaskTitleAsync(task.Id, "  New title  ");

        var updated = await tasks.GetByIdAsync(task.Id);
        Assert.NotNull(updated);
        Assert.Equal("New title", updated!.Title);
    }

    [Fact]
    public async Task SetTaskDeadlineAsync_WhenDeadlineIsInPast_ThrowsValidationException()
    {
        // Unit: TodoService.SetTaskDeadlineAsync
        // Behavior: deadline cannot be in the past relative to IClock.UtcNow

        var (sut, lists, tasks, clock) = CreateSut();
        var list = new TaskList("Work");
        lists.Seed(list);

        var task = new TodoTask(list.Id, "Submit timesheet");
        tasks.Seed(task);

        var pastDeadline = clock.UtcNow.AddMinutes(-1);

        await Assert.ThrowsAsync<ValidationException>(() =>
            sut.SetTaskDeadlineAsync(task.Id, pastDeadline));
    }

    [Fact]
    public async Task DeleteTaskAsync_WhenTaskExists_RemovesTask()
    {
        // Unit: TodoService.DeleteTaskAsync
        // Behavior: deletes an existing task and is persisted in repository state

        var (sut, lists, tasks, _) = CreateSut();
        var list = new TaskList("Admin");
        lists.Seed(list);

        var task = new TodoTask(list.Id, "Clean up");
        tasks.Seed(task);

        await sut.DeleteTaskAsync(task.Id);

        Assert.False(tasks.Contains(task.Id));
    }
}
