using AutoMapper;
using User.Data.Access.Helpers;
using User.Data.Access.Helpers.DTO.UserProfile;
using User.Data.Contracts;
using User.Data.Contracts.Helpers.DTO.Message;
using User.Data.Object.Entities;
using User.Services.Business.Exceptions;
using User.Services.Contracts;
using Newtonsoft.Json;
using Project.Services.Business.Exceptions;
using System.ComponentModel.DataAnnotations;

namespace User.Services.Business;
public class UserProfileService : IUserProfileService
{
    private readonly IUserRepository _userRepository;
    private readonly IUserProfileRepository _userProfileRepository;
    private readonly IUserProfileSkillMappingRepository _userProfileSkillMappingRepository;
    private readonly ISkillRepository _skillRepository;
    private readonly IQueueMessageSenderService _queueMessageSenderService;
    private readonly IPdfService _pdfService;
    private readonly IUserProfileDataProcessorService _userProfileDataProcessorService;
    private readonly IMapper _mapper;

    public UserProfileService (
        IUserRepository userRepository,
        IUserProfileRepository userProfileRepository, 
        IUserProfileSkillMappingRepository userProfileSkillMappingRepository,
        ISkillRepository skillRepository,
        IQueueMessageSenderService queueMessageSenderService,
        IPdfService pdfService,
        IUserProfileDataProcessorService userProfileDataProcessorService,
        IMapper mapper
        )
    {  
        _userRepository = userRepository;
        _userProfileRepository = userProfileRepository;
        _userProfileSkillMappingRepository = userProfileSkillMappingRepository;
        _skillRepository = skillRepository;
        _queueMessageSenderService = queueMessageSenderService;
        _pdfService = pdfService;
        _userProfileDataProcessorService = userProfileDataProcessorService;
        _mapper = mapper;
    }

    public async Task<UserProfileDto> GetUserProfileByIdAsync(Guid userProfileId)
    {
        var userProfile = await _userRepository.GetUserWithProfileByIdAsync(userProfileId);

        if (userProfile is null)
        {
            throw new ModelNotFoundException("An error occured while fetching your profile!");
        }

        return _mapper.Map<UserProfileDto>(userProfile);
    }

    public async Task EditUserProfileByIdAsync(Guid userProfileId, UserProfileDto newUserProfile, bool updateDataFromCV = false)
    {

        if (newUserProfile.Skills is not null && newUserProfile.Skills.Count() > 20)
        {
            throw new ValidationException("You can't have more than 20 skills!");
        }

        if (newUserProfile.UserCV is null && updateDataFromCV)
        {
            throw new ValidationException("You must upload your CV!");
        }

        var oldUserProfile = await _userProfileRepository.GetUserProfileByIdAsync(userProfileId);

        if (oldUserProfile == null)
        {
            throw new ModelNotFoundException("An error occured while updateing your profile!");
        }

        if (oldUserProfile.UserCV is not null && newUserProfile.UserCV is null)
        {
            _pdfService.DeleteUserCV(oldUserProfile.User.Username);
            oldUserProfile.UserCV = null;
        }

        if (newUserProfile.UserCVFile is not null)
        {
            await _pdfService.SaveUserCVAsync(newUserProfile.UserCVFile, oldUserProfile);
        }

        if (updateDataFromCV)
        {
            await _userProfileDataProcessorService.UpdateUserProfileByCVDataAsync(oldUserProfile);
            return;
        }

        oldUserProfile.FirstName = newUserProfile.FirstName;
        oldUserProfile.LastName = newUserProfile.LastName;
        oldUserProfile.Country = newUserProfile.Country;
        oldUserProfile.State = newUserProfile.State;
        oldUserProfile.City = newUserProfile.City;
        oldUserProfile.Education = newUserProfile.Education;
        oldUserProfile.Experience = newUserProfile.Experience;

        if (newUserProfile.Skills is not null)
        {
            var oldUserSkills = oldUserProfile.Skills.Select(x => x.SkillName).ToList();
            var newUserSkills = newUserProfile.Skills.ToList();
            await _userProfileDataProcessorService.AddNewSkillsAsync(newUserProfile.Skills.ToList());
            var skillsToBeAdded = _userProfileDataProcessorService.GetUserSkillsToBeAdded(oldUserSkills, newUserSkills);
            var skillsToBeRemoved = _userProfileDataProcessorService.GetUserSkillsToBeRemoved(oldUserSkills, newUserSkills);

            var UserProfileSkillMappingsToBeAdded = skillsToBeAdded.Select(x => new UserProfileSkillMapping
            {
                SkillName = x,
                UserProfileId = oldUserProfile.Id
            }).ToList();

            var UserProfileSkillMappingsToBeRemoved = skillsToBeRemoved.Select(x => new UserProfileSkillMapping
            {
                SkillName = x,
                UserProfileId = oldUserProfile.Id
            }).ToList();

            await _userProfileSkillMappingRepository.AddUserProfileSkillMappingsAsync(UserProfileSkillMappingsToBeRemoved);
            await _userProfileSkillMappingRepository.AddUserProfileSkillMappingsAsync(UserProfileSkillMappingsToBeAdded);
        }

        await _userProfileRepository.UpdateUserProfileAsync(oldUserProfile);
    }

    public async Task SendRecommandJobsMessageAsync(Guid userProfileId)
    {
        var userProfile = await _userProfileRepository.GetUserProfileByIdAsync(userProfileId);

        if (userProfile is null)
        {
            throw new ModelNotFoundException("An error occured while recommanding jobs!");
        }

        if (userProfile.Skills is null || userProfile.Skills.Count() < 3)
        {
            throw new JobRecommendationException("You must have at least 3 skills setted in your profile to get job recommendations!");
        }

        var userProfileDto = _mapper.Map<UserMessageDetailsDto>(userProfile);

        var queueMessage = new UserMessageDto
        {
            UserMessageDetails = userProfileDto,
            MessageType = AppConstants.RECOMMEND_JOBS_MESSAGE
        };

        await _queueMessageSenderService.SendMessageAsync(JsonConvert.SerializeObject(queueMessage));
    }
}

