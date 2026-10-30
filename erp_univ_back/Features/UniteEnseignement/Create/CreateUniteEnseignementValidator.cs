namespace erp_univ_back.Features.UniteEnseignement.Create;

using FluentValidation;

public class CreateUniteEnseignementValidator : AbstractValidator<CreateUniteEnseignementRequest>
{
    public CreateUniteEnseignementValidator()
    {
        RuleFor(x => x.Nom)
            .NotEmpty().WithMessage("Le nom de l'unité d'enseignement est obligatoire.")
            .MaximumLength(150).WithMessage("Le nom de l'unité d'enseignement ne doit pas dépasser 150 caractères.");

        RuleFor(x => x.Credit)
            .GreaterThan(0).WithMessage("Le nombre de crédits doit être strictement supérieur à 0.")
            .LessThanOrEqualTo(60).WithMessage("Le nombre de crédits ne peut pas dépasser 60.");

        RuleFor(x => x.Semestre)
            .InclusiveBetween(1, 2).WithMessage("Le semestre doit être 1 ou 2.");

        RuleFor(x => x.FiliereNiveauId)
            .NotEmpty().WithMessage("L'identifiant de la filière-niveau est obligatoire.");

        When(x => !string.IsNullOrEmpty(x.Code), () =>
        {
            RuleFor(x => x.Code!)
                .MaximumLength(50).WithMessage("Le code ne doit pas dépasser 50 caractères.");
        });
    }
}

