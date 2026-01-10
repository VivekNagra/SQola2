using Todo.Domain.Exceptions;

namespace Todo.Domain.Entities;

public sealed class TodoTask
{
    public Guid Id { get; private set; } = Guid.NewGuid();

    public Guid ListId { get; private set; }
    public string Title { get; private set; }
    public string Description { get; private set; }
    public bool IsCompleted { get; private set; }
    public DateTimeOffset? Deadline { get; private set; }

    private TodoTask()
    {
        Title = string.Empty;       // for EF
        Description = string.Empty; // for EF
    }

    public TodoTask(Guid listId, string title, string description)
    {
        if (listId == Guid.Empty)
            throw new ValidationException("ListId is required.");

        ListId = listId;
        Title = NormalizeTitle(title);
        Description = NormalizeDescription(description);

        IsCompleted = false;
        Deadline = null;
    }

    public void UpdateTitle(string title)
    {
        Title = NormalizeTitle(title);
    }

    public void UpdateDescription(string description)
    {
        Description = NormalizeDescription(description);
    }

    public void MoveToList(Guid listId)
    {
        if (listId == Guid.Empty)
            throw new ValidationException("ListId is required.");

        ListId = listId;
    }

    public void MarkCompleted()
    {
        IsCompleted = true;
    }

    public void MarkInProgress()
    {
        IsCompleted = false;
    }

    public void SetDeadline(DateTimeOffset? deadline)
    {
        Deadline = deadline; // not-in-the-past rule enforced in Application using IClock
    }

    private static string NormalizeTitle(string title)
    {
        var normalized = (title ?? string.Empty).Trim();

        if (normalized.Length == 0)
            throw new ValidationException("Task title cannot be empty.");

        if (normalized.Length > 200)
            throw new ValidationException("Task title cannot exceed 200 characters.");

        return normalized;
    }

    private static string NormalizeDescription(string description)
    {
        var normalized = (description ?? string.Empty).Trim();

        const int minLen = 10;
        const int maxLen = 2000;

        if (normalized.Length < minLen)
            throw new ValidationException($"Task description must be at least {minLen} characters.");

        if (normalized.Length > maxLen)
            throw new ValidationException($"Task description cannot exceed {maxLen} characters.");

        return normalized;
    }
}
