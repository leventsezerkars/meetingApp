using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using ToplantiApp.Domain.Entities;
using ToplantiApp.Domain.Interfaces;

namespace ToplantiApp.Infrastructure.Services;

public class MailService : IMailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<MailService> _logger;

    public MailService(IConfiguration configuration, ILogger<MailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendWelcomeEmailAsync(User user)
    {
        var subject = "Hoş Geldiniz!";
        var body = $@"
            <h2>Merhaba {user.FirstName} {user.LastName},</h2>
            <p>Toplantı yönetim sistemimize hoş geldiniz!</p>
            <p>Artık toplantılarınızı kolayca yönetebilirsiniz.</p>
            <br/>
            <p>İyi günler dileriz.</p>";

        await SendEmailAsync(user.Email, subject, body, fromAddress: null);
    }

    public async Task SendMeetingInvitationAsync(string toEmail, string participantName, Meeting meeting, string meetingUrl)
    {
        var subject = $"Toplantı Daveti: {meeting.Name}";
        var body = $@"
            <h2>Merhaba {participantName},</h2>
            <p><strong>{meeting.CreatedBy?.FullName ?? "Organizatör"}</strong> sizi bir toplantıya davet etti.</p>
            <table style='border-collapse:collapse; margin:15px 0;'>
                <tr><td style='padding:5px 15px 5px 0; font-weight:bold;'>Toplantı:</td><td>{meeting.Name}</td></tr>
                <tr><td style='padding:5px 15px 5px 0; font-weight:bold;'>Başlangıç:</td><td>{meeting.StartDate:dd.MM.yyyy HH:mm}</td></tr>
                <tr><td style='padding:5px 15px 5px 0; font-weight:bold;'>Bitiş:</td><td>{meeting.EndDate:dd.MM.yyyy HH:mm}</td></tr>
                <tr><td style='padding:5px 15px 5px 0; font-weight:bold;'>Açıklama:</td><td>{meeting.Description ?? "-"}</td></tr>
            </table>
            <p>Toplantı sayfasına erişim linki:</p>
            <p><a href='{meetingUrl}' style='background:#0d6efd;color:#fff;padding:10px 20px;text-decoration:none;border-radius:5px;'>Toplantıya Katıl</a></p>
            <p><small>Bu linke sadece toplantı saatleri içinde erişebilirsiniz.</small></p>";

        var fromAddress = meeting.CreatedBy?.Email;
        await SendEmailAsync(toEmail, subject, body, fromAddress);
    }

    public async Task SendMeetingNotificationAsync(string toEmail, string participantName, Meeting meeting, string subject, string message)
    {
        var body = $@"
            <h2>Merhaba {participantName},</h2>
            <p>{message}</p>
            <p><strong>Toplantı:</strong> {meeting.Name}</p>
            <p><strong>Tarih:</strong> {meeting.StartDate:dd.MM.yyyy HH:mm} - {meeting.EndDate:dd.MM.yyyy HH:mm}</p>";

        var fromAddress = meeting.CreatedBy?.Email;
        await SendEmailAsync(toEmail, subject, body, fromAddress);
    }

    private async Task SendEmailAsync(string toEmail, string subject, string htmlBody, string? fromAddress = null)
    {
        var host = _configuration["Mail:Host"];
        if (string.IsNullOrWhiteSpace(host))
        {
            _logger.LogWarning("Mail:Host yapilandirilmamis; e-posta gonderimi atlaniyor. To: {To}, Subject: {Subject}", toEmail, subject);
            return;
        }

        try
        {
            var email = new MimeMessage();
            var from = !string.IsNullOrWhiteSpace(fromAddress)
                ? fromAddress
                : (_configuration["Mail:From"] ?? "noreply@toplanti.app");
            email.From.Add(MailboxAddress.Parse(from));
            email.To.Add(MailboxAddress.Parse(toEmail));
            email.Subject = subject;

            var builder = new BodyBuilder { HtmlBody = htmlBody };
            email.Body = builder.ToMessageBody();

            var port = int.Parse(_configuration["Mail:Port"] ?? "587");
            var useTls = string.Equals(_configuration["Mail:SecureSocketOptions"], "None", StringComparison.OrdinalIgnoreCase)
                ? SecureSocketOptions.None
                : SecureSocketOptions.StartTls;

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(host, port, useTls);

            var username = _configuration["Mail:Username"];
            if (!string.IsNullOrWhiteSpace(username))
            {
                await smtp.AuthenticateAsync(username, _configuration["Mail:Password"] ?? "");
            }

            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);

            _logger.LogInformation("E-posta gönderildi: {To} - {Subject}", toEmail, subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "E-posta gönderilemedi: {To} - {Subject}", toEmail, subject);
        }
    }
}
