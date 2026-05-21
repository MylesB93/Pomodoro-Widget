using PomodoroWidget.Core;

namespace PomodoroWidget.App;

public partial class MainPage : ContentPage
{
    private readonly HomeScreenWidgetController _controller = new(new PomodoroTimer());
    private readonly IDispatcherTimer _uiTickTimer;
    private PomodoroStatus _status;

    public MainPage()
    {
        InitializeComponent();
        _status = _controller.StopTimer();
        UpdateDisplay();

        _uiTickTimer = Dispatcher.CreateTimer();
        _uiTickTimer.Interval = TimeSpan.FromSeconds(1);
        _uiTickTimer.Tick += OnUiTick;
        _uiTickTimer.Start();
    }

    private void OnStartClicked(object sender, EventArgs e)
    {
        _status = _controller.StartTimer();
        UpdateDisplay();
    }

    private void OnStopClicked(object sender, EventArgs e)
    {
        _status = _controller.StopTimer();
        UpdateDisplay();
    }

    private void OnAdvanceMinuteClicked(object sender, EventArgs e)
    {
        _status = _controller.Tick(TimeSpan.FromMinutes(1));
        UpdateDisplay();
    }

    private void OnUiTick(object? sender, EventArgs e)
    {
        if (!_status.IsRunning)
        {
            return;
        }

        _status = _controller.Tick(TimeSpan.FromSeconds(1));
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        StateValueLabel.Text = _status.IsRunning ? "Running" : "Stopped";
        PhaseValueLabel.Text = _status.Phase.ToString();
        RemainingValueLabel.Text = _status.Remaining.ToString(@"mm\:ss");
        SummaryValueLabel.Text = _controller.BuildDailySummaryText();
    }
}
