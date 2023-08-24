using System.Data;
using Dapper;
using Microsoft.Data.Sqlite;
using MonitorApp.Domain.Models;

namespace MonitorApp.DataAccess.Services;

/// <summary>
/// DbService Class
/// </summary>
public class DbService : IDbService
{
    private readonly string _connectionString;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="connectionHelper"></param>
    public DbService(IConnectionHelper connectionHelper)
    {
        _connectionString = connectionHelper.GetConnectionString();

        InitializeDatabase();
    }

    private void InitializeDatabase()
    {
        using IDbConnection con = new SqliteConnection(_connectionString);
        con.Open();
        const string createProcessesToMonitorTableQuery = """

                                                          CREATE TABLE IF NOT EXISTS ProcessesToMonitor
                                                          (
                                                          Id INTEGER PRIMARY KEY,
                                                          PID INTEGER,
                                                          SessionId INTEGER,
                                                          NoOfInstances INTEGER,
                                                          ProcessName TEXT,
                                                          DisplayName TEXT,
                                                          HasAWindow INTEGER,
                                                          FullPath TEXT,
                                                          Status INTEGER,
                                                          StartedAt TEXT,
                                                          StoppedAt TEXT,
                                                          TryRestarting INTEGER,
                                                          RestartingAttempts INTEGER,
                                                          SendAlertEmail INTEGER,
                                                          UpdatedDateTime TEXT
                                                                          );
                                                          """;

        con.Execute(createProcessesToMonitorTableQuery);

        const string createEmailDetailsTableQuery = """

                                                    CREATE TABLE IF NOT EXISTS EmailDetails
                                                    (
                                                    Id INTEGER PRIMARY KEY,
                                                    Email TEXT,
                                                    Password TEXT,
                                                    EmailTo TEXT
                                                    );
                                                    """;

        con.Execute(createEmailDetailsTableQuery);


        const string createActivationTableQuery = """

                                                    CREATE TABLE IF NOT EXISTS ActivationTable
                                                    (
                                                    Id INTEGER PRIMARY KEY,
                                                    Key TEXT,
                                                    Email TEXT
                                                    );
                                                    """;

        con.Execute(createActivationTableQuery);


        /*var insertTempData = """

                             INSERT INTO EmailDetails
                             (Email, Password, EmailTo)
                             VALUES
                             (@Email, @Password, @EmailTo);
                             """;

        var tempData = new
        {
            Email = "hammas143@gmail.com",
            Password = "",
            EmailTo = ""
        };

        con.Execute(insertTempData, tempData);*/
    }


    ///<inheritdoc />
    public async Task<int> SaveAsync(ProcessToMonitor process)
    {
        using IDbConnection con = new SqliteConnection(_connectionString);
        process.UpdatedDateTime = DateTime.UtcNow;
        if (process.Id > 0)
        {
            const string updateQuery = """

                                       UPDATE ProcessesToMonitor

                                       SET PID = @PID, SessionId = @SessionId, NoOfInstances = @NoOfInstances, ProcessName = @ProcessName, DisplayName = @DisplayName, HasAWindow = @HasAWindow, FullPath = @FullPath,
                                       Status = @Status, StartedAt = @StartedAt, StoppedAt = @StoppedAt, TryRestarting = @TryRestarting,
                                       RestartingAttempts = @RestartingAttempts, SendAlertEmail = @SendAlertEmail, UpdatedDateTime = @UpdatedDateTime

                                       WHERE Id = @Id;
                                               
                                       """;
            return await con.ExecuteAsync(updateQuery, process);
        }

        const string insertQuery = """

                                   INSERT INTO ProcessesToMonitor
                                   (PID, SessionId, NoOfInstances, ProcessName, DisplayName, HasAWindow, FullPath, Status, StartedAt, StoppedAt, TryRestarting,
                                   RestartingAttempts, SendAlertEmail, UpdatedDateTime)
                                   VALUES
                                   (@PID, @SessionId, @NoOfInstances, @ProcessName, @DisplayName, @HasAWindow, @FullPath, @Status, @StartedAt, @StoppedAt, @TryRestarting,
                                   @RestartingAttempts, @SendAlertEmail, @UpdatedDateTime);
                                   SELECT last_insert_rowid();
                                           
                                   """;

        return await con.QuerySingleAsync<int>(insertQuery, process);
    }


    ///<inheritdoc />
    public Task<bool> RemoveAsync(int appId)
    {
        using IDbConnection con = new SqliteConnection(_connectionString);
        const string query = @"DELETE FROM ProcessesToMonitor WHERE Id = @Id;";
        var rowsAffected = con.ExecuteAsync(query, new { Id = appId }).Result;

        return Task.FromResult(rowsAffected > 0);
    }

    ///<inheritdoc />
    public async Task<ProcessToMonitor> GetAsync(int appId)
    {
        using IDbConnection con = new SqliteConnection(_connectionString);
        const string query = @"SELECT * FROM ProcessesToMonitor WHERE Id = @Id;";

        return await con.QueryFirstOrDefaultAsync<ProcessToMonitor>(query, new
        {
            Id = appId
        });
    }

    ///<inheritdoc />
    public async Task<IEnumerable<ProcessToMonitor>> GetAllAsync()
    {
        using IDbConnection con = new SqliteConnection(_connectionString);
        const string query = "SELECT * FROM ProcessesToMonitor;";

        return await con.QueryAsync<ProcessToMonitor>(query);
    }


    public async Task<int> SaveEmailDetailsAsync(EmailDetails email)
    {
        using IDbConnection con = new SqliteConnection(_connectionString);
        if (email.Id > 0)
        {
            const string updateQuery = """
                                       UPDATE EmailDetails
                                       SET Email = @Email, Password = @Password, EmailTo = @EmailTo
                                       WHERE Id = @Id;
                                       """;
            return await con.ExecuteAsync(updateQuery, email);
        }

        const string insertQuery = """

                                   INSERT INTO EmailDetails
                                   (Email, Password, EmailTo)
                                   VALUES
                                   (@Email, @Password, @EmailTo);
                                   SELECT last_insert_rowid();
                                   """;
        return await con.QuerySingleAsync<int>(insertQuery, email);
    }


    public async Task<EmailDetails?> GetEmailDetailsAsync()
    {
        using IDbConnection con = new SqliteConnection(_connectionString);
        const string query = "SELECT * FROM EmailDetails";

        return await con.QueryFirstOrDefaultAsync<EmailDetails>(query);
    }

    public async Task<int> ActivateAsync(string key, string email)
    {
        using IDbConnection con = new SqliteConnection(_connectionString);
        const string insertQuery = """

                                   INSERT INTO ActivationTable
                                   (Key, Email)
                                   VALUES
                                   (@Key, @Email);
                                   
                                   SELECT last_insert_rowid();
                                           
                                   """;

        return await con.QuerySingleAsync<int>(insertQuery, new {Key = key, Email = email});
    }

    
    public async Task<string> GetActivationKeyAsync()
    {
        using IDbConnection con = new SqliteConnection(_connectionString);
        const string selectQuery = """

                                   SELECT Key FROM ActivationTable;
                                           
                                   """;

        return await con.QuerySingleOrDefaultAsync<string>(selectQuery);
    }
}