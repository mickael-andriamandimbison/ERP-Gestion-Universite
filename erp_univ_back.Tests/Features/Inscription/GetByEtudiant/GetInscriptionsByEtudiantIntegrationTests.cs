using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using erp_univ_back.Features.AnneeAcademique.Create;
using erp_univ_back.Features.Etablissement;
using erp_univ_back.Features.Etablissement.Create;
using erp_univ_back.Features.Etudiant.Create;
using erp_univ_back.Features.Filiere.Create;
using erp_univ_back.Features.FiliereNiveau.Create;
using erp_univ_back.Features.Inscription.GetByEtudiant;
using erp_univ_back.Features.Inscription.Reinscription;
using erp_univ_back.Features.Niveau.Create;
using erp_univ_back.Shared.Data;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

namespace erp_univ_back.Tests.Features.Inscription.GetByEtudiant;

public class GetInscriptionsByEtudiantIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _http;
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    public GetInscriptionsByEtudiantIntegrationTests(WebApplicationFactory<Program> factory)
    {
        var dbName = "DbTest_Inscription_GetByEtudiant_" + Guid.NewGuid().ToString();

        var application = factory.WithWebHostBuilder(builder =>
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

        _http = application.CreateClient();
    }

    private async Task<Guid> CreerEtablissementAsync(string nom = "Univ Inscription GetByEtudiant")
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

    private async Task<CreateEtudiantResponse> CreerEtudiantAsync(Guid etablissementId, string nom = "Rakoto")
    {
        var request = new CreateEtudiantRequest(
            Nom: nom,
            Prenom: "Hery",
            DateNaissance: new DateOnly(2003, 1, 15),
            LieuNaissance: "Antananarivo",
            NumeroTelephone: "+26134" + Random.Shared.Next(10000000, 99999999),
            Email: $"etu_{Guid.NewGuid():N}@univ.mg"
        );

        using var message = new HttpRequestMessage(HttpMethod.Post, "/api/etudiants");
        message.Headers.Add("X-EtablissementId", etablissementId.ToString());
        message.Content = JsonContent.Create(request);

        var response = await _http.SendAsync(message);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await response.Content.ReadFromJsonAsync<CreateEtudiantResponse>(_jsonOptions);
        return created!;
    }

    private async Task<CreateFiliereResponse> CreerFiliereAsync(Guid etablissementId, string code, string nom)
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

    private async Task<CreateNiveauResponse> CreerNiveauAsync(Guid etablissementId, string code, string nom, int ordre)
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

    private async Task<CreateAnneeAcademiqueResponse> CreerAnneeAcademiqueAsync(Guid etablissementId, string libelle, DateOnly debut, DateOnly fin)
    {
        var request = new CreateAnneeAcademiqueRequest(
            Libelle: libelle,
            DateDebut: debut,
            DateFin: fin,
            EstActive: true
        );

        using var message = new HttpRequestMessage(HttpMethod.Post, "/api/annees-academiques");
        message.Headers.Add("X-EtablissementId", etablissementId.ToString());
        message.Content = JsonContent.Create(request);

        var response = await _http.SendAsync(message);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await response.Content.ReadFromJsonAsync<CreateAnneeAcademiqueResponse>(_jsonOptions);
        return created!;
    }

    private async Task InscrireEtudiantAsync(Guid etablissementId, Guid etudiantId, Guid classeId, Guid anneeId)
    {
        var request = new ReinscriptionRequest(etudiantId, classeId, anneeId);

        using var message = new HttpRequestMessage(HttpMethod.Post, "/api/inscriptions/reinscription");
        message.Headers.Add("X-EtablissementId", etablissementId.ToString());
        message.Content = JsonContent.Create(request);

        var response = await _http.SendAsync(message);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task GetByEtudiant_DoitRetourner200OK_AvecHistoriqueInscriptions()
    {
        // Arrange
        var etablissementId = await CreerEtablissementAsync();
        var etudiant = await CreerEtudiantAsync(etablissementId, "Ramanandraibe");

        var filiere = await CreerFiliereAsync(etablissementId, "INFO", "Informatique");
        var n1 = await CreerNiveauAsync(etablissementId, "L1", "Licence 1", 1);
        var n2 = await CreerNiveauAsync(etablissementId, "L2", "Licence 2", 2);

        var c1 = await CreerFiliereNiveauAsync(etablissementId, filiere.Id, n1.Id);
        var a1 = await CreerAnneeAcademiqueAsync(etablissementId, "2024-2025", new DateOnly(2024, 9, 1), new DateOnly(2025, 6, 30));
        await InscrireEtudiantAsync(etablissementId, etudiant.Id, c1.Id, a1.Id);

        var c2 = await CreerFiliereNiveauAsync(etablissementId, filiere.Id, n2.Id);
        var a2 = await CreerAnneeAcademiqueAsync(etablissementId, "2025-2026", new DateOnly(2025, 9, 1), new DateOnly(2026, 6, 30));
        await InscrireEtudiantAsync(etablissementId, etudiant.Id, c2.Id, a2.Id);

        using var message = new HttpRequestMessage(HttpMethod.Get, $"/api/inscriptions/etudiant/{etudiant.Id}");
        message.Headers.Add("X-EtablissementId", etablissementId.ToString());

        // Act
        var response = await _http.SendAsync(message);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<List<GetInscriptionsByEtudiantResponse>>(_jsonOptions);
        result.Should().NotBeNull();
        result!.Should().HaveCount(2);
        // Doit être ordonné par date début décroissant (2025-2026 en premier, puis 2024-2025)
        result![0].AnneeAcademiqueLibelle.Should().Be("2025-2026");
        result[1].AnneeAcademiqueLibelle.Should().Be("2024-2025");
    }

    [Fact]
    public async Task GetByEtudiant_DoitRetourner404NotFound_QuandEtudiantInexistant()
    {
        // Arrange
        var etablissementId = await CreerEtablissementAsync();

        using var message = new HttpRequestMessage(HttpMethod.Get, $"/api/inscriptions/etudiant/{Guid.NewGuid()}");
        message.Headers.Add("X-EtablissementId", etablissementId.ToString());

        // Act
        var response = await _http.SendAsync(message);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetByEtudiant_DoitRetourner404NotFound_QuandConsultationDepuisAutreEtablissement()
    {
        // Arrange : Multi-tenant test
        var etablissement1 = await CreerEtablissementAsync("Univ 1");
        var etablissement2 = await CreerEtablissementAsync("Univ 2");

        var etudiantEtab1 = await CreerEtudiantAsync(etablissement1, "EtuPrive");

        // Act : Tentative de consultation depuis le tenant 2
        using var message = new HttpRequestMessage(HttpMethod.Get, $"/api/inscriptions/etudiant/{etudiantEtab1.Id}");
        message.Headers.Add("X-EtablissementId", etablissement2.ToString());

        var response = await _http.SendAsync(message);

        // Assert : Invisible pour le tenant 2 -> 404
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}

