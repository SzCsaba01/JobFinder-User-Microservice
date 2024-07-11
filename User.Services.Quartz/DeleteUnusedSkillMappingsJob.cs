using Quartz;
using User.Services.Contracts;

namespace User.Quartz;
public class DeleteUnusedSkillMappingsJob : IJob
{
    private readonly ISkillService _skillService;

    public DeleteUnusedSkillMappingsJob(ISkillService skillService)
    {
        _skillService = skillService;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        await _skillService.DeleteUnmappedSkillsAsync();
    }
}
