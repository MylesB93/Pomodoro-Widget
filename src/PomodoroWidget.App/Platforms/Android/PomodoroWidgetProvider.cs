using Android.Appwidget;
using Android.Content;
using Android.Runtime;
using Android.Widget;
using PomodoroWidget.Core;

namespace PomodoroWidget.App;

[Register("com.companyname.pomodorowidget.app.PomodoroWidgetProvider")]
public sealed class PomodoroWidgetProvider : AppWidgetProvider
{
    public const string ActionStart = "com.companyname.pomodorowidget.app.action.START";
    public const string ActionStop = "com.companyname.pomodorowidget.app.action.STOP";

    public override void OnUpdate(Context context, AppWidgetManager appWidgetManager, int[] appWidgetIds)
    {
        UpdateAllWidgets(context, PomodoroWidgetHost.GetStatus(context));
    }

    public override void OnReceive(Context? context, Intent? intent)
    {
        base.OnReceive(context, intent);

        if (context is null || intent?.Action is null)
        {
            return;
        }

        switch (intent.Action)
        {
            case ActionStart:
                PomodoroWidgetHost.Start(context);
                break;
            case ActionStop:
                PomodoroWidgetHost.Stop(context);
                break;
        }
    }

    internal static void UpdateAllWidgets(Context context, PomodoroStatus status)
    {
        var appContext = context.ApplicationContext;
        var appWidgetManager = AppWidgetManager.GetInstance(appContext);
        var widgetComponent = new ComponentName(appContext, Java.Lang.Class.FromType(typeof(PomodoroWidgetProvider)));
        var appWidgetIds = appWidgetManager.GetAppWidgetIds(widgetComponent);

        foreach (var appWidgetId in appWidgetIds)
        {
            var remoteViews = BuildRemoteViews(appContext, status);
            appWidgetManager.UpdateAppWidget(appWidgetId, remoteViews);
        }
    }

    private static RemoteViews BuildRemoteViews(Context context, PomodoroStatus status)
    {
        var remoteViews = new RemoteViews(context.PackageName, Resource.Layout.pomodoro_widget);
        remoteViews.SetTextViewText(Resource.Id.phaseText, PhaseToDisplay(status.Phase));
        remoteViews.SetTextViewText(Resource.Id.remainingText, FormatRemaining(status.Remaining));
        remoteViews.SetTextViewText(Resource.Id.summaryText, $"{status.CompletedFocusSessionsToday} completed today");
        remoteViews.SetTextViewText(Resource.Id.stateText, status.IsRunning ? "Running" : "Stopped");
        remoteViews.SetOnClickPendingIntent(Resource.Id.startButton, BuildActionPendingIntent(context, ActionStart));
        remoteViews.SetOnClickPendingIntent(Resource.Id.stopButton, BuildActionPendingIntent(context, ActionStop));
        return remoteViews;
    }

    private static PendingIntent BuildActionPendingIntent(Context context, string action)
    {
        var intent = new Intent(context, typeof(PomodoroWidgetProvider)).SetAction(action);
        var requestCode = action.GetHashCode();
        return PendingIntent.GetBroadcast(context, requestCode, intent, PendingIntentFlags.Immutable | PendingIntentFlags.UpdateCurrent);
    }

    private static string PhaseToDisplay(PomodoroPhase phase) => phase switch
    {
        PomodoroPhase.Focus => "Focus",
        PomodoroPhase.ShortBreak => "Short Break",
        PomodoroPhase.LongBreak => "Long Break",
        _ => phase.ToString()
    };

    private static string FormatRemaining(TimeSpan remaining) => $"{(int)remaining.TotalMinutes:00}:{remaining.Seconds:00}";
}
