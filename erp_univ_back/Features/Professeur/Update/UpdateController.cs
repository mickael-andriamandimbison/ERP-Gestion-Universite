using erp_univ_back.Features.Professeur.GetAll;
using Microsoft.AspNetCore.Mvc;

namespace erp_univ_back.Features.Professeur.Update
{
    [ApiController]
    [Route("api/professeurs")]
    public class UpdateController(IUpdateProfesseurService _service) : ControllerBase
    {
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ProfesseurResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProfRequest request, CancellationToken ct)
        {
            var response = await _service.ExecuteAsync(id, request, ct);
            return Ok(response);
        }
    }
}
