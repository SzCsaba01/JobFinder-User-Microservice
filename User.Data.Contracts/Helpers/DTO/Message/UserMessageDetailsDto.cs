namespace User.Data.Contracts.Helpers.DTO.Message;
public class UserMessageDetailsDto
{
    public Guid UserProfileId { get; set; }
    public string? Country { get; set; }
    public string? State { get; set; }
    public string? City { get; set; }
    public string? Education { get; set; }
    public string? Experience { get; set; }
    public ICollection<string>? Skills { get; set; }
}
