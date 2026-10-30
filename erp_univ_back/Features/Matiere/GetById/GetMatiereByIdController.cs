namespace erp_univ_back.Features.Matiere.GetById;

using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/matieres")]
public class GetMatiereByIdController(IGetMatiereByIdService service) : ControllerBase
{
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(GetMatiereByIdResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenirParId([FromRoute] Guid id, CancellationToken ct)
    {
        var response = await service.ExecuteAsync(id, ct);
        return Ok(response);
    }
}

