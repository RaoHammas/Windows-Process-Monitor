using System;
using System.Windows.Threading;

namespace MonitorApp.Controls;

public class SnackbarMessage
{
    public string Message { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public Action<SnackbarMessage>? TimeOut { get; set; }

    public void Start()
    {
        var timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(5)
        };

        timer.Tick += (_, _) =>
        {
            timer.Stop();
            TimeOut?.Invoke(this);
        };

        timer.Start();
    }
}