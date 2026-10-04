namespace MinhaApi.Exceptions;

public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message)
    {
    }

    public static NotFoundException Para(string entidade, object id)
        => new($"{entidade} com Id {id} não foi encontrado.");
}
