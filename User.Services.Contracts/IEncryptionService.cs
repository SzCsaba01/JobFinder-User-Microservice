namespace User.Services.Contracts;
public interface IEncryptionService
{
    public string GenerateHashedPassword(string password);
}
