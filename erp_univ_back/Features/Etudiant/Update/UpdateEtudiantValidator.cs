using FluentValidation;

namespace erp_univ_back.Features.Etudiant.Update;

public class UpdateEtudiantValidator : AbstractValidator<UpdateEtudiantRequest>
{
    public UpdateEtudiantValidator()
    {
        RuleFor(x => x.Nom)
            .NotEmpty().WithMessage("Le nom de l'étudiant ne peut pas être vide.")
            .MaximumLength(100).WithMessage("Le nom de l'étudiant ne peut pas dépasser 100 caractères.")
            .Matches(@"^[a-zA-ZÀ-ÿ\s\-']+$").WithMessage("Le nom contient des caractères non autorisés (chiffres, symboles ou emojis interdits).");

        RuleFor(x => x.Prenom)
            .MaximumLength(100).WithMessage("Le prénom de l'étudiant ne peut pas dépasser 100 caractères.");

        RuleFor(x => x.LieuNaissance)
            .NotEmpty().WithMessage("Le lieu de naissance est obligatoire.")
            .MaximumLength(100).WithMessage("Le lieu de naissance ne peut pas dépasser 100 caractères.");

        RuleFor(x => x.NumeroCin)
            .MaximumLength(17).WithMessage("Le numéro CIN ne peut pas dépasser 17 caractères.");

        RuleFor(x => x.NumeroTelephone)
            .NotEmpty().WithMessage("Le numéro de téléphone est obligatoire.")
            .MinimumLength(10).WithMessage("Le numéro doit contenir au moins 10 caractères.")
            .MaximumLength(20).WithMessage("Le numéro de téléphone ne peut pas dépasser 20 caractères.")
            .Matches(@"^\+?[0-9]+$").WithMessage("Le numéro de téléphone ne doit contenir que des chiffres (et éventuellement un '+' au début).");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("L'email de l'étudiant ne peut pas être vide.")
            .MaximumLength(100).WithMessage("L'email de l'étudiant ne peut pas dépasser 100 caractères.")
            .Matches(@"^[^@\s]+@[^@\s]+\.[^@\s]+$").WithMessage("L'email doit contenir une extension valide (ex: .com, .mg).");
    }
}

