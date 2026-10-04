using Microsoft.AspNetCore.Mvc;
using MinhaApi.Controllers;
using MinhaApi.Services;

namespace MinhaApi.Tests;

public class HelloTests
{
    [Fact]
    public void HelloService_RetornaHelloWorld()
    {
        Assert.Equal("Hello World", new HelloService().GetMessage());
    }

    [Fact]
    public void HelloController_Get_Retorna200ComMensagemDoService()
    {
        var controller = new HelloController(new HelloService());

        var result = controller.Get();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal("Hello World", ok.Value);
    }
}
