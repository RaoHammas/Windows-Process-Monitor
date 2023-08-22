namespace MonitorApp.Domain.Models;

public class EmailDetails
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string EmailTo { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}