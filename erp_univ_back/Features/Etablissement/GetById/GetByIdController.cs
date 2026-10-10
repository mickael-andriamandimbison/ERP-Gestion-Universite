using Microsoft.AspNetCore.Mvc;
using erp_univ_back.Features.Etablissement.Create;


namespace erp_univ_back.Features.Etablissement.GetById
{

    [ApiController]
    [Route("api/etablissements")]
    public class GetByIdController(IGetByIdService _getByIdService) : ControllerBase
    {
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(EtablissementResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(Guid id)
        {
            var response = await _getByIdService.ExecuteAsync(id);
            return Ok(response);
        }
    }
}