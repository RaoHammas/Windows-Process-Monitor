using System;
using System.Collections.Generic;
using System.Management;
using System.Text;

namespace MonitorApp.Services
{
    public class ProcessWatcherService : IDisposable, IProcessWatcherService
    {
        public Action<string, string>? ProcessStatusChanged { get; set; }
        private readonly List<string> _monitoredProcesses = new();
        private ManagementEventWatcher? _watcherDeletion;
        private ManagementEventWatcher? _watcherCreation;

        public void StartMonitoringAll(List<string> processes)
        {
            foreach (var process in processes)
            {
                if (!_monitoredProcesses.Contains(process))
                {
                    _monitoredProcesses.Add(process);
                }
            }

            RestartEventWatchers();
        }

        public void StartMonitoring(string processName)
        {
            if (!_monitoredProcesses.Contains(processName))
            {
                _monitoredProcesses.Add(processName);
                RestartEventWatchers();
            }
        }

        public void StopMonitoring(string processName)
        {
            if (_monitoredProcesses.Contains(processName))
            {
                _monitoredProcesses.Remove(processName);

                if (_monitoredProcesses.Count == 0)
                {
                    Dispose();
                }
                else
                {
                    RestartEventWatchers();
                }
            }
        }

        public void PauseMonitoringAll()
        {
            if (_watcherDeletion != null)
            {
                _watcherDeletion.Stop();
                _watcherDeletion.Dispose();
                _watcherDeletion = null;
            }

            if (_watcherCreation != null)
            {
                _watcherCreation.Stop();
                _watcherCreation.Dispose();
                _watcherCreation = null;
            }
        }

        public void ResumeMonitoringAll()
        {
            RestartEventWatchers();
        }

        private void StartDeletionEventWatcher()
        {
            try
            {
                var queryBuilder = new StringBuilder();

                foreach (var processName in _monitoredProcesses)
                {
                    queryBuilder.Append($"TargetInstance.Name = '{processName}' OR ");
                }

                var queryString =
                    $"SELECT * FROM __InstanceDeletionEvent WITHIN 1 WHERE TargetInstance ISA 'Win32_Process' AND ({queryBuilder.ToString().TrimEnd(" OR ".ToCharArray())})";

                var query = new WqlEventQuery(queryString);
                _watcherDeletion = new ManagementEventWatcher(new ManagementScope("\\\\.\\root\\CIMV2"), query);
                _watcherDeletion.EventArrived += WatcherOnDeletionEventArrived;

                _watcherDeletion.Start();
            }
            catch (Exception ex)
            {
                // Handle the exception
            }
        }

        private void StartCreationEventWatcher()
        {
            try
            {
                var queryBuilder = new StringBuilder();

                foreach (var processName in _monitoredProcesses)
                {
                    queryBuilder.Append($"TargetInstance.Name = '{processName}' OR ");
                }

                var queryString =
                    $"SELECT * FROM __InstanceCreationEvent WITHIN 1 WHERE TargetInstance ISA 'Win32_Process' AND ({queryBuilder.ToString().TrimEnd(" OR ".ToCharArray())})";

                var query = new WqlEventQuery(queryString);
                _watcherCreation = new ManagementEventWatcher(new ManagementScope("\\\\.\\root\\CIMV2"), query);
                _watcherCreation.EventArrived += WatcherOnCreationEventArrived;

                _watcherCreation.Start();
            }
            catch (Exception ex)
            {
                // Handle the exception
            }
        }

        private void RestartEventWatchers()
        {
            PauseMonitoringAll();

            if (_monitoredProcesses.Count > 0)
            {
                StartDeletionEventWatcher();
                StartCreationEventWatcher();
            }
        }

        private void WatcherOnDeletionEventArrived(object sender, EventArrivedEventArgs e)
        {
            try
            {
                if (e.NewEvent["TargetInstance"] is ManagementBaseObject eventType)
                {
                    var processName = eventType["Name"].ToString();
                    if (processName != null)
                    {
                        ProcessStatusChanged?.Invoke(processName, "Stopped");
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle the exception
            }
        }

        private void WatcherOnCreationEventArrived(object sender, EventArrivedEventArgs e)
        {
            try
            {
                if (e.NewEvent["TargetInstance"] is ManagementBaseObject eventType)
                {
                    var processName = eventType["Name"].ToString();
                    if (processName != null)
                    {
                        ProcessStatusChanged?.Invoke(processName, "Started");
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle the exception
            }
        }

        public void Dispose()
        {
            PauseMonitoringAll();
            _monitoredProcesses.Clear();
        }
    }
}