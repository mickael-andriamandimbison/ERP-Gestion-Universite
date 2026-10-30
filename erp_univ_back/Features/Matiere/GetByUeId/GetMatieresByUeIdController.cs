namespace erp_univ_back.Features.Matiere.GetByUeId;

using Microsoft.AspNetCore.Mvc;

[ApiController]
public class GetMatieresByUeIdController(IGetMatieresByUeIdService service) : ControllerBase
{
    [HttpGet("api/unite-enseignements/{ueId:guid}/matieres")]
    [ProducesResponseType(typeof(List<MatiereItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenirParUeId([FromRoute] Guid ueId, CancellationToken ct)
    {
        var response = await service.ExecuteAsync(new GetMatieresByUeIdRequest(ueId), ct);
        return Ok(response);
    }
}

