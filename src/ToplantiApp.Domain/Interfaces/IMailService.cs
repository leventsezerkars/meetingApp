using ToplantiApp.Domain.Entities;

namespace ToplantiApp.Domain.Interfaces;

public interface IMailService
{
    Task SendWelcomeEmailAsync(User user);
    Task SendMeetingInvitationAsync(string toEmail, string participantName, Meeting meeting, string meetingUrl);
    Task SendMeetingNotificationAsync(string toEmail, string participantName, Meeting meeting, string subject, string message);
}
