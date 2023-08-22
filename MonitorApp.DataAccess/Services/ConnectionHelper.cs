using System.Configuration;

namespace MonitorApp.DataAccess.Services;

public class ConnectionHelper : IConnectionHelper
{
    public string GetConnectionString()
    {
        string connString = ConfigurationManager.ConnectionStrings["DBConnectionString"].ToString();

        return connString;
    }
}