using Microsoft.AspNetCore.Mvc;
using MinhaApi.Dtos;
using MinhaApi.Models;
using MinhaApi.Services;

namespace MinhaApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ObjetosController : ControllerBase
{
    private readonly IObjetoService _service;

    public ObjetosController(IObjetoService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<List<Objeto>>> Listar()
    {
        return Ok(await _service.ListarAsync());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Objeto>> BuscarPorId(int id)
    {
        var objeto = await _service.BuscarPorIdAsync(id);
        if (objeto is null)
            return NotFound();

        return Ok(objeto);
    }

    [HttpPost]
    public async Task<ActionResult<Objeto>> Criar([FromBody] ObjetoCreateDto dto)
    {
        var criado = await _service.CriarAsync(dto);
        return CreatedAtAction(nameof(BuscarPorId), new { id = criado.Id }, criado);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<Objeto>> Atualizar(int id, [FromBody] ObjetoUpdateDto dto)
    {
        var atualizado = await _service.AtualizarAsync(id, dto);
        if (atualizado is null)
            return NotFound();

        return Ok(atualizado);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Remover(int id)
    {
        var removido = await _service.RemoverAsync(id);
        if (!removido)
            return NotFound();

        return NoContent();
    }
}
