using Microsoft.AspNetCore.Mvc;

namespace erp_univ_back.Features.Etablissement.Delete
{
    [ApiController]
    [Route("api/etablissements")]
    public class DeleteController(IDeleteService _service) : ControllerBase
    {
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken ct)
        {
            await _service.ExecuteAsync(id, ct);
            return Ok(new { message = "Etablissement supprimé avec succès" });
        }
    }
}
