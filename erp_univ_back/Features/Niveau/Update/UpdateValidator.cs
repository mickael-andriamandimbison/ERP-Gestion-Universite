namespace erp_univ_back.Features.Niveau.Update;

using erp_univ_back.Shared.Exceptions;

public static class UpdateNiveauValidator
{
    public static void Valider(UpdateNiveauRequest request)
    {
        if (request is null)
            throw new BadRequestException("Les données du niveau sont obligatoires.");

        if (string.IsNullOrWhiteSpace(request.Code))
            throw new BadRequestException("Le code du niveau est obligatoire.");

        if (request.Code.Trim().Length > 20)
            throw new BadRequestException("Le code ne doit pas dépasser 20 caractères.");

        if (string.IsNullOrWhiteSpace(request.Nom))
            throw new BadRequestException("Le nom du niveau est obligatoire.");

        if (request.Nom.Trim().Length > 100)
            throw new BadRequestException("Le nom ne doit pas dépasser 100 caractères.");

        if (request.Ordre <= 0)
            throw new BadRequestException("L'ordre d'affichage doit être un entier supérieur à 0.");
    }
}
