using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using MonitorApp.Controls;
using MonitorApp.DataAccess.Services;
using MonitorApp.Domain.Models;
using MonitorApp.Helpers;
using MonitorApp.Messages;
using MonitorApp.Services;

namespace MonitorApp.ViewModels;

public partial class ShellViewModel : ObservableObject, IShell
{
    private readonly int _currentSessionId;
    private readonly IProcessHelper _processHelper;
    private readonly IDbService _dbService;
    private readonly IDialogService _dialogService;
    private readonly IEmailSettingsViewModel _emailSettingsViewModel;
    private readonly IEmailService _emailService;
    [ObservableProperty] private ISnackbarControlViewModel _snackbarControlViewModel;
    private readonly IProcessWatcherService _processWatcherService;
    private readonly IActivationViewModel _activationViewModel;


    [ObservableProperty] private ObservableCollection<ProcessToMonitor> _allRunningProcesses = new();
    [ObservableProperty] private ObservableCollection<ProcessToMonitor> _allRunningProcessesShown = new();
    [ObservableProperty] private ObservableCollection<ProcessToMonitor> _monitoringProcesses = new();

    [ObservableProperty] private bool _isMonitoringOn;
    [ObservableProperty] private bool _showProcessesWithWindowsOnly;
    [ObservableProperty] private bool _showForCurrentUserOnly;
    [ObservableProperty] private IProcessSettingsViewModel _processSettingsViewModel;


    public ShellViewModel(
        IProcessHelper processHelper,
        IDbService dbService,
        IProcessSettingsViewModel processSettingsViewModel,
        IDialogService dialogService,
        IEmailSettingsViewModel emailSettingsViewModel,
        IEmailService emailService,
        ISnackbarControlViewModel snackbarControlViewModel,
        IProcessWatcherService processWatcherService,
        IActivationViewModel activationViewModel
    )
    {
        _processHelper = processHelper;
        _dbService = dbService;
        _processSettingsViewModel = processSettingsViewModel;
        _dialogService = dialogService;
        _emailSettingsViewModel = emailSettingsViewModel;
        _emailService = emailService;
        _snackbarControlViewModel = snackbarControlViewModel;
        _processWatcherService = processWatcherService;
        _activationViewModel = activationViewModel;
        _currentSessionId = processHelper.GetCurrentProcessSessionId();


        ShowProcessesWithWindowsOnly = false;
        _processWatcherService.ProcessStatusChanged += ProcessStatusChanged;
        ListenToMessages();


        LoadAllRunningProcessesCommand.ExecuteAsync(null);
        LoadAllMonitoringProcessesCommand.ExecuteAsync(null);
    }


    private void ListenToMessages()
    {
        WeakReferenceMessenger.Default.Register<RefreshMonitoringAppsMessage>(this,
            (_, _) => { LoadAllMonitoringProcessesCommand.ExecuteAsync(null); });
    }

    private void ProcessStatusChanged(string processName, string status)
    {
        //difference between processName and DisplayName is that processName contains extension like notepad.exe
        var process = MonitoringProcesses.FirstOrDefault(x => x.ProcessName == processName);
        if (process != null)
        {
            if (status == "Stopped")
            {
                var processRunning = _processHelper.GetProcesses(process.DisplayName);
                if (processRunning != null)
                {
                    //means one instance is still running
                    process.NoOfInstances = processRunning.Count;
                    _dbService.SaveAsync(process);
                    return;
                }


                process.StoppedAt = DateTime.UtcNow;
                process.Status = ProcessStatus.Stopped;
                process.NoOfInstances = 0;

                _dbService.SaveAsync(process);
                SnackbarControlViewModel.Show($"{process.DisplayName} has stopped running");

                if (process.SendAlertEmail)
                {
                    var metadata = new EmailMetadata
                    {
                        Subject = $"{process.DisplayName} has stopped working",
                        ToAddress = _emailSettingsViewModel.EmailDetails.Email,
                        Body = GetEmailBody(process, "has stopped working")
                    };

                    var emailResp = _emailService.Send(metadata);

                    SnackbarControlViewModel.Show($"{emailResp}");
                }

                if (process is { TryRestarting: true, RestartingAttempts: > 0 })
                {
                    _ = TryStartingApp(process);
                }
            }
            else if (status == "Started")
            {
                var processesRunning = _processHelper.GetProcesses(process.DisplayName);
                if (processesRunning != null)
                {
                    var processStarted = processesRunning[0];
                    processStarted.NoOfInstances = processesRunning.Count;
                    _ = ProcessStarted(process, processStarted);
                }
            }
        }
    }

    private async Task ProcessStarted(ProcessToMonitor process, ProcessToMonitor processStarted)
    {
        process.PID = processStarted.PID;
        process.StartedAt = DateTime.UtcNow;
        process.SessionId = processStarted.SessionId;
        process.NoOfInstances = processStarted.NoOfInstances;
        process.Status = ProcessStatus.Monitoring;
        process.ProcessName = processStarted.ProcessName;
        process.DisplayName = processStarted.DisplayName;
        process.HasAWindow = processStarted.HasAWindow;
        process.FullPath = processStarted.FullPath;

        await _dbService.SaveAsync(process);
        SnackbarControlViewModel.Show($"{processStarted.DisplayName} has started running again");

        if (process.SendAlertEmail)
        {
            var metadata = new EmailMetadata
            {
                Subject = $"{process.DisplayName} has started working again",
                ToAddress = _emailSettingsViewModel.EmailDetails.Email,
                Body = GetEmailBody(process, "has started working again")
            };

            var emailResp = await _emailService.Send(metadata);

            SnackbarControlViewModel.Show(emailResp);
        }
    }


    private async Task TryStartingApp(ProcessToMonitor process)
    {
        for (var i = 1; i <= process.RestartingAttempts; i++)
        {
            var processStarted = _processHelper.TryStarting(process);
            if (processStarted is not null)
            {
                break;
            }

            if (process.SendAlertEmail)
            {
                var metadata = new EmailMetadata
                {
                    Subject = $"{process.DisplayName} has failed to start",
                    ToAddress = _emailSettingsViewModel.EmailDetails.Email,
                    Body = GetEmailBody(process, "has failed to start")
                };

                var emailResp = await _emailService.Send(metadata);

                SnackbarControlViewModel.Show(emailResp);
            }
        }
    }

    [RelayCommand]
    public async Task LoadAllMonitoringProcessesAsync()
    {
        IsMonitoringOn = false;

        MonitoringProcesses.Clear();
        MonitoringProcesses = new ObservableCollection<ProcessToMonitor>(await _dbService.GetAllAsync());

        SnackbarControlViewModel.Show("Processes monitoring list loading completed");

        //Check on load the status of each process that is being monitored
        foreach (var process in MonitoringProcesses)
        {
            var processRunning = _processHelper.GetProcesses(process.DisplayName);
            if (processRunning != null)
            {
                //means one instance is still running
                process.NoOfInstances = processRunning.Count;
                process.Status = ProcessStatus.Monitoring;
                await _dbService.SaveAsync(process);
            }
            else
            {
                process.Status = ProcessStatus.Stopped;
                process.StoppedAt = DateTime.UtcNow;
                process.NoOfInstances = 0;
                
                await _dbService.SaveAsync(process);
            }

        }

        _processWatcherService.StartMonitoringAll(MonitoringProcesses.Select(x => x.ProcessName).ToList());
        IsMonitoringOn = true;
    }

    [RelayCommand]
    public async Task LoadAllRunningProcesses()
    {
        AllRunningProcesses = new ObservableCollection<ProcessToMonitor>();
        await Task.Run(() =>
        {
            foreach (var process in _processHelper.GetAllRunning())
            {
                AllRunningProcesses.Add(process);
            }

            ApplyFilter();
        });

        SnackbarControlViewModel.Show("System apps loading completed");
    }


    [RelayCommand]
    public async Task MonitorApp(ProcessToMonitor? process)
    {
        if (process is not null)
        {
            if (MonitoringProcesses.FirstOrDefault(x => x.ProcessName == process.ProcessName) != null)
            {
                SnackbarControlViewModel.Show("This process is already being monitored");
                return;
            }

            _processWatcherService.StartMonitoring(process.ProcessName);
            process.StartedAt = DateTime.UtcNow;
            process.Status = ProcessStatus.Monitoring;
            process.NoOfInstances = 1;

            var savedId = await _dbService.SaveAsync(process);
            if (savedId > 0)
            {
                process.Id = savedId;

                MonitoringProcesses.Add(process);
                SnackbarControlViewModel.Show("App is successfully added to the monitoring list.");
            }
            else
            {
                SnackbarControlViewModel.Show("FAILED: Something happened at server side. Please try again...");
            }
        }
    }

    [RelayCommand]
    public async Task StopMonitoringApp(ProcessToMonitor? process)
    {
        if (process is not null)
        {
            _processWatcherService.StartMonitoring(process.ProcessName);
            if (await _dbService.RemoveAsync(process.Id))
            {
                MonitoringProcesses.Remove(process);
                SnackbarControlViewModel.Show("App is successfully removed from the Monitoring list.");
            }
            else
            {
                SnackbarControlViewModel.Show("FAILED: Something happened at server side. Please try again...");
            }
        }
    }


    [RelayCommand]
    public void ApplyFilter()
    {
        IEnumerable<ProcessToMonitor> query = AllRunningProcesses;
        if (ShowProcessesWithWindowsOnly)
        {
            query = query.Where(x => x.HasAWindow);
        }

        if (ShowForCurrentUserOnly)
        {
            query = query.Where(x => x.SessionId == _currentSessionId);
        }

        AllRunningProcessesShown = new ObservableCollection<ProcessToMonitor>(query);
    }


    [RelayCommand]
    public async Task OpenAppSettings(ProcessToMonitor process)
    {
        ProcessSettingsViewModel.ProcessMonitoring = process.GetClone();
        await _dialogService.ShowProcessSettings(ProcessSettingsViewModel);
    }

    [RelayCommand]
    public async Task OpenEmailSettings()
    {
        var key = await _dbService.GetActivationKeyAsync();
        if (KeysHelper.IsValidKey(key))
        {
            var mail = await _dbService.GetEmailDetailsAsync();
            if (mail != null)
            {
                _emailSettingsViewModel.EmailDetails = mail;
            }

            await _dialogService.ShowEmailSettings(_emailSettingsViewModel);
        }
        else
        {
            await _dialogService.ShowActivation(_activationViewModel);
        }

    }
   
    [RelayCommand]
    public void StartProcess(ProcessToMonitor process)
    {
        _processHelper.TryStarting(process);
    }

    [RelayCommand]
    public void KillProcess(ProcessToMonitor process)
    {
        _processHelper.KillProcess(process);
    }

    [RelayCommand]
    public void ToggleMonitoringPauseResume()
    {
        if (IsMonitoringOn)
        {
            IsMonitoringOn = false;
            _processWatcherService.PauseMonitoringAll();
            SnackbarControlViewModel.Show("Paused monitoring");
        }
        else
        {
            IsMonitoringOn = true;
            _processWatcherService.ResumeMonitoringAll();
            SnackbarControlViewModel.Show("Resumed monitoring");
        }
    }

    private string GetEmailBody(ProcessToMonitor process, string msg)
    {
        var body = $"""
                    Hi,
                     
                    {process.DisplayName} {msg}.

                    Event details are below:
                    ---------------------------------------
                    Process Name: {process.ProcessName}
                    Name: {process.DisplayName}
                    PID: {process.PID},
                    Stop Time: {process.StoppedAt}
                    Status: {process.Status}
                    Auto Start: {process.TryRestarting}
                    ---------------------------------------

                    Sent from, Process Monitor app.
                    Thanks.
                    """;
        return body;
    }

    public void Dispose()
    {
        try
        {
            _processWatcherService.Dispose();
        }
        catch (Exception)
        {
            // ignored
        }
    }
}