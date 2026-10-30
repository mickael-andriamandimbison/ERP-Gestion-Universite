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
using erp_univ_back.Features.Inscription.ChangeStatut;
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

namespace erp_univ_back.Tests.Features.Inscription.ChangeStatut;

public class ChangeStatutInscriptionIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _http;
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    public ChangeStatutInscriptionIntegrationTests(WebApplicationFactory<Program> factory)
    {
        var dbName = "DbTest_Inscription_ChangeStatut_" + Guid.NewGuid().ToString();

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

    private async Task<Guid> CreerEtablissementAsync(string nom = "Univ Inscription Statut")
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

    private async Task<ReinscriptionResponse> InscrireEtudiantAsync(Guid etablissementId, string nom = "Rakoto")
    {
        // 1. Etudiant
        var etuReq = new CreateEtudiantRequest(
            Nom: nom,
            Prenom: "Hery",
            DateNaissance: new DateOnly(2002, 3, 15),
            LieuNaissance: "Antananarivo",
            NumeroTelephone: "+26134" + Random.Shared.Next(10000000, 99999999),
            Email: $"etu_{Guid.NewGuid():N}@univ.mg"
        );
        using var etuMsg = new HttpRequestMessage(HttpMethod.Post, "/api/etudiants");
        etuMsg.Headers.Add("X-EtablissementId", etablissementId.ToString());
        etuMsg.Content = JsonContent.Create(etuReq);
        var etuRes = await _http.SendAsync(etuMsg);
        var etudiant = await etuRes.Content.ReadFromJsonAsync<CreateEtudiantResponse>(_jsonOptions);

        // 2. Filiere & Niveau & Classe
        var filReq = new CreateFiliereRequest("INFO", "Informatique");
        using var filMsg = new HttpRequestMessage(HttpMethod.Post, "/api/filieres");
        filMsg.Headers.Add("X-EtablissementId", etablissementId.ToString());
        filMsg.Content = JsonContent.Create(filReq);
        var filRes = await _http.SendAsync(filMsg);
        var fil = await filRes.Content.ReadFromJsonAsync<CreateFiliereResponse>(_jsonOptions);

        var nivReq = new CreateNiveauRequest("L1", "Licence 1", 1);
        using var nivMsg = new HttpRequestMessage(HttpMethod.Post, "/api/niveaux");
        nivMsg.Headers.Add("X-EtablissementId", etablissementId.ToString());
        nivMsg.Content = JsonContent.Create(nivReq);
        var nivRes = await _http.SendAsync(nivMsg);
        var niv = await nivRes.Content.ReadFromJsonAsync<CreateNiveauResponse>(_jsonOptions);

        var fnReq = new CreateFiliereNiveauRequest(fil!.Id, niv!.Id);
        using var fnMsg = new HttpRequestMessage(HttpMethod.Post, "/api/filiere-niveaux");
        fnMsg.Headers.Add("X-EtablissementId", etablissementId.ToString());
        fnMsg.Content = JsonContent.Create(fnReq);
        var fnRes = await _http.SendAsync(fnMsg);
        var fn = await fnRes.Content.ReadFromJsonAsync<CreateFiliereNiveauResponse>(_jsonOptions);

        // 3. Annee
        var anneeReq = new CreateAnneeAcademiqueRequest("2025-2026", new DateOnly(2025, 9, 1), new DateOnly(2026, 6, 30), EstActive: true);
        using var anneeMsg = new HttpRequestMessage(HttpMethod.Post, "/api/annees-academiques");
        anneeMsg.Headers.Add("X-EtablissementId", etablissementId.ToString());
        anneeMsg.Content = JsonContent.Create(anneeReq);
        var anneeRes = await _http.SendAsync(anneeMsg);
        var annee = await anneeRes.Content.ReadFromJsonAsync<CreateAnneeAcademiqueResponse>(_jsonOptions);

        // 4. Inscription
        var reinscriptionReq = new ReinscriptionRequest(etudiant!.Id, fn!.Id, annee!.Id);
        using var reinscriptionMsg = new HttpRequestMessage(HttpMethod.Post, "/api/inscriptions/reinscription");
        reinscriptionMsg.Headers.Add("X-EtablissementId", etablissementId.ToString());
        reinscriptionMsg.Content = JsonContent.Create(reinscriptionReq);
        var reinscriptionRes = await _http.SendAsync(reinscriptionMsg);
        var created = await reinscriptionRes.Content.ReadFromJsonAsync<ReinscriptionResponse>(_jsonOptions);
        return created!;
    }

    [Fact]
    public async Task Patch_DoitRetourner200OK_QuandChangementStatutEstValide()
    {
        // Arrange
        var etablissementId = await CreerEtablissementAsync();
        var inscription = await InscrireEtudiantAsync(etablissementId, "Rabe");

        var request = new ChangeStatutInscriptionRequest(StatutInscription.Abandon);

        using var message = new HttpRequestMessage(HttpMethod.Patch, $"/api/inscriptions/{inscription.Id}/statut");
        message.Headers.Add("X-EtablissementId", etablissementId.ToString());
        message.Content = JsonContent.Create(request, options: _jsonOptions);

        // Act
        var response = await _http.SendAsync(message);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ChangeStatutInscriptionResponse>(_jsonOptions);
        result.Should().NotBeNull();
        result!.Id.Should().Be(inscription.Id);
        result.Statut.Should().Be(StatutInscription.Abandon);
    }

    [Fact]
    public async Task Patch_DoitRetourner404NotFound_QuandInscriptionInexistante()
    {
        // Arrange
        var etablissementId = await CreerEtablissementAsync();
        var request = new ChangeStatutInscriptionRequest(StatutInscription.Annule);

        using var message = new HttpRequestMessage(HttpMethod.Patch, $"/api/inscriptions/{Guid.NewGuid()}/statut");
        message.Headers.Add("X-EtablissementId", etablissementId.ToString());
        message.Content = JsonContent.Create(request, options: _jsonOptions);

        // Act
        var response = await _http.SendAsync(message);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Patch_DoitRetourner404NotFound_QuandModificationDepuisAutreEtablissement()
    {
        // Arrange : Multi-tenant test
        var etablissement1 = await CreerEtablissementAsync("Univ 1");
        var etablissement2 = await CreerEtablissementAsync("Univ 2");

        var inscriptionEtab1 = await InscrireEtudiantAsync(etablissement1, "EtuEtabUn");

        var request = new ChangeStatutInscriptionRequest(StatutInscription.Annule);

        // Act : Tentative de mise à jour depuis le tenant 2
        using var message = new HttpRequestMessage(HttpMethod.Patch, $"/api/inscriptions/{inscriptionEtab1.Id}/statut");
        message.Headers.Add("X-EtablissementId", etablissement2.ToString());
        message.Content = JsonContent.Create(request, options: _jsonOptions);

        var response = await _http.SendAsync(message);

        // Assert : Invisible pour le tenant 2 -> 404
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
