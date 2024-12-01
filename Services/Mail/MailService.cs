using System.Net.Mail;
using System.Net;
using StoreRewards.Controllers;
using StoreRewards.DTOs;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace StoreRewards.Services.Email
{
    public class MailService : IMailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<UserMail> _logger;


        public MailService(IConfiguration configuration,ILogger<UserMail> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<ResponseResult> SendEmailAsync(string to, string subject, string body, List<string>? attachmentPaths)
        {
            string? Host = Environment.GetEnvironmentVariable("Host")?.ToString();
            string? Port = Environment.GetEnvironmentVariable("Port");
            string? Username = Environment.GetEnvironmentVariable("Username");
            string? Password = Environment.GetEnvironmentVariable("Password");
            if (Host is not null && Port is not null && Username is not null && Password is not null)
            {

                try
                {
                    var smtpClient = new SmtpClient(Host)
                    {
                        Port = int.Parse(Port),
                        Credentials = new NetworkCredential(Username, Password),
                        EnableSsl = true,
                    };

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress(Username),
                        Subject = subject,
                        Body = body,
                        IsBodyHtml = true,
                    };

                    if (attachmentPaths is not null && attachmentPaths.Count > 0)
                    {
                        foreach (var attachmentPath in attachmentPaths)
                        {
                            if (!string.IsNullOrEmpty(attachmentPath) && File.Exists(attachmentPath))
                            {
                                mailMessage.Attachments.Add(new Attachment(attachmentPath));
                            }
                        }
                    }

                    mailMessage.To.Add(to);

                    //smtpClient.UseDefaultCredentials = true;

                    await smtpClient.SendMailAsync(mailMessage);


                    return new ResponseResult
                    {
                        Success = true,
                        ErrorMessage = ""
                    };
                }

                catch (SmtpException smtpEx)
                {
                    // Handle specific SMTP exceptions like invalid credentials, server issues, etc.
                    _logger.LogInformation($"Mail-Error : SMTP error: {smtpEx.Message}");

                    return new ResponseResult
                    {
                        Success = false,
                        ErrorMessage = $"Mail-Error : SMTP error: {smtpEx.Message}"
                    };


                }
                catch (Exception ex)
                {
                    // Handle other exceptions like invalid file paths, etc.
                    _logger.LogInformation($"Mail-Error : Error sending emails: {ex.Message}");

                    return new ResponseResult
                    {
                        Success = false,
                        ErrorMessage = $"Mail-Error : Error sending emails: {ex.Message}"
                    };

                }

            }

            return new ResponseResult
            {
                Success = true,
                ErrorMessage = $"Invalid data for the mail"
            };
        }
    }
}