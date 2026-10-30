using erp_univ_back.Features.Niveau.Create;
using FluentValidation.TestHelper;
using Xunit;

namespace erp_univ_back.Tests.Features.Niveau.Create;

public class CreateNiveauValidatorTests
{
    private readonly CreateNiveauValidator _validator = new();

    private CreateNiveauRequest GetValidRequest() => new(
        Code: "L1",
        Nom: "Licence 1ère année",
        Ordre: 1
    );

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Validator_DoitBloquer_QuandCodeEstVide(string? codeVide)
    {
        var request = GetValidRequest() with { Code = codeVide! };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Code);
    }

    [Fact]
    public void Validator_DoitBloquer_QuandCodeDepasseLongueurMax()
    {
        var request = GetValidRequest() with { Code = new string('A', 11) };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Code);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Validator_DoitBloquer_QuandNomEstVide(string? nomVide)
    {
        var request = GetValidRequest() with { Nom = nomVide! };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Nom);
    }

    [Fact]
    public void Validator_DoitBloquer_QuandNomDepasseLongueurMax()
    {
        var request = GetValidRequest() with { Nom = new string('A', 101) };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Nom);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-10)]
    public void Validator_DoitBloquer_QuandOrdreEstInferieurOuEgalAZero(int ordreInvalide)
    {
        var request = GetValidRequest() with { Ordre = ordreInvalide };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Ordre);
    }

    [Fact]
    public void Validator_DoitAccepter_QuandRequeteEstValide()
    {
        var request = GetValidRequest();
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }
}

