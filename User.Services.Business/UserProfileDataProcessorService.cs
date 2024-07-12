using AutoMapper;
using Project.Services.Business.Exceptions;
using User.Data.Contracts;
using User.Data.Contracts.Helpers.DTO.Location;
using User.Data.Object.Entities;
using User.Services.Contracts;

namespace User.Services.Business;
public class UserProfileDataProcessorService : IUserProfileDataProcessorService
{
    private readonly IUserProfileRepository _userProfileRepository;
    private readonly ISkillRepository _skillRepository;
    private readonly IUserProfileSkillMappingRepository _userProfileSkillMappingRepository;
    private readonly IPdfService _pdfService;
    private readonly IOpenAIService _openAIService;
    private readonly ILocationCommunicationService _locationCommunicationService;
    private readonly IMapper _mapper;

    public UserProfileDataProcessorService(
        IUserProfileRepository userProfileRepository,
        ISkillRepository skillRepository,
        IUserProfileSkillMappingRepository userProfileSkillMappingRepository,
        IPdfService pdfService,
        IOpenAIService openAIService,
        ILocationCommunicationService locationCommunicationService,
        IMapper mapper
        )
    {
        _userProfileRepository = userProfileRepository;
        _skillRepository = skillRepository;
        _userProfileSkillMappingRepository = userProfileSkillMappingRepository;
        _pdfService = pdfService;
        _openAIService = openAIService;
        _locationCommunicationService = locationCommunicationService;
        _mapper = mapper;
    }

    public List<string> GetUserSkillsToBeAdded(List<string> oldUserSkills, List<string> newUserSkills)
    {
        if (newUserSkills is null || !newUserSkills.Any())
        {
            return new List<string>();
        }

        if (oldUserSkills is null || !oldUserSkills.Any())
        {
            return newUserSkills;
        }

        var skillsToBeAdded = newUserSkills.Except(oldUserSkills).ToList();

        return skillsToBeAdded;
    }

    public List<string> GetUserSkillsToBeRemoved(List<string> oldUserSkills, List<string> newUserSkills)
    {
        if (oldUserSkills is null || !oldUserSkills.Any())
        {
            return new List<string>();
        }

        if (newUserSkills is null || !newUserSkills.Any())
        {
            return oldUserSkills;
        }

        var skillsToBeRemoved = oldUserSkills.Except(newUserSkills).ToList();

        return skillsToBeRemoved;
    }

    public async Task AddNewSkillsAsync(List<string> skills)
    {
        var allSkills = await _skillRepository.GetAllSkillsAsync();

        var newSkills = skills.Where(x => x != "" && !allSkills.Any(y => y.Skill.ToLower().Equals(x.ToLower()))).ToList();

        if (newSkills.Any())
        {
            var newSkillsEntities = newSkills.Select(x => new SkillEntity
            {
                Skill = x
            }).ToList();

            await _skillRepository.AddSkillsAsync(newSkillsEntities);
        }
    }

    public async Task UpdateUserProfileByCVDataAsync(UserProfileEntity oldUserProfile)
    { 
        if (oldUserProfile is null)
        {
            throw new ModelNotFoundException("An error occured while fetching your profile!");
        }

        var extractedText = _pdfService.ExtractTextFromPdfByUsername(oldUserProfile.User.Username);
        if (!String.IsNullOrEmpty(extractedText))
        {
            var extractedData = await _openAIService.ExtractInformationFromTextAsync(extractedText);

            string firstName = ExtractSection(extractedData, "First Name:");
            string lastName = ExtractSection(extractedData, "Last Name:");
            string skills = ExtractSection(extractedData, "Skills:");
            string country = ExtractSection(extractedData, "Country:");
            string state = ExtractSection(extractedData, "State:");
            string city = ExtractSection(extractedData, "City:");
            string education = ExtractSection(extractedData, "Education:");
            string experience = ExtractSection(extractedData, "Experience:");

            if (!String.IsNullOrEmpty(firstName))
            {
                oldUserProfile.FirstName = firstName;
            }
            if (!String.IsNullOrEmpty(lastName))
            {
                oldUserProfile.LastName = lastName;
            }
            if (!String.IsNullOrEmpty(country) || !String.IsNullOrEmpty(state) || !String.IsNullOrEmpty(city))
            {
                await UpdateUserLocation(oldUserProfile, country, state, city);
            }
            if (!String.IsNullOrEmpty(education))
            {
                oldUserProfile.Education = education;
            }
            if (!String.IsNullOrEmpty(experience))
            {
                oldUserProfile.Experience = experience;
            }

            List<string> skillsList = skills.Split("\n").ToList().Select(x => x.Trim()).ToList();
            if (skillsList.Any())
            {
                await AddNewSkillsAsync(skillsList);

                var oldUserSkills = oldUserProfile.Skills is not null? oldUserProfile.Skills.Select(x => x.SkillName).ToList() : new List<string>();
                var newUserSkills = skillsList;

                var skillsToBeAdded = GetUserSkillsToBeAdded(oldUserSkills, newUserSkills);
                var skillsToBeRemoved = GetUserSkillsToBeRemoved(oldUserSkills, newUserSkills);

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

                await _userProfileSkillMappingRepository.DeleteUserProfileSkillMappingsAsync(UserProfileSkillMappingsToBeRemoved);
                await _userProfileSkillMappingRepository.AddUserProfileSkillMappingsAsync(UserProfileSkillMappingsToBeAdded);
            }
        }

        await _userProfileRepository.UpdateUserProfileAsync(oldUserProfile);
    }

    private static string ExtractSection(string response, string sectionTitle)
    {
        var lines = response.Split("\n");
        bool sectionStarted = false;
        string sectionContent = "";

        foreach (var line in lines)
        {
            if (line.StartsWith(sectionTitle))
            {
                sectionStarted = true;
                sectionContent = line.Substring(sectionTitle.Length).Trim();
            }
            else if (sectionStarted && (line.StartsWith("First Name:") || line.StartsWith("Last Name:") ||
                                        line.StartsWith("Skills:") || line.StartsWith("Country:") ||
                                        line.StartsWith("State:") || line.StartsWith("City:") ||
                                        line.StartsWith("Education:") || line.StartsWith("Experience:")))
            {
                break;
            }
            else if (sectionStarted)
            {
                sectionContent += "\n" + line.Trim();
            }
        }

        return sectionContent.Trim();
    }

    private async Task UpdateUserLocation(UserProfileEntity oldUserProfile, string country, string state, string city)
    {
        if (!String.IsNullOrEmpty(country))
        {
            var countries = await _locationCommunicationService.GetCountriesByNamesAsync(new List<string> { country });
            if (countries.Any())
            {
                oldUserProfile.Country = countries.FirstOrDefault().CountryIso2Code;
            }
            else
            {
                oldUserProfile.Country = null;
            }
        }
        else
        {
            oldUserProfile.Country = null;
        }

        if (!String.IsNullOrEmpty(state))
        {
            if (oldUserProfile.Country is null)
            {
                oldUserProfile.State = null;
                oldUserProfile.City = null;
                return;
            }

            var countryStateCityRegions = new List<CountryStateCityRegionDto>
            {
                 new CountryStateCityRegionDto
                {
                    State = state,
                    Country = oldUserProfile.Country
                }
            };

            var states = await _locationCommunicationService.GetStatesByCountryAndStateNamesAsync(countryStateCityRegions);
            if (states.Any())
            {
                oldUserProfile.State = states.FirstOrDefault().StateCode;
                if (oldUserProfile.Country != states.FirstOrDefault().CountryIso2Code)
                {
                    oldUserProfile.Country = states.FirstOrDefault().CountryIso2Code;
                }
            }
            else
            {
                oldUserProfile.State = null;
            }
        }
        else
        {

            oldUserProfile.State = null;
        }

        if (!String.IsNullOrEmpty(city))
        {
            var cities = await _locationCommunicationService.GetCitiesByCityAndCountryNames(new List<CountryStateCityRegionDto>
            {
                new CountryStateCityRegionDto
                {
                    City = city,
                    Country = oldUserProfile.Country
                }
            });
            if (cities.Any())
            {
                oldUserProfile.City = cities.FirstOrDefault().City;
                if (cities.Count == 1)
                {
                    if (oldUserProfile.State != cities.FirstOrDefault().StateCode)
                    {
                        oldUserProfile.State = cities.FirstOrDefault().StateCode;
                    }

                    if (oldUserProfile.Country != cities.FirstOrDefault().CountryIso2Code)
                    {
                        oldUserProfile.Country = cities.FirstOrDefault().CountryIso2Code;
                    }
                }
            }
            else
            {
                oldUserProfile.City = null;
            }
        }
        else
        {
            oldUserProfile.City = null;
        }
    }
}
