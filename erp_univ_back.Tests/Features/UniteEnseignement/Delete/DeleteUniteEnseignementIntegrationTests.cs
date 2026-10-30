namespace erp_univ_back.Tests.Features.UniteEnseignement.Delete;

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using erp_univ_back.Features.Etablissement;
using erp_univ_back.Features.Etablissement.Create;
using erp_univ_back.Features.Filiere.Create;
using erp_univ_back.Features.FiliereNiveau.Create;
using erp_univ_back.Features.Niveau.Create;
using erp_univ_back.Features.UniteEnseignement.Create;
using erp_univ_back.Shared.Data;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

public class DeleteUniteEnseignementIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _http;
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    public DeleteUniteEnseignementIntegrationTests(WebApplicationFactory<Program> factory)
    {
        var dbName = "DbTest_UE_Delete_" + Guid.NewGuid().ToString();

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

    private async Task<Guid> CreerEtablissementAsync(string nom = "Univ UE Delete Test")
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

    private async Task<Guid> CreerFiliereNiveauAsync(Guid etablissementId, string filiereCode = "INF", string niveauCode = "L1")
    {
        var filiereReq = new CreateFiliereRequest(filiereCode, "Informatique " + filiereCode, null);
        using var filiereMsg = new HttpRequestMessage(HttpMethod.Post, "/api/filieres");
        filiereMsg.Headers.Add("X-EtablissementId", etablissementId.ToString());
        filiereMsg.Content = JsonContent.Create(filiereReq);
        var filiereRes = await _http.SendAsync(filiereMsg);
        var filiere = await filiereRes.Content.ReadFromJsonAsync<CreateFiliereResponse>(_jsonOptions);

        var niveauReq = new CreateNiveauRequest(niveauCode, "Licence " + niveauCode, 1);
        using var niveauMsg = new HttpRequestMessage(HttpMethod.Post, "/api/niveaux");
        niveauMsg.Headers.Add("X-EtablissementId", etablissementId.ToString());
        niveauMsg.Content = JsonContent.Create(niveauReq);
        var niveauRes = await _http.SendAsync(niveauMsg);
        var niveau = await niveauRes.Content.ReadFromJsonAsync<CreateNiveauResponse>(_jsonOptions);

        var fnReq = new CreateFiliereNiveauRequest(filiere!.Id, niveau!.Id);
        using var fnMsg = new HttpRequestMessage(HttpMethod.Post, "/api/filiere-niveaux");
        fnMsg.Headers.Add("X-EtablissementId", etablissementId.ToString());
        fnMsg.Content = JsonContent.Create(fnReq);
        var fnRes = await _http.SendAsync(fnMsg);
        var fn = await fnRes.Content.ReadFromJsonAsync<CreateFiliereNiveauResponse>(_jsonOptions);

        return fn!.Id;
    }

    private async Task<CreateUniteEnseignementResponse> CreerUeAsync(Guid etablissementId, Guid fnId, string nom = "Algorithmique 1", int credit = 5, int semestre = 1)
    {
        var request = new CreateUniteEnseignementRequest(nom, credit, semestre, fnId);
        using var msg = new HttpRequestMessage(HttpMethod.Post, "/api/unites-enseignement");
        msg.Headers.Add("X-EtablissementId", etablissementId.ToString());
        msg.Content = JsonContent.Create(request);

        var response = await _http.SendAsync(msg);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await response.Content.ReadFromJsonAsync<CreateUniteEnseignementResponse>(_jsonOptions);
        return created!;
    }

    [Fact]
    public async Task Delete_DoitRetourner204NoContent_QuandUeExiste()
    {
        // Arrange
        var etablissementId = await CreerEtablissementAsync();
        var fnId = await CreerFiliereNiveauAsync(etablissementId, "INF", "L1");
        var ue = await CreerUeAsync(etablissementId, fnId, "Structure de Données", 5, 1);

        using var message = new HttpRequestMessage(HttpMethod.Delete, $"/api/unites-enseignement/{ue.Id}");
        message.Headers.Add("X-EtablissementId", etablissementId.ToString());

        // Act
        var response = await _http.SendAsync(message);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Vérification de la suppression effective : une 2ème tentative retourne 404
        using var messageVerification = new HttpRequestMessage(HttpMethod.Delete, $"/api/unites-enseignement/{ue.Id}");
        messageVerification.Headers.Add("X-EtablissementId", etablissementId.ToString());
        var responseVerification = await _http.SendAsync(messageVerification);
        responseVerification.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_DoitRetourner404NotFound_QuandUeInexistante()
    {
        // Arrange
        var etablissementId = await CreerEtablissementAsync();

        using var message = new HttpRequestMessage(HttpMethod.Delete, $"/api/unites-enseignement/{Guid.NewGuid()}");
        message.Headers.Add("X-EtablissementId", etablissementId.ToString());

        // Act
        var response = await _http.SendAsync(message);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_DoitIsolerDonnees_EntreEtablissements()
    {
        // Arrange
        var etabA = await CreerEtablissementAsync("Univ A");
        var etabB = await CreerEtablissementAsync("Univ B");

        var fnIdA = await CreerFiliereNiveauAsync(etabA, "DROIT", "L1");
        var ueA = await CreerUeAsync(etabA, fnIdA, "Droit Romain", 3, 1);

        // Tentative de suppression de l'UE de l'établissement A avec le header de l'établissement B
        using var message = new HttpRequestMessage(HttpMethod.Delete, $"/api/unites-enseignement/{ueA.Id}");
        message.Headers.Add("X-EtablissementId", etabB.ToString());

        // Act
        var response = await _http.SendAsync(message);

        // Assert (doit être refusé avec 404 car l'UE n'existe pas dans le tenant B)
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}

