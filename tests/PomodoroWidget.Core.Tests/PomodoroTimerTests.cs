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

    [Fact]
    public void ResetStopsTimerAndRestoresRemainingTimeForCurrentPhase()
    {
        var timer = new PomodoroTimer();
        var widgetController = new HomeScreenWidgetController(timer);

        widgetController.StartTimer();
        widgetController.Tick(TimeSpan.FromMinutes(10));

        var status = widgetController.ResetTimer();

        Assert.False(status.IsRunning);
        Assert.Equal(PomodoroPhase.Focus, status.Phase);
        Assert.Equal(TimeSpan.FromMinutes(25), status.Remaining);
    }

    [Fact]
    public void UpdatingSettingsResetsCurrentPhaseRemainingAndAffectsSubsequentPhases()
    {
        var timer = new PomodoroTimer();
        var widgetController = new HomeScreenWidgetController(timer);

        var updated = widgetController.UpdateSettings(new PomodoroSettings(focusMinutes: 30, shortBreakMinutes: 7, longBreakMinutes: 20));
        widgetController.StartTimer();
        var shortBreakStatus = widgetController.Tick(TimeSpan.FromMinutes(30));

        Assert.Equal(TimeSpan.FromMinutes(30), updated.Remaining);
        Assert.Equal(PomodoroPhase.ShortBreak, shortBreakStatus.Phase);
        Assert.Equal(TimeSpan.FromMinutes(7), shortBreakStatus.Remaining);
    }

    [Fact]
    public void UpdatingSettingsToDefaultRestoresDefaultFocusDuration()
    {
        var timer = new PomodoroTimer();
        var widgetController = new HomeScreenWidgetController(timer);

        widgetController.UpdateSettings(new PomodoroSettings(focusMinutes: 30, shortBreakMinutes: 7, longBreakMinutes: 20));
        var resetStatus = widgetController.UpdateSettings(PomodoroSettings.Default);

        Assert.Equal(TimeSpan.FromMinutes(25), resetStatus.Remaining);
        Assert.Equal(25, widgetController.GetSettings().FocusMinutes);
        Assert.Equal(5, widgetController.GetSettings().ShortBreakMinutes);
        Assert.Equal(15, widgetController.GetSettings().LongBreakMinutes);
    }

    [Fact]
    public void AdvanceWorksWhenTimerIsStopped()
    {
        var timer = new PomodoroTimer();
        var widgetController = new HomeScreenWidgetController(timer);

        var status = widgetController.Tick(TimeSpan.FromMinutes(1));

        Assert.False(status.IsRunning);
        Assert.Equal(PomodoroPhase.Focus, status.Phase);
        Assert.Equal(TimeSpan.FromMinutes(24), status.Remaining);
    }

    [Fact]
    public void ResetFocusedPeriodsTodaySetsCompletedCountToZero()
    {
        var clock = new FakeDateTimeProvider(new DateTimeOffset(2026, 5, 17, 9, 0, 0, TimeSpan.Zero));
        var timer = new PomodoroTimer(dateTimeProvider: clock);
        var widgetController = new HomeScreenWidgetController(timer);

        widgetController.StartTimer();
        widgetController.Tick(TimeSpan.FromMinutes(25));

        var status = widgetController.ResetFocusedPeriodsToday();

        Assert.Equal(0, status.CompletedFocusSessionsToday);
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
