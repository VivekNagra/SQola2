using Microsoft.Extensions.DependencyInjection;
using Todo.Application.Services;
using Todo.Infrastructure.Persistence;
using Todo.IntegrationTests.Infrastructure;
//using Xunit;

namespace Todo.IntegrationTests;

public sealed class TodoPersistenceIntegrationTests : IClassFixture<TodoApiFactory>
{
    private readonly TodoApiFactory _factory;

    public TodoPersistenceIntegrationTests(TodoApiFactory factory) => _factory = factory;

    [Fact]
    public async Task CreateTaskAsync_PersistsTaskAndCanBeReadBack()
    {
        using var scope = _factory.Services.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<Todo.Application.Services.TodoService>();
        var db = scope.ServiceProvider.GetRequiredService<TodoDbContext>();

        var list = await service.CreateListAsync("Integration");
        var created = await service.CreateTaskAsync(list.Id, "Persist me", "Persist this task");

        var readBack = await db.Tasks.FindAsync(created.Id);

        Assert.NotNull(readBack);
        Assert.Equal("Persist me", readBack!.Title);
        Assert.Equal(list.Id, readBack.ListId);
        Assert.False(readBack.IsCompleted);
    }


    [Fact]
    public async Task MarkTaskCompletedAsync_PersistsCompletionState()
    {
        using var scope = _factory.Services.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<TodoService>();
        var db = scope.ServiceProvider.GetRequiredService<TodoDbContext>();

        var list = await service.CreateListAsync("Integration2");
        var task = await service.CreateTaskAsync(list.Id, "Complete me", "Please complete me");

        await service.MarkTaskCompletedAsync(task.Id);

        var readBack = await db.Tasks.FindAsync(task.Id);

        Assert.NotNull(readBack);
        Assert.True(readBack!.IsCompleted);
    }

}