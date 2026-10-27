using erp_univ_back.Features.Inscription.Reinscription;
using FluentValidation.TestHelper;
using Xunit;

namespace erp_univ_back.Tests.Features.Inscription.Reinscription;

public class ReinscriptionValidatorTests
{
    private readonly ReinscriptionValidator _validator = new();

    private ReinscriptionRequest GetValidRequest() => new(
        EtudiantId: Guid.NewGuid(),
        FiliereNiveauId: Guid.NewGuid(),
        AnneeAcademiqueId: Guid.NewGuid()
    );

    [Fact]
    public void Validator_DoitBloquer_QuandEtudiantIdEstVide()
    {
        var request = GetValidRequest() with { EtudiantId = Guid.Empty };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.EtudiantId);
    }

    [Fact]
    public void Validator_DoitBloquer_QuandFiliereNiveauIdEstVide()
    {
        var request = GetValidRequest() with { FiliereNiveauId = Guid.Empty };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.FiliereNiveauId);
    }

    [Fact]
    public void Validator_DoitBloquer_QuandAnneeAcademiqueIdEstVide()
    {
        var request = GetValidRequest() with { AnneeAcademiqueId = Guid.Empty };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.AnneeAcademiqueId);
    }

    [Fact]
    public void Validator_DoitAccepter_QuandRequeteEstValide()
    {
        var request = GetValidRequest();
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }
}

