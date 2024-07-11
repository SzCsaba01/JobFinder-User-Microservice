using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace User.Data.Object.Entities;

[Table("Roles")]
public class RoleEntity
{
    [Key]
    public Guid Id { get; set; }

    [Required(ErrorMessage = "Role name is required")]
    [MaxLength(15, ErrorMessage = "Role name cannot be longer than 15 characters")]
    public string RoleName { get; set; }

    public List<UserEntity> Users { get; set; }
}
