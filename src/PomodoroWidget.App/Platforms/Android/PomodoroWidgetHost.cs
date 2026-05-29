using PomodoroWidget.Core;

namespace PomodoroWidget.App;

internal static class PomodoroWidgetHost
{
    private static readonly object SyncRoot = new();
    private static readonly AppWidgetStateSink WidgetStateSink = new();
    private static readonly PomodoroTimer Timer = new(widgetStateSink: WidgetStateSink);
    internal static readonly HomeScreenWidgetController Controller = new(Timer);
    private static readonly TimeSpan TickInterval = TimeSpan.FromSeconds(1);

    private static System.Threading.Timer? _ticker;
    private static Android.Content.Context? _appContext;

    public static PomodoroStatus GetStatus(Android.Content.Context context)
    {
        lock (SyncRoot)
        {
            EnsureContext(context);
            return Timer.GetStatus();
        }
    }

    public static PomodoroStatus Start(Android.Content.Context context)
    {
        lock (SyncRoot)
        {
            EnsureContext(context);
            var status = Controller.StartTimer();
            EnsureTicker();
            return status;
        }
    }

    public static PomodoroStatus Stop(Android.Content.Context context)
    {
        lock (SyncRoot)
        {
            EnsureContext(context);
            var status = Controller.StopTimer();
            StopTicker();
            return status;
        }
    }

    private static void EnsureContext(Android.Content.Context context)
    {
        _appContext = context.ApplicationContext;
        WidgetStateSink.SetContext(_appContext);
    }

    private static void EnsureTicker()
    {
        _ticker ??= new System.Threading.Timer(
            _ =>
            {
                lock (SyncRoot)
                {
                    if (_appContext is null || !Timer.IsRunning)
                    {
                        return;
                    }

                    Controller.Tick(TickInterval);
                }
            },
            null,
            TickInterval,
            TickInterval);
    }

    private static void StopTicker()
    {
        _ticker?.Dispose();
        _ticker = null;
    }
}
