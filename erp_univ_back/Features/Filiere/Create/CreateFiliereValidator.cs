using FluentValidation;

namespace erp_univ_back.Features.Filiere.Create;

public class CreateFiliereValidator : AbstractValidator<CreateFiliereRequest>
{
    public CreateFiliereValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Le code de la filière est obligatoire.")
            .MaximumLength(20).WithMessage("Le code de la filière ne doit pas dépasser 20 caractères.");

        RuleFor(x => x.Nom)
            .NotEmpty().WithMessage("Le nom de la filière est obligatoire.")
            .MaximumLength(150).WithMessage("Le nom de la filière ne doit pas dépasser 150 caractères.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("La description ne doit pas dépasser 500 caractères.");
    }
}

