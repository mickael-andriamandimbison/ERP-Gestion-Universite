namespace erp_univ_back.Features.Matiere.Update;

using FluentValidation;

public class UpdateMatiereValidator : AbstractValidator<UpdateMatiereRequest>
{
    public UpdateMatiereValidator()
    {
        RuleFor(x => x.Nom)
            .NotEmpty().WithMessage("Le nom de la matière est obligatoire.")
            .MaximumLength(150).WithMessage("Le nom de la matière ne doit pas dépasser 150 caractères.");

        RuleFor(x => x.Coefficient)
            .GreaterThanOrEqualTo(1).WithMessage("Le coefficient doit être supérieur ou égal à 1.");

        When(x => !string.IsNullOrWhiteSpace(x.Code), () =>
        {
            RuleFor(x => x.Code!)
                .MaximumLength(50).WithMessage("La longueur du code ne doit pas dépasser 50 caractères.");
        });

        RuleFor(x => x.UniteEnseignementId)
            .Must(id => !id.HasValue || id.Value != Guid.Empty)
            .WithMessage("L'identifiant de l'unité d'enseignement ne doit pas être vide.");
    }
}

