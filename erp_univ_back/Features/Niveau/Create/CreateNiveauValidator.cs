namespace erp_univ_back.Features.Niveau.Create;

using FluentValidation;

public class CreateNiveauValidator : AbstractValidator<CreateNiveauRequest>
{
    public CreateNiveauValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Le code du niveau est obligatoire.")
            .MaximumLength(10).WithMessage("Le code ne doit pas dépasser 10 caractères.");

        RuleFor(x => x.Nom)
            .NotEmpty().WithMessage("Le nom du niveau est obligatoire.")
            .MaximumLength(100).WithMessage("Le nom ne doit pas dépasser 100 caractères.");

        RuleFor(x => x.Ordre)
            .GreaterThan(0).WithMessage("L'ordre d'affichage doit être un entier supérieur à 0.");
    }
}
