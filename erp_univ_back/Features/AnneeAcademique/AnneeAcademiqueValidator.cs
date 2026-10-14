using System.Text.RegularExpressions;
using erp_univ_back.Shared.Exceptions;

namespace erp_univ_back.Features.AnneeAcademique
{
    public static partial class AnneeAcademiqueValidator
    {
        [GeneratedRegex(@"^(\d{4})\s*[-/]\s*(\d{4})$")]
        private static partial Regex AnneeAcademiquePattern();

        public static void Valider(string libelle, DateOnly dateDebut, DateOnly dateFin)
        {
            if (string.IsNullOrWhiteSpace(libelle))
                throw new BadRequestException("Le libellé de l'année académique est obligatoire.");

            var trimmedLibelle = libelle.Trim();
            if (trimmedLibelle.Length > 15)
                throw new BadRequestException("Le libellé de l'année académique ne doit pas dépasser 15 caractères.");

            if (dateFin <= dateDebut)
                throw new BadRequestException("La date de fin doit être postérieure à la date de début.");

            var match = AnneeAcademiquePattern().Match(trimmedLibelle);
            if (!match.Success)
            {
                throw new BadRequestException("Le libellé doit respecter le format 'AAAA-AAAA' (ex: 2025-2026).");
            }

            var anneeDebut = int.Parse(match.Groups[1].Value);
            var anneeFin = int.Parse(match.Groups[2].Value);

            if (anneeFin != anneeDebut + 1)
            {
                throw new BadRequestException($"L'année de fin ({anneeFin}) doit être l'année suivant l'année de début ({anneeDebut}).");
            }

            if (dateDebut.Year != anneeDebut)
            {
                throw new BadRequestException($"L'année de la date de début ({dateDebut.Year}) doit correspondre à la première année du libellé ({anneeDebut}).");
            }

            if (dateFin.Year != anneeFin)
            {
                throw new BadRequestException($"L'année de la date de fin ({dateFin.Year}) doit correspondre à la deuxième année du libellé ({anneeFin}).");
            }
        }
    }
}
