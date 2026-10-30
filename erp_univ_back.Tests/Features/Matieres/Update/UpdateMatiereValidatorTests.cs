namespace erp_univ_back.Tests.Features.Matieres.Update;

using erp_univ_back.Features.Matiere.Update;
using FluentValidation.TestHelper;
using Xunit;

public class UpdateMatiereValidatorTests
{
    private readonly UpdateMatiereValidator _validator = new();

    [Fact]
    public void Validate_DoitReussir_QuandRequeteEstValide()
    {
        // Arrange
        var request = new UpdateMatiereRequest(
            Nom: "Algorithmique Avancée",
            Coefficient: 3,
            Code: "ALGO-NEW",
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
        var request = new UpdateMatiereRequest(
            Nom: nom!,
            Coefficient: 2
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
        var request = new UpdateMatiereRequest(
            Nom: new string('A', 151),
            Coefficient: 2
        );

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Nom)
            .WithErrorMessage("Le nom de la matière ne doit pas dépasser 150 caractères.");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_DoitEchouer_QuandCoefficientEstInferieurA1(int coefficient)
    {
        // Arrange
        var request = new UpdateMatiereRequest(
            Nom: "Algorithmique",
            Coefficient: coefficient
        );

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Coefficient)
            .WithErrorMessage("Le coefficient doit être supérieur ou égal à 1.");
    }

    [Fact]
    public void Validate_DoitEchouer_QuandCodeDepasse50Caracteres()
    {
        // Arrange
        var request = new UpdateMatiereRequest(
            Nom: "Algorithmique",
            Coefficient: 2,
            Code: new string('C', 51)
        );

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Code)
            .WithErrorMessage("La longueur du code ne doit pas dépasser 50 caractères.");
    }

    [Fact]
    public void Validate_DoitEchouer_QuandUniteEnseignementIdEstVide()
    {
        // Arrange
        var request = new UpdateMatiereRequest(
            Nom: "Algorithmique",
            Coefficient: 2,
            UniteEnseignementId: Guid.Empty
        );

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UniteEnseignementId)
            .WithErrorMessage("L'identifiant de l'unité d'enseignement ne doit pas être vide.");
    }
}

