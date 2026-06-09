using PomodoroWidget.Core;

namespace PomodoroWidget.App;

public partial class MainPage : ContentPage
{
#if ANDROID
    private readonly HomeScreenWidgetController _controller = PomodoroWidgetHost.Controller;
#else
    private readonly HomeScreenWidgetController _controller = new(new PomodoroTimer());
#endif
    private readonly IDispatcherTimer _uiTickTimer;
    private PomodoroStatus _status;

    public MainPage()
    {
        InitializeComponent();
#if ANDROID
        _status = PomodoroWidgetHost.GetStatus(GetAndroidContext());
#else
        _status = _controller.GetStatus();
#endif
        UpdateDisplay();
        SetSettingsInputValues(_controller.GetSettings());

        _uiTickTimer = Dispatcher.CreateTimer();
        _uiTickTimer.Interval = TimeSpan.FromSeconds(1);
        _uiTickTimer.Tick += OnUiTick;
        _uiTickTimer.Start();
    }

    private void OnStartClicked(object sender, EventArgs e)
    {
#if ANDROID
        _status = PomodoroWidgetHost.Start(GetAndroidContext());
#else
        _status = _controller.StartTimer();
#endif
        UpdateDisplay();
    }

    private void OnStopClicked(object sender, EventArgs e)
    {
#if ANDROID
        _status = PomodoroWidgetHost.Stop(GetAndroidContext());
#else
        _status = _controller.StopTimer();
#endif
        UpdateDisplay();
    }

    private void OnResetClicked(object sender, EventArgs e)
    {
#if ANDROID
        _status = PomodoroWidgetHost.Reset(GetAndroidContext());
#else
        _status = _controller.ResetTimer();
#endif
        UpdateDisplay();
    }

    private void OnAdvanceMinuteClicked(object sender, EventArgs e)
    {
        _status = _controller.Tick(TimeSpan.FromMinutes(1));
        UpdateDisplay();
    }

    private void OnSettingsClicked(object sender, EventArgs e)
    {
        SettingsPanel.IsVisible = !SettingsPanel.IsVisible;
    }

    private async void OnApplySettingsClicked(object sender, EventArgs e)
    {
        if (!int.TryParse(FocusMinutesEntry.Text, out var focusMinutes) || focusMinutes <= 0 ||
            !int.TryParse(ShortBreakMinutesEntry.Text, out var shortBreakMinutes) || shortBreakMinutes <= 0 ||
            !int.TryParse(LongBreakMinutesEntry.Text, out var longBreakMinutes) || longBreakMinutes <= 0)
        {
            await DisplayAlert("Invalid settings", "Please enter positive whole minutes for all settings.", "OK");
            return;
        }

        _status = _controller.UpdateSettings(new PomodoroSettings(
            focusMinutes: focusMinutes,
            shortBreakMinutes: shortBreakMinutes,
            longBreakMinutes: longBreakMinutes));
        SetSettingsInputValues(_controller.GetSettings());
        UpdateDisplay();
    }

    private void OnResetToDefaultSettingsClicked(object sender, EventArgs e)
    {
        _status = _controller.UpdateSettings(PomodoroSettings.Default);
        SetSettingsInputValues(_controller.GetSettings());
        UpdateDisplay();
    }

    private async void OnResetFocusedPeriodsClicked(object sender, EventArgs e)
    {
        var confirmed = await DisplayAlert(
            "Reset Focused Periods",
            "Are you sure you want to reset focused periods back to 0?",
            "Yes",
            "No");

        if (!confirmed)
        {
            return;
        }

        _status = _controller.ResetFocusedPeriodsToday();
        UpdateDisplay();
    }

    private void OnUiTick(object? sender, EventArgs e)
    {
#if ANDROID
        // The widget host's background ticker already advances the shared timer every second.
        // Just refresh the display from the current timer state to avoid double-advancing.
        _status = PomodoroWidgetHost.GetStatus(GetAndroidContext());
        UpdateDisplay();
#else
        if (!_status.IsRunning)
        {
            return;
        }

        _status = _controller.Tick(TimeSpan.FromSeconds(1));
        UpdateDisplay();
#endif
    }

#if ANDROID
    private static Android.Content.Context GetAndroidContext() =>
        Android.App.Application.Context
        ?? throw new InvalidOperationException("Android application context is unavailable.");
#endif

    private void UpdateDisplay()
    {
        StateValueLabel.Text = _status.IsRunning ? "Running" : "Stopped";
        PhaseValueLabel.Text = _status.Phase.ToString();
        RemainingValueLabel.Text = _status.Remaining.ToString(@"mm\:ss");
        SummaryValueLabel.Text = _controller.BuildDailySummaryText();
    }

    private void SetSettingsInputValues(PomodoroSettings settings)
    {
        FocusMinutesEntry.Text = settings.FocusMinutes.ToString();
        ShortBreakMinutesEntry.Text = settings.ShortBreakMinutes.ToString();
        LongBreakMinutesEntry.Text = settings.LongBreakMinutes.ToString();
    }
}
