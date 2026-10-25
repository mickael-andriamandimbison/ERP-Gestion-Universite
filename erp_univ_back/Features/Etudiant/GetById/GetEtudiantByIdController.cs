using Microsoft.AspNetCore.Mvc;

namespace erp_univ_back.Features.Etudiant.GetById;

[ApiController]
[Route("api/etudiants")]
public class GetEtudiantByIdController(IGetEtudiantByIdService _getByIdService) : ControllerBase
{
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(GetEtudiantByIdResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetEtudiantById(Guid id, CancellationToken ct)
    {
        var response = await _getByIdService.ExecuteAsync(id, ct);
        return Ok(response);
    }
}
