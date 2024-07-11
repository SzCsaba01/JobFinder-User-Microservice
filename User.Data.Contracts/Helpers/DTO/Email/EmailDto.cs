namespace User.Data.Access.Helpers.DTO.Email;
public class EmailDto
{
    public string To { get; set; }
    public Guid UserProfileId { get; set; }
    public string Subject { get; set; }
    public string Body { get; set; }
}
