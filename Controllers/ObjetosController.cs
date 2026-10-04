using Microsoft.AspNetCore.Mvc;
using MinhaApi.Dtos;
using MinhaApi.Services;

namespace MinhaApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ObjetosController : ControllerBase
{
    private readonly IObjetoService _service;

    public ObjetosController(IObjetoService service)
    {
        _service = service;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<ObjetoResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PagedResponse<ObjetoResponseDto>>> Listar(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        return Ok(await _service.ListarAsync(page, pageSize, cancellationToken));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ObjetoResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ObjetoResponseDto>> BuscarPorId(int id, CancellationToken cancellationToken)
    {
        return Ok(await _service.BuscarPorIdAsync(id, cancellationToken));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ObjetoResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ObjetoResponseDto>> Criar([FromBody] ObjetoCreateDto dto, CancellationToken cancellationToken)
    {
        var criado = await _service.CriarAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(BuscarPorId), new { id = criado.Id }, criado);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ObjetoResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ObjetoResponseDto>> Atualizar(int id, [FromBody] ObjetoUpdateDto dto, CancellationToken cancellationToken)
    {
        return Ok(await _service.AtualizarAsync(id, dto, cancellationToken));
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Remover(int id, CancellationToken cancellationToken)
    {
        await _service.RemoverAsync(id, cancellationToken);
        return NoContent();
    }
}
