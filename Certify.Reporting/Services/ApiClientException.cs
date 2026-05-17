using System.Net;

namespace Certify.Reporting.Services;

public class ApiClientException : Exception
{
    public ApiClientException(HttpStatusCode statusCode, string message)
        : base(message)
    {
        StatusCode = statusCode;
    }

    public HttpStatusCode StatusCode { get; }
}
