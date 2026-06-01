namespace PomodoroWidget.Core;

public sealed record PomodoroStatus(
    bool IsRunning,
    PomodoroPhase Phase,
    TimeSpan Remaining,
    int CompletedFocusSessionsToday);
