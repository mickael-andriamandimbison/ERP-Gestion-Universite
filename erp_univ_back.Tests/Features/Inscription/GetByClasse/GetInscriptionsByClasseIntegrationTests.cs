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
using erp_univ_back.Features.Inscription.GetByClasse;
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

namespace erp_univ_back.Tests.Features.Inscription.GetByClasse;

public class GetInscriptionsByClasseIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _http;
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    public GetInscriptionsByClasseIntegrationTests(WebApplicationFactory<Program> factory)
    {
        var dbName = "DbTest_Inscription_GetByClasse_" + Guid.NewGuid().ToString();

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

    private async Task<Guid> CreerEtablissementAsync(string nom = "Univ Inscription GetByClasse")
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

    private async Task<CreateEtudiantResponse> CreerEtudiantAsync(Guid etablissementId, string nom)
    {
        var request = new CreateEtudiantRequest(
            Nom: nom,
            Prenom: "PrenomTest",
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

    private async Task<CreateFiliereNiveauResponse> CreerClasseAsync(Guid etablissementId, string filiereCode, string niveauCode)
    {
        var filRequest = new CreateFiliereRequest(filiereCode, $"Filiere {filiereCode}");
        using var filMsg = new HttpRequestMessage(HttpMethod.Post, "/api/filieres");
        filMsg.Headers.Add("X-EtablissementId", etablissementId.ToString());
        filMsg.Content = JsonContent.Create(filRequest);
        var filRes = await _http.SendAsync(filMsg);
        var fil = await filRes.Content.ReadFromJsonAsync<CreateFiliereResponse>(_jsonOptions);

        var nivRequest = new CreateNiveauRequest(niveauCode, $"Niveau {niveauCode}", 1);
        using var nivMsg = new HttpRequestMessage(HttpMethod.Post, "/api/niveaux");
        nivMsg.Headers.Add("X-EtablissementId", etablissementId.ToString());
        nivMsg.Content = JsonContent.Create(nivRequest);
        var nivRes = await _http.SendAsync(nivMsg);
        var niv = await nivRes.Content.ReadFromJsonAsync<CreateNiveauResponse>(_jsonOptions);

        var fnRequest = new CreateFiliereNiveauRequest(fil!.Id, niv!.Id);
        using var fnMsg = new HttpRequestMessage(HttpMethod.Post, "/api/filiere-niveaux");
        fnMsg.Headers.Add("X-EtablissementId", etablissementId.ToString());
        fnMsg.Content = JsonContent.Create(fnRequest);
        var fnRes = await _http.SendAsync(fnMsg);
        var fn = await fnRes.Content.ReadFromJsonAsync<CreateFiliereNiveauResponse>(_jsonOptions);
        return fn!;
    }

    private async Task<CreateAnneeAcademiqueResponse> CreerAnneeAcademiqueAsync(Guid etablissementId, string libelle)
    {
        var parts = libelle.Split('-');
        var startYear = int.Parse(parts[0].Trim());
        var endYear = int.Parse(parts[1].Trim());

        var request = new CreateAnneeAcademiqueRequest(
            Libelle: libelle,
            DateDebut: new DateOnly(startYear, 9, 1),
            DateFin: new DateOnly(endYear, 6, 30),
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
    public async Task GetByClasse_DoitRetourner200OK_AvecListeInscritsTrieeParNom()
    {
        // Arrange
        var etablissementId = await CreerEtablissementAsync();
        var classe = await CreerClasseAsync(etablissementId, "DROIT", "L1");
        var annee = await CreerAnneeAcademiqueAsync(etablissementId, "2025-2026");

        var e1 = await CreerEtudiantAsync(etablissementId, "Zafy");
        var e2 = await CreerEtudiantAsync(etablissementId, "Andry");

        await InscrireEtudiantAsync(etablissementId, e1.Id, classe.Id, annee.Id);
        await InscrireEtudiantAsync(etablissementId, e2.Id, classe.Id, annee.Id);

        using var message = new HttpRequestMessage(HttpMethod.Get, $"/api/inscriptions/classe/{classe.Id}/annee/{annee.Id}");
        message.Headers.Add("X-EtablissementId", etablissementId.ToString());

        // Act
        var response = await _http.SendAsync(message);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<List<GetInscriptionsByClasseResponse>>(_jsonOptions);
        result.Should().NotBeNull();
        result!.Should().HaveCount(2);
        // Doit être trié par nom (Andry puis Zafy)
        result![0].Nom.Should().Be("Andry");
        result[1].Nom.Should().Be("Zafy");
    }

    [Fact]
    public async Task GetByClasse_DoitRetourner404NotFound_QuandClasseInexistante()
    {
        // Arrange
        var etablissementId = await CreerEtablissementAsync();
        var annee = await CreerAnneeAcademiqueAsync(etablissementId, "2024-2025");

        using var message = new HttpRequestMessage(HttpMethod.Get, $"/api/inscriptions/classe/{Guid.NewGuid()}/annee/{annee.Id}");
        message.Headers.Add("X-EtablissementId", etablissementId.ToString());

        // Act
        var response = await _http.SendAsync(message);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetByClasse_DoitRetourner404NotFound_QuandAnneeInexistante()
    {
        // Arrange
        var etablissementId = await CreerEtablissementAsync();
        var classe = await CreerClasseAsync(etablissementId, "ECO", "L2");

        using var message = new HttpRequestMessage(HttpMethod.Get, $"/api/inscriptions/classe/{classe.Id}/annee/{Guid.NewGuid()}");
        message.Headers.Add("X-EtablissementId", etablissementId.ToString());

        // Act
        var response = await _http.SendAsync(message);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetByClasse_DoitRetourner404NotFound_QuandConsultationDepuisAutreEtablissement()
    {
        // Arrange : Multi-tenant test
        var etablissement1 = await CreerEtablissementAsync("Univ 1");
        var etablissement2 = await CreerEtablissementAsync("Univ 2");

        var classeEtab1 = await CreerClasseAsync(etablissement1, "MED", "P1");
        var anneeEtab1 = await CreerAnneeAcademiqueAsync(etablissement1, "2025-2026");

        // Act : Tentative de consultation depuis l'établissement 2
        using var message = new HttpRequestMessage(HttpMethod.Get, $"/api/inscriptions/classe/{classeEtab1.Id}/annee/{anneeEtab1.Id}");
        message.Headers.Add("X-EtablissementId", etablissement2.ToString());

        var response = await _http.SendAsync(message);

        // Assert : Invisible pour le tenant 2 -> 404
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
