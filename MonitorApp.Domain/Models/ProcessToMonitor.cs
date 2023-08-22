using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MonitorApp.Domain.Models;

public class ProcessToMonitor : INotifyPropertyChanged
{
    private int _id;
    private int _pid;
    private int _sessionId;
    private bool _hasAWindow;
    private string _processName = string.Empty;
    private string _displayName = string.Empty;
    private string _fullPath = string.Empty;
    private DateTime _startedAt;
    private DateTime? _stoppedAt;
    private ProcessStatus _status = ProcessStatus.Running;
    private bool _tryRestarting;
    private int _restartingAttempts;
    private bool _sendAlertEmail;
    private DateTime _updatedDateTime;
    private int _noOfInstances;

    public int NoOfInstances
    {
        get => _noOfInstances;
        set => SetField(ref _noOfInstances, value);
    }

    public int Id
    {
        get => _id;
        set => SetField(ref _id, value);
    }

    public int PID
    {
        get => _pid;
        set => SetField(ref _pid, value);
    }

    public int SessionId
    {
        get => _sessionId;
        set => SetField(ref _sessionId, value);
    }

    public bool HasAWindow
    {
        get => _hasAWindow;
        set => SetField(ref _hasAWindow, value);
    }

    public string ProcessName
    {
        get => _processName;
        set => SetField(ref _processName, value);
    }

    public string DisplayName
    {
        get => _displayName;
        set => SetField(ref _displayName, value);
    }

    public string FullPath
    {
        get => _fullPath;
        set => SetField(ref _fullPath, value);
    }

    public DateTime StartedAt
    {
        get => _startedAt;
        set => SetField(ref _startedAt, value);
    }

    public DateTime? StoppedAt
    {
        get => _stoppedAt;
        set => SetField(ref _stoppedAt, value);
    }

    public ProcessStatus Status
    {
        get => _status;
        set => SetField(ref _status, value);
    }


    public bool TryRestarting
    {
        get => _tryRestarting;
        set => SetField(ref _tryRestarting, value);
    }

    public int RestartingAttempts
    {
        get => _restartingAttempts;
        set => SetField(ref _restartingAttempts, value);
    }

    public bool SendAlertEmail
    {
        get => _sendAlertEmail;
        set => SetField(ref _sendAlertEmail, value);
    }

    public DateTime UpdatedDateTime
    {
        get => _updatedDateTime;
        set => SetField(ref _updatedDateTime, value);
    }

    public ProcessToMonitor GetClone()
    {
        return (ProcessToMonitor)MemberwiseClone();
    }


    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}