namespace MinhaApi.Services;

public interface IHelloService
{
    string GetMessage();
}

public class HelloService : IHelloService
{
    public string GetMessage()
    {
        return "Hello World";
    }
}
