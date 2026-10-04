using Microsoft.AspNetCore.Mvc;
using MinhaApi.Controllers;
using MinhaApi.Dtos;
using MinhaApi.Exceptions;
using MinhaApi.Services;
using Moq;

namespace MinhaApi.Tests;

public class ObjetosControllerTests
{
    private readonly Mock<IObjetoService> _serviceMock = new(MockBehavior.Strict);
    private ObjetosController CriarController() => new(_serviceMock.Object);

    [Fact]
    public async Task Listar_Retorna200ComPaginado()
    {
        var paginado = new PagedResponse<ObjetoResponseDto>
        {
            Items = [new() { Id = 1, Nome = "A" }],
            TotalCount = 1,
            Page = 1,
            PageSize = 10
        };
        _serviceMock
            .Setup(s => s.ListarAsync(1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(paginado);

        var result = await CriarController().Listar(1, 10);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(200, ok.StatusCode);
        Assert.Same(paginado, ok.Value);
    }

    [Fact]
    public async Task BuscarPorId_Existente_Retorna200()
    {
        var dto = new ObjetoResponseDto { Id = 1, Nome = "A" };
        _serviceMock
            .Setup(s => s.BuscarPorIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(dto);

        var result = await CriarController().BuscarPorId(1, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(dto, ok.Value);
    }

    [Fact]
    public async Task BuscarPorId_Inexistente_PropagaNotFoundParaOHandlerGlobal()
    {
        _serviceMock
            .Setup(s => s.BuscarPorIdAsync(999, It.IsAny<CancellationToken>()))
            .ThrowsAsync(NotFoundException.Para("Objeto", 999));

        await Assert.ThrowsAsync<NotFoundException>(
            () => CriarController().BuscarPorId(999, CancellationToken.None));
    }

    [Fact]
    public async Task Criar_Retorna201ComRotaParaBuscarPorId()
    {
        var criado = new ObjetoResponseDto { Id = 7, Nome = "Novo" };
        _serviceMock
            .Setup(s => s.CriarAsync(It.IsAny<ObjetoCreateDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(criado);

        var result = await CriarController()
            .Criar(new ObjetoCreateDto { Nome = "Novo" }, CancellationToken.None);

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(201, created.StatusCode);
        Assert.Equal(nameof(ObjetosController.BuscarPorId), created.ActionName);
        Assert.Equal(7, created.RouteValues!["id"]);
        Assert.Same(criado, created.Value);
    }

    [Fact]
    public async Task Atualizar_Existente_Retorna200()
    {
        var dto = new ObjetoResponseDto { Id = 1, Nome = "Atualizado" };
        _serviceMock
            .Setup(s => s.AtualizarAsync(1, It.IsAny<ObjetoUpdateDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(dto);

        var result = await CriarController()
            .Atualizar(1, new ObjetoUpdateDto { Nome = "Atualizado" }, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(dto, ok.Value);
    }

    [Fact]
    public async Task Remover_Existente_Retorna204()
    {
        _serviceMock
            .Setup(s => s.RemoverAsync(1, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await CriarController().Remover(1, CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
    }
}
