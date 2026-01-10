using Todo.Domain.Exceptions;

namespace Todo.Domain.Entities;

public sealed class TaskList
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Name { get; private set; }

    private TaskList() { Name = string.Empty; } // for EF

    public TaskList(string name)
    {
        Name = NormalizeName(name);
    }

    public void Rename(string name)
    {
        Name = NormalizeName(name);
    }

    private static string NormalizeName(string name)
    {
        var normalized = (name ?? string.Empty).Trim();

        if (normalized.Length == 0)
            throw new ValidationException("List name cannot be empty.");

        if (normalized.Length > 80)
            throw new ValidationException("List name cannot exceed 80 characters.");

        return normalized;
    }
}
