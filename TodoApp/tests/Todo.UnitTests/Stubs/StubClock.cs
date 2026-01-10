using Todo.Application.Abstractions;

namespace Todo.UnitTests.Stubs;

internal sealed class StubClock : IClock
{
    public DateTimeOffset UtcNow { get; set; }
}
