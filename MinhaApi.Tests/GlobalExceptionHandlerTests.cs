using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using MinhaApi.Exceptions;
using MinhaApi.Middleware;
using Moq;

namespace MinhaApi.Tests;

public class GlobalExceptionHandlerTests
{
    private static (GlobalExceptionHandler handler, DefaultHttpContext context) Criar(
        string? environment = null)
    {
        var envMock = new Mock<IWebHostEnvironment>(MockBehavior.Strict);
        envMock.SetupGet(e => e.EnvironmentName).Returns(environment ?? Environments.Development);

        var handler = new GlobalExceptionHandler(
            NullLogger<GlobalExceptionHandler>.Instance, envMock.Object);

        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        return (handler, context);
    }

    private static async Task<ProblemDetails> LerProblemAsync(HttpResponse response)
    {
        response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(response.Body);
        var json = await reader.ReadToEndAsync();
        return JsonSerializer.Deserialize<ProblemDetails>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
    }

    [Fact]
    public async Task NotFoundException_Retorna404ComDetalhe()
    {
        var (handler, context) = Criar();

        var tratado = await handler.TryHandleAsync(
            context, NotFoundException.Para("Objeto", 1), CancellationToken.None);

        Assert.True(tratado);
        Assert.Equal(404, context.Response.StatusCode);
        var problem = await LerProblemAsync(context.Response);
        Assert.Equal("Recurso não encontrado", problem.Title);
        Assert.Contains("Id 1", problem.Detail);
    }

    [Fact]
    public async Task BadRequestException_Retorna400()
    {
        var (handler, context) = Criar();

        await handler.TryHandleAsync(
            context, new BadRequestException("CEP inválido."), CancellationToken.None);

        Assert.Equal(400, context.Response.StatusCode);
        var problem = await LerProblemAsync(context.Response);
        Assert.Equal("Requisição inválida", problem.Title);
    }

    [Fact]
    public async Task ErroDesconhecido_EmDevelopment_ExpõeMensagem()
    {
        var (handler, context) = Criar(Environments.Development);

        await handler.TryHandleAsync(
            context, new InvalidOperationException("quebrou"), CancellationToken.None);

        Assert.Equal(500, context.Response.StatusCode);
        var problem = await LerProblemAsync(context.Response);
        Assert.Equal("quebrou", problem.Detail);
    }

    [Fact]
    public async Task ErroDesconhecido_EmProduction_OmiteDetalhe()
    {
        var (handler, context) = Criar(Environments.Production);

        await handler.TryHandleAsync(
            context, new InvalidOperationException("segredo interno"), CancellationToken.None);

        Assert.Equal(500, context.Response.StatusCode);
        var problem = await LerProblemAsync(context.Response);
        Assert.DoesNotContain("segredo", problem.Detail);
    }
}
