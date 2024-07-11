namespace User.Services.Contracts;
public interface ISkillService
{
    public Task<ICollection<string>> GetAllSkillsAsync();
    public Task DeleteUnmappedSkillsAsync();
}
