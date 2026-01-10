using Moq;
using Todo.Application.Abstractions;
using Todo.Application.Services;
using Todo.Domain.Entities;
using Todo.Domain.Exceptions;
using Xunit;

namespace Todo.UnitTests.Services;

public sealed class TodoServiceMoqEpBvaTests
{
    private static TodoService CreateSut(
        Mock<IListRepository> listRepo,
        Mock<ITaskRepository> taskRepo,
        Mock<IClock> clock)
    {
        return new TodoService(listRepo.Object, taskRepo.Object, clock.Object);
    }

    [Fact]
    public async Task CreateTaskAsync_EP_InvalidPartition_ListDoesNotExist_ThrowsNotFoundException()
    {
        // EP: Partition 1 = list exists (valid), Partition 2 = list missing (invalid)
        var listRepo = new Mock<IListRepository>(MockBehavior.Strict);
        var taskRepo = new Mock<ITaskRepository>(MockBehavior.Strict);
        var clock = new Mock<IClock>(MockBehavior.Strict);

        var listId = Guid.NewGuid();

        listRepo.Setup(r => r.GetByIdAsync(listId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((TaskList?)null);

        var sut = CreateSut(listRepo, taskRepo, clock);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            sut.CreateTaskAsync(listId, "Valid title", "Valid description"));

        listRepo.VerifyAll();
    }

    [Fact]
    public async Task CreateTaskAsync_BVA_TitleBoundary_OverMaxLength_ThrowsValidationException()
    {
        // BVA: max title length is 200. Test 201 (max+1) should fail.
        var listRepo = new Mock<IListRepository>(MockBehavior.Strict);
        var taskRepo = new Mock<ITaskRepository>(MockBehavior.Strict);
        var clock = new Mock<IClock>(MockBehavior.Strict);

        var listId = Guid.NewGuid();
        listRepo.Setup(r => r.GetByIdAsync(listId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new TaskList("Work") { /* if your TaskList has different ctor, adjust */ });

        // If your TaskList ID is generated internally, you may need to return a TaskList
        // seeded to have listId. If your Domain entity doesn't allow it, then instead
        // return any non-null list and let the service use the passed listId as reference.

        var sut = CreateSut(listRepo, taskRepo, clock);

        var title201 = new string('a', 201);

        await Assert.ThrowsAsync<ValidationException>(() =>
            sut.CreateTaskAsync(listId, title201, "Valid description"));

        listRepo.VerifyAll();
        taskRepo.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task CreateTaskAsync_BVA_DescriptionBoundary_UnderMinLength_ThrowsValidationException()
    {
        // BVA: min description length is 10. Test 9 (min-1) should fail.
        var listRepo = new Mock<IListRepository>(MockBehavior.Strict);
        var taskRepo = new Mock<ITaskRepository>(MockBehavior.Strict);
        var clock = new Mock<IClock>(MockBehavior.Strict);

        var listId = Guid.NewGuid();
        listRepo.Setup(r => r.GetByIdAsync(listId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new TaskList("Work"));

        var sut = CreateSut(listRepo, taskRepo, clock);

        var desc9 = new string('b', 9);

        await Assert.ThrowsAsync<ValidationException>(() =>
            sut.CreateTaskAsync(listId, "Valid title", desc9));

        listRepo.VerifyAll();
        taskRepo.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task CreateTaskAsync_ValidInput_CallsAddAndSaveChanges()
    {
        // Moq interaction verification (also useful for assignment)
        var listRepo = new Mock<IListRepository>(MockBehavior.Strict);
        var taskRepo = new Mock<ITaskRepository>(MockBehavior.Strict);
        var clock = new Mock<IClock>(MockBehavior.Strict);

        var listId = Guid.NewGuid();
        listRepo.Setup(r => r.GetByIdAsync(listId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new TaskList("Work"));

        taskRepo.Setup(r => r.AddAsync(It.IsAny<TodoTask>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

        taskRepo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

        var sut = CreateSut(listRepo, taskRepo, clock);

        var created = await sut.CreateTaskAsync(listId, "Valid title", "Valid description");

        Assert.Equal(listId, created.ListId);
        Assert.Equal("Valid title", created.Title);
        Assert.Equal("Valid description", created.Description);

        taskRepo.Verify(r => r.AddAsync(It.IsAny<TodoTask>(), It.IsAny<CancellationToken>()), Times.Once);
        taskRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        listRepo.VerifyAll();
    }
}
