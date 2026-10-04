using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Options;
using MinhaApi.Dtos;
using MinhaApi.Exceptions;
using MinhaApi.Settings;

namespace MinhaApi.Services;

public interface IViaCepService
{
    Task<ViaCepResponseDto> BuscarEnderecoPorCepAsync(string cep, CancellationToken cancellationToken = default);
}

public class ViaCepService : IViaCepService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ViaCepService> _logger;

    public ViaCepService(
        HttpClient httpClient,
        IOptions<ViaCepSettings> settings,
        ILogger<ViaCepService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;

        var baseUrl = settings.Value.BaseUrl;
        if (!baseUrl.EndsWith('/'))
            baseUrl += "/";

        _httpClient.BaseAddress = new Uri(baseUrl);
        _httpClient.Timeout = TimeSpan.FromSeconds(10);
    }

    public async Task<ViaCepResponseDto> BuscarEnderecoPorCepAsync(string cep, CancellationToken cancellationToken = default)
    {
        var normalizado = new string((cep ?? string.Empty).Where(char.IsDigit).ToArray());

        if (normalizado.Length != 8)
            throw new BadRequestException("CEP inválido. Informe os 8 dígitos do CEP.");

        _logger.LogInformation("Buscando endereço na ViaCEP para o CEP {Cep}", normalizado);

        HttpResponseMessage response;
        try
        {
            response = await _httpClient.GetAsync($"{normalizado}/json/", cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Falha de rede ao chamar a ViaCEP para o CEP {Cep}", normalizado);
            throw new HttpRequestException("Serviço de CEP indisponível no momento. Tente novamente mais tarde.", ex);
        }

        if (response.StatusCode == HttpStatusCode.NotFound)
            throw NotFoundException.Para("CEP", normalizado);

        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(cancellationToken);

        if (json.Contains("\"erro\"", StringComparison.OrdinalIgnoreCase))
            throw NotFoundException.Para("CEP", normalizado);

        var endereco = JsonSerializer.Deserialize<ViaCepResponseDto>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (endereco is null || string.IsNullOrWhiteSpace(endereco.Cep))
            throw NotFoundException.Para("CEP", normalizado);

        return endereco;
    }
}
