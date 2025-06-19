using System.Data;
using System.Net;
using System.Net.Mail;
using System.ComponentModel.DataAnnotations;
using dotsend.Utility;

namespace dotsend.Utility
{
    public class EmailService
    {
        public static async Task<string> SendEmailAsync(Notification dto)
        {
            try
            {
                if (string.IsNullOrEmpty(dto.To) && string.IsNullOrEmpty(dto.Bcc))
                {
                    return "No recipients specified.";
                }

                var smtpInfo = GetSmtpClientConfiguration();

                using var mailMessage = new MailMessage
                {
                    From = new MailAddress(smtpInfo.From, smtpInfo.FromDisplayName),
                    Subject = dto.Subject,
                    Body = dto.HtmlBody,
                    IsBodyHtml = true
                };

                // Add recipients
                foreach (var email in dto.To?.Split(';', StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>())
                    mailMessage.To.Add(email.Trim());

                foreach (var email in dto.Bcc?.Split(';', StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>())
                    mailMessage.Bcc.Add(email.Trim());

                foreach (var email in dto.Cc?.Split(';', StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>())
                    mailMessage.CC.Add(email.Trim());

                using var smtpClient = new SmtpClient(smtpInfo.SmtpServer)
                {
                    Credentials = new NetworkCredential(smtpInfo.UserName, smtpInfo.Password),
                    Port = smtpInfo.Port,
                    EnableSsl = smtpInfo.EnableSsl == "Y"
                };

                try
                {
                    await smtpClient.SendMailAsync(mailMessage);
                    return "Success: Email sent.";
                }
                catch (SmtpException ex)
                {
                    return $"SMTP Failure: {ex.StatusCode} - {ex.Message}";
                }
            }
            catch (Exception ex)
            {
                return $"General Failure: {ex.Message}";
            }
        }

        public static SmtpClientConfiguration? GetSmtpClientConfiguration()
        {
            try
            {

                return new SmtpClientConfiguration
                {
                    SmtpServer = FileHandler.GetAppSettingsConfig("SmtpServer"),
                    Port = Int32.Parse(FileHandler.GetAppSettingsConfig("Port")),
                    From = FileHandler.GetAppSettingsConfig("From"),
                    FromDisplayName = FileHandler.GetAppSettingsConfig("FromDisplayName"),
                    UserName = FileHandler.GetAppSettingsConfig("UserName"),
                    Password = Security.Decrypt(FileHandler.GetAppSettingsConfig("Harsh")),
                    EnableSsl = FileHandler.GetAppSettingsConfig("EnableSsl"),
                    SendNotification = FileHandler.GetAppSettingsConfig("SendNotification")
                };

            }
            catch (Exception ex)
            {

                Console.WriteLine($"Error getting smtp configuration: {ex.Message}");
            }
            return null;
        }

    }

    public class SmtpClientConfiguration
    {
        [Required]
        public string? SmtpServer { get; set; }
        [Required]
        public int Port { get; set; }
        [Required]
        public string? From { get; set; }
        [Required]
        public string? FromDisplayName { get; set; }
        [Required]
        public string? UserName { get; set; }
        [Required]
        public string? Password { get; set; }
        [Required]
        public string? EnableSsl { get; set; }
        [StringLength(1)]
        public string? SendNotification { get; set; }
    }

    public class Notification
    {

        public string? To { get; set; }
        public string? Bcc { get; set; }
        public string? Cc { get; set; }
        public string? AttachmentPath { get; set; }
        public string? Subject { get; set; }
        public string? HtmlBody { get; set; }
    }
}
