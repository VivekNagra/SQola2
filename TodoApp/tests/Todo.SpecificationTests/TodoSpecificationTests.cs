using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Todo.Infrastructure.Persistence;
using Todo.SpecificationTests.Infrastructure;
using Xunit;

namespace Todo.SpecificationTests;

public sealed class TodoSpecificationTests : IClassFixture<TodoApiFactory>
{
    private readonly TodoApiFactory _factory;

    public TodoSpecificationTests(TodoApiFactory factory) => _factory = factory;

    [Fact]
    public async Task GivenAList_WhenCreatingAndCompletingATask_ThenTaskIsCompleted()
    {
        // Given
        var client = _factory.CreateClient();

        var createListResp = await client.PostAsJsonAsync("/lists", new { name = "SpecList" });
        createListResp.EnsureSuccessStatusCode();

        var createdList = await createListResp.Content.ReadFromJsonAsync<CreatedList>();
        Assert.NotNull(createdList);

        // When
        var createTaskResp = await client.PostAsJsonAsync("/tasks", new
        {
            listId = createdList!.Id,
            title = "Write specification test"
        });
        createTaskResp.EnsureSuccessStatusCode();

        var createdTask = await createTaskResp.Content.ReadFromJsonAsync<CreatedTask>();
        Assert.NotNull(createdTask);

        var completeResp = await client.PatchAsync($"/tasks/{createdTask!.Id}/complete", content: null);
        completeResp.EnsureSuccessStatusCode();

        // Then (verify persisted state, not just HTTP)
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TodoDbContext>();

        var task = await db.Tasks.FindAsync(createdTask.Id);
        Assert.NotNull(task);
        Assert.True(task!.IsCompleted);
        Assert.Equal("Write specification test", task.Title);
        Assert.Equal(createdList.Id, task.ListId);
    }

    private sealed record CreatedList(Guid Id, string Name);
    private sealed record CreatedTask(Guid Id, Guid ListId, string Title, bool IsCompleted, DateTimeOffset? Deadline);
}
