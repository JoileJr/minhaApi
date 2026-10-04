using System.ComponentModel.DataAnnotations;

namespace MinhaApi.Dtos;

public class ObjetoCreateDto
{
    [Required]
    [MaxLength(100)]
    public string Nome { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Descricao { get; set; }
}
