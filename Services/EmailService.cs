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
		private readonly string _email = ConfigurationManager.AppSettings["EmailUsername"];
        private readonly string _password = ConfigurationManager.AppSettings["EmailPassword"];
        private readonly string _host = ConfigurationManager.AppSettings["EmailHost"];
        private readonly int _port = Convert.ToInt32(ConfigurationManager.AppSettings["EmailPort"]);
        private readonly bool _enableSSL = Convert.ToBoolean(ConfigurationManager.AppSettings["EmailEnableSSL"]);

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                using (var client = new SmtpClient(_host, _port))
                {
                    client.Credentials = new NetworkCredential(_email, _password);
                    client.EnableSsl = _enableSSL;

                    var mailMessage = new MailMessage(_email, toEmail, subject, body);
                    mailMessage.IsBodyHtml = true;

                    await client.SendMailAsync(mailMessage);
                }
            }   
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Email Error: " + ex.Message);
            }
        }
    }
}