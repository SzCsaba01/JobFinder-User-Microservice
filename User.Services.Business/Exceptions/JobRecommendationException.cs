using Project.Services.Business.Exceptions;
using System.Net;

namespace User.Services.Business.Exceptions;
public class JobRecommendationException : ApiExceptionBase
{
    public JobRecommendationException() : base(HttpStatusCode.InternalServerError, "Job recommendation failed") {}

    public JobRecommendationException(string message) : base(HttpStatusCode.InternalServerError, message, "Job recommendation failed") {}

    public JobRecommendationException(string message, Exception innerException) : base(HttpStatusCode.InternalServerError, message, innerException, "Job recommendation failed") {}
}
