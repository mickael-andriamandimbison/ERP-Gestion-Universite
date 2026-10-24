using Microsoft.AspNetCore.Mvc;
namespace erp_univ_back.Features.Niveau.Create
{

    [ApiController]
    [Route("api/niveaux")]
    public class CreateController(ICreateNiveauService service) : ControllerBase
    {
        [HttpPost]
        [ProducesResponseType(typeof(CreateNiveauResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Creer([FromBody] CreateNiveauRequest request, CancellationToken ct)
        {
            var response = await service.ExecuteAsync(request, ct);
            return CreatedAtAction(nameof(Creer), new { id = response.Id }, response);
        }
    }
}