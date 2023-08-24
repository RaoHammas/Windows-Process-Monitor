using System.Threading.Tasks;

namespace MonitorApp.Services;

public interface IDialogService
{
    Task ShowProcessSettings(object dataContext);
    Task ShowEmailSettings(object dataContext);
    Task ShowActivation(object dataContext);
}