using System.Net;

namespace BuildingBlocks.Exception
{
    public class UnauthorizedException : CustomException
    {
        public UnauthorizedException(string message, int? code = null)
            : base(message, HttpStatusCode.Unauthorized) // Default to 401 if no code is provided
        {
        }
    }
}
