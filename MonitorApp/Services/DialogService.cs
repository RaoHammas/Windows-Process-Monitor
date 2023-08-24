using System.Threading.Tasks;
using MaterialDesignThemes.Wpf;
using MonitorApp.Views;

namespace MonitorApp.Services;

public class DialogService : IDialogService
{
    public async Task ShowProcessSettings(object dataContext)
    {
        var view = new ProcessSettingsView
        {
            DataContext = dataContext
        };

        await DialogHost.Show(view, "RootDialog");
    }

    public async Task ShowEmailSettings(object dataContext)
    {
        var view = new EmailSettingsView()
        {
            DataContext = dataContext
        };

        await DialogHost.Show(view, "RootDialog");
    }

    public async Task ShowActivation(object dataContext)
    {
        var view = new ActivationView()
        {
            DataContext = dataContext
        };

        await DialogHost.Show(view, "RootDialog");
    }
}