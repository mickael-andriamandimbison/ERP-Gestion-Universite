using System.Text.RegularExpressions;
using FluentValidation;

namespace erp_univ_back.Features.AnneeAcademique.Update;

public partial class UpdateAnneeAcademiqueValidator : AbstractValidator<UpdateAnneeAcademiqueRequest>
{
    [GeneratedRegex(@"^(\d{4})\s*[-/]\s*(\d{4})$")]
    private static partial Regex AnneeAcademiquePattern();

    public UpdateAnneeAcademiqueValidator()
    {
        RuleFor(x => x.Libelle)
            .NotEmpty().WithMessage("Le libellé de l'année académique est obligatoire.")
            .MaximumLength(15).WithMessage("Le libellé de l'année académique ne doit pas dépasser 15 caractères.")
            .Matches(@"^(\d{4})\s*[-/]\s*(\d{4})$").WithMessage("Le libellé doit respecter le format 'AAAA-AAAA' (ex: 2025-2026).");

        RuleFor(x => x.DateFin)
            .GreaterThan(x => x.DateDebut)
            .WithMessage("La date de fin doit être postérieure à la date de début.");

        RuleFor(x => x)
            .Custom((req, context) =>
            {
                if (string.IsNullOrWhiteSpace(req.Libelle)) return;

                var match = AnneeAcademiquePattern().Match(req.Libelle.Trim());
                if (!match.Success) return;

                var anneeDebut = int.Parse(match.Groups[1].Value);
                var anneeFin = int.Parse(match.Groups[2].Value);

                if (anneeFin != anneeDebut + 1)
                {
                    context.AddFailure(nameof(req.Libelle), $"L'année de fin ({anneeFin}) doit être l'année suivant l'année de début ({anneeDebut}).");
                }

                if (req.DateDebut.Year != anneeDebut)
                {
                    context.AddFailure(nameof(req.DateDebut), $"L'année de la date de début ({req.DateDebut.Year}) doit correspondre à la première année du libellé ({anneeDebut}).");
                }

                if (req.DateFin.Year != anneeFin)
                {
                    context.AddFailure(nameof(req.DateFin), $"L'année de la date de fin ({req.DateFin.Year}) doit correspondre à la deuxième année du libellé ({anneeFin}).");
                }
            });
    }
}

