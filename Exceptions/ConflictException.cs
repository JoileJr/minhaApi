namespace MinhaApi.Exceptions;

public class ConflictException : AppException
{
    public ConflictException(string message)
        : base(StatusCodes.Status409Conflict, "Conflito ao persistir os dados", message)
    {
    }
}
