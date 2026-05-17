namespace PomodoroWidget.Core;

public sealed class PomodoroSettings
{
    public static PomodoroSettings Default { get; } = new();

    public int FocusMinutes { get; }
    public int ShortBreakMinutes { get; }
    public int LongBreakMinutes { get; }
    public int FocusSessionsBeforeLongBreak { get; }

    public PomodoroSettings(
        int focusMinutes = 25,
        int shortBreakMinutes = 5,
        int longBreakMinutes = 15,
        int focusSessionsBeforeLongBreak = 4)
    {
        if (focusMinutes <= 0) throw new ArgumentOutOfRangeException(nameof(focusMinutes));
        if (shortBreakMinutes <= 0) throw new ArgumentOutOfRangeException(nameof(shortBreakMinutes));
        if (longBreakMinutes <= 0) throw new ArgumentOutOfRangeException(nameof(longBreakMinutes));
        if (focusSessionsBeforeLongBreak <= 0) throw new ArgumentOutOfRangeException(nameof(focusSessionsBeforeLongBreak));

        FocusMinutes = focusMinutes;
        ShortBreakMinutes = shortBreakMinutes;
        LongBreakMinutes = longBreakMinutes;
        FocusSessionsBeforeLongBreak = focusSessionsBeforeLongBreak;
    }
}
