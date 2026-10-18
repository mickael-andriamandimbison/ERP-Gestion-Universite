using Microsoft.AspNetCore.Mvc;

namespace erp_univ_back.Features.Professeur.GetById;

[ApiController]
[Route("api/professeurs")]
public class GetByIdController(IGetByIdService _getByIdService) : ControllerBase
{
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ProfesseurGetByIdResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProfesseurById(Guid id, CancellationToken ct)
    {
        var response = await _getByIdService.ExecuteAsync(id, ct);
        return Ok(response);
    }
}
