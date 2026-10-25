namespace erp_univ_back.Features.Inscription.ChangeStatut;

using FluentValidation;

public class ChangeStatutInscriptionValidator : AbstractValidator<ChangeStatutInscriptionRequest>
{
    public ChangeStatutInscriptionValidator()
    {
        RuleFor(x => x.Statut)
            .IsInEnum()
            .WithMessage("Le statut d'inscription spécifié est invalide.");
    }
}

public class ChangeStatutValidator : AbstractValidator<ChangeStatutRequest>
{
    public ChangeStatutValidator()
    {
        RuleFor(x => x.Statut)
            .IsInEnum()
            .WithMessage("Le statut d'inscription spécifié est invalide.");
    }
}
