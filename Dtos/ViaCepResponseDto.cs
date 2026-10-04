using System.Text.Json.Serialization;

namespace MinhaApi.Dtos;

public class ViaCepResponseDto
{
    [JsonPropertyName("cep")]
    public string Cep { get; set; } = string.Empty;

    [JsonPropertyName("logradouro")]
    public string Logradouro { get; set; } = string.Empty;

    [JsonPropertyName("complemento")]
    public string Complemento { get; set; } = string.Empty;

    [JsonPropertyName("bairro")]
    public string Bairro { get; set; } = string.Empty;

    [JsonPropertyName("localidade")]
    public string Localidade { get; set; } = string.Empty;

    [JsonPropertyName("uf")]
    public string Uf { get; set; } = string.Empty;

    [JsonPropertyName("ibge")]
    public string Ibge { get; set; } = string.Empty;

    [JsonPropertyName("ddd")]
    public string Ddd { get; set; } = string.Empty;
}
