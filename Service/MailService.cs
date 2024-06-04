using CertificateRobot.Interface;
using Microsoft.Extensions.Configuration;
using System.Net.Mail;

namespace CertificateRobot.Service
{
    internal class MailService : IMessageService
    {
        private readonly SmtpClient _client;
        private readonly IConfiguration _configuration;

        public MailService(IConfiguration configuration)
        {
            _configuration = configuration;
            _client = new SmtpClient
            {
                Host = _configuration["Smtp:Host"],
                Port = Convert.ToInt32(_configuration["Smtp:Port"]),
                DeliveryMethod = SmtpDeliveryMethod.Network,
                EnableSsl = Convert.ToBoolean(_configuration["Smtp:EnableSsl"]),
                Credentials = new System.Net.NetworkCredential(_configuration["Smtp:UserNo"], _configuration["Smtp:Password"])
            };
        }

        public Task<bool> SendMessage(string message)
        {
            try
            {
                var from = string.IsNullOrEmpty(_configuration["Smtp:From"]) ? "Certificate Robot" : _configuration["Smtp:From"];
                MailMessage mail = new MailMessage();
                mail.From = new MailAddress(from);
                mail.To.Add(_configuration["Smtp:To"]);
                mail.IsBodyHtml = true;
                mail.Subject = "证书机器人提醒您";
                mail.Body = message;

                return Task.Run(() =>
                {
                    _client.Send(mail);
                    return true;
                });
            }
            catch (Exception e)
            {
                throw new Exception($"发送邮件失败：{e.Message}");
            }
        }
    }
}