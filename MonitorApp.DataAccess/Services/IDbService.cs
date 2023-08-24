using MonitorApp.Domain.Models;

namespace MonitorApp.DataAccess.Services;

/// <summary>
/// Interface for <see cref="DbService"/>
/// </summary>
public interface IDbService
{
    /// <summary>
    /// Save process in database
    /// </summary>
    /// <param name="process">ProcessToMonitor</param>
    /// <returns>Task of Saved Id if saved successfully else 0</returns>
    Task<int> SaveAsync(ProcessToMonitor process);

    /// <summary>
    /// Removes an process from monitoring table
    /// </summary>
    /// <param name="appId">Id of ProcessToMonitor object</param>
    /// <returns>Task of True if successfully deleted else false</returns>
    Task<bool> RemoveAsync(int appId);

    /// <summary>
    /// Gets an process by ID
    /// </summary>
    /// <param name="appId">ProcessToMonitor Id</param>
    /// <returns>Task of ProcessToMonitor or null if not found</returns>
    Task<ProcessToMonitor> GetAsync(int appId);

    /// <summary>
    /// Gets all apps being monitored
    /// </summary>
    /// <returns>Task of IEnumerable of ProcessToMonitor</returns>
    Task<IEnumerable<ProcessToMonitor>> GetAllAsync();

    /// <summary>
    /// Save User email details
    /// </summary>
    /// <param name="email">email details</param>
    /// <returns>Id of saved email record</returns>
    Task<int> SaveEmailDetailsAsync(EmailDetails email);

    /// <summary>
    /// Get's email details of user
    /// </summary>
    /// <returns></returns>
    Task<EmailDetails?> GetEmailDetailsAsync();

    /// <summary>
    /// Activate application
    /// </summary>
    /// <returns></returns>
    Task<int> ActivateAsync(string key, string email);

    /// <summary>
    /// Get activation key
    /// </summary>
    /// <returns></returns>
    Task<string> GetActivationKeyAsync();
}