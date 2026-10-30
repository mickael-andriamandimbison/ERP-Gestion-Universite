namespace erp_univ_back.Tests.Features.Matieres.Create;

using erp_univ_back.Features.Matiere.Create;
using FluentValidation.TestHelper;
using Xunit;

public class CreateMatiereValidatorTests
{
    private readonly CreateMatiereValidator _validator = new();

    [Fact]
    public void Validate_DoitReussir_QuandRequeteEstValide()
    {
        // Arrange
        var request = new CreateMatiereRequest(
            Nom: "Algorithmique Avancée",
            Code: "ALGO",
            Coefficient: 2,
            UniteEnseignementId: Guid.NewGuid()
        );

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_DoitEchouer_QuandNomEstVide(string? nom)
    {
        // Arrange
        var request = new CreateMatiereRequest(
            Nom: nom!,
            Code: "ALGO",
            Coefficient: 2,
            UniteEnseignementId: Guid.NewGuid()
        );

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Nom)
            .WithErrorMessage("Le nom de la matière est obligatoire.");
    }

    [Fact]
    public void Validate_DoitEchouer_QuandNomDepasse150Caracteres()
    {
        // Arrange
        var request = new CreateMatiereRequest(
            Nom: new string('A', 151),
            Code: "ALGO",
            Coefficient: 2,
            UniteEnseignementId: Guid.NewGuid()
        );

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Nom)
            .WithErrorMessage("Le nom de la matière ne doit pas dépasser 150 caractères.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_DoitEchouer_QuandCodeEstVide(string? code)
    {
        // Arrange
        var request = new CreateMatiereRequest(
            Nom: "Algorithmique Avancée",
            Code: code!,
            Coefficient: 2,
            UniteEnseignementId: Guid.NewGuid()
        );

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Code)
            .WithErrorMessage("Le code est un champ obligatoire.");
    }

    [Fact]
    public void Validate_DoitEchouer_QuandCodeDepasse10Caracteres()
    {
        // Arrange
        var request = new CreateMatiereRequest(
            Nom: "Algorithmique Avancée",
            Code: "12345678901", // 11 caractères
            Coefficient: 2,
            UniteEnseignementId: Guid.NewGuid()
        );

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Code)
            .WithErrorMessage("La longueur du code doit être inférieure ou égale à 10 caractères.");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_DoitEchouer_QuandCoefficientEstInferieurA1(int coefficient)
    {
        // Arrange
        var request = new CreateMatiereRequest(
            Nom: "Algorithmique Avancée",
            Code: "ALGO",
            Coefficient: coefficient,
            UniteEnseignementId: Guid.NewGuid()
        );

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Coefficient)
            .WithErrorMessage("Le coefficient doit être supérieur ou égal à 1.");
    }

    [Fact]
    public void Validate_DoitEchouer_QuandUniteEnseignementIdEstVide()
    {
        // Arrange
        var request = new CreateMatiereRequest(
            Nom: "Algorithmique Avancée",
            Code: "ALGO",
            Coefficient: 2,
            UniteEnseignementId: Guid.Empty
        );

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UniteEnseignementId)
            .WithErrorMessage("L'identifiant de l'unité d'enseignement est obligatoire.");
    }
}

