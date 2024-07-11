using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace User.Data.Object.Entities;

[Table("Skills")]
public class SkillEntity
{
    [Key]
    [Required(ErrorMessage = "Skill name is required")]
    [MaxLength(100, ErrorMessage = "Skill name cannot be longer than 100 characters")]
    public string Skill { get; set; }

    public ICollection<UserProfileSkillMapping> UserProfiles { get; set; }
}
