using Project.Services.Business.Exceptions;
using System.Net;

namespace User.Services.Business.Exceptions;
public class TokenExpiredException : ApiExceptionBase
{
    public TokenExpiredException() : base(HttpStatusCode.BadRequest, "Token expired") { }

    public TokenExpiredException(string message) : base(HttpStatusCode.BadRequest, message, "Token expired") { }

    public TokenExpiredException(string message, Exception innerException) : base(HttpStatusCode.BadRequest, message, innerException, "Token expired") { }
}