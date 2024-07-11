using User.Services.Contracts;
using System.Security.Cryptography;

namespace User.Services.Business;
public class EncryptionService : IEncryptionService
{
    private const string passwordSalt = "b$S^P@oJ#cTs@!T";
    private const string passwordPepper = "P@sSVv&R#dPepPe!r";

    public string GenerateHashedPassword(string password)
    {
        using var hmac = SHA256.Create();

        var hashedPassword = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(passwordSalt + password + passwordPepper));

        return System.Text.Encoding.UTF8.GetString(hashedPassword);
    }
}
