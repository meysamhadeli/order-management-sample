namespace BuildingBlocks.Exception;

public class DomainException : CustomException
{
    public DomainException(string message, int? code = null) : base(message, code: code)
    {

    }
}
