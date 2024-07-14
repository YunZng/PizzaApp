#nullable disable
using System.Net;
using System.Net.Mail;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
namespace WebPWrecover.Services;

public class EmailSender : IEmailSender
{
  private readonly ILogger _logger;

  public EmailSender(IOptions<AuthMessageSenderOptions> optionsAccessor,
                     ILogger<EmailSender> logger)
  {
    Options = optionsAccessor.Value;
    _logger = logger;
  }

  public AuthMessageSenderOptions Options { get; } //Set with Secret Manager.

  public Task SendEmailAsync(string toEmail, string subject, string message)
  {
    var fromEmail = Options.Email!;
    var fromPwd = Options.Password;
    var client = new SmtpClient(Options.Server, 587)
    {
      EnableSsl = true,
      Credentials = new NetworkCredential(fromEmail, fromPwd),
    };

    return client.SendMailAsync(new MailMessage(Options.Email, toEmail, subject, message));
  }
}