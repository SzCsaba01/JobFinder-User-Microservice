using AutoMapper;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
using System.Net;
using System.Text;
using System.Text.Json;
using User.Data.Access.Helpers;
using User.Data.Access.Helpers.DTO.Authentication;
using User.Data.Access.Helpers.DTO.User;
using User.Data.Access.Helpers.DTO.UserProfile;
using User.Data.Contracts;
using User.Data.Contracts.Helpers.DTO.Location;
using User.Data.Contracts.Helpers.DTO.Message;
using User.Data.Object.Entities;
using User.Services.Business;
using User.Services.Contracts;
using Xunit;

public class ServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IEncryptionService> _encryptionServiceMock;
    private readonly Mock<ITokenService> _tokenServiceMock;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly Mock<ISkillRepository> _skillRepositoryMock;
    private readonly Mock<IUserProfileRepository> _userProfileRepositoryMock;
    private readonly Mock<IUserProfileSkillMappingRepository> _userProfileSkillMappingRepositoryMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly Mock<IQueueMessageSenderService> _queueMessageSenderServiceMock;
    private readonly Mock<ILocationCommunicationService> _locationCommunicationServiceMock;
    private readonly Mock<IOpenAIService> _openAIServiceMock;
    private readonly Mock<IPdfService> _pdfServiceMock;
    private readonly Mock<IUserProfileDataProcessorService> _userProfileDataProcessorServiceMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IServer> _serverMock;

    private readonly AuthenticationService _authenticationService;
    private readonly EmailService _emailService;
    private readonly EncryptionService _encryptionService;
    private readonly LocationCommunicationService _locationCommunicationService;
    private readonly PdfService _pdfService;
    private readonly SkillService _skillService;
    private readonly TokenService _tokenService;
    private readonly UserProfileService _userProfileService;
    private readonly UserService _userService;
    private readonly UserProfileDataProcessorService _userProfileDataProcessorService;

    public ServiceTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _encryptionServiceMock = new Mock<IEncryptionService>();
        _tokenServiceMock = new Mock<ITokenService>();
        _configurationMock = new Mock<IConfiguration>();
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        _httpClientFactoryMock = new Mock<IHttpClientFactory>();
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        _skillRepositoryMock = new Mock<ISkillRepository>();
        _userProfileRepositoryMock = new Mock<IUserProfileRepository>();
        _userProfileSkillMappingRepositoryMock = new Mock<IUserProfileSkillMappingRepository>();
        _emailServiceMock = new Mock<IEmailService>();
        _queueMessageSenderServiceMock = new Mock<IQueueMessageSenderService>();
        _locationCommunicationServiceMock = new Mock<ILocationCommunicationService>();
        _openAIServiceMock = new Mock<IOpenAIService>();
        _pdfServiceMock = new Mock<IPdfService>();
        _userProfileDataProcessorServiceMock = new Mock<IUserProfileDataProcessorService>();
        _mapperMock = new Mock<IMapper>();
        _serverMock = new Mock<IServer>();

        _authenticationService = new AuthenticationService(
            _userRepositoryMock.Object,
            _encryptionServiceMock.Object,
            _tokenServiceMock.Object);

        _emailService = new EmailService(
            _userRepositoryMock.Object,
            _configurationMock.Object);

        _encryptionService = new EncryptionService();

        var httpClient = new HttpClient(_httpMessageHandlerMock.Object);
        _locationCommunicationService = new LocationCommunicationService(httpClient);

        _pdfService = new PdfService();

        _skillService = new SkillService(_skillRepositoryMock.Object);

        _tokenService = new TokenService(_configurationMock.Object);

        _userProfileService = new UserProfileService(
            _userRepositoryMock.Object,
            _userProfileRepositoryMock.Object,
            _userProfileSkillMappingRepositoryMock.Object,
            _skillRepositoryMock.Object,
            _queueMessageSenderServiceMock.Object,
            _pdfServiceMock.Object,
            _userProfileDataProcessorServiceMock.Object,
            _mapperMock.Object);

        _userService = new UserService(
            _userRepositoryMock.Object,
            _userProfileRepositoryMock.Object,
            _emailServiceMock.Object,
            _queueMessageSenderServiceMock.Object,
            _tokenServiceMock.Object,
            _pdfServiceMock.Object,
            _userProfileDataProcessorServiceMock.Object,
            _encryptionServiceMock.Object,
            _mapperMock.Object);

        _userProfileDataProcessorService = new UserProfileDataProcessorService(
            _userProfileRepositoryMock.Object,
            _skillRepositoryMock.Object,
            _userProfileSkillMappingRepositoryMock.Object,
            _pdfServiceMock.Object,
            _openAIServiceMock.Object,
            _locationCommunicationServiceMock.Object,
            _mapperMock.Object);
    }

    // AuthenticationService Tests
    [Fact]
    public async Task AuthenticationService_LoginAsync_ValidCredentials_ReturnsAuthResponse()
    {
        var request = new AuthenticationRequestDto { UserCredential = "test", Password = "password" };
        var user = new UserEntity { IsEmailConfirmed = true, Role = new RoleEntity { RoleName = "User" } };

        _encryptionServiceMock.Setup(x => x.GenerateHashedPassword(request.Password)).Returns("hashedpassword");
        _userRepositoryMock.Setup(x => x.GetUserByUsernameOrEmailAndPasswordAsync(request.UserCredential, "hashedpassword")).ReturnsAsync(user);
        _tokenServiceMock.Setup(x => x.GetAuthentificationJwtAsync(user)).ReturnsAsync("token");

        var result = await _authenticationService.LoginAsync(request);

        Assert.NotNull(result);
        Assert.Equal("token", result.Token);
        Assert.Equal("User", result.Role);
    }

    // EncryptionService Tests
    [Fact]
    public void EncryptionService_GenerateHashedPassword_ReturnsHashedPassword()
    {
        var password = "password";

        var result = _encryptionService.GenerateHashedPassword(password);

        Assert.NotNull(result);
    }

    // LocationCommunicationService Tests
    [Fact]
    public async Task LocationCommunicationService_GetCitiesByCityAndCountryNames_ReturnsLocationDtos()
    {
        var httpMessageHandler = new Mock<HttpMessageHandler>();
        var httpClient = new HttpClient(httpMessageHandler.Object)
        {
            BaseAddress = new Uri("http://test.com/")
        };

        var service = new LocationCommunicationService(httpClient);
        var countryStateCityRegions = new List<CountryStateCityRegionDto> { new CountryStateCityRegionDto() };

        httpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(new List<LocationDto>()))
            });

        var result = await service.GetCitiesByCityAndCountryNames(countryStateCityRegions);

        Assert.NotNull(result);
        Assert.IsType<List<LocationDto>>(result);
    }

    [Fact]
    public async Task LocationCommunicationService_GetCountriesByNamesAsync_ReturnsLocationDtos()
    {
        var httpMessageHandler = new Mock<HttpMessageHandler>();
        var httpClient = new HttpClient(httpMessageHandler.Object)
        {
            BaseAddress = new Uri("http://test.com/")
        };

        var service = new LocationCommunicationService(httpClient);
        var countryNames = new List<string> { "Country1" };

        httpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(new List<LocationDto>()))
            });

        var result = await service.GetCountriesByNamesAsync(countryNames);

        Assert.NotNull(result);
        Assert.IsType<List<LocationDto>>(result);
    }

    [Fact]
    public async Task LocationCommunicationService_GetStatesByNamesAsync_ReturnsLocationDtos()
    {
        var httpMessageHandler = new Mock<HttpMessageHandler>();
        var httpClient = new HttpClient(httpMessageHandler.Object)
        {
            BaseAddress = new Uri("http://test.com/")
        };

        var service = new LocationCommunicationService(httpClient);
        var stateNames = new List<string> { "State1" };

        httpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(new List<LocationDto>()))
            });

        var result = await service.GetStatesByNamesAsync(stateNames);

        Assert.NotNull(result);
        Assert.IsType<List<LocationDto>>(result);
    }

    // PdfService Tests
    [Fact]
    public void PdfService_DeleteUserCV_DeletesDirectory()
    {
        var username = "testuser";
        var folderName = Path.Combine(AppConstants.RESOURCES, AppConstants.CV, username);
        var pathToDelete = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).FullName, AppConstants.MICROSERVICE_NAME, folderName);

        Directory.CreateDirectory(pathToDelete);

        _pdfService.DeleteUserCV(username);

        Assert.False(Directory.Exists(pathToDelete));
    }

    [Fact]
    public async Task PdfService_SaveUserCVAsync_SavesFile()
    {
        var serverAddress = "http://localhost";
        _serverMock.Setup(s => s.Features.Get<IServerAddressesFeature>().Addresses).Returns(new List<string> { serverAddress });

        var formFileMock = new Mock<IFormFile>();
        var userProfile = new UserProfileEntity { User = new UserEntity { Username = "test_user" } };

        var ms = new MemoryStream(Encoding.UTF8.GetBytes("Dummy PDF content"));
        formFileMock.Setup(_ => _.OpenReadStream()).Returns(ms);
        formFileMock.Setup(_ => _.FileName).Returns("dummy.pdf");
        formFileMock.Setup(_ => _.Length).Returns(ms.Length);
        formFileMock.Setup(_ => _.ContentType).Returns("application/pdf");

        var pdfService = new PdfService();

        await pdfService.SaveUserCVAsync(formFileMock.Object, userProfile);

        var filePath = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).FullName, AppConstants.MICROSERVICE_NAME, AppConstants.RESOURCES, AppConstants.CV, "test_user", "test_user_CV.pdf");
        Assert.True(File.Exists(filePath));
        File.Delete(filePath);
    }

    // SkillService Tests
    [Fact]
    public async Task SkillService_GetAllSkillsAsync_ReturnsSkills()
    {
        var skills = new List<SkillEntity>
        {
            new SkillEntity { Skill = "Skill1" },
            new SkillEntity { Skill = "Skill2" }
        };

        _skillRepositoryMock.Setup(x => x.GetAllSkillsAsync()).ReturnsAsync(skills);

        var result = await _skillService.GetAllSkillsAsync();

        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task SkillService_DeleteUnmappedSkillsAsync_DeletesSkills()
    {
        var skills = new List<SkillEntity>
        {
            new SkillEntity { Skill = "Skill1" },
            new SkillEntity { Skill = "Skill2" }
        };

        _skillRepositoryMock.Setup(x => x.GetAllUnmappedSkills()).ReturnsAsync(skills);

        await _skillService.DeleteUnmappedSkillsAsync();

        _skillRepositoryMock.Verify(x => x.DeleteSkillsAsync(skills), Times.Once);
    }

    // TokenService Tests
    [Fact]
    public async Task TokenService_GenerateRandomTokenAsync_ReturnsToken()
    {
        var result = await _tokenService.GenerateRandomTokenAsync();

        Assert.NotNull(result);
    }

    [Fact]
    public async Task TokenService_GetAuthentificationJwtAsync_ReturnsJwtToken()
    {
        var configMock = new Mock<IConfiguration>();
        var jwtSectionMock = new Mock<IConfigurationSection>();
        jwtSectionMock.Setup(s => s.Value).Returns("your_jwt_secret_key");
        configMock.Setup(c => c.GetSection("Jwt:Key")).Returns(jwtSectionMock.Object);

        var user = new UserEntity { UserProfile = new UserProfileEntity { Id = Guid.NewGuid() }, Username = "test_user", Role = new RoleEntity { RoleName = "User" } };

        var tokenService = new TokenService(configMock.Object);

        var result = await tokenService.GetAuthentificationJwtAsync(user);

        Assert.NotNull(result);
        Assert.IsType<string>(result);
    }

    // UserProfileService Tests
    [Fact]
    public async Task UserProfileService_EditUserProfileByIdAsync_UpdatesUserProfile()
    {
        var userProfileId = Guid.NewGuid();
        var oldUserProfile = new UserProfileEntity
        {
            Id = userProfileId,
            User = new UserEntity { Username = "test_user" },
            Skills = new List<UserProfileSkillMapping>()
        };

        var newUserProfile = new UserProfileDto
        {
            FirstName = "NewFirstName",
            LastName = "NewLastName",
            Skills = new List<string> { "C#", "ASP.NET" }
        };

        _userProfileRepositoryMock.Setup(x => x.GetUserProfileByIdAsync(userProfileId)).ReturnsAsync(oldUserProfile);
        _userProfileDataProcessorServiceMock.Setup(x => x.AddNewSkillsAsync(It.IsAny<List<string>>())).Returns(Task.CompletedTask);
        _userProfileDataProcessorServiceMock.Setup(x => x.GetUserSkillsToBeAdded(It.IsAny<List<string>>(), It.IsAny<List<string>>())).Returns(new List<string>());
        _userProfileDataProcessorServiceMock.Setup(x => x.GetUserSkillsToBeRemoved(It.IsAny<List<string>>(), It.IsAny<List<string>>())).Returns(new List<string>());

        await _userProfileService.EditUserProfileByIdAsync(userProfileId, newUserProfile, false);

        _userProfileRepositoryMock.Verify(x => x.UpdateUserProfileAsync(It.Is<UserProfileEntity>(u => u.FirstName == newUserProfile.FirstName)), Times.Once);
    }

    [Fact]
    public async Task UserProfileService_SendRecommandJobsMessageAsync_SendsMessage()
    {
        var userProfileId = Guid.NewGuid();
        var userProfile = new UserProfileEntity
        {
            Id = userProfileId,
            Skills = new List<UserProfileSkillMapping>
            {
                new UserProfileSkillMapping { SkillName = "Skill1" },
                new UserProfileSkillMapping { SkillName = "Skill2" },
                new UserProfileSkillMapping { SkillName = "Skill3" }
            }
        };

        _userProfileRepositoryMock.Setup(x => x.GetUserProfileByIdAsync(userProfileId)).ReturnsAsync(userProfile);
        _mapperMock.Setup(x => x.Map<UserMessageDetailsDto>(userProfile)).Returns(new UserMessageDetailsDto());

        await _userProfileService.SendRecommandJobsMessageAsync(userProfileId);

        _queueMessageSenderServiceMock.Verify(x => x.SendMessageAsync(It.IsAny<string>()), Times.Once);
    }

    // UserService Tests
    [Fact]
    public async Task UserService_GetFilteredUsersPaginatedAsync_ReturnsFilteredUsers()
    {
        var userSearchDto = new FilteredUsersSearchDto { Page = 0, PageSize = 1 };
        var users = new List<UserEntity> { new UserEntity() };

        _userRepositoryMock.Setup(x => x.GetFilteredUsersAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(users);

        var result = await _userService.GetFilteredUsersPaginatedAsync(userSearchDto);

        Assert.Single(result.Users);
    }


    [Fact]
    public async Task UserService_RegisterUserAsync_RegistersUser()
    {
        var userRegistrationDto = new UserRegistrationDto
        {
            Username = "testuser",
            Password = "password",
            RepeatPassword = "password",
            Email = "test@example.com"
        };

        _mapperMock.Setup(x => x.Map<UserEntity>(userRegistrationDto)).Returns(new UserEntity { Username = "testuser" });
        _mapperMock.Setup(x => x.Map<UserProfileEntity>(userRegistrationDto)).Returns(new UserProfileEntity { User = new UserEntity { Username = "testuser" } });
        _encryptionServiceMock.Setup(x => x.GenerateHashedPassword("password")).Returns("hashedpassword");
        _tokenServiceMock.Setup(x => x.GenerateRandomTokenAsync()).ReturnsAsync("token");

        await _userService.RegisterUserAsync(userRegistrationDto);

        _userRepositoryMock.Verify(x => x.AddUserAsync(It.IsAny<UserEntity>()), Times.Once);
        _userProfileRepositoryMock.Verify(x => x.AddUserProfileAsync(It.IsAny<UserProfileEntity>()), Times.Once);
    }

    [Fact]
    public async Task UserService_VerifyIfResetPasswordTokenExistsAsync_ReturnsTrue()
    {
        var token = "token";
        var user = new UserEntity { ResetPasswordTokenExpirationDate = DateTime.Now.AddMinutes(10) };

        _userRepositoryMock.Setup(x => x.GetUserByResetPasswordTokenAsync(token)).ReturnsAsync(user);

        var result = await _userService.VerifyIfResetPasswordTokenExistsAsync(token);

        Assert.True(result);
    }

    [Fact]
    public async Task UserService_VerifyEmailByRegistrationTokenAsync_VerifiesEmail()
    {
        var token = "token";
        var user = new UserEntity { RegistrationDate = DateTime.Now.AddHours(-1) };

        _userRepositoryMock.Setup(x => x.GetUserByRegistrationTokenAsync(token)).ReturnsAsync(user);

        await _userService.VerifyEmailByRegistrationTokenAsync(token);

        _userRepositoryMock.Verify(x => x.UpdateUserAsync(user), Times.Once);
    }

    [Fact]
    public async Task UserService_SendResetPasswordTokenByEmailAsync_SendsEmail()
    {
        var email = "test@example.com";
        var user = new UserEntity { IsEmailConfirmed = true, Username = "testuser" };

        _userRepositoryMock.Setup(x => x.GetUserByEmailAsync(email)).ReturnsAsync(user);
        _tokenServiceMock.Setup(x => x.GenerateRandomTokenAsync()).ReturnsAsync("token");

        await _userService.SendResetPasswordTokenByEmailAsync(email);

        _userRepositoryMock.Verify(x => x.UpdateUserAsync(user), Times.Once);
    }

    [Fact]
    public async Task UserService_ChangePasswordAsync_ChangesPassword()
    {
        var changePasswordDto = new UserChangePasswordDto
        {
            NewPassword = "newpassword",
            RepeatNewPassword = "newpassword",
            ResetPasswordToken = "token"
        };

        var user = new UserEntity { ResetPasswordTokenExpirationDate = DateTime.Now.AddMinutes(10) };

        _userRepositoryMock.Setup(x => x.GetUserByResetPasswordTokenAsync(changePasswordDto.ResetPasswordToken)).ReturnsAsync(user);
        _encryptionServiceMock.Setup(x => x.GenerateHashedPassword("newpassword")).Returns("hashedpassword");

        await _userService.ChangePasswordAsync(changePasswordDto);

        _userRepositoryMock.Verify(x => x.UpdateUserAsync(user), Times.Once);
    }

    [Fact]
    public async Task UserService_DeleteUserByUsernameAsync_DeletesUser()
    {
        var username = "testuser";
        var user = new UserEntity
        {
            Username = username,
            UserProfile = new UserProfileEntity
            {
                Id = Guid.NewGuid(),
                Skills = new List<UserProfileSkillMapping> { new UserProfileSkillMapping { SkillName = "Skill1" } }
            }
        };

        _userRepositoryMock.Setup(x => x.GetUserByUsernameAsync(username)).ReturnsAsync(user);

        await _userService.DeleteUserByUsernameAsync(username);

        _userRepositoryMock.Verify(x => x.DeleteUserAsync(user), Times.Once);
    }

    //UserProfileDataProcessorService Tests
    [Fact]
    public void GetUserSkillsToBeAdded_WithValidSkills_ReturnsSkillsToBeAdded()
    {
        var oldSkills = new List<string> { "C#", "ASP.NET" };
        var newSkills = new List<string> { "C#", "ASP.NET", "JavaScript" };

        var result = _userProfileDataProcessorService.GetUserSkillsToBeAdded(oldSkills, newSkills);

        Assert.Single(result);
        Assert.Contains("JavaScript", result);
    }

    [Fact]
    public void GetUserSkillsToBeRemoved_WithValidSkills_ReturnsSkillsToBeRemoved()
    {
        var oldSkills = new List<string> { "C#", "ASP.NET", "JavaScript" };
        var newSkills = new List<string> { "C#", "ASP.NET" };

        var result = _userProfileDataProcessorService.GetUserSkillsToBeRemoved(oldSkills, newSkills);

        Assert.Single(result);
        Assert.Contains("JavaScript", result);
    }

    [Fact]
    public async Task AddNewSkillsAsync_WithNewSkills_AddsSkills()
    {
        var skills = new List<string> { "C#", "ASP.NET", "JavaScript" };
        var existingSkills = new List<SkillEntity> { new SkillEntity { Skill = "C#" }, new SkillEntity { Skill = "ASP.NET" } };

        _skillRepositoryMock.Setup(x => x.GetAllSkillsAsync()).ReturnsAsync(existingSkills);

        await _userProfileDataProcessorService.AddNewSkillsAsync(skills);

        _skillRepositoryMock.Verify(x => x.AddSkillsAsync(It.Is<List<SkillEntity>>(list => list.Count == 1 && list[0].Skill == "JavaScript")), Times.Once);
    }

    [Fact]
    public async Task AddNewSkillsAsync_WithNoNewSkills_DoesNotAddSkills()
    {
        var skills = new List<string> { "C#", "ASP.NET" };
        var existingSkills = new List<SkillEntity> { new SkillEntity { Skill = "C#" }, new SkillEntity { Skill = "ASP.NET" } };

        _skillRepositoryMock.Setup(x => x.GetAllSkillsAsync()).ReturnsAsync(existingSkills);

        await _userProfileDataProcessorService.AddNewSkillsAsync(skills);

        _skillRepositoryMock.Verify(x => x.AddSkillsAsync(It.IsAny<List<SkillEntity>>()), Times.Never);
    }
}
