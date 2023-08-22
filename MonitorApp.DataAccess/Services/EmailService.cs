using System.Net;
using System.Net.Mail;
using FluentEmail.Core;
using FluentEmail.Core.Models;
using FluentEmail.Smtp;
using MonitorApp.Domain.Models;

namespace MonitorApp.DataAccess.Services;

public class EmailService : IEmailService
{
    private readonly IDbService _dbService;
    private const string SmtpServer = "smtp.gmail.com";
    private const int SmtpPort = 587; //tls 587 //ssl 465

    public EmailService(IDbService dbService)
    {
        _dbService = dbService;
    }

    public async Task<string> Send(EmailMetadata emailMetadata, CancellationToken? cancellationToken = null)
    {
        var emailDetails = await _dbService.GetEmailDetailsAsync();
        if (emailDetails == null)
        {
            return "User's email details not found.";
        }

        var sender = new SmtpSender(() => new SmtpClient
        {
            Host = SmtpServer,
            Port = SmtpPort,
            EnableSsl = true,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(emailDetails.Email, emailDetails.Password),
            DeliveryMethod = SmtpDeliveryMethod.Network, 
            Timeout = 5000,
            DeliveryFormat = SmtpDeliveryFormat.International,
        });

        try
        {
            var email = new Email
            {
                Sender = sender,
                Data = new EmailData
                {
                    FromAddress = new Address(emailDetails.Email),
                    ToAddresses = new List<Address> { new() { EmailAddress = emailDetails.EmailTo } },
                    Subject = emailMetadata.Subject,
                    Body = emailMetadata.Body,
                    Priority = Priority.High,
                }
            };

            var response = await email.SendAsync(cancellationToken);
            if (response.Successful)
            {
                return "Email sent successfully.";
            }

            return string.Join("\n", response.ErrorMessages);
        }
        catch (Exception ex)
        {
            return "An error occurred: " + ex.Message;
        }
    }
}