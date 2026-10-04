namespace MinhaApi.Exceptions;

public class NotFoundException : AppException
{
    public NotFoundException(string message)
        : base(StatusCodes.Status404NotFound, "Recurso não encontrado", message)
    {
    }

    public static NotFoundException Para(string entidade, object id)
        => new($"{entidade} com Id {id} não foi encontrado.");
}
