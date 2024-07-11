using User.Data.Object.Entities;
using Microsoft.AspNetCore.Http;

namespace User.Services.Contracts;
public interface IPdfService
{
    public string ExtractTextFromPdfByUsername(string path);
    public Task SaveUserCVAsync(IFormFile userCV, UserProfileEntity userProfile);
    public void DeleteUserCV(string username);
}
