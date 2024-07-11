namespace User.Data.Access.Helpers.DTO.User;
public class FilteredUsersSearchDto
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public string? Username { get; set; }
    public string? Education { get; set; }
    public string? Country { get; set; }
    public string? State { get; set; }
    public string? City { get; set; }
    public string? Experience { get; set; }
}
