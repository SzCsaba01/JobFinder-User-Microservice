using User.Data.Access.Helpers.DTO.Email;

namespace User.Services.Contracts;
public interface IEmailService
{
    public Task SendResetPasswordEmailAsync(string username, string token, string email);
    public Task SendEmailVerificationAsync(string username, string token, string email);
    public Task SendFeedbackEmailsAsync(List<EmailDto> emails);
}
