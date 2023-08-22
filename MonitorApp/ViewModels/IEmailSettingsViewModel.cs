using System.Threading.Tasks;
using MonitorApp.Domain.Models;

namespace MonitorApp.ViewModels;

public interface IEmailSettingsViewModel
{
    public EmailDetails EmailDetails { get; set; }
    Task SaveAsync();
}