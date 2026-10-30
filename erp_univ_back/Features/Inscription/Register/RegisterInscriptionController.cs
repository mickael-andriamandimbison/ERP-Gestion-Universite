namespace erp_univ_back.Features.Inscription.Register;

using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/inscriptions")]
public class RegisterInscriptionController(IRegisterInscriptionService service) : ControllerBase
{
    [HttpPost("register")]
    [ProducesResponseType(typeof(RegisterInscriptionResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RegisterAsync([FromBody] RegisterInscriptionRequest request, CancellationToken ct)
    {
        var response = await service.ExecuteAsync(request, ct);
        return StatusCode(StatusCodes.Status201Created, response);
    }
}

