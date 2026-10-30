using erp_univ_back.Features.Inscription;
using erp_univ_back.Features.Inscription.ChangeStatut;
using FluentValidation.TestHelper;
using Xunit;

namespace erp_univ_back.Tests.Features.Inscription.ChangeStatut;

public class ChangeStatutInscriptionValidatorTests
{
    private readonly ChangeStatutInscriptionValidator _validator = new();

    [Theory]
    [InlineData(StatutInscription.Inscrit)]
    [InlineData(StatutInscription.Abandon)]
    [InlineData(StatutInscription.Annule)]
    public void Validator_DoitAccepter_QuandStatutEstValide(StatutInscription statutValide)
    {
        var request = new ChangeStatutInscriptionRequest(statutValide);
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData((StatutInscription)99)]
    [InlineData((StatutInscription)(-1))]
    public void Validator_DoitBloquer_QuandStatutEstInvalide(StatutInscription statutInvalide)
    {
        var request = new ChangeStatutInscriptionRequest(statutInvalide);
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Statut);
    }
}

