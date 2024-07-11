namespace User.Services.Contracts;
public interface IOpenAIService
{
    public Task<string> ExtractInformationFromTextAsync(string text);
}
