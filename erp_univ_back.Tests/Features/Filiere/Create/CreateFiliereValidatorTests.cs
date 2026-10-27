using erp_univ_back.Features.Filiere.Create;
using FluentValidation.TestHelper;
using Xunit;

namespace erp_univ_back.Tests.Features.Filiere.Create;

public class CreateFiliereValidatorTests
{
    private readonly CreateFiliereValidator _validator = new();

    private CreateFiliereRequest GetValidRequest() => new(
        Code: "INFO",
        Nom: "Informatique et Télécommunications",
        Description: "Filière dédiée au développement et aux réseaux."
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
        var request = GetValidRequest() with { Code = new string('A', 21) };
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
        var request = GetValidRequest() with { Nom = new string('A', 151) };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Nom);
    }

    [Fact]
    public void Validator_DoitBloquer_QuandDescriptionDepasseLongueurMax()
    {
        var request = GetValidRequest() with { Description = new string('A', 501) };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Validator_DoitAccepter_QuandRequeteEstValide()
    {
        var request = GetValidRequest();
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }
}

