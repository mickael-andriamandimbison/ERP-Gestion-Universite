using FluentValidation;

namespace erp_univ_back.Features.Professeur.Update;

public class UpdateProfesseurValidator : AbstractValidator<UpdateProfesseurRequest>
{
    public UpdateProfesseurValidator()
    {
        RuleFor(x => x.Nom)
            .NotEmpty().WithMessage("Le nom du professeur est obligatoire.")
            .MaximumLength(100).WithMessage("Le nom du professeur ne peut pas dépasser 100 caractères.")
            .Matches(@"^[a-zA-ZÀ-ÿ\s\-']+$").WithMessage("Le nom contient des caractères non autorisés.");

        RuleFor(x => x.Prenom)
            .MaximumLength(100).WithMessage("Le prénom du professeur ne peut pas dépasser 100 caractères.");

        RuleFor(x => x.LieuNaissance)
            .NotEmpty().WithMessage("Le lieu de naissance est obligatoire.")
            .MaximumLength(100).WithMessage("Le lieu de naissance ne peut pas dépasser 100 caractères.");

        RuleFor(x => x.NumeroCin)
            .NotEmpty().WithMessage("Le numéro CIN est obligatoire.")
            .MaximumLength(17).WithMessage("Le numéro CIN ne peut pas dépasser 17 caractères.")
            .Matches(@"^[0-9]+$").WithMessage("Le numéro CIN ne doit contenir que des chiffres.");

        RuleFor(x => x.NumeroTelephone)
            .NotEmpty().WithMessage("Le numéro de téléphone est obligatoire.")
            .MinimumLength(10).WithMessage("Le numéro de téléphone doit contenir au moins 10 caractères.")
            .MaximumLength(20).WithMessage("Le numéro de téléphone ne peut pas dépasser 20 caractères.")
            .Matches(@"^\+?[0-9]+$").WithMessage("Le numéro de téléphone ne doit contenir que des chiffres (et éventuellement un '+' au début).");

        When(x => !string.IsNullOrWhiteSpace(x.Email), () =>
        {
            RuleFor(x => x.Email!)
                .MaximumLength(100).WithMessage("L'email ne peut pas dépasser 100 caractères.")
                .Matches(@"^[^@\s]+@[^@\s]+\.[^@\s]+$").WithMessage("L'email doit avoir un format valide.");
        });

        RuleFor(x => x.DateNaissance)
            .Must(d => d < DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("La date de naissance doit être dans le passé.");
    }
}

