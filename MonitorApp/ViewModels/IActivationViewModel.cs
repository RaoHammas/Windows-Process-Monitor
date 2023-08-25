using System.Threading.Tasks;

namespace MonitorApp.ViewModels;

public interface IActivationViewModel
{
    public bool IsActivationSuccessful { get; set; }
    Task Activate();
}