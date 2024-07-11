using AutoMapper;
using User.Data.Access.Helpers.DTO.User;
using User.Data.Access.Helpers.DTO.UserProfile;
using User.Data.Contracts.Helpers.DTO.Message;
using User.Data.Object.Entities;

namespace User.Data.Access.Helpers;
public class Mapper : Profile
{
    public Mapper()
    {
        CreateMap<UserRegistrationDto, UserEntity>()
            .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.Username))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.Password, opt => opt.MapFrom(src => src.Password));


        CreateMap<UserRegistrationDto, UserProfileEntity>()
            .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName))
            .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName));

        CreateMap<UserEntity, FilteredUserDto>()
            .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.UserProfile.FirstName))
            .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.UserProfile.LastName))
            .ForMember(dest => dest.Country, opt => opt.MapFrom(src => src.UserProfile.Country))
            .ForMember(dest => dest.State, opt => opt.MapFrom(src => src.UserProfile.State))
            .ForMember(dest => dest.City, opt => opt.MapFrom(src => src.UserProfile.City))
            .ForMember(dest => dest.Education, opt => opt.MapFrom(src => src.UserProfile.Education))
            .ForMember(dest => dest.Experience, opt => opt.MapFrom(src => src.UserProfile.Experience))
            .ForMember(dest => dest.Skills, opt => opt.MapFrom(src => src.UserProfile.Skills.Select(x => x.SkillName).ToList()));

        CreateMap<UserEntity, UserProfileDto>()
            .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.Username))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.UserProfile.FirstName))
            .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.UserProfile.LastName))
            .ForMember(dest => dest.Country, opt => opt.MapFrom(src => src.UserProfile.Country))
            .ForMember(dest => dest.State, opt => opt.MapFrom(src => src.UserProfile.State))
            .ForMember(dest => dest.City, opt => opt.MapFrom(src => src.UserProfile.City))
            .ForMember(dest => dest.Education, opt => opt.MapFrom(src => src.UserProfile.Education))
            .ForMember(dest => dest.Experience, opt => opt.MapFrom(src => src.UserProfile.Experience))
            .ForMember(dest => dest.UserCV, opt => opt.MapFrom(src => src.UserProfile.UserCV))
            .ForMember(dest => dest.Skills, opt => opt.MapFrom(src => src.UserProfile.Skills.Select(x => x.SkillName).ToList()));

        CreateMap<UserProfileEntity, UserMessageDetailsDto>()
            .ForMember(dest => dest.UserProfileId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Country, opt => opt.MapFrom(src => src.Country))
            .ForMember(dest => dest.State, opt => opt.MapFrom(src => src.State))
            .ForMember(dest => dest.City, opt => opt.MapFrom(src => src.City))
            .ForMember(dest => dest.Education, opt => opt.MapFrom(src => src.Education))
            .ForMember(dest => dest.Experience, opt => opt.MapFrom(src => src.Experience))
            .ForMember(dest => dest.Skills, opt => opt.MapFrom(src => src.Skills.Select(x => x.SkillName).ToList()));
    }
}
