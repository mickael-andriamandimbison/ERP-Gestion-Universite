using Microsoft.AspNetCore.Mvc;
using 

namespace erp_univ_back.Features.Etudiant.GetAll
{
    [ApiController]   
    [Route("api/etudiants")]
    public class GetAllController(IGetAllEtudiantService _service) : ControllerBase{
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<EtudiantGetAllResponse>),StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]

        public async Task<IActionResult> GetAll(CancellationToken ct){
            var result = await _service.ExecuteAsync(ct);
            return Ok(result);
        }
    }

}