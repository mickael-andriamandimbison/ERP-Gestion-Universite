using FluentValidation;

namespace erp_univ_back.Features.Etudiant.GetAll;

public class GetAllEtudiantValidator : AbstractValidator<GetAllEtudiantRequest>
{
    private static readonly string[] ChampsTriAutorises = ["nom", "prenom", "matricule", "email", "datecreation"];

    public GetAllEtudiantValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Le numéro de page doit être supérieur ou égal à 1.");

        RuleFor(x => x.TaillePage)
            .InclusiveBetween(1, 100)
            .WithMessage("La taille de page doit être comprise entre 1 et 100.");

        RuleFor(x => x.Recherche)
            .MaximumLength(100)
            .WithMessage("Le terme de recherche ne peut pas dépasser 100 caractères.");

        RuleFor(x => x.ChampTri)
            .Must(champ => string.IsNullOrWhiteSpace(champ) || ChampsTriAutorises.Contains(champ.Trim().ToLowerInvariant()))
            .WithMessage("Le champ de tri spécifié n'est pas autorisé. Valeurs acceptées : nom, prenom, matricule, email, datecreation.");
    }
}

