using Microsoft.AspNetCore.Mvc;
namespace erp_univ_back.Features.Niveau.Create
{

    [ApiController]
    [Route("api/niveaux")]
    public class CreateNiveauController(ICreateNiveauService service) : ControllerBase
    {
        [HttpPost]
        [ProducesResponseType(typeof(CreateNiveauResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> CreerAsync([FromBody] CreateNiveauRequest request, CancellationToken ct)
        {
            var response = await service.ExecuteAsync(request, ct);
            return CreatedAtAction(nameof(CreerAsync), new { id = response.Id }, response);
        }
    }
}