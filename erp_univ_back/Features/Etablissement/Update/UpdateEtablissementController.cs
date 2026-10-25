namespace erp_univ_back.Features.Etablissement.Update;

using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/etablissements")]
public class UpdateEtablissementController(IUpdateEtablissementService updateService) : ControllerBase
{
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(UpdateEtablissementResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateAsync([FromRoute] Guid id, [FromBody] UpdateEtablissementRequest request, CancellationToken ct)
    {
        var response = await updateService.ExecuteAsync(id, request, ct);
        return Ok(response);
    }
}
