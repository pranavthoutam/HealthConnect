﻿using System.Net;
using System.Net.Mail;

namespace HealthConnect.Services
{
    public class EmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public  Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var smtpclient = new SmtpClient(_configuration["EmailSettings:Host"])
            {
                Port = int.Parse(_configuration["EmailSettings:Port"]),
                Credentials = new NetworkCredential(_configuration["EmailSettings:Username"], _configuration["EmailSettings:Password"]),
                EnableSsl = true,
            };

            var mailMessage = new MailMessage
            {
                From =new MailAddress(_configuration["EmailSettings:From"]),
                Subject = subject,  
                Body = body,
                IsBodyHtml = true
            };

            mailMessage.To.Add(toEmail);
            return smtpclient.SendMailAsync(mailMessage);
        }

    }
}

