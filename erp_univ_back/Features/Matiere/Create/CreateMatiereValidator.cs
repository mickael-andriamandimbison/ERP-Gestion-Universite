using FluentValidation;

namespace erp_univ_back.Features.Matiere.Create;

public class CreateMatiereValidator : AbstractValidator<CreateMatiereRequest>
{
    public CreateMatiereValidator()
    {
        RuleFor(x => x.Nom)
            .NotEmpty().WithMessage("Le nom de la matière est obligatoire.")
            .MaximumLength(150).WithMessage("Le nom de la matière ne doit pas dépasser 150 caractères.");

        RuleFor(x => x.UniteEnseignementId)
            .NotEmpty().WithMessage("L'identifiant de l'unité d'enseignement est obligatoire.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Le code est un champ obligatoire.")
            .MaximumLength(10).WithMessage("La longueur du code doit être inférieure ou égale à 10 caractères.");

        RuleFor(x => x.Coefficient)
            .GreaterThanOrEqualTo(1).WithMessage("Le coefficient doit être supérieur ou égal à 1.");
    }
}
