using MonitorApp.Domain.Models;

namespace MonitorApp.DataAccess.Services;

public interface IEmailService
{
    Task<string> Send(EmailMetadata emailMetadata, CancellationToken? cancellationToken = null);
}