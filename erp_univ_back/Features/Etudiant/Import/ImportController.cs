using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace erp_univ_back.Features.Etudiant.Import
{
    [ApiController]
    [Route("api/etudiants")]
    public class ImportController(IImportService _importService) : ControllerBase
    {
        [HttpPost("import")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ImportEtudiantReport), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ImportEtudiants([FromForm] ImportFileRequest? request, CancellationToken cancellationToken)
        {
            var file = request?.File 
                ?? (Request.HasFormContentType 
                    ? Request.Form.Files["file"] ?? Request.Form.Files["File"] ?? Request.Form.Files.FirstOrDefault() 
                    : null);

            if (file == null || file.Length == 0)
                return BadRequest(new { message = "Veuillez sélectionner un fichier (.csv ou .xlsx) avant d'envoyer la requête." });

            var report = await _importService.ExecuteAsync(file, cancellationToken);
            return Ok(report);
        }
    }
}
