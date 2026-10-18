using erp_univ_back.Shared.Exceptions;
namespace erp_univ_back.Features.Niveau.Create
{
    public static class CreateNiveauValidator
    {
        public static void Valider(CreateNiveauRequest request)
        {
            if (request is null)
                throw new BadHttpRequestException("Les données du niveau sont obligatoires.");

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
}