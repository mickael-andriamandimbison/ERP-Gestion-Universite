using erp_univ_back.Features.FiliereNiveau.Create;
using FluentValidation.TestHelper;
using Xunit;

namespace erp_univ_back.Tests.Features.FiliereNiveau.Create;

public class CreateFiliereNiveauValidatorTests
{
    private readonly CreateFiliereNiveauValidator _validator = new();

    private CreateFiliereNiveauRequest GetValidRequest() => new(
        FiliereId: Guid.NewGuid(),
        NiveauId: Guid.NewGuid()
    );

    [Fact]
    public void Validator_DoitBloquer_QuandFiliereIdEstVide()
    {
        var request = GetValidRequest() with { FiliereId = Guid.Empty };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.FiliereId);
    }

    [Fact]
    public void Validator_DoitBloquer_QuandNiveauIdEstVide()
    {
        var request = GetValidRequest() with { NiveauId = Guid.Empty };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.NiveauId);
    }

    [Fact]
    public void Validator_DoitAccepter_QuandRequeteEstValide()
    {
        var request = GetValidRequest();
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }
}

