namespace User.Data.Access.Helpers;
public static class AppConstants
{
    public const string FE_APP_VERIFY_EMAIL_URL = "https://localhost:4200/verify-email";
    public const string FE_APP_CHANGE_PASSWORD_URL = "https://localhost:4200/change-password";
    public const string OPENAI_API_URL = "https://api.openai.com/v1/chat/completions";
    public const int MAX_FILE_SIZE = 5242880;
    public const string USER_DELETE_MESSAGE = "DeleteUser";
    public const string RECOMMEND_JOBS_MESSAGE = "RecommendJobs";
    public const string RESOURCES = "Resources";
    public const string CV = "CV";
    public const string COUNTRY_API_URL = "api/Country/";
    public const string STATE_API_URL = "api/State/";
    public const string CITY_API_URL = "api/City/";
    public const string MICROSERVICE_NAME = "User.Microservice";
    public const string MICROSERVICE_URL = "https://localhost:5278";
    public const int RESET_PASSWORD_TOKEN_VALIDATION_TIME = 30;
    public const int JWT_TOKEN_VALIDATION_TIME = 24;
    public const string USER_ROLE = "User";
    public const string ADMIN_ROLE = "Admin";
}
