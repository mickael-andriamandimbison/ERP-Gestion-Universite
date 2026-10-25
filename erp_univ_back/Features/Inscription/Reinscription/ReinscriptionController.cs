namespace erp_univ_back.Features.Inscription.Reinscription;

using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/inscriptions")]
public class ReinscriptionController(IReinscriptionService service) : ControllerBase
{
    [HttpPost("reinscription")]
    [ProducesResponseType(typeof(ReinscriptionResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ReinscrireAsync([FromBody] ReinscriptionRequest request, CancellationToken ct)
    {
        var response = await service.ExecuteAsync(request, ct);
        return StatusCode(StatusCodes.Status201Created, response);
    }
}
