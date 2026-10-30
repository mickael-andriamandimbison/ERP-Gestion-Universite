namespace erp_univ_back.Tests.Features.Matieres.Delete;

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using erp_univ_back.Features.Etablissement;
using erp_univ_back.Features.Etablissement.Create;
using erp_univ_back.Features.Filiere.Create;
using erp_univ_back.Features.FiliereNiveau.Create;
using erp_univ_back.Features.Matiere.Create;
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

public class DeleteMatiereIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _http;
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    public DeleteMatiereIntegrationTests(WebApplicationFactory<Program> factory)
    {
        var dbName = "DbTest_Matiere_Delete_" + Guid.NewGuid().ToString();

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

    private async Task<Guid> CreerEtablissementAsync(string nom = "Univ Matiere Delete Test")
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

    private async Task<CreateUniteEnseignementResponse> CreerUeAsync(Guid etablissementId)
    {
        var filiereReq = new CreateFiliereRequest("INF", "Informatique INF", null);
        using var filiereMsg = new HttpRequestMessage(HttpMethod.Post, "/api/filieres");
        filiereMsg.Headers.Add("X-EtablissementId", etablissementId.ToString());
        filiereMsg.Content = JsonContent.Create(filiereReq);
        var filiereRes = await _http.SendAsync(filiereMsg);
        var filiere = await filiereRes.Content.ReadFromJsonAsync<CreateFiliereResponse>(_jsonOptions);

        var niveauReq = new CreateNiveauRequest("L1", "Licence 1", 1);
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

        var ueReq = new CreateUniteEnseignementRequest("UE Informatique", 6, 1, fn!.Id);
        using var ueMsg = new HttpRequestMessage(HttpMethod.Post, "/api/unites-enseignement");
        ueMsg.Headers.Add("X-EtablissementId", etablissementId.ToString());
        ueMsg.Content = JsonContent.Create(ueReq);
        var ueRes = await _http.SendAsync(ueMsg);
        var ue = await ueRes.Content.ReadFromJsonAsync<CreateUniteEnseignementResponse>(_jsonOptions);

        return ue!;
    }

    private async Task<CreateMatiereResponse> CreerMatiereAsync(Guid etablissementId, Guid ueId, string nom = "Algorithmique 1", string code = "ALGO")
    {
        var request = new CreateMatiereRequest(nom, code, 3, ueId);
        using var msg = new HttpRequestMessage(HttpMethod.Post, "/api/matieres");
        msg.Headers.Add("X-EtablissementId", etablissementId.ToString());
        msg.Content = JsonContent.Create(request);

        var response = await _http.SendAsync(msg);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await response.Content.ReadFromJsonAsync<CreateMatiereResponse>(_jsonOptions);
        return created!;
    }

    [Fact]
    public async Task Supprimer_DoitRetourner204_QuandMatiereExiste()
    {
        // Arrange
        var etablissementId = await CreerEtablissementAsync();
        var ue = await CreerUeAsync(etablissementId);
        var matiere = await CreerMatiereAsync(etablissementId, ue.Id, "Base de Données", "BDD");

        using var deleteMsg = new HttpRequestMessage(HttpMethod.Delete, $"/api/matieres/{matiere.Id}");
        deleteMsg.Headers.Add("X-EtablissementId", etablissementId.ToString());

        // Act
        var deleteRes = await _http.SendAsync(deleteMsg);

        // Assert
        deleteRes.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Une seconde tentative de suppression doit renvoyer 404
        using var deleteMsg2 = new HttpRequestMessage(HttpMethod.Delete, $"/api/matieres/{matiere.Id}");
        deleteMsg2.Headers.Add("X-EtablissementId", etablissementId.ToString());
        var deleteRes2 = await _http.SendAsync(deleteMsg2);
        deleteRes2.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Supprimer_DoitRetourner404_QuandMatiereInexistante()
    {
        // Arrange
        var etablissementId = await CreerEtablissementAsync();
        using var deleteMsg = new HttpRequestMessage(HttpMethod.Delete, $"/api/matieres/{Guid.NewGuid()}");
        deleteMsg.Headers.Add("X-EtablissementId", etablissementId.ToString());

        // Act
        var response = await _http.SendAsync(deleteMsg);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Supprimer_DoitIsolerDonnees_EntreEtablissements()
    {
        // Arrange
        var etabA = await CreerEtablissementAsync("Univ A");
        var etabB = await CreerEtablissementAsync("Univ B");

        var ueA = await CreerUeAsync(etabA);
        var matiereA = await CreerMatiereAsync(etabA, ueA.Id, "Algorithmique", "ALGO");

        // Établissement B tente de supprimer la matière créée par l'Établissement A
        using var deleteMsg = new HttpRequestMessage(HttpMethod.Delete, $"/api/matieres/{matiereA.Id}");
        deleteMsg.Headers.Add("X-EtablissementId", etabB.ToString());

        // Act
        var response = await _http.SendAsync(deleteMsg);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}

