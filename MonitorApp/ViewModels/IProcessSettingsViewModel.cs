using MonitorApp.Domain.Models;

namespace MonitorApp.ViewModels;

public interface IProcessSettingsViewModel
{
    public ProcessToMonitor ProcessMonitoring { get; set; }
}