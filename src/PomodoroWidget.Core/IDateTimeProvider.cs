namespace PomodoroWidget.Core;

public interface IDateTimeProvider
{
    DateTimeOffset Now { get; }
}

public sealed class SystemDateTimeProvider : IDateTimeProvider
{
    public DateTimeOffset Now => DateTimeOffset.Now;
}
