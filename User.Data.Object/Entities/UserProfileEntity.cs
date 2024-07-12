using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace User.Data.Object.Entities;

[Table("UserProfiles")]
public class UserProfileEntity
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = "User is required")]
    public Guid UserId { get; set; }

    [Required(ErrorMessage = "First name is required")]
    [MaxLength(100, ErrorMessage = "First name cannot be longer than 100 characters")]
    [MinLength(2, ErrorMessage = "First name cannot be shorter than 2 characters")]
    public string FirstName { get; set; }

    [Required(ErrorMessage = "Last name is required")]
    [MaxLength(100, ErrorMessage = "Last name cannot be longer than 100 characters")]
    [MinLength(2, ErrorMessage = "Last name cannot be shorter than 2 characters")]
    public string LastName { get; set; }

    [MaxLength(100, ErrorMessage = "Country cannot be longer than 100 characters")]
    public string? Country { get; set; }

    [MaxLength(100, ErrorMessage = "State cannot be longer than 100 characters")]
    public string? State { get; set; }

    [MaxLength(100, ErrorMessage = "City cannot be longer than 100 characters")]
    public string? City { get; set; }

    [MaxLength(1000, ErrorMessage = "Education cannot be longer than 1000 characters")]
    public string? Education { get; set; }

    [MaxLength(1000, ErrorMessage = "Experience cannot be longer than 1000 characters")]
    public string? Experience { get; set; }

    public string? UserCV { get; set; }

    public ICollection<UserProfileSkillMapping>? Skills { get; set; }

    public UserEntity User { get; set; }
}
