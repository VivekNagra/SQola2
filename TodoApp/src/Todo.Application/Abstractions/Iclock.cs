namespace Todo.Application.Abstractions;

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}
