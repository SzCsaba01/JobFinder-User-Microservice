using Project.Services.Business.Exceptions;
using System.Net;

namespace User.Services.Business.Exceptions;
public class FileException : ApiExceptionBase
{
    public FileException() : base(HttpStatusCode.BadRequest, "File exception")
    {
    }

    public FileException(string message) : base(HttpStatusCode.BadRequest, message, "File exception")
    {
    }

    public FileException(string message, Exception innerException) : base(HttpStatusCode.BadRequest, message, innerException, "File exception")
    {
    }
}