namespace erp_univ_back.Features.Inscription.Create;

using FluentValidation;

public class CreateInscriptionValidator : AbstractValidator<CreateInscriptionRequest>
{
    public CreateInscriptionValidator()
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
