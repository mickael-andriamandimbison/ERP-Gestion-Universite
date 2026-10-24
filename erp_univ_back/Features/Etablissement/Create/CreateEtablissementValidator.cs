using FluentValidation;

namespace erp_univ_back.Features.Etablissement.Create;


public class CreateEtablissementValidator : AbstractValidator<CreateEtablissementRequest>
{
    public CreateEtablissementValidator()
    {
        RuleFor(x => x.Nom)
        .NotEmpty().WithMessage("Le nom de l'établissement ne peut pas être vide.")
        .MaximumLength(100).WithMessage("Le nom de l'établissement ne peut pas dépasser 100 caractères.");

        RuleFor(x => x.Adresse)
        .NotEmpty().WithMessage("L'adresse de l'établissement ne peut pas être vide.")
        .MaximumLength(200).WithMessage("L'adresse de l'établissement ne peut pas dépasser 200 caractères.");

        RuleFor(x => x.CodePostal)
        .NotEmpty().WithMessage("Le code postal de l'établissement ne peut pas être vide.")
        .MaximumLength(10).WithMessage("Le code postal de l'établissement ne peut pas dépasser 10 caractères.");

        RuleFor(x => x.Ville)
        .NotEmpty().WithMessage("La ville de l'établissement ne peut pas être vide.")
        .MaximumLength(50).WithMessage("La ville de l'établissement ne peut pas dépasser 50 caractères.");

        RuleFor(x => x.Pays)
        .NotEmpty().WithMessage("Le pays de l'établissement ne peut pas être vide.")
        .MaximumLength(50).WithMessage("Le pays de l'établissement ne peut pas dépasser 50 caractères.");

        RuleFor(x => x.Telephone)
        .NotEmpty().WithMessage("Le numéro de téléphone de l'établissement ne peut pas être vide.")
        .MaximumLength(20).WithMessage("Le numéro de téléphone de l'établissement ne peut pas dépasser 20 caractères.")
        .Matches(@"^\+?[0-9]+$").WithMessage("Le numéro de téléphone doit contenir uniquement des chiffres (et éventuellement un '+' au début).");

        RuleFor(x => x.Email)
        .NotEmpty().WithMessage("L'email de l'établissement ne peut pas être vide.")
        .MaximumLength(100).WithMessage("L'email de l'établissement ne peut pas dépasser 100 caractères.")
        .Matches(@"^[^@\s]+@[^@\s]+\.[^@\s]+$").WithMessage("L'email doit contenir une extension valide (ex: .com, .mg).");

        RuleFor(x => x.SiteWeb)
        .MaximumLength(100).WithMessage("Le site web de l'établissement ne peut pas dépasser 100 caractères.");

        RuleFor(x => x.Logo)
        .MaximumLength(200).WithMessage("Le logo de l'établissement ne peut pas dépasser 200 caractères.");

        RuleFor(x => x.Sigle)
        .MaximumLength(30).WithMessage("Le sigle de l'établissement ne peut pas dépasser 30 caractères.");

        RuleFor(x => x.Description)
        .MaximumLength(500).WithMessage("La description de l'établissement ne peut pas dépasser 500 caractères.");

        RuleFor(x => x.Statut)
        .IsInEnum().WithMessage("Le statut de l'établissement n'est pas valide.");
    }
}