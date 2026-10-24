namespace erp_univ_back.Features.Inscription.Create;

using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/inscriptions")]
public class CreateInscriptionController(ICreateInscriptionService service) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(CreateInscriptionResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> InscrireAsync([FromBody] CreateInscriptionRequest request, CancellationToken ct)
    {
        var response = await service.ExecuteAsync(request, ct);
        return StatusCode(StatusCodes.Status201Created, response);
    }
}
