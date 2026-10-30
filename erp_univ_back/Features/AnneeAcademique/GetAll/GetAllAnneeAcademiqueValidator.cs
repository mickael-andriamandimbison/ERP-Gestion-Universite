using FluentValidation;

namespace erp_univ_back.Features.AnneeAcademique.GetAll;

public class GetAllAnneeAcademiqueValidator : AbstractValidator<GetAllAnneeAcademiqueRequest>
{
    public GetAllAnneeAcademiqueValidator()
    {
        RuleFor(x => x.Recherche)
            .MaximumLength(100)
            .WithMessage("Le terme de recherche ne peut pas dépasser 100 caractères.");
    }
}

