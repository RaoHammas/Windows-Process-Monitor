using System.Collections.Generic;
using MonitorApp.Domain.Models;

namespace MonitorApp.Helpers;

public interface IProcessHelper
{
    /// <summary>
    /// Get's a process by it's name
    /// </summary>
    /// <param name="processName">Name of Process</param>
    /// <returns>Process or Null</returns>
    List<ProcessToMonitor>? GetProcesses(string processName);

    /// <summary>
    /// Gets all running processes and applications
    /// </summary>
    /// <returns>IEnumerable of ProcessToMonitor object</returns>
    IEnumerable<ProcessToMonitor> GetAllRunning();

    /// <summary>
    /// Checks if a Process is running
    /// </summary>
    /// <returns>True if Running else False</returns>
    bool IsRunning(ProcessToMonitor process);

    /// <summary>
    /// Try to restart a process
    /// </summary>
    /// <returns>New ProcessToMonitor after restarting or Null if failed to restart</returns>
    ProcessToMonitor? TryStarting(ProcessToMonitor process);

    /// <summary>
    /// Exists a process and kill it on OS level
    /// </summary>
    /// <param name="process"></param>
    void KillProcess(ProcessToMonitor process);

    /// <summary>
    /// Get current session Id of the application
    /// </summary>
    /// <returns>Session id of current process</returns>
    int GetCurrentProcessSessionId();
}