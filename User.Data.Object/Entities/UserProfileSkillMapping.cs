using System.ComponentModel.DataAnnotations.Schema;

namespace User.Data.Object.Entities;

[Table("UserProfileSkillMappings")]
public class UserProfileSkillMapping
{
    public Guid UserProfileId { get; set; }

    public UserProfileEntity UserProfile { get; set; }

    public string SkillName { get; set; }

    public SkillEntity Skill { get; set; }
}
