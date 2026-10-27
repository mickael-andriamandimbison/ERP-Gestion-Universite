using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using erp_univ_back.Features.AnneeAcademique;
using erp_univ_back.Features.Etablissement;
using erp_univ_back.Features.Etablissement.Create;
using erp_univ_back.Features.Etudiant;
using erp_univ_back.Features.Filiere.Create;
using erp_univ_back.Features.FiliereNiveau.Create;
using erp_univ_back.Features.Inscription;
using erp_univ_back.Features.Niveau.Create;
using erp_univ_back.Shared.Data;
using erp_univ_back.Shared.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

namespace erp_univ_back.Tests.Features.FiliereNiveau.Delete;

public class DeleteFiliereNiveauIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _application;
    private readonly HttpClient _http;
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    public DeleteFiliereNiveauIntegrationTests(WebApplicationFactory<Program> factory)
    {
        var dbName = "DbTest_FiliereNiveau_Delete_" + Guid.NewGuid().ToString();

        _application = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.RemoveAll(typeof(DbContextOptions<ErpUnivDbContext>));
                services.RemoveAll(typeof(DbContextOptions));
                services.RemoveAll(typeof(IDbContextOptionsConfiguration<ErpUnivDbContext>));
                services.RemoveAll(typeof(ErpUnivDbContext));

                services.AddDbContext<ErpUnivDbContext>(options =>
                {
                    options.UseInMemoryDatabase(dbName);
                });
            });
        });

        _http = _application.CreateClient();
    }

    private async Task<Guid> CreerEtablissementAsync(string nom = "Univ FiliereNiveau Delete")
    {
        var request = new CreateEtablissementRequest(
            Nom: $"{nom}_{Guid.NewGuid():N}",
            Adresse: "Campus Universitaire",
            CodePostal: "101",
            Ville: "Antananarivo",
            Pays: "Madagascar",
            Telephone: "+26134" + Random.Shared.Next(10000000, 99999999),
            Email: $"univ_{Guid.NewGuid():N}@test.mg",
            Statut: StatusEtablissement.Actif
        );

        var response = await _http.PostAsJsonAsync("/api/etablissements", request);
        var created = await response.Content.ReadFromJsonAsync<CreateEtablissementResponse>(_jsonOptions);
        return created!.Id;
    }

    private async Task<CreateFiliereResponse> CreerFiliereAsync(Guid etablissementId, string code = "INFO", string nom = "Informatique")
    {
        var request = new CreateFiliereRequest(code, nom, "Description");

        using var message = new HttpRequestMessage(HttpMethod.Post, "/api/filieres");
        message.Headers.Add("X-EtablissementId", etablissementId.ToString());
        message.Content = JsonContent.Create(request);

        var response = await _http.SendAsync(message);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await response.Content.ReadFromJsonAsync<CreateFiliereResponse>(_jsonOptions);
        return created!;
    }

    private async Task<CreateNiveauResponse> CreerNiveauAsync(Guid etablissementId, string code = "L1", string nom = "Licence 1", int ordre = 1)
    {
        var request = new CreateNiveauRequest(code, nom, ordre);

        using var message = new HttpRequestMessage(HttpMethod.Post, "/api/niveaux");
        message.Headers.Add("X-EtablissementId", etablissementId.ToString());
        message.Content = JsonContent.Create(request);

        var response = await _http.SendAsync(message);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await response.Content.ReadFromJsonAsync<CreateNiveauResponse>(_jsonOptions);
        return created!;
    }

    private async Task<CreateFiliereNiveauResponse> CreerFiliereNiveauAsync(Guid etablissementId, Guid filiereId, Guid niveauId)
    {
        var request = new CreateFiliereNiveauRequest(filiereId, niveauId);

        using var message = new HttpRequestMessage(HttpMethod.Post, "/api/filiere-niveaux");
        message.Headers.Add("X-EtablissementId", etablissementId.ToString());
        message.Content = JsonContent.Create(request);

        var response = await _http.SendAsync(message);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await response.Content.ReadFromJsonAsync<CreateFiliereNiveauResponse>(_jsonOptions);
        return created!;
    }

    private async Task InscrireEtudiantAsync(Guid etablissementId, Guid filiereNiveauId)
    {
        using var scope = _application.Services.CreateScope();
        var tenantService = scope.ServiceProvider.GetRequiredService<ITenantService>();
        tenantService.DefinirEtablissement(etablissementId);
        var db = scope.ServiceProvider.GetRequiredService<ErpUnivDbContext>();

        var etudiant = new EtudiantEntity
        {
            Id = Guid.NewGuid(),
            EtablissementId = etablissementId,
            Matricule = "ETU_" + Guid.NewGuid().ToString("N")[..8],
            Nom = "Dupont",
            Prenom = "Jean",
            Email = $"etu_{Guid.NewGuid():N}@test.mg",
            NumeroTelephone = "+26134" + Random.Shared.Next(10000000, 99999999),
            DateNaissance = new DateOnly(2002, 1, 1),
            LieuNaissance = "Antananarivo"
        };

        var annee = new AnneeAcademiqueEntity
        {
            Id = Guid.NewGuid(),
            EtablissementId = etablissementId,
            Libelle = "2024-2025",
            DateDebut = new DateOnly(2024, 9, 1),
            DateFin = new DateOnly(2025, 6, 30),
            EstActive = true
        };

        var inscription = new InscriptionEntity
        {
            Id = Guid.NewGuid(),
            EtablissementId = etablissementId,
            EtudiantId = etudiant.Id,
            AnneeAcademiqueId = annee.Id,
            FiliereNiveauId = filiereNiveauId,
            Statut = StatutInscription.Inscrit
        };

        db.Etudiants.Add(etudiant);
        db.AnneeAcademiques.Add(annee);
        db.Inscriptions.Add(inscription);
        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task Delete_DoitRetourner204NoContent_QuandFiliereNiveauExiste()
    {
        // Arrange
        var etablissementId = await CreerEtablissementAsync();
        var filiere = await CreerFiliereAsync(etablissementId, "DROIT", "Droit Privé");
        var niveau = await CreerNiveauAsync(etablissementId, "L1_DR", "Licence 1 Droit", 1);
        var filiereNiveau = await CreerFiliereNiveauAsync(etablissementId, filiere.Id, niveau.Id);

        using var message = new HttpRequestMessage(HttpMethod.Delete, $"/api/filiere-niveaux/{filiereNiveau.Id}");
        message.Headers.Add("X-EtablissementId", etablissementId.ToString());

        // Act
        var response = await _http.SendAsync(message);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Delete_DoitRetourner404NotFound_QuandFiliereNiveauInexistant()
    {
        // Arrange
        var etablissementId = await CreerEtablissementAsync();

        using var message = new HttpRequestMessage(HttpMethod.Delete, $"/api/filiere-niveaux/{Guid.NewGuid()}");
        message.Headers.Add("X-EtablissementId", etablissementId.ToString());

        // Act
        var response = await _http.SendAsync(message);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_DoitRetourner404NotFound_QuandSuppressionDepuisAutreEtablissement()
    {
        // Arrange : Sécurité multi-tenant
        var etablissement1 = await CreerEtablissementAsync("Univ 1");
        var etablissement2 = await CreerEtablissementAsync("Univ 2");

        var filiere = await CreerFiliereAsync(etablissement1, "GEST", "Gestion");
        var niveau = await CreerNiveauAsync(etablissement1, "M1_G", "Master 1 Gestion", 4);
        var filiereNiveau = await CreerFiliereNiveauAsync(etablissement1, filiere.Id, niveau.Id);

        // Act : Tentative de suppression depuis l'établissement 2
        using var message = new HttpRequestMessage(HttpMethod.Delete, $"/api/filiere-niveaux/{filiereNiveau.Id}");
        message.Headers.Add("X-EtablissementId", etablissement2.ToString());

        var response = await _http.SendAsync(message);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_DoitRetourner409Conflict_QuandInscriptionsRattachees()
    {
        // Arrange
        var etablissementId = await CreerEtablissementAsync();
        var filiere = await CreerFiliereAsync(etablissementId, "AGRO", "Agronomie");
        var niveau = await CreerNiveauAsync(etablissementId, "L2_AG", "Licence 2 Agro", 2);
        var filiereNiveau = await CreerFiliereNiveauAsync(etablissementId, filiere.Id, niveau.Id);

        // Rattacher une inscription
        await InscrireEtudiantAsync(etablissementId, filiereNiveau.Id);

        // Act : Tentative de suppression
        using var message = new HttpRequestMessage(HttpMethod.Delete, $"/api/filiere-niveaux/{filiereNiveau.Id}");
        message.Headers.Add("X-EtablissementId", etablissementId.ToString());

        var response = await _http.SendAsync(message);

        // Assert : Doit refuser la suppression
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }
}

