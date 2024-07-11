namespace User.Services.Contracts;
public interface IQueueMessageSenderService
{
    public Task SendMessageAsync(string message);
}
