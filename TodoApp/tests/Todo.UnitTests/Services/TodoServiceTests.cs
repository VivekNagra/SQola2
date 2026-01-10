using Todo.Application.Services;
using Todo.Domain.Entities;
using Todo.Domain.Exceptions;
using Todo.UnitTests.Fakes;
using Todo.UnitTests.Stubs;
//using Xunit;

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

        var task = await sut.CreateTaskAsync(list.Id, "Finish report", "Complete the report for school submission.");

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
            sut.CreateTaskAsync(missingListId, "Task title", "Task description"));

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

        var task = new TodoTask(list.Id, "Old title", "Old description");
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

        var task = new TodoTask(list.Id, "Submit timesheet", "Submit the timesheet for this week");
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

        var task = new TodoTask(list.Id, "Clean up", "Clean up the room bruv");
        tasks.Seed(task);

        await sut.DeleteTaskAsync(task.Id);

        Assert.False(tasks.Contains(task.Id));
    }

    [Fact]
public async Task CreateListAsync_WhenNameHasWhitespace_TrimsName()
{
    var (sut, _, _, _) = CreateSut();

    var list = await sut.CreateListAsync("  Work  ");

    Assert.Equal("Work", list.Name);
    Assert.NotEqual(Guid.Empty, list.Id);
}

[Fact]
public async Task CreateListAsync_WhenNameIsEmpty_ThrowsValidationException()
{
    var (sut, _, _, _) = CreateSut();

    await Assert.ThrowsAsync<ValidationException>(() => sut.CreateListAsync("   "));
}

[Fact]
public async Task MarkTaskInProgressAsync_WhenTaskExists_SetsIsCompletedFalse()
{
    var (sut, lists, tasks, _) = CreateSut();
    var list = new TaskList("Work");
    lists.Seed(list);

    var task = new TodoTask(list.Id, "Test", "Test description");
    task.MarkCompleted();
    tasks.Seed(task);

    await sut.MarkTaskInProgressAsync(task.Id);

    var updated = await tasks.GetByIdAsync(task.Id);
    Assert.NotNull(updated);
    Assert.False(updated!.IsCompleted);
}

[Fact]
public async Task MoveTaskToListAsync_WhenNewListExists_UpdatesListId()
{
    var (sut, lists, tasks, _) = CreateSut();

    var fromList = new TaskList("From");
    var toList = new TaskList("To");
    lists.Seed(fromList);
    lists.Seed(toList);

    var task = new TodoTask(fromList.Id, "Move me", "Move this task to another list");
    tasks.Seed(task);

    await sut.MoveTaskToListAsync(task.Id, toList.Id);

    var updated = await tasks.GetByIdAsync(task.Id);
    Assert.NotNull(updated);
    Assert.Equal(toList.Id, updated!.ListId);
}

[Fact]
public async Task DeleteTaskAsync_WhenTaskDoesNotExist_DoesNotThrow()
{
    var (sut, _, _, _) = CreateSut();

    var missingTaskId = Guid.NewGuid();

    var ex = await Record.ExceptionAsync(() => sut.DeleteTaskAsync(missingTaskId));
    Assert.Null(ex);
}

[Fact]
public async Task CreateListAsync_CallsAddAndSaveChanges()
{
    var tasks = new Todo.UnitTests.Spies.SpyTaskRepository();
    var lists = new Todo.UnitTests.Spies.SpyListRepository();
    var clock = new Todo.UnitTests.Stubs.StubClock { UtcNow = new DateTimeOffset(2026, 01, 10, 12, 0, 0, TimeSpan.Zero) };

    var sut = new Todo.Application.Services.TodoService(lists, tasks, clock);

    await sut.CreateListAsync("SpyList");

    Assert.Equal(1, lists.AddCalls);
    Assert.Equal(1, lists.SaveChangesCalls);
}

[Fact]
public async Task CreateTaskAsync_CallsAddAndSaveChanges()
{
    var tasks = new Todo.UnitTests.Spies.SpyTaskRepository();
    var lists = new Todo.UnitTests.Spies.SpyListRepository();
    var clock = new Todo.UnitTests.Stubs.StubClock { UtcNow = new DateTimeOffset(2026, 01, 10, 12, 0, 0, TimeSpan.Zero) };

    var sut = new Todo.Application.Services.TodoService(lists, tasks, clock);

    var list = new Todo.Domain.Entities.TaskList("Spy");
    lists.Seed(list);

    await sut.CreateTaskAsync(list.Id, "SpyTask", "Spy task description");

    Assert.Equal(1, tasks.AddCalls);
    Assert.Equal(1, tasks.SaveChangesCalls);
}

[Fact]
public async Task MarkTaskCompletedAsync_CallsSaveChanges()
{
    var tasks = new Todo.UnitTests.Spies.SpyTaskRepository();
    var lists = new Todo.UnitTests.Spies.SpyListRepository();
    var clock = new Todo.UnitTests.Stubs.StubClock { UtcNow = new DateTimeOffset(2026, 01, 10, 12, 0, 0, TimeSpan.Zero) };

    var sut = new Todo.Application.Services.TodoService(lists, tasks, clock);

    var list = new Todo.Domain.Entities.TaskList("Spy");
    lists.Seed(list);

    var task = new Todo.Domain.Entities.TodoTask(list.Id, "Complete me", "Complete the task");
    tasks.Seed(task);

    await sut.MarkTaskCompletedAsync(task.Id);

    Assert.True(task.IsCompleted);
    Assert.Equal(1, tasks.SaveChangesCalls);
}

[Fact]
public async Task SetTaskDeadlineAsync_WhenDeadlineEqualsNow_DoesNotThrow()
{
    var (sut, lists, tasks, clock) = CreateSut();
    var list = new TaskList("Work");
    lists.Seed(list);

    var task = new TodoTask(list.Id, "Deadline boundary", "Deadline boundary task");
    tasks.Seed(task);

    var ex = await Record.ExceptionAsync(() => sut.SetTaskDeadlineAsync(task.Id, clock.UtcNow));
    Assert.Null(ex);

    var updated = await tasks.GetByIdAsync(task.Id);
    Assert.Equal(clock.UtcNow, updated!.Deadline);
}

[Fact]
public async Task MoveTaskToListAsync_WhenNewListIsMissing_ThrowsNotFoundException()
{
    var (sut, lists, tasks, _) = CreateSut();

    var fromList = new TaskList("From");
    lists.Seed(fromList);

    var task = new TodoTask(fromList.Id, "Move me", "Move this task to another list");
    tasks.Seed(task);

    var missingListId = Guid.NewGuid();

    await Assert.ThrowsAsync<NotFoundException>(() =>
        sut.MoveTaskToListAsync(task.Id, missingListId));
}


}


