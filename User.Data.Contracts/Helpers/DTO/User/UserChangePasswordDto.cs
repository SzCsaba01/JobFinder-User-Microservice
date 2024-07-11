namespace User.Data.Access.Helpers.DTO.User;
public class UserChangePasswordDto
{
    public string ResetPasswordToken { get; set; }
    public string NewPassword { get; set; }
    public string RepeatNewPassword { get; set; }
}
