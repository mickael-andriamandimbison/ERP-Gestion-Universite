namespace erp_univ_back.Tests.Features.UniteEnseignement;

using erp_univ_back.Features.Etablissement;
using erp_univ_back.Features.Filiere;
using erp_univ_back.Features.FiliereNiveau;
using erp_univ_back.Features.Niveau;
using erp_univ_back.Features.UniteEnseignement;
using erp_univ_back.Features.UniteEnseignement.Utils;
using erp_univ_back.Shared.Data;
using erp_univ_back.Shared.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

public class UniteEnseignementCodeGeneratorTests
{
    private readonly ErpUnivDbContext _db;
    private readonly UniteEnseignementCodeGenerator _generator;
    private readonly Guid _etablissementId = Guid.NewGuid();

    public UniteEnseignementCodeGeneratorTests()
    {
        var tenantService = new TenantService();
        tenantService.DefinirEtablissement(_etablissementId);

        var options = new DbContextOptionsBuilder<ErpUnivDbContext>()
            .UseInMemoryDatabase("DbTest_UE_Generator_" + Guid.NewGuid())
            .Options;

        _db = new ErpUnivDbContext(options, tenantService);
        _generator = new UniteEnseignementCodeGenerator(_db);
    }

    [Fact]
    public void FormaterBase_DoitFormaterCorrectement_AvecMajusculesEtTrim()
    {
        var code = _generator.FormaterBase("  inf  ", "  l1  ");
        code.Should().Be("UE-INF-L1");
    }

    [Fact]
    public async Task GenererAsync_DoitGenererCodeDeBase_QuandAucuneUeExiste()
    {
        // Arrange
        var filiere = new FiliereEntity { Id = Guid.NewGuid(), Code = "INF", Nom = "Informatique", EtablissementId = _etablissementId };
        var niveau = new NiveauEntity { Id = Guid.NewGuid(), Code = "L1", Nom = "Licence 1", Ordre = 1, EtablissementId = _etablissementId };
        var filiereNiveau = new FiliereNiveauEntity
        {
            Id = Guid.NewGuid(),
            FiliereId = filiere.Id,
            Filiere = filiere,
            NiveauId = niveau.Id,
            Niveau = niveau,
            EtablissementId = _etablissementId
        };

        _db.Filieres.Add(filiere);
        _db.Niveaux.Add(niveau);
        _db.FiliereNiveaux.Add(filiereNiveau);
        await _db.SaveChangesAsync();

        // Act
        var result = await _generator.GenererAsync(filiereNiveau.Id);

        // Assert
        result.Should().Be("UE-INF-L1");
    }

    [Fact]
    public async Task GenererAsync_DoitIncrementerSuffixe_QuandCodeExisteDeja()
    {
        // Arrange
        var filiere = new FiliereEntity { Id = Guid.NewGuid(), Code = "INF", Nom = "Informatique", EtablissementId = _etablissementId };
        var niveau = new NiveauEntity { Id = Guid.NewGuid(), Code = "L1", Nom = "Licence 1", Ordre = 1, EtablissementId = _etablissementId };
        var filiereNiveau = new FiliereNiveauEntity
        {
            Id = Guid.NewGuid(),
            FiliereId = filiere.Id,
            Filiere = filiere,
            NiveauId = niveau.Id,
            Niveau = niveau,
            EtablissementId = _etablissementId
        };

        _db.Filieres.Add(filiere);
        _db.Niveaux.Add(niveau);
        _db.FiliereNiveaux.Add(filiereNiveau);

        // Première UE existante avec le code de base
        _db.UniteEnseignements.Add(new UniteEnseignementEntity
        {
            Id = Guid.NewGuid(),
            Code = "UE-INF-L1",
            Nom = "Algorithmique 1",
            Credit = 5,
            Semestre = 1,
            FiliereNiveauId = filiereNiveau.Id,
            EtablissementId = _etablissementId
        });
        await _db.SaveChangesAsync();

        // Act
        var result = await _generator.GenererAsync(filiereNiveau.Id);

        // Assert
        result.Should().Be("UE-INF-L1-2");
    }

    [Fact]
    public async Task GenererAsync_DoitLancerException_QuandFiliereNiveauIntrouvable()
    {
        // Act & Assert
        var act = async () => await _generator.GenererAsync(Guid.NewGuid());
        await act.Should().ThrowAsync<InvalidOperationException>();
    }
}
