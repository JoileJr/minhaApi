namespace MinhaApi.Settings;

public class ViaCepSettings
{
    public const string SectionName = "ExternalApis:ViaCep";

    public string BaseUrl { get; set; } = "https://viacep.com.br/ws/";
}
