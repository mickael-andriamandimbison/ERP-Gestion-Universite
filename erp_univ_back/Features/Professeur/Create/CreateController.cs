using Microsoft.AspNetCore.Mvc;

namespace erp_univ_back.Features.Professeur.Create
{
    [ApiController]
    [Route("api/professeurs")]
    public class CreateController(ICreateService _service) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> CreateProf([FromBody] CreateProfRequest request, CancellationToken ct)
        {
            var response = await _service.ExecuteAsync(request, ct);
            return CreatedAtAction(nameof(CreateProf), new { id = response.Id }, response);
        }
    }
}