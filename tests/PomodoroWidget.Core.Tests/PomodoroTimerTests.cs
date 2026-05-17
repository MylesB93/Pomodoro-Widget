namespace PomodoroWidget.Core.Tests;

public class PomodoroTimerTests
{
    [Fact]
    public void TimerStartsWithDefaultFocusDurationAndStoppedState()
    {
        var timer = new PomodoroTimer();

        var status = timer.GetStatus();

        Assert.False(status.IsRunning);
        Assert.Equal(PomodoroPhase.Focus, status.Phase);
        Assert.Equal(TimeSpan.FromMinutes(25), status.Remaining);
        Assert.Equal(0, status.CompletedFocusSessionsToday);
    }

    [Fact]
    public void CompletingFocusSessionIncrementsTodayCountAndStartsShortBreak()
    {
        var clock = new FakeDateTimeProvider(new DateTimeOffset(2026, 5, 17, 9, 0, 0, TimeSpan.Zero));
        var timer = new PomodoroTimer(dateTimeProvider: clock);

        timer.Start();
        var status = timer.Advance(TimeSpan.FromMinutes(25));

        Assert.Equal(PomodoroPhase.ShortBreak, status.Phase);
        Assert.Equal(TimeSpan.FromMinutes(5), status.Remaining);
        Assert.Equal(1, status.CompletedFocusSessionsToday);
    }

    [Fact]
    public void FourthCompletedFocusSessionStartsLongBreak()
    {
        var clock = new FakeDateTimeProvider(new DateTimeOffset(2026, 5, 17, 9, 0, 0, TimeSpan.Zero));
        var timer = new PomodoroTimer(dateTimeProvider: clock);

        timer.Start();

        for (var cycle = 0; cycle < 3; cycle++)
        {
            timer.Advance(TimeSpan.FromMinutes(25));
            timer.Advance(TimeSpan.FromMinutes(5));
        }

        var status = timer.Advance(TimeSpan.FromMinutes(25));

        Assert.Equal(PomodoroPhase.LongBreak, status.Phase);
        Assert.Equal(TimeSpan.FromMinutes(15), status.Remaining);
        Assert.Equal(4, status.CompletedFocusSessionsToday);
    }

    [Fact]
    public void CompletedFocusSessionsAreTrackedPerDay()
    {
        var clock = new FakeDateTimeProvider(new DateTimeOffset(2026, 5, 17, 9, 0, 0, TimeSpan.Zero));
        var timer = new PomodoroTimer(dateTimeProvider: clock);

        timer.Start();
        timer.Advance(TimeSpan.FromMinutes(25));
        clock.Now = clock.Now.AddDays(1);

        var status = timer.GetStatus();

        Assert.Equal(0, status.CompletedFocusSessionsToday);
        Assert.Equal(1, timer.CompletedFocusSessionsByDate[new DateOnly(2026, 5, 17)]);
    }

    [Fact]
    public void WidgetControllerCanStartStopAndShowDailySummary()
    {
        var timer = new PomodoroTimer();
        var widgetController = new HomeScreenWidgetController(timer);

        var started = widgetController.StartTimer();
        widgetController.StopTimer();

        Assert.True(started.IsRunning);
        Assert.Equal("0 focused periods completed today", widgetController.BuildDailySummaryText());
    }

    private sealed class FakeDateTimeProvider : IDateTimeProvider
    {
        public FakeDateTimeProvider(DateTimeOffset now)
        {
            Now = now;
        }

        public DateTimeOffset Now { get; set; }
    }
}
