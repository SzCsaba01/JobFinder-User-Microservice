using Microsoft.EntityFrameworkCore;
using User.Data.Access;
using User.Data.Access.Data;
using User.Data.Object.Entities;
using Xunit;

namespace User.Tests
{
    public class RepositoryTests
    {
        private readonly DbContextOptions<DataContext> _contextOptions;

        public RepositoryTests()
        {
            _contextOptions = new DbContextOptionsBuilder<DataContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()) 
                .Options;
        }

        [Fact]
        public async Task GetAllUnmappedSkills_ReturnsSkillsWithoutUserProfiles()
        {
            using var context = new DataContext(_contextOptions);
            var unmappedSkill = new SkillEntity { Skill = "C#", UserProfiles = new List<UserProfileSkillMapping>() };
            var mappedSkill = new SkillEntity
            {
                Skill = "Java",
                UserProfiles = new List<UserProfileSkillMapping>
                {
                    new UserProfileSkillMapping
                    {
                        UserProfileId = Guid.NewGuid(),
                        SkillName = "Java",
                        UserProfile = new UserProfileEntity { Id = Guid.NewGuid(), FirstName = "John", LastName = "Doe" }
                    }
                }
            };

            context.Skills.AddRange(unmappedSkill, mappedSkill);
            await context.SaveChangesAsync();

            var repository = new SkillRepository(context);
            var result = await repository.GetAllUnmappedSkills();

            Assert.Single(result);
            Assert.Equal("C#", result.First().Skill);
        }

        [Fact]
        public async Task GetAllSkillsAsync_ReturnsAllSkills()
        {
            using var context = new DataContext(_contextOptions);
            context.Skills.AddRange(
                new SkillEntity { Skill = "C#" },
                new SkillEntity { Skill = "Java" }
            );
            await context.SaveChangesAsync();

            var repository = new SkillRepository(context);
            var result = await repository.GetAllSkillsAsync();

            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task AddSkillsAsync_AddsSkillsToDatabase()
        {
            using var context = new DataContext(_contextOptions);
            var skills = new List<SkillEntity> { new SkillEntity { Skill = "Python" } };

            var repository = new SkillRepository(context);
            await repository.AddSkillsAsync(skills);

            var skillInDb = await context.Skills.FirstOrDefaultAsync(s => s.Skill == "Python");
            Assert.NotNull(skillInDb);
        }

        [Fact]
        public async Task DeleteSkillsAsync_RemovesSkillsFromDatabase()
        {
            using var context = new DataContext(_contextOptions);
            var skills = new List<SkillEntity> { new SkillEntity { Skill = "C#" } };
            await context.Skills.AddRangeAsync(skills);
            await context.SaveChangesAsync();

            var repository = new SkillRepository(context);
            await repository.DeleteSkillsAsync(skills);

            var skillInDb = await context.Skills.FirstOrDefaultAsync(s => s.Skill == "C#");
            Assert.Null(skillInDb);
        }

        [Fact]
        public async Task GetUserProfileByIdAsync_ReturnsUserProfile()
        {
            using var context = new DataContext(_contextOptions);
            var user = new UserEntity
            {
                Id = Guid.NewGuid(),
                Username = "john.doe",
                Email = "john.doe@example.com",
                Password = "Password123!",
                RegistrationToken = "regtoken",
                IsEmailConfirmed = true,
                RegistrationDate = DateTime.UtcNow,
                RoleId = Guid.NewGuid()
            };
            var userProfile = new UserProfileEntity
            {
                Id = user.Id,
                UserId = user.Id,
                FirstName = "John",
                LastName = "Doe",
                User = user
            };
            user.UserProfile = userProfile;

            await context.Users.AddAsync(user);
            await context.UserProfiles.AddAsync(userProfile);
            await context.SaveChangesAsync();

            var repository = new UserProfileRepository(context);
            var result = await repository.GetUserProfileByIdAsync(userProfile.Id);

            Assert.Equal(userProfile.Id, result.Id);
        }

        [Fact]
        public async Task AddUserProfileAsync_AddsUserProfileToDatabase()
        {
            using var context = new DataContext(_contextOptions);
            var userProfile = new UserProfileEntity { Id = Guid.NewGuid(), FirstName = "John", LastName = "Doe" };

            var repository = new UserProfileRepository(context);
            await repository.AddUserProfileAsync(userProfile);

            var userProfileInDb = await context.UserProfiles.FirstOrDefaultAsync(up => up.Id == userProfile.Id);
            Assert.NotNull(userProfileInDb);
        }

        [Fact]
        public async Task UpdateUserProfileAsync_UpdatesUserProfileInDatabase()
        {
            using var context = new DataContext(_contextOptions);
            var userProfile = new UserProfileEntity { Id = Guid.NewGuid(), FirstName = "John", LastName = "Doe" };
            await context.UserProfiles.AddAsync(userProfile);
            await context.SaveChangesAsync();

            userProfile.FirstName = "Jane";
            var repository = new UserProfileRepository(context);
            await repository.UpdateUserProfileAsync(userProfile);

            var userProfileInDb = await context.UserProfiles.FirstOrDefaultAsync(up => up.Id == userProfile.Id);
            Assert.Equal("Jane", userProfileInDb.FirstName);
        }

        [Fact]
        public async Task AddUserProfileSkillMappingsAsync_AddsMappingsToDatabase()
        {
            using var context = new DataContext(_contextOptions);
            var userProfileId = Guid.NewGuid();
            var skillName = "C#";
            var userProfile = new UserProfileEntity { Id = userProfileId, FirstName = "John", LastName = "Doe" };
            var skill = new SkillEntity { Skill = skillName };

            await context.UserProfiles.AddAsync(userProfile);
            await context.Skills.AddAsync(skill);
            await context.SaveChangesAsync();

            var mappings = new List<UserProfileSkillMapping>
            {
                new UserProfileSkillMapping { UserProfileId = userProfileId, SkillName = skillName }
            };

            var repository = new UserProfileSkillMappingRepository(context);
            await repository.AddUserProfileSkillMappingsAsync(mappings);

            var mappingInDb = await context.UserProfileSkillMappings.FirstOrDefaultAsync(m => m.SkillName == skillName);
            Assert.NotNull(mappingInDb);
        }

        [Fact]
        public async Task DeleteUserProfileSkillMappingsAsync_RemovesMappingsFromDatabase()
        {
            using var context = new DataContext(_contextOptions);
            var userProfileId = Guid.NewGuid();
            var skillName = "C#";
            var userProfile = new UserProfileEntity { Id = userProfileId, FirstName = "John", LastName = "Doe" };
            var skill = new SkillEntity { Skill = skillName };

            await context.UserProfiles.AddAsync(userProfile);
            await context.Skills.AddAsync(skill);
            await context.SaveChangesAsync();

            var mappings = new List<UserProfileSkillMapping>
            {
                new UserProfileSkillMapping { UserProfileId = userProfileId, SkillName = skillName }
            };
            await context.UserProfileSkillMappings.AddRangeAsync(mappings);
            await context.SaveChangesAsync();

            var repository = new UserProfileSkillMappingRepository(context);
            await repository.DeleteUserProfileSkillMappingsAsync(mappings);

            var mappingInDb = await context.UserProfileSkillMappings.FirstOrDefaultAsync(m => m.SkillName == skillName);
            Assert.Null(mappingInDb);
        }

        [Fact]
        public async Task GetUserWithProfileByIdAsync_ReturnsUserWithProfile()
        {
            using var context = new DataContext(_contextOptions);
            var userId = Guid.NewGuid();
            var userProfile = new UserProfileEntity { Id = userId, FirstName = "John", LastName = "Doe" };
            var user = new UserEntity
            {
                Id = userId,
                Username = "john.doe",
                Email = "john.doe@example.com",
                Password = "Password123!",
                RegistrationToken = "regtoken",
                IsEmailConfirmed = true,
                RegistrationDate = DateTime.UtcNow,
                RoleId = Guid.NewGuid(),
                UserProfile = userProfile
            };
            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();

            var repository = new UserRepository(context);
            var result = await repository.GetUserWithProfileByIdAsync(userId);

            Assert.NotNull(result);
            Assert.Equal(userId, result?.Id);
        }

        [Fact]
        public async Task GetUserByUsernameAsync_ReturnsUser()
        {
            using var context = new DataContext(_contextOptions);
            var user = new UserEntity
            {
                Username = "testuser",
                Email = "testuser@example.com",
                Password = "Password123!",
                RegistrationToken = "regtoken",
                IsEmailConfirmed = true,
                RegistrationDate = DateTime.UtcNow,
                RoleId = Guid.NewGuid()
            };
            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();

            var repository = new UserRepository(context);
            var result = await repository.GetUserByUsernameAsync("testuser");

            Assert.NotNull(result);
            Assert.Equal("testuser", result?.Username);
        }

        [Fact]
        public async Task GetUserByEmailAsync_ReturnsUser()
        {
            using var context = new DataContext(_contextOptions);
            var user = new UserEntity
            {
                Username = "testuser",
                Email = "testuser@example.com",
                Password = "Password123!",
                RegistrationToken = "regtoken",
                IsEmailConfirmed = true,
                RegistrationDate = DateTime.UtcNow,
                RoleId = Guid.NewGuid()
            };
            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();

            var repository = new UserRepository(context);
            var result = await repository.GetUserByEmailAsync("testuser@example.com");

            Assert.NotNull(result);
            Assert.Equal("testuser@example.com", result?.Email);
        }

        [Fact]
        public async Task GetUserByUsernameOrEmailAsync_ReturnsUser()
        {
            using var context = new DataContext(_contextOptions);
            var user = new UserEntity
            {
                Username = "testuser",
                Email = "testuser@example.com",
                Password = "Password123!",
                RegistrationToken = "regtoken",
                IsEmailConfirmed = true,
                RegistrationDate = DateTime.UtcNow,
                RoleId = Guid.NewGuid()
            };
            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();

            var repository = new UserRepository(context);
            var result = await repository.GetUserByUsernameOrEmailAsync("testuser");

            Assert.NotNull(result);
            Assert.Equal("testuser", result?.Username);

            result = await repository.GetUserByUsernameOrEmailAsync("testuser@example.com");

            Assert.NotNull(result);
            Assert.Equal("testuser@example.com", result?.Email);
        }

        [Fact]
        public async Task GetUserByUsernameOrEmailAndPasswordAsync_ReturnsUser()
        {
            using var context = new DataContext(_contextOptions);
            var role = new RoleEntity { RoleName = "User" };
            await context.Roles.AddAsync(role);
            await context.SaveChangesAsync();

            var user = new UserEntity
            {
                Username = "testuser",
                Email = "testuser@example.com",
                Password = "Password123!",
                RegistrationToken = "regtoken",
                IsEmailConfirmed = true,
                RegistrationDate = DateTime.UtcNow,
                RoleId = role.Id
            };
            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();

            var repository = new UserRepository(context);
            var result = await repository.GetUserByUsernameOrEmailAndPasswordAsync("testuser", "Password123!");

            Assert.NotNull(result);
            Assert.Equal("testuser", result?.Username);

            result = await repository.GetUserByUsernameOrEmailAndPasswordAsync("testuser@example.com", "Password123!");

            Assert.NotNull(result);
            Assert.Equal("testuser@example.com", result?.Email);
        }

        [Fact]
        public async Task GetUserByRegistrationTokenAsync_ReturnsUser()
        {
            using var context = new DataContext(_contextOptions);
            var user = new UserEntity
            {
                Username = "testuser",
                Email = "testuser@example.com",
                Password = "Password123!",
                RegistrationToken = "regtoken",
                IsEmailConfirmed = true,
                RegistrationDate = DateTime.UtcNow,
                RoleId = Guid.NewGuid()
            };
            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();

            var repository = new UserRepository(context);
            var result = await repository.GetUserByRegistrationTokenAsync("regtoken");

            Assert.NotNull(result);
            Assert.Equal("regtoken", result?.RegistrationToken);
        }

        [Fact]
        public async Task GetUserByResetPasswordTokenAsync_ReturnsUser()
        {
            using var context = new DataContext(_contextOptions);
            var user = new UserEntity
            {
                Username = "testuser",
                Email = "testuser@example.com",
                Password = "Password123!",
                RegistrationToken = "regtoken",
                ResetPasswordToken = "resettoken",
                IsEmailConfirmed = true,
                RegistrationDate = DateTime.UtcNow,
                RoleId = Guid.NewGuid()
            };
            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();

            var repository = new UserRepository(context);
            var result = await repository.GetUserByResetPasswordTokenAsync("resettoken");

            Assert.NotNull(result);
            Assert.Equal("resettoken", result?.ResetPasswordToken);
        }

        [Fact]
        public async Task GetUsersPaginatedAsync_ReturnsPaginatedUsers()
        {
            using var context = new DataContext(_contextOptions);
            var users = new List<UserEntity>
            {
                new UserEntity
                {
                    Username = "user1",
                    Email = "user1@example.com",
                    Password = "Password123!",
                    RegistrationToken = "regtoken1",
                    IsEmailConfirmed = true,
                    RegistrationDate = DateTime.UtcNow,
                    RoleId = Guid.NewGuid()
                },
                new UserEntity
                {
                    Username = "user2",
                    Email = "user2@example.com",
                    Password = "Password123!",
                    RegistrationToken = "regtoken2",
                    IsEmailConfirmed = true,
                    RegistrationDate = DateTime.UtcNow,
                    RoleId = Guid.NewGuid()
                },
                new UserEntity
                {
                    Username = "user3",
                    Email = "user3@example.com",
                    Password = "Password123!",
                    RegistrationToken = "regtoken3",
                    IsEmailConfirmed = true,
                    RegistrationDate = DateTime.UtcNow,
                    RoleId = Guid.NewGuid()
                }
            };
            await context.Users.AddRangeAsync(users);
            await context.SaveChangesAsync();

            var repository = new UserRepository(context);
            var result = await repository.GetUsersPaginatedAsync(1, 2);

            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetUsersByUserProfileIdsAsync_ReturnsUsersByProfileIds()
        {
            using var context = new DataContext(_contextOptions);
            var userProfileIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
            var users = new List<UserEntity>
            {
                new UserEntity
                {
                    Username = "user1",
                    Email = "user1@example.com",
                    Password = "Password123!",
                    RegistrationToken = "regtoken1",
                    IsEmailConfirmed = true,
                    RegistrationDate = DateTime.UtcNow,
                    RoleId = Guid.NewGuid(),
                    UserProfile = new UserProfileEntity { Id = userProfileIds[0], FirstName = "John", LastName = "Doe" }
                },
                new UserEntity
                {
                    Username = "user2",
                    Email = "user2@example.com",
                    Password = "Password123!",
                    RegistrationToken = "regtoken2",
                    IsEmailConfirmed = true,
                    RegistrationDate = DateTime.UtcNow,
                    RoleId = Guid.NewGuid(),
                    UserProfile = new UserProfileEntity { Id = userProfileIds[1], FirstName = "Jane", LastName = "Doe" }
                }
            };
            await context.Users.AddRangeAsync(users);
            await context.SaveChangesAsync();

            var repository = new UserRepository(context);
            var result = await repository.GetUsersByUserProfileIdsAsync(userProfileIds);

            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetFilteredUsersAsync_ReturnsFilteredUsers()
        {
            using var context = new DataContext(_contextOptions);
            var role = new RoleEntity { RoleName = "User" };
            await context.Roles.AddAsync(role);
            await context.SaveChangesAsync();

            var users = new List<UserEntity>
            {
                new UserEntity
                {
                    Username = "user1",
                    Email = "user1@example.com",
                    Password = "Password123!",
                    RegistrationToken = "regtoken1",
                    IsEmailConfirmed = true,
                    RegistrationDate = DateTime.UtcNow,
                    RoleId = role.Id,
                    UserProfile = new UserProfileEntity
                    {
                        FirstName = "John",
                        LastName = "Doe",
                        Country = "USA",
                        State = "CA",
                        City = "San Francisco",
                        Education = "Bachelors",
                        Experience = "5 years"
                    }
                },
                new UserEntity
                {
                    Username = "user2",
                    Email = "user2@example.com",
                    Password = "Password123!",
                    RegistrationToken = "regtoken2",
                    IsEmailConfirmed = true,
                    RegistrationDate = DateTime.UtcNow,
                    RoleId = role.Id,
                    UserProfile = new UserProfileEntity
                    {
                        FirstName = "Jane",
                        LastName = "Doe",
                        Country = "USA",
                        State = "NY",
                        City = "New York",
                        Education = "Masters",
                        Experience = "10 years"
                    }
                }
            };
            await context.Users.AddRangeAsync(users);
            await context.SaveChangesAsync();

            var repository = new UserRepository(context);
            var result = await repository.GetFilteredUsersAsync("user1", "USA", "CA", "San Francisco", "Bachelors", "5 years");

            Assert.Single(result);
            Assert.Equal("user1", result.First().Username);
        }

        [Fact]
        public async Task AddUserAsync_AddsUserToDatabase()
        {
            using var context = new DataContext(_contextOptions);
            var role = new RoleEntity { RoleName = "User" };
            await context.Roles.AddAsync(role);
            await context.SaveChangesAsync();

            var user = new UserEntity
            {
                Username = "testuser",
                Email = "testuser@example.com",
                Password = "Password123!",
                RegistrationToken = "regtoken",
                IsEmailConfirmed = true,
                RegistrationDate = DateTime.UtcNow,
                RoleId = role.Id
            };

            var repository = new UserRepository(context);
            await repository.AddUserAsync(user);

            var userInDb = await context.Users.FirstOrDefaultAsync(u => u.Username == "testuser");
            Assert.NotNull(userInDb);
            Assert.Equal(role.Id, userInDb.RoleId);
        }

        [Fact]
        public async Task UpdateUserAsync_UpdatesUserInDatabase()
        {
            using var context = new DataContext(_contextOptions);
            var user = new UserEntity
            {
                Username = "testuser",
                Email = "testuser@example.com",
                Password = "Password123!",
                RegistrationToken = "regtoken",
                IsEmailConfirmed = true,
                RegistrationDate = DateTime.UtcNow,
                RoleId = Guid.NewGuid()
            };
            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();

            user.Username = "updateduser";
            var repository = new UserRepository(context);
            await repository.UpdateUserAsync(user);

            var userInDb = await context.Users.FirstOrDefaultAsync(u => u.Id == user.Id);
            Assert.Equal("updateduser", userInDb.Username);
        }

        [Fact]
        public async Task DeleteUserAsync_DeletesUserFromDatabase()
        {
            using var context = new DataContext(_contextOptions);
            var user = new UserEntity
            {
                Username = "testuser",
                Email = "testuser@example.com",
                Password = "Password123!",
                RegistrationToken = "regtoken",
                IsEmailConfirmed = true,
                RegistrationDate = DateTime.UtcNow,
                RoleId = Guid.NewGuid()
            };
            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();

            var repository = new UserRepository(context);
            await repository.DeleteUserAsync(user);

            var userInDb = await context.Users.FirstOrDefaultAsync(u => u.Username == "testuser");
            Assert.Null(userInDb);
        }
    }
}
