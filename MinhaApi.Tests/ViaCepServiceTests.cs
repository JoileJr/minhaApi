using System.Net;
using System.Text;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using MinhaApi.Exceptions;
using MinhaApi.Services;
using MinhaApi.Settings;

namespace MinhaApi.Tests;

public class ViaCepServiceTests
{
    private sealed class FakeHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _responder;
        public HttpRequestMessage? UltimaRequisicao { get; private set; }

        public FakeHandler(Func<HttpRequestMessage, HttpResponseMessage> responder)
        {
            _responder = responder;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            UltimaRequisicao = request;
            return Task.FromResult(_responder(request));
        }
    }

    private sealed class FalhaDeRedeHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
            => throw new HttpRequestException("rede fora");
    }

    private static ViaCepService CriarService(HttpMessageHandler handler)
    {
        var httpClient = new HttpClient(handler);
        var settings = Options.Create(new ViaCepSettings { BaseUrl = "https://viacep.com.br/ws/" });
        return new ViaCepService(httpClient, settings, NullLogger<ViaCepService>.Instance);
    }

    private static HttpResponseMessage JsonOk(string json) => new(HttpStatusCode.OK)
    {
        Content = new StringContent(json, Encoding.UTF8, "application/json")
    };

    [Fact]
    public async Task CepValido_RetornaEndereco()
    {
        var service = CriarService(new FakeHandler(_ => JsonOk(
            """{"cep":"01310-100","logradouro":"Avenida Paulista","localidade":"São Paulo","uf":"SP","ibge":"3550308","ddd":"11"}""")));

        var resultado = await service.BuscarEnderecoPorCepAsync("01310100");

        Assert.Equal("01310-100", resultado.Cep);
        Assert.Equal("Avenida Paulista", resultado.Logradouro);
        Assert.Equal("SP", resultado.Uf);
    }

    [Fact]
    public async Task CepComMascara_NormalizaAntesDeChamar()
    {
        var handler = new FakeHandler(_ => JsonOk("""{"cep":"01310-100"}"""));
        var service = CriarService(handler);

        await service.BuscarEnderecoPorCepAsync("01310-100");

        Assert.EndsWith("/01310100/json/", handler.UltimaRequisicao!.RequestUri!.ToString());
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("123")]
    [InlineData("")]
    public async Task CepInvalido_LancaBadRequest(string cep)
    {
        var service = CriarService(new FakeHandler(_ => JsonOk("{}")));

        var ex = await Assert.ThrowsAsync<BadRequestException>(
            () => service.BuscarEnderecoPorCepAsync(cep));

        Assert.Equal(400, ex.StatusCode);
    }

    [Fact]
    public async Task ErroTrue_LancaNotFound()
    {
        var service = CriarService(new FakeHandler(_ => JsonOk("""{"erro": "true"}""")));

        var ex = await Assert.ThrowsAsync<NotFoundException>(
            () => service.BuscarEnderecoPorCepAsync("00000000"));

        Assert.Equal(404, ex.StatusCode);
    }

    [Fact]
    public async Task Status404_LancaNotFound()
    {
        var service = CriarService(new FakeHandler(_ => new HttpResponseMessage(HttpStatusCode.NotFound)));

        await Assert.ThrowsAsync<NotFoundException>(
            () => service.BuscarEnderecoPorCepAsync("00000000"));
    }

    [Fact]
    public async Task FalhaDeRede_LancaHttpRequestException()
    {
        var service = CriarService(new FalhaDeRedeHandler());

        await Assert.ThrowsAsync<HttpRequestException>(
            () => service.BuscarEnderecoPorCepAsync("01310100"));
    }
}
