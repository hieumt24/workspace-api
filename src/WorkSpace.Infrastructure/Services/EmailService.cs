using System.Net;
using System.Net.Mail;
using System.Text;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using WorkSpace.Application.DTOs.Email;
using WorkSpace.Application.Exceptions;
using WorkSpace.Application.Interfaces.Services;
using WorkSpace.Domain.ConfigOptions;


namespace WorkSpace.Infrastructure.Services;

public class EmailService : IEmailService
{
    public MailSettings _mailSettings { get; }
    public ILogger<EmailService> _logger { get; }

    public EmailService(IOptions<MailSettings> mailSettings, ILogger<EmailService> logger)
    {
        _mailSettings = mailSettings.Value;
        _logger = logger;
    }

    public async Task SendAsync(EmailRequest request)
    {
        try
        {
            MailMessage message = new MailMessage(_mailSettings.EmailFrom, request.To, request.Subject, request.Body);
            message.IsBodyHtml = true;
            message.BodyEncoding = Encoding.UTF8;
            message.SubjectEncoding = Encoding.UTF8;

            message.ReplyToList.Add(new MailAddress(_mailSettings.EmailFrom));
            using var smtpClient = new SmtpClient(_mailSettings.SmtpHost);
            smtpClient.Port = _mailSettings.SmtpPort;
            smtpClient.EnableSsl = true;
            smtpClient.Credentials = new NetworkCredential(_mailSettings.SmtpUser, _mailSettings.SmtpPass);
            try
            {
                await smtpClient.SendMailAsync(message);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex.Message, ex);
            throw new ApiException(ex.Message);
        }
    }
}