using User.Data.Access.Helpers;
using User.Data.Access.Helpers.DTO.Email;
using User.Data.Contracts;
using User.Services.Contracts;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;

namespace User.Services.Business;
public class EmailService : IEmailService
{
    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _config;

    public EmailService
        (
            IUserRepository userRepository,
            IConfiguration config
        )
    {
        _userRepository = userRepository;
        _config = config;
    }

    public async Task SendResetPasswordEmailAsync(string username, string uri, string email)
    {
        var emailDto = new EmailDto
        {
            To = email,
            Subject = "Reset Password",
            Body = CreateForgotPasswordMessageBodyByUri(username, uri)
        };

        await SendEmailAsync(emailDto);
    }

    public async Task SendEmailVerificationAsync(string username, string uri, string email)
    {
        var emailDto = new EmailDto
        {
            To = email,
            Subject = "Email Verification",
            Body = CreateEmailVerificationMessageBody(username, uri)
        };

        await SendEmailAsync(emailDto);
    }

    public async Task SendFeedbackEmailsAsync(List<EmailDto> emails)
    {
        var userProfileIds = emails.Select(x => x.UserProfileId).Distinct().ToList();

        if (userProfileIds is null) 
        {
            return;
        }

        var users = await _userRepository.GetUsersByUserProfileIdsAsync(userProfileIds);
        var emailsToSend = new List<EmailDto>();

        foreach (var user in users) 
        {
            var userEmail = emails.Where(x => x.UserProfileId == user.UserProfile.Id).FirstOrDefault();
            var userEmailsToSend = emails.Where(x => x.UserProfileId == user.UserProfile.Id).ToList();

            if (userEmail is null)
            {
                continue;
            }

            userEmailsToSend.ForEach( x =>
            {
                var emailDto = new EmailDto
                {
                    To = user.Email,
                    Subject = x.Subject,
                    Body = x.Body
                };

                emailsToSend.Add(emailDto);
            });
        }

        var totalEmailsToSend = emails.Count;
        var maxEmailsToSendPerMinute = 5;

        for (int i = 0; i < totalEmailsToSend; i += maxEmailsToSendPerMinute)
        {
            var emailsToSendBatch = emailsToSend.Skip(i).Take(maxEmailsToSendPerMinute).ToList();

            foreach (var emailToSend in emailsToSendBatch)
            {
                try
                {
                    await SendEmailAsync(emailToSend);
                }
                catch (Exception ex)
                {
                    continue;
                }
            }

            await Task.Delay(60000);
        }
    }

    private async Task SendEmailAsync(EmailDto emailDto)
    {
        string fromMail = _config["EmailCredentials:Email"];
        string fromPassword = _config["EmailCredentials:Password"];
        MailMessage message = new MailMessage();
        
        message.From = new MailAddress(fromMail);
        message.Subject = emailDto.Subject;
        message.To.Add(new MailAddress(emailDto.To));
        message.Body = emailDto.Body;
        message.IsBodyHtml = true;

        var smtpClient = new SmtpClient("smtp.gmail.com")
        {
            Port = 587,
            Credentials = new NetworkCredential(fromMail, fromPassword),
            EnableSsl = true,
        };

        await smtpClient.SendMailAsync(message);
    }

    private string CreateForgotPasswordMessageBodyByUri(string username, string Uri)
    {
        var bodyBuilder =
             $"Dear {username}, <br><br>" +
             $"You have {AppConstants.RESET_PASSWORD_TOKEN_VALIDATION_TIME} minutes to click the link and reset your password " +
             $"<a href={Uri}>link</a>";

        return bodyBuilder;
    }

    private string CreateEmailVerificationMessageBody(string username, string Uri)
    {
        var bodyBuilder =
            $"Dear {username}, <br><br>" +
            $"Verify your email by using the " +
            $"<a href={Uri}>link</a>";

        return bodyBuilder;
    }
}
