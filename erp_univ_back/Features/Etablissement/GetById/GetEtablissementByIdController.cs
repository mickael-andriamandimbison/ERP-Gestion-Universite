namespace erp_univ_back.Features.Etablissement.GetById;

using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/etablissements")]
public class GetEtablissementByIdController(IGetEtablissementByIdService _getByIdService) : ControllerBase
{
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(GetEtablissementByIdResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken ct)
    {
        var response = await _getByIdService.ExecuteAsync(id, ct);
        return Ok(response);
    }
}
