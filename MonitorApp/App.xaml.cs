using System;
using System.Configuration;
using System.Windows;
using CommunityToolkit.Mvvm.Messaging;
using MaterialDesignThemes.Wpf;
using Microsoft.Extensions.DependencyInjection;
using MonitorApp.Controls;
using MonitorApp.DataAccess.Services;
using MonitorApp.Helpers;
using MonitorApp.Services;
using MonitorApp.ViewModels;
using MonitorApp.Views;

namespace MonitorApp;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    #region Props

    /// <summary>
    /// Gets the current <see cref="App"/> instance in use
    /// </summary>
    public new static App Current => (App)Application.Current;

    /// <summary>
    /// Gets the <see cref="IServiceProvider"/> instance to resolve application services.
    /// </summary>
    public IServiceProvider Services { get; }

    /// <summary>
    /// Shell View Model
    /// </summary>
    private IShell? _shellViewModel;

    #endregion

    /// <summary>
    /// Constructor
    /// </summary>
    public App()
    {
        Services = ConfigureServices();
    }

    /// <summary>
    /// When application starts set initial view
    /// </summary>
    /// <param name="e"></param>
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        _shellViewModel = Services.GetService<IShell>();
        ShellWindow shellView = new()
        {
            DataContext = _shellViewModel,
        };

        shellView.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _shellViewModel?.Dispose();
        base.OnExit(e);
    }

    /// <summary>
    /// Configures the services for the application.
    /// </summary>
    private static IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();

        services.AddTransient<IShell, ShellViewModel>();
        services.AddTransient<IDialogService, DialogService>();
        services.AddTransient<IProcessSettingsViewModel, ProcessSettingsViewModel>();
        services.AddTransient<IEmailSettingsViewModel, EmailSettingsViewModel>();
        services.AddTransient<ISnackbarControlViewModel, SnackbarControlViewModel>();
        services.AddTransient<IActivationViewModel, ActivationViewModel>();
        services.AddTransient<ISnackbarMessageQueue, SnackbarMessageQueue>();


        services.AddSingleton<IProcessHelper, ProcessHelper>();
        services.AddSingleton<IDbService, DbService>();
        services.AddSingleton<IMessenger, WeakReferenceMessenger>();
        services.AddSingleton<IProcessWatcherService, ProcessWatcherService>();
        services.AddSingleton<IEmailService, EmailService>();
        services.AddSingleton<IConnectionHelper, ConnectionHelper>();


        return services.BuildServiceProvider();
    }
}