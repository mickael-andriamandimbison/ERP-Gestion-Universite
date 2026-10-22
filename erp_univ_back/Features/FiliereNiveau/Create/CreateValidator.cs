namespace erp_univ_back.Features.FiliereNiveau.Create;

using erp_univ_back.Shared.Exceptions;

public static class CreateFiliereNiveauValidator
{
    public static void Valider(CreateFiliereNiveauRequest request)
    {
        if (request is null)
        {
            throw new BadRequestException("Les données de l'association filière-niveau sont obligatoires.");
        }

        if (request.FiliereId == Guid.Empty)
        {
            throw new BadRequestException("L'identifiant de la filière est obligatoire.");
        }

        if (request.NiveauId == Guid.Empty)
        {
            throw new BadRequestException("L'identifiant du niveau est obligatoire.");
        }

        if (!string.IsNullOrWhiteSpace(request.LibelleAffiche) && request.LibelleAffiche.Trim().Length > 150)
        {
            throw new BadRequestException("Le libellé affiché ne doit pas dépasser 150 caractères.");
        }
    }
}
