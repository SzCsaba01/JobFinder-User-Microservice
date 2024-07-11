using User.Data.Access.Helpers;
using User.Data.Object.Entities;
using User.Services.Business.Exceptions;
using User.Services.Contracts;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using UglyToad.PdfPig;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;

namespace User.Services.Business;
public class PdfService : IPdfService
{
    private readonly IServer _server;

    public PdfService(IServer server)
    {
        _server = server; 
    }

    public void DeleteUserCV(string username)
    {
        var folderName = Path.Combine(AppConstants.RESOURCES, AppConstants.CV, username);
        var pathToDelete = "";

        if (Environment.GetEnvironmentVariable("RUNNING_IN_DOCKER") is null)
        {
            pathToDelete = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).FullName, AppConstants.MICROSERVICE_NAME, folderName);
        }
        else
        {
            pathToDelete = Path.Combine(Directory.GetCurrentDirectory(), folderName);
        }

        if (Directory.Exists(pathToDelete))
        {
            Directory.Delete(pathToDelete, true);
        }
    }

    public async Task SaveUserCVAsync(IFormFile userCV, UserProfileEntity userProfile)
    {
        if (userCV.Length == 0)
        {
            throw new FileException("File is empty!");
        }

        if (userCV.Length > AppConstants.MAX_FILE_SIZE)
        {
            throw new FileException("File is too big! File size can be max 5 mb!");
        }

        if (userCV.ContentType != "application/pdf")
        {
            throw new FileException("File is not a PDF!");
        }

        var file = userCV;
        var folderName = Path.Combine(AppConstants.RESOURCES, AppConstants.CV, userProfile.User.Username);

        var pathToSave = "";
        var fullPath = "";
        var fileName = userProfile.User.Username + "_CV" + Path.GetExtension(file.FileName);
        if (Environment.GetEnvironmentVariable("RUNNING_IN_DOCKER") is null)
        {
            pathToSave = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).FullName, AppConstants.MICROSERVICE_NAME, folderName);
            fullPath = Path.Combine(pathToSave, fileName);
            var serverAddress = _server.Features.Get<IServerAddressesFeature>().Addresses.First();
            var dbPath = Path.Combine(serverAddress, folderName, fileName);
            userProfile.UserCV = dbPath;
        }
        else
        {
            pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);
            fullPath = Path.Combine(pathToSave, fileName);
            var dbPath = Path.Combine(AppConstants.MICROSERVICE_URL, folderName, fileName);
            userProfile.UserCV = dbPath;
        }

        if (!Directory.Exists(pathToSave))
        {
            Directory.CreateDirectory(pathToSave);
        }

        using (var stream = new FileStream(fullPath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
            stream.Close();
        }
    }

    public string ExtractTextFromPdfByUsername(string username)
    {
        var folderName = Path.Combine(AppConstants.RESOURCES, AppConstants.CV, username);
        var fileName = username + "_CV.pdf";
        var fullPath = "";

        if (Environment.GetEnvironmentVariable("RUNNING_IN_DOCKER") is null)
        {
            fullPath = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).FullName, AppConstants.MICROSERVICE_NAME, folderName, fileName);
        }
        else
        {
            fullPath = Path.Combine(Directory.GetCurrentDirectory(), folderName, fileName);
        }

        if (!File.Exists(fullPath))
        {
            throw new Exception("File not found");
        }

        string extractedText = "";

        using (var pdf = PdfDocument.Open(fullPath))
        {
            foreach (var page in pdf.GetPages())
            {
                extractedText += page.Text;
            }
        }

        return extractedText;
    }
}
