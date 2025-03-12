using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Configuration;

namespace Sem3EProjectOnlineCPFH.Services
{
    public class EmailService
    {
        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                // Lấy cấu hình SMTP từ web.config
                var fromEmail = ConfigurationManager.AppSettings["EmailUsername"];
                var password = ConfigurationManager.AppSettings["EmailPassword"];
                var smtpHost = ConfigurationManager.AppSettings["EmailHost"];
                var smtpPort = int.Parse(ConfigurationManager.AppSettings["EmailPort"]);
                var enableSSL = bool.Parse(ConfigurationManager.AppSettings["EnableSSL"]);

                var smtpClient = new SmtpClient(smtpHost)
                {
                    Port = smtpPort,
                    Credentials = new NetworkCredential(fromEmail, password),
                    EnableSsl = enableSSL // Kích hoạt SSL/TLS
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(fromEmail, "Hyprics"),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };
                mailMessage.To.Add(toEmail);

                await smtpClient.SendMailAsync(mailMessage);
                System.Diagnostics.Debug.WriteLine("Email sent successfully to " + toEmail);
            }
            catch (SmtpException ex)
            {
                System.Diagnostics.Debug.WriteLine("Email Error: " + ex.Message);
                throw;
            }
        }
    }

}