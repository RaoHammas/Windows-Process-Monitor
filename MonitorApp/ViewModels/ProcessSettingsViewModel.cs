using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using MaterialDesignThemes.Wpf;
using MonitorApp.DataAccess.Services;
using MonitorApp.Domain.Models;
using MonitorApp.Messages;

namespace MonitorApp.ViewModels;

public partial class ProcessSettingsViewModel : ObservableObject, IProcessSettingsViewModel
{
    private readonly IDbService _dbService;
    [ObservableProperty] private ISnackbarMessageQueue _snackbarQueue;
    [ObservableProperty] private ProcessToMonitor _processMonitoring;


    public ProcessSettingsViewModel(IDbService dbService, ISnackbarMessageQueue snackbarMessageQueue)
    {
        _dbService = dbService;
        _snackbarQueue = snackbarMessageQueue;
        _processMonitoring = new ProcessToMonitor();


    }

    [RelayCommand]
    public async Task SaveSettings()
    {
        SnackbarQueue.Enqueue(await _dbService.SaveAsync(ProcessMonitoring) > 0
            ? "Settings Saved successfully!"
            : "Settings Failed to save. Try again...");

        //=> shell viewmodel
        WeakReferenceMessenger.Default.Send(new RefreshMonitoringAppsMessage());
    }
}