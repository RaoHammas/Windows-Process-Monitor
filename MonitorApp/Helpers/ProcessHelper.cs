using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using MonitorApp.Domain.Models;

namespace MonitorApp.Helpers;

public class ProcessHelper : IProcessHelper
{
    ///<inheritdoc/>
    public List<ProcessToMonitor>? GetProcesses(string processName)
    {
        var running = Process.GetProcesses().Where(x => x.Id > 0 && x.ProcessName == processName).ToList();
        if (running.Any())
        {
            var list = new List<ProcessToMonitor>();
            foreach (Process runner in running)
            {
                var process = MapProcess(runner);
                if (process != null)
                {
                    list.Add(process);
                }
            }

            return list;
        }

        return null;
    }

    ///<inheritdoc/>
    public IEnumerable<ProcessToMonitor> GetAllRunning()
    {
        var processCollection = Process.GetProcesses().Where(x => x.Id > 0).DistinctBy(x => x.ProcessName);
        foreach (var p in processCollection)
        {
            var process = MapProcess(p);
            if (process != null)
            {
                yield return process;
            }
        }
    }

    ///<inheritdoc/>
    public bool IsRunning(ProcessToMonitor process)
    {
        var foundProcesses = Process.GetProcessesByName(process.DisplayName);
        return foundProcesses.Length >= 1;
    }

    ///<inheritdoc/>
    public void KillProcess(ProcessToMonitor process)
    {
        var runningProcess = Process.GetProcessesByName(process.DisplayName);
        foreach (var p in runningProcess)
        {
            p.Kill();
        }
    }


    ///<inheritdoc/>
    public ProcessToMonitor? TryStarting(ProcessToMonitor process)
    {
        var newProcess = new Process();
        newProcess.StartInfo.FileName = process.FullPath;
        var status = newProcess.Start();
        if (status)
        {
            return MapProcess(newProcess);
        }

        return null;
    }


    ///<inheritdoc/>
    public int GetCurrentProcessSessionId()
    {
        return Process.GetCurrentProcess().SessionId;
    }

    private ProcessToMonitor? MapProcess(Process process)
    {
        var monitor = new ProcessToMonitor
        {
            PID = process.Id,
            SessionId = process.SessionId,
            DisplayName = process.ProcessName,
            HasAWindow = !string.IsNullOrEmpty(process.MainWindowTitle),
            FullPath = GetMainModuleFilePath(process.Id),
            Status = ProcessStatus.Running,
            StartedAt = DateTime.UtcNow,
            StoppedAt = null
        };

        if (string.IsNullOrEmpty(monitor.FullPath))
        {
            return null;
        }

        monitor.ProcessName = process.MainModule!.ModuleName!;
        return monitor;
    }

    private string GetMainModuleFilePath(int processId)
    {
        string wmiQueryString = "SELECT ProcessId, ExecutablePath FROM Win32_Process WHERE ProcessId = " + processId;
        using var searcher = new ManagementObjectSearcher(wmiQueryString);
        using var results = searcher.Get();
        var mo = results.Cast<ManagementObject>().FirstOrDefault();

        return mo != null ? (string)mo["ExecutablePath"] : string.Empty;
    }
}