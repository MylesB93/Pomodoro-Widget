namespace PomodoroWidget.Core;

public sealed class HomeScreenWidgetController
{
    private readonly PomodoroTimer _timer;

    public HomeScreenWidgetController(PomodoroTimer timer)
    {
        _timer = timer;
    }

    public PomodoroStatus StartTimer() => _timer.Start();

    public PomodoroStatus StopTimer() => _timer.Stop();

    public PomodoroStatus Tick(TimeSpan elapsed) => _timer.Advance(elapsed);

    public string BuildDailySummaryText()
    {
        var count = _timer.GetStatus().CompletedFocusSessionsToday;
        var label = count == 1 ? "focused period" : "focused periods";
        return $"{count} {label} completed today";
    }
}
