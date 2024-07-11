namespace User.Data.Access.Helpers.DTO.User;
public class FilteredUsersPaginationDto
{
    public ICollection<FilteredUserDto> Users { get; set; }
    public int NumberOfUsers { get; set; }
}
