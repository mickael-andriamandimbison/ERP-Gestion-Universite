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
    }
}
