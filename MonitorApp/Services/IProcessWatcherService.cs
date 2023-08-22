using System;
using System.Collections.Generic;

namespace MonitorApp.Services;

public interface IProcessWatcherService
{
    Action<string, string>? ProcessStatusChanged { get; set; }
    void StartMonitoringAll(List<string> processes);
    void StartMonitoring(string processName);
    void StopMonitoring(string processName);
    void PauseMonitoringAll();
    void ResumeMonitoringAll();
    void Dispose();
}