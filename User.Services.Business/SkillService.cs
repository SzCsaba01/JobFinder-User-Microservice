using User.Data.Contracts;
using User.Services.Contracts;

namespace User.Services.Business;
public class SkillService : ISkillService
{
    private readonly ISkillRepository _skillRepository;

    public SkillService(ISkillRepository skillRepository)
    {
        _skillRepository = skillRepository;
    }

    public async Task<ICollection<string>> GetAllSkillsAsync()
    {
        var skills = await _skillRepository.GetAllSkillsAsync();

        return skills.Select(s => s.Skill).ToList();
    }

    public async Task DeleteUnmappedSkillsAsync()
    {
        var skills = await _skillRepository.GetAllUnmappedSkills();

        await _skillRepository.DeleteSkillsAsync(skills.ToList());
    }
}
