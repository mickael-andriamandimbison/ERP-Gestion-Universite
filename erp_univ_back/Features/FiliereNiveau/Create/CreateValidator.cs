namespace erp_univ_back.Features.FiliereNiveau.Create;

using FluentValidation;

public class CreateFiliereNiveauValidator : AbstractValidator<CreateFiliereNiveauRequest>
{
    public CreateFiliereNiveauValidator()
    {
        RuleFor(x => x.FiliereId)
            .NotEmpty().WithMessage("L'identifiant de la filière est obligatoire.");

        RuleFor(x => x.NiveauId)
            .NotEmpty().WithMessage("L'identifiant du niveau est obligatoire.");

        RuleFor(x => x.LibelleAffiche)
            .MaximumLength(150).WithMessage("Le libellé affiché ne doit pas dépasser 150 caractères.")
            .When(x => !string.IsNullOrWhiteSpace(x.LibelleAffiche));
    }
}
