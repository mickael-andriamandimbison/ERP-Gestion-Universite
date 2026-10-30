namespace erp_univ_back.Features.Etudiant.Create;

using FluentValidation;

public class CreateEtudiantRequestValidator : AbstractValidator<CreateEtudiantRequest>
{
    public CreateEtudiantRequestValidator()
    {
        RuleFor(x => x.Nom)
            .NotEmpty().WithMessage("Le nom de l'étudiant ne peut pas être vide.")
            .MaximumLength(100).WithMessage("Le nom de l'étudiant ne peut pas dépasser 100 caractères.")
            .Matches(@"^[a-zA-ZÀ-ÿ\s\-']+$").WithMessage("Le nom contient des caractères non autorisés (chiffres, symboles ou emojis interdits).");

        RuleFor(x => x.Prenom)
            .MaximumLength(100).WithMessage("Le prenom de l'étudiant ne peut pas dépasser 100 caractères.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("L'email de l'étudiant ne peut pas être vide.")
            .MaximumLength(100).WithMessage("L'email de l'étudiant ne peut pas dépasser 100 caractères.")
            .Matches(@"^[^@\s]+@[^@\s]+\.[^@\s]+$").WithMessage("L'email doit contenir une extension valide (ex: .com, .mg).");

        RuleFor(x => x.NumeroTelephone)
            .NotEmpty().WithMessage("Le numéro de téléphone est obligatoire.")
            .MinimumLength(10).WithMessage("Le numéro doit contenir au moins 10 caractères.")
            .MaximumLength(15).WithMessage("Le numéro est trop long.")
            .Matches(@"^\+?[0-9]+$").WithMessage("Le numéro de téléphone ne doit contenir que des chiffres (et éventuellement un '+' au début).");

        When(x => !string.IsNullOrEmpty(x.NumeroCin), () =>
        {
            RuleFor(x => x.NumeroCin!)
                .Matches(@"^[0-9]{12}$")
                .WithMessage("Le numéro CIN ne doit comporter que des chiffres et être d'une longueur de 12 caractères.");
        });
    }
}
