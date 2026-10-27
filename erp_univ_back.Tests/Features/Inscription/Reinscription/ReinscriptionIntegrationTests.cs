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
using erp_univ_back.Features.Inscription;
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

namespace erp_univ_back.Tests.Features.Inscription.Reinscription;

public class ReinscriptionIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _http;
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    public ReinscriptionIntegrationTests(WebApplicationFactory<Program> factory)
    {
        var dbName = "DbTest_Inscription_Reinscription_" + Guid.NewGuid().ToString();

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

    private async Task<Guid> CreerEtablissementAsync(string nom = "Univ Inscription")
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
            DateNaissance: new DateOnly(2003, 7, 10),
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
        // 1. Filière
        var filRequest = new CreateFiliereRequest(filiereCode, $"Filiere {filiereCode}");
        using var filMsg = new HttpRequestMessage(HttpMethod.Post, "/api/filieres");
        filMsg.Headers.Add("X-EtablissementId", etablissementId.ToString());
        filMsg.Content = JsonContent.Create(filRequest);
        var filRes = await _http.SendAsync(filMsg);
        var fil = await filRes.Content.ReadFromJsonAsync<CreateFiliereResponse>(_jsonOptions);

        // 2. Niveau
        var nivRequest = new CreateNiveauRequest(niveauCode, $"Niveau {niveauCode}", 1);
        using var nivMsg = new HttpRequestMessage(HttpMethod.Post, "/api/niveaux");
        nivMsg.Headers.Add("X-EtablissementId", etablissementId.ToString());
        nivMsg.Content = JsonContent.Create(nivRequest);
        var nivRes = await _http.SendAsync(nivMsg);
        var niv = await nivRes.Content.ReadFromJsonAsync<CreateNiveauResponse>(_jsonOptions);

        // 3. FilièreNiveau
        var fnRequest = new CreateFiliereNiveauRequest(fil!.Id, niv!.Id);
        using var fnMsg = new HttpRequestMessage(HttpMethod.Post, "/api/filiere-niveaux");
        fnMsg.Headers.Add("X-EtablissementId", etablissementId.ToString());
        fnMsg.Content = JsonContent.Create(fnRequest);
        var fnRes = await _http.SendAsync(fnMsg);
        var fn = await fnRes.Content.ReadFromJsonAsync<CreateFiliereNiveauResponse>(_jsonOptions);
        return fn!;
    }

    private async Task<CreateAnneeAcademiqueResponse> CreerAnneeAcademiqueAsync(Guid etablissementId, string libelle, bool estActive = true)
    {
        var parts = libelle.Split('-');
        var startYear = int.Parse(parts[0].Trim());
        var endYear = int.Parse(parts[1].Trim());

        var request = new CreateAnneeAcademiqueRequest(
            Libelle: libelle,
            DateDebut: new DateOnly(startYear, 9, 1),
            DateFin: new DateOnly(endYear, 6, 30),
            EstActive: estActive
        );

        using var message = new HttpRequestMessage(HttpMethod.Post, "/api/annees-academiques");
        message.Headers.Add("X-EtablissementId", etablissementId.ToString());
        message.Content = JsonContent.Create(request);

        var response = await _http.SendAsync(message);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await response.Content.ReadFromJsonAsync<CreateAnneeAcademiqueResponse>(_jsonOptions);
        return created!;
    }

    [Fact]
    public async Task Reinscrire_DoitRetourner201Created_QuandRequeteEstValide()
    {
        // Arrange
        var etablissementId = await CreerEtablissementAsync();
        var etudiant = await CreerEtudiantAsync(etablissementId, "Ravelo");
        var classe = await CreerClasseAsync(etablissementId, "INFO", "L1");
        var annee = await CreerAnneeAcademiqueAsync(etablissementId, "2025-2026", estActive: true);

        var request = new ReinscriptionRequest(etudiant.Id, classe.Id, annee.Id);

        using var message = new HttpRequestMessage(HttpMethod.Post, "/api/inscriptions/reinscription");
        message.Headers.Add("X-EtablissementId", etablissementId.ToString());
        message.Content = JsonContent.Create(request);

        // Act
        var response = await _http.SendAsync(message);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var created = await response.Content.ReadFromJsonAsync<ReinscriptionResponse>(_jsonOptions);
        created.Should().NotBeNull();
        created!.Id.Should().NotBeEmpty();
        created.EtudiantId.Should().Be(etudiant.Id);
        created.FiliereNiveauId.Should().Be(classe.Id);
        created.AnneeAcademiqueId.Should().Be(annee.Id);
        created.Statut.Should().Be(StatutInscription.Inscrit);
    }

    [Fact]
    public async Task Reinscrire_DoitRetourner400BadRequest_QuandChampsObligatoiresVides()
    {
        // Arrange
        var etablissementId = await CreerEtablissementAsync();
        var request = new ReinscriptionRequest(Guid.Empty, Guid.Empty, Guid.Empty);

        using var message = new HttpRequestMessage(HttpMethod.Post, "/api/inscriptions/reinscription");
        message.Headers.Add("X-EtablissementId", etablissementId.ToString());
        message.Content = JsonContent.Create(request);

        // Act
        var response = await _http.SendAsync(message);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Reinscrire_DoitRetourner400BadRequest_QuandAnneeAcademiqueEstInactive()
    {
        // Arrange
        var etablissementId = await CreerEtablissementAsync();
        var etudiant = await CreerEtudiantAsync(etablissementId, "Razaka");
        var classe = await CreerClasseAsync(etablissementId, "GEST", "M1");
        var anneeInactive = await CreerAnneeAcademiqueAsync(etablissementId, "2020-2021", estActive: false);

        var request = new ReinscriptionRequest(etudiant.Id, classe.Id, anneeInactive.Id);

        using var message = new HttpRequestMessage(HttpMethod.Post, "/api/inscriptions/reinscription");
        message.Headers.Add("X-EtablissementId", etablissementId.ToString());
        message.Content = JsonContent.Create(request);

        // Act
        var response = await _http.SendAsync(message);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Reinscrire_DoitRetourner404NotFound_QuandEtudiantInexistant()
    {
        // Arrange
        var etablissementId = await CreerEtablissementAsync();
        var classe = await CreerClasseAsync(etablissementId, "DROIT", "L2");
        var annee = await CreerAnneeAcademiqueAsync(etablissementId, "2024-2025", estActive: true);

        var request = new ReinscriptionRequest(Guid.NewGuid(), classe.Id, annee.Id);

        using var message = new HttpRequestMessage(HttpMethod.Post, "/api/inscriptions/reinscription");
        message.Headers.Add("X-EtablissementId", etablissementId.ToString());
        message.Content = JsonContent.Create(request);

        // Act
        var response = await _http.SendAsync(message);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Reinscrire_DoitRetourner409Conflict_QuandEtudiantDejaInscritDansCetteClasseEtAnnee()
    {
        // Arrange
        var etablissementId = await CreerEtablissementAsync();
        var etudiant = await CreerEtudiantAsync(etablissementId, "Andria");
        var classe = await CreerClasseAsync(etablissementId, "AGRO", "L3");
        var annee = await CreerAnneeAcademiqueAsync(etablissementId, "2026-2027", estActive: true);

        var request = new ReinscriptionRequest(etudiant.Id, classe.Id, annee.Id);

        // Première inscription réussie
        using var firstMsg = new HttpRequestMessage(HttpMethod.Post, "/api/inscriptions/reinscription");
        firstMsg.Headers.Add("X-EtablissementId", etablissementId.ToString());
        firstMsg.Content = JsonContent.Create(request);
        var firstRes = await _http.SendAsync(firstMsg);
        firstRes.StatusCode.Should().Be(HttpStatusCode.Created);

        // Act : Deuxième tentative identique
        using var secondMsg = new HttpRequestMessage(HttpMethod.Post, "/api/inscriptions/reinscription");
        secondMsg.Headers.Add("X-EtablissementId", etablissementId.ToString());
        secondMsg.Content = JsonContent.Create(request);
        var secondRes = await _http.SendAsync(secondMsg);

        // Assert : Doublon interdit
        secondRes.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Reinscrire_DoitRetourner404NotFound_QuandEtudiantAppartientAUnAutreEtablissement()
    {
        // Arrange : Multi-tenant test
        var etablissement1 = await CreerEtablissementAsync("Univ 1");
        var etablissement2 = await CreerEtablissementAsync("Univ 2");

        var etudiantEtab1 = await CreerEtudiantAsync(etablissement1, "EtudiantEtabUn");
        var classeEtab2 = await CreerClasseAsync(etablissement2, "MED", "P1");
        var anneeEtab2 = await CreerAnneeAcademiqueAsync(etablissement2, "2025-2026", estActive: true);

        // Tentative d'inscrire l'étudiant d'Etab 1 dans l'Etab 2
        var request = new ReinscriptionRequest(etudiantEtab1.Id, classeEtab2.Id, anneeEtab2.Id);

        using var message = new HttpRequestMessage(HttpMethod.Post, "/api/inscriptions/reinscription");
        message.Headers.Add("X-EtablissementId", etablissement2.ToString());
        message.Content = JsonContent.Create(request);

        // Act
        var response = await _http.SendAsync(message);

        // Assert : Invisible pour Etab 2 -> 404
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
