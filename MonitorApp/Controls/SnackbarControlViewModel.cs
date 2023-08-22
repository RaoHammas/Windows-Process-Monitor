using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace MonitorApp.Controls;

public partial class SnackbarControlViewModel : ObservableObject, ISnackbarControlViewModel
{
    [ObservableProperty] private ObservableCollection<SnackbarMessage> _messages;

    public SnackbarControlViewModel()
    {
        Messages = new ObservableCollection<SnackbarMessage>();
    }

    [RelayCommand]
    public void Show(string message)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            var snack = new SnackbarMessage
            {
                Message = message,
            };

            snack.TimeOut += Close;
            Messages.Add(snack);
            snack.IsActive = true;
            snack.Start();
        });
    }

    [RelayCommand]
    private void Close(SnackbarMessage snack)
    {
        snack.TimeOut -= Close;
        snack.IsActive = false;
        Messages.Remove(snack);
    }
}