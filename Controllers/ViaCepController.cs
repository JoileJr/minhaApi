using Microsoft.AspNetCore.Mvc;
using MinhaApi.Dtos;
using MinhaApi.Services;

namespace MinhaApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ViaCepController : ControllerBase
{
    private readonly IViaCepService _service;

    public ViaCepController(IViaCepService service)
    {
        _service = service;
    }

    [HttpGet("{cep}")]
    [ProducesResponseType(typeof(ViaCepResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ViaCepResponseDto>> BuscarEndereco(
        string cep,
        CancellationToken cancellationToken)
    {
        return Ok(await _service.BuscarEnderecoPorCepAsync(cep, cancellationToken));
    }
}
