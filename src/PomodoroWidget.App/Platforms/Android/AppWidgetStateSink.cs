using Android.Content;
using PomodoroWidget.Core;

namespace PomodoroWidget.App;

internal sealed class AppWidgetStateSink : IWidgetStateSink
{
    private Context? _context;

    public void SetContext(Context context)
    {
        _context = context.ApplicationContext;
    }

    public void Update(PomodoroStatus status)
    {
        if (_context is null)
        {
            return;
        }

        PomodoroWidgetProvider.UpdateAllWidgets(_context, status);
    }
}
