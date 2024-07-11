using AutoMapper;
using User.Data.Access.Helpers;
using User.Data.Access.Helpers.DTO.User;
using User.Data.Contracts;
using User.Data.Contracts.Helpers.DTO.Message;
using User.Data.Object.Entities;
using User.Services.Business.Exceptions;
using User.Services.Contracts;
using Newtonsoft.Json;
using Project.Services.Business.Exceptions;

namespace User.Services.Business;
public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IUserProfileRepository _userProfileRepository;
    private readonly IEmailService _emailService;
    private readonly IQueueMessageSenderService _queueMessageSenderService;
    private readonly ITokenService _tokenService;
    private readonly IPdfService _pdfService;
    private readonly IUserProfileDataProcessorService _userProfileDataProcessorService;
    private readonly IEncryptionService _encryptyonService;
    private readonly IMapper _mapper;

    public UserService(
        IUserRepository userRepository, 
        IUserProfileRepository userProfileRepository,
        IEmailService emailService,
        IQueueMessageSenderService queueMessageSenderService,
        ITokenService tokenService,
        IPdfService pdfService,
        IUserProfileDataProcessorService userProfileDataProcessorService,
        IEncryptionService encryptionService,
        IMapper mapper
        )
    {
        _userRepository = userRepository;
        _userProfileRepository = userProfileRepository;
        _emailService = emailService;
        _queueMessageSenderService = queueMessageSenderService;
        _tokenService = tokenService;
        _pdfService = pdfService;
        _userProfileDataProcessorService = userProfileDataProcessorService;
        _encryptyonService = encryptionService;
        _mapper = mapper;
    }

    public async Task<FilteredUsersPaginationDto> GetFilteredUsersPaginatedAsync(FilteredUsersSearchDto userSearchDto)
    {
        var users = await _userRepository.GetFilteredUsersAsync(userSearchDto.Username, userSearchDto.Country, userSearchDto.State, userSearchDto.City, userSearchDto.Education, userSearchDto.Experience);

        var totalUsers = users.Count;

        var paginatedUsers = users
            .Skip(userSearchDto.Page * userSearchDto.PageSize)
            .Take(userSearchDto.PageSize)
            .ToList();

        var filteredUsers = paginatedUsers.Select(u => _mapper.Map<FilteredUserDto>(u)).ToList();

        return new FilteredUsersPaginationDto
        {
            Users = filteredUsers,
            NumberOfUsers = totalUsers
        };
    }

    public async Task RegisterUserAsync(UserRegistrationDto user)
    {
        if (user.Password != user.RepeatPassword)
        {
            throw new PasswordException("Passwords do not match!");
        }

        if (await VerifyExistingUsernameAsync(user.Username))
        {
            throw new AlreadyExistsException("Username already exists!");
        }

        if (await VerifyExistingEmailAsync(user.Email))
        {
            throw new AlreadyExistsException("Email already exists!");
        }

        var userEntity = _mapper.Map<UserEntity>(user);
        var userProfileEntity = _mapper.Map<UserProfileEntity>(user);
        userProfileEntity.User = userEntity;

        userEntity.Password = _encryptyonService.GenerateHashedPassword(user.Password);
        userEntity.RegistrationToken = await _tokenService.GenerateRandomTokenAsync() + userEntity.Username;
        userEntity.RegistrationDate = DateTime.Now;

        var uri = $"{AppConstants.FE_APP_VERIFY_EMAIL_URL}/{userEntity.RegistrationToken}";

        await _userRepository.AddUserAsync(userEntity);
        await _userProfileRepository.AddUserProfileAsync(userProfileEntity);

        await _emailService.SendEmailVerificationAsync(userEntity.Username, uri, userEntity.Email);

        if (user.UserCV is not null)
        {
            await _pdfService.SaveUserCVAsync(user.UserCV, userProfileEntity);
            await _userProfileDataProcessorService.UpdateUserProfileByCVDataAsync(userProfileEntity);
        }
    }

    public async Task<bool> VerifyIfResetPasswordTokenExistsAsync(string resetPasswordToken)
    {
        var user = await _userRepository.GetUserByResetPasswordTokenAsync(resetPasswordToken);

        if (user is null || user.ResetPasswordTokenExpirationDate < DateTime.Now)
        {
            return false;
        }

        return true;
    }

    public async Task VerifyEmailByRegistrationTokenAsync(string registrationToken)
    {
        var user = await _userRepository.GetUserByRegistrationTokenAsync(registrationToken);

        if (user is null)
        {
            throw new ModelNotFoundException("User Not found!");
        }

        user.IsEmailConfirmed = true;

        await _userRepository.UpdateUserAsync(user);
    }

    public async Task SendResetPasswordTokenByEmailAsync(string email)
    {
        var user = await _userRepository.GetUserByEmailAsync(email);

        if (user is null)
        {
            throw new ModelNotFoundException("Email Not found!");
        }

        if (user.IsEmailConfirmed == false)
        {
            throw new AuthenticationException("Email is not confirmed!");
        }

        user.ResetPasswordToken = await _tokenService.GenerateRandomTokenAsync() + email;
        user.ResetPasswordTokenExpirationDate = DateTime.Now.AddMinutes(AppConstants.RESET_PASSWORD_TOKEN_VALIDATION_TIME);

        await _userRepository.UpdateUserAsync(user);

        var uri = Path.Combine(AppConstants.FE_APP_CHANGE_PASSWORD_URL, user.ResetPasswordToken);

        await _emailService.SendResetPasswordEmailAsync(user.Username, uri, user.Email);
    }

    public async Task ChangePasswordAsync(UserChangePasswordDto changePasswordDto)
    {
        if (changePasswordDto.NewPassword != changePasswordDto.RepeatNewPassword)
        {
            throw new PasswordException("Passwords do not match!");
        }

        var user = await _userRepository.GetUserByResetPasswordTokenAsync(changePasswordDto.ResetPasswordToken);

        if (user is null)
        {
            throw new ModelNotFoundException("User Not found!");
        }

        if (user.ResetPasswordTokenExpirationDate < DateTime.Now)
        {
            throw new TokenExpiredException("Reset Password link has expired!");
        }

        user.Password = _encryptyonService.GenerateHashedPassword(changePasswordDto.NewPassword);
        user.ResetPasswordToken = null;
        user.ResetPasswordTokenExpirationDate = null;

        await _userRepository.UpdateUserAsync(user);
    }

    public async Task DeleteUserByUsernameAsync(string username)
    {
        var user = await _userRepository.GetUserByUsernameAsync(username);

        if (user is null)
        {
            throw new ModelNotFoundException("User Not found!");
        }

        var queueMessage = new UserMessageDto
        {
            UserMessageDetails = _mapper.Map<UserMessageDetailsDto>(user.UserProfile),
            MessageType = AppConstants.USER_DELETE_MESSAGE
        };

        _pdfService.DeleteUserCV(username);
        
        await _queueMessageSenderService.SendMessageAsync(JsonConvert.SerializeObject(queueMessage));
        await _userRepository.DeleteUserAsync(user);
    }

    private async Task<bool> VerifyExistingUsernameAsync(string username)
    {
        var user = await _userRepository.GetUserByUsernameOrEmailAsync(username);
        return user is not null;
    }

    private async Task<bool> VerifyExistingEmailAsync(string email)
    {
        var user = await _userRepository.GetUserByUsernameOrEmailAsync(email);
        return user is not null;
    }
}
