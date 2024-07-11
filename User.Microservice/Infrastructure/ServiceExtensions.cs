using User.Data.Access;
using User.Data.Access.Helpers;
using User.Data.Contracts;
using User.Services.Business;
using User.Services.Contracts;
using Quartz;
using User.Quartz;

namespace User.API.Infrastructure;

public static class ServiceExtensions
{
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserProfileRepository, UserProfileRepository>();
        services.AddScoped<IUserProfileSkillMappingRepository, UserProfileSkillMappingRepository>();
        services.AddScoped<ISkillRepository, SkillRepository>();

        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IUserProfileService, UserProfileService>();
        services.AddScoped<ISkillService, SkillService>();
        services.AddScoped<IQueueMessageSenderService, QueueMessageSenderService>();

        services.AddScoped<IUserProfileDataProcessorService, UserProfileDataProcessorService>();
        services.AddScoped<IOpenAIService, OpenAIService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IPdfService, PdfService>();
        services.AddScoped<IEncryptionService, EncryptionService>();

        services.AddHostedService<QueueMessageReceiverService>();

        string locationMicroserviceURL = Environment.GetEnvironmentVariable("LOCATION_MICROSERVICE_URL") ?? "http://localhost:5204";
        services.AddHttpClient<ILocationCommunicationService, LocationCommunicationService>(client => {
            client.BaseAddress = new Uri(locationMicroserviceURL);
        });

        services.AddAutoMapper(typeof(Mapper));

        services.AddQuartz(q =>
        {
            var jobKey = new JobKey("DeleteUnmappedSkillsJob");
            q.AddJob<DeleteUnusedSkillMappingsJob>(j => j.WithIdentity(jobKey));

            q.AddTrigger(t => t
                .ForJob(jobKey)
                .WithIdentity("DeleteUnmappedSkillsJobTrigger")
                .WithCronSchedule("0 0 6 ? * *"));
        });
        
        services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

        services.AddCors(options => options.AddPolicy(
            name: "NgOrigins",
            policy => {
                policy.WithOrigins("http://localhost:4200").AllowAnyMethod().AllowAnyHeader().AllowCredentials();
            }));

        return services;
    }
}
