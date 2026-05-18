# Pomodoro-Widget

Core pomodoro logic for a .NET MAUI widget/app scenario.

## Implemented behavior
- Focus timer defaults to **25 minutes**.
- Short break defaults to **5 minutes** after each focus session.
- Long break defaults to **15 minutes** after every **4** completed focus sessions.
- Completed focus sessions are tracked per day.
- A widget-facing controller can start/stop the timer and provide daily summary text.

## Run locally

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download)

### Steps
1. Open a terminal in the repository root:
   ```bash
   cd /home/runner/work/Pomodoro-Widget/Pomodoro-Widget
   ```
2. Restore dependencies:
   ```bash
   dotnet restore PomodoroWidget.slnx
   ```
3. Build the solution:
   ```bash
   dotnet build PomodoroWidget.slnx
   ```
4. Run tests:
   ```bash
   dotnet test PomodoroWidget.slnx
   ```

> Note: this repository currently contains the core timer library and tests.
