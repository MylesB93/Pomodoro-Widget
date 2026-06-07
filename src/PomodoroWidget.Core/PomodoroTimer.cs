using System.Collections.ObjectModel;

namespace PomodoroWidget.Core;

public sealed class PomodoroTimer
{
    private PomodoroSettings _settings;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IWidgetStateSink? _widgetStateSink;
    private readonly Dictionary<DateOnly, int> _completedFocusSessionsByDate = new();

    private PomodoroPhase _currentPhase = PomodoroPhase.Focus;
    private TimeSpan _remaining;
    private int _completedFocusSessionsInCycle;

    public PomodoroTimer(
        PomodoroSettings? settings = null,
        IDateTimeProvider? dateTimeProvider = null,
        IWidgetStateSink? widgetStateSink = null)
    {
        _settings = settings ?? PomodoroSettings.Default;
        _dateTimeProvider = dateTimeProvider ?? new SystemDateTimeProvider();
        _widgetStateSink = widgetStateSink;
        _remaining = GetDurationForPhase(_currentPhase);
    }

    public bool IsRunning { get; private set; }

    public IReadOnlyDictionary<DateOnly, int> CompletedFocusSessionsByDate => new ReadOnlyDictionary<DateOnly, int>(_completedFocusSessionsByDate);

    public PomodoroStatus Start()
    {
        IsRunning = true;
        return PublishStatus();
    }

    public PomodoroStatus Stop()
    {
        IsRunning = false;
        return PublishStatus();
    }

    public PomodoroStatus Reset()
    {
        IsRunning = false;
        _remaining = GetDurationForPhase(_currentPhase);
        return PublishStatus();
    }

    public PomodoroStatus Advance(TimeSpan elapsed)
    {
        if (!IsRunning || elapsed <= TimeSpan.Zero)
        {
            return PublishStatus();
        }

        while (elapsed >= _remaining)
        {
            elapsed -= _remaining;
            TransitionToNextPhase();
        }

        _remaining -= elapsed;
        return PublishStatus();
    }

    public PomodoroStatus GetStatus() => PublishStatus();

    public PomodoroSettings GetSettings() => _settings;

    public PomodoroStatus UpdateSettings(PomodoroSettings settings)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _remaining = GetDurationForPhase(_currentPhase);
        return PublishStatus();
    }

    private void TransitionToNextPhase()
    {
        if (_currentPhase == PomodoroPhase.Focus)
        {
            IncrementCompletedFocusSessions();
            _completedFocusSessionsInCycle++;

            if (_completedFocusSessionsInCycle >= _settings.FocusSessionsBeforeLongBreak)
            {
                _completedFocusSessionsInCycle = 0;
                _currentPhase = PomodoroPhase.LongBreak;
            }
            else
            {
                _currentPhase = PomodoroPhase.ShortBreak;
            }
        }
        else
        {
            _currentPhase = PomodoroPhase.Focus;
        }

        _remaining = GetDurationForPhase(_currentPhase);
    }

    private void IncrementCompletedFocusSessions()
    {
        var today = DateOnly.FromDateTime(_dateTimeProvider.Now.LocalDateTime.Date);
        _completedFocusSessionsByDate[today] = _completedFocusSessionsByDate.GetValueOrDefault(today) + 1;
    }

    private TimeSpan GetDurationForPhase(PomodoroPhase phase) => phase switch
    {
        PomodoroPhase.Focus => TimeSpan.FromMinutes(_settings.FocusMinutes),
        PomodoroPhase.ShortBreak => TimeSpan.FromMinutes(_settings.ShortBreakMinutes),
        PomodoroPhase.LongBreak => TimeSpan.FromMinutes(_settings.LongBreakMinutes),
        _ => throw new InvalidOperationException($"Unsupported phase: {phase}")
    };

    private PomodoroStatus PublishStatus()
    {
        var today = DateOnly.FromDateTime(_dateTimeProvider.Now.LocalDateTime.Date);
        var status = new PomodoroStatus(
            IsRunning,
            _currentPhase,
            _remaining,
            _completedFocusSessionsByDate.GetValueOrDefault(today));

        _widgetStateSink?.Update(status);
        return status;
    }
}
