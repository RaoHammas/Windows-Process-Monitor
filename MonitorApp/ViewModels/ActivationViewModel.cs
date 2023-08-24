using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MaterialDesignThemes.Wpf;
using MonitorApp.DataAccess.Services;
using MonitorApp.Helpers;

namespace MonitorApp.ViewModels;

public partial class ActivationViewModel : ObservableObject, IActivationViewModel
{
    private readonly IDbService _dbService;

    [ObservableProperty] string _userInputKey = string.Empty;
    [ObservableProperty] string _userEmail = string.Empty;
    [ObservableProperty] bool _isActivationSuccessful;
    [ObservableProperty] private ISnackbarMessageQueue _snackbarMessageQueue;
    private readonly IProcessHelper _processHelper;

    public ActivationViewModel(
        IDbService dbService,
        ISnackbarMessageQueue snackbarMessageQueue,
        IProcessHelper processHelper
        )
    {
        _dbService = dbService;
        _snackbarMessageQueue = snackbarMessageQueue;
        _processHelper = processHelper;
    }

    [RelayCommand]
    public async Task Activate()
    {
        if (string.IsNullOrEmpty(UserInputKey) || string.IsNullOrEmpty(UserEmail))
        {
            SnackbarMessageQueue.Enqueue("Provide valid email and key.");
            return;
        }

        if (KeysHelper.IsValidKey(UserInputKey))
        {
            var resp = await _dbService.ActivateAsync(UserInputKey, UserEmail);
            if (resp > 0)
            {
                IsActivationSuccessful = true;
            }
            else
            {
                SnackbarMessageQueue.Enqueue("Invalid Key!");
            }
        }
        else
        { 
            SnackbarMessageQueue.Enqueue("Invalid Key!");
        }
    }

    [RelayCommand]
    public void GetKey()
    {
        _processHelper.OpenActivationKeysLinkInBrowser();
    }
}