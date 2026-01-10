using Microsoft.EntityFrameworkCore;
using Todo.Application.Abstractions;
using Todo.Application.Services;
using Todo.Infrastructure;
using Todo.Infrastructure.Persistence;
using Todo.Infrastructure.Repositories;
using Todo.Domain.Exceptions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Db
builder.Services.AddDbContext<TodoDbContext>(options =>
{
    // For now: local file db during dev
// Later: integration tests will override this with SQLite in-memory.
    options.UseSqlite("Data Source=todo.db");
});

// App services
builder.Services.AddScoped<IClock, SystemClock>();
builder.Services.AddScoped<IListRepository, ListRepository>();
builder.Services.AddScoped<ITaskRepository, TaskRepository>();
builder.Services.AddScoped<TodoService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/", () => Results.Ok("Todo API running"));

app.MapPost("/lists", async (TodoService service, CreateListRequest req) =>
{
    try
    {
        var list = await service.CreateListAsync(req.Name);
        return Results.Created($"/lists/{list.Id}", new { list.Id, list.Name });
    }
    catch (ValidationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.MapPost("/tasks", async (TodoService service, CreateTaskRequest req) =>
{
    try
    {
        var task = await service.CreateTaskAsync(req.ListId, req.Title, req.Description);
        return Results.Created($"/tasks/{task.Id}", new
        {
            task.Id,
            task.ListId,
            task.Title,
            task.Description,
            task.IsCompleted,
            task.Deadline
        });
    }
    catch (NotFoundException ex)
    {
        return Results.NotFound(new { error = ex.Message });
    }
    catch (ValidationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.MapPatch("/tasks/{id:guid}/title", async (TodoService service, Guid id, UpdateTaskTitleRequest req) =>
{
    try
    {
        await service.UpdateTaskTitleAsync(id, req.Title);
        return Results.NoContent();
    }
    catch (NotFoundException ex)
    {
        return Results.NotFound(new { error = ex.Message });
    }
    catch (ValidationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.MapPatch("/tasks/{id:guid}/deadline", async (TodoService service, Guid id, SetDeadlineRequest req) =>
{
    try
    {
        await service.SetTaskDeadlineAsync(id, req.Deadline);
        return Results.NoContent();
    }
    catch (NotFoundException ex)
    {
        return Results.NotFound(new { error = ex.Message });
    }
    catch (ValidationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.MapPatch("/tasks/{id:guid}/complete", async (TodoService service, Guid id) =>
{
    try
    {
        await service.MarkTaskCompletedAsync(id);
        return Results.NoContent();
    }
    catch (NotFoundException ex)
    {
        return Results.NotFound(new { error = ex.Message });
    }
});

app.MapPatch("/tasks/{id:guid}/in-progress", async (TodoService service, Guid id) =>
{
    try
    {
        await service.MarkTaskInProgressAsync(id);
        return Results.NoContent();
    }
    catch (NotFoundException ex)
    {
        return Results.NotFound(new { error = ex.Message });
    }
});

app.MapPatch("/tasks/{id:guid}/move", async (TodoService service, Guid id, MoveTaskRequest req) =>
{
    try
    {
        await service.MoveTaskToListAsync(id, req.ListId);
        return Results.NoContent();
    }
    catch (NotFoundException ex)
    {
        return Results.NotFound(new { error = ex.Message });
    }
    catch (ValidationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.MapDelete("/tasks/{id:guid}", async (TodoService service, Guid id) =>
{
    await service.DeleteTaskAsync(id);
    return Results.NoContent();
});


app.Run();

// Required for WebApplicationFactory in tests
public partial class Program { }

public sealed record CreateListRequest(string Name);
public sealed record CreateTaskRequest(Guid ListId, string Title, string Description);
public sealed record UpdateTaskTitleRequest(string Title);
public sealed record SetDeadlineRequest(DateTimeOffset? Deadline);
public sealed record MoveTaskRequest(Guid ListId);
