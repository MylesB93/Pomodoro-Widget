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
- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- Visual Studio 2022 with the **.NET Multi-platform App UI development** workload
- Android SDK + emulator configured in Visual Studio

### Steps
1. Open a terminal in the repository root:
   ```bash
   cd Pomodoro-Widget
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

## Run in emulator

1. Open `PomodoroWidget.slnx`.
2. Set **PomodoroWidget.App** as the startup project.
3. In the device dropdown, choose an Android emulator.
4. Press **F5** to build and launch the app in the emulator.
