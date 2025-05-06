using System.Net;

namespace BuildingBlocks.Exception
{
    public class ForbiddenException : CustomException
    {
        public ForbiddenException(string message, int? code = null)
            : base(message, HttpStatusCode.Forbidden) // Default to 403 if no code is provided
        {
        }
    }
}
