using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MaterialDesignThemes.Wpf;
using MonitorApp.DataAccess.Services;
using MonitorApp.Domain.Models;

namespace MonitorApp.ViewModels;

public partial class EmailSettingsViewModel : ObservableObject, IEmailSettingsViewModel
{
    private readonly IDbService _dbService;
    [ObservableProperty] private ISnackbarMessageQueue _snackbarQueue;
    [ObservableProperty] private EmailDetails _emailDetails;

    public EmailSettingsViewModel(IDbService dbService, ISnackbarMessageQueue snackbarMessageQueue)
    {
        _dbService = dbService;
        _snackbarQueue = snackbarMessageQueue;
        EmailDetails = new EmailDetails();
    }

    [RelayCommand]
    public async Task SaveAsync()
    {
        var resp = await _dbService.SaveEmailDetailsAsync(EmailDetails);
        EmailDetails.Id = resp;
        if (resp > 0)
        {
            SnackbarQueue.Enqueue("Saved successfully.");
            return;
        }

        SnackbarQueue.Enqueue("Failed to save.");
    }
}