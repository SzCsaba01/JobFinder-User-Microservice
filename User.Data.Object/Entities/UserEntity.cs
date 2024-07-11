using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace User.Data.Object.Entities;

[Table("Users")]
public class UserEntity
{
    [Key]
    public Guid Id { get; set; }

    [Required(ErrorMessage = "Username is required")]
    [MaxLength(30, ErrorMessage = "Username cannot be longer than 30 characters")]
    [MinLength(5, ErrorMessage = "Username cannot be shorter than 5 characters")]
    [RegularExpression("[a-zA-Z0-9._]+", ErrorMessage = "Username can contain only lower, upper cases and numbers")]
    public string Username { get; set; }

    [Required(ErrorMessage = "Password is required")]
    [RegularExpression("^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[-+_!@#$%^&*.,?]).+$", ErrorMessage = "Password must contain at least one upper case, one lower case, one number and one special character")]
    public string Password { get; set; }

    public string? ResetPasswordToken { get; set; }

    public DateTime? ResetPasswordTokenExpirationDate { get; set; }

    [Required(ErrorMessage = "Email is required")]
    [MaxLength(50, ErrorMessage = "Email cannot be longer than 50 characters")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; }

    [Required(ErrorMessage = "Registration date is required")]
    public DateTime RegistrationDate { get; set; }

    [Required(ErrorMessage = "Registration token is required")]
    public string RegistrationToken { get; set; }

    [Required(ErrorMessage = "Is email confirmed is required")]
    public bool IsEmailConfirmed { get; set; }

    [Required(ErrorMessage = "Role is required")]
    public Guid RoleId { get; set; }

    public RoleEntity Role { get; set; }

    public UserProfileEntity UserProfile { get; set; }
}
