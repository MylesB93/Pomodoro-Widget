namespace PomodoroWidget.Core;

public interface IWidgetStateSink
{
    void Update(PomodoroStatus status);
}
