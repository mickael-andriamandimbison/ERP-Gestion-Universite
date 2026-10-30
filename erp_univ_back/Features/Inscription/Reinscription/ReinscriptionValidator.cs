namespace erp_univ_back.Features.Inscription.Reinscription;

using FluentValidation;

public class ReinscriptionValidator : AbstractValidator<ReinscriptionRequest>
{
    public ReinscriptionValidator()
    {
        RuleFor(x => x.EtudiantId)
            .NotEmpty()
            .WithMessage("L'identifiant de l'étudiant est obligatoire.");

        RuleFor(x => x.FiliereNiveauId)
            .NotEmpty()
            .WithMessage("L'identifiant de la classe (filière/niveau) est obligatoire.");

        RuleFor(x => x.AnneeAcademiqueId)
            .NotEmpty()
            .WithMessage("L'identifiant de l'année académique est obligatoire.");
    }
}

