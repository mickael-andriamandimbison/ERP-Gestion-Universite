namespace erp_univ_back.Tests.Features.UniteEnseignement.Update;

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
using erp_univ_back.Features.UniteEnseignement.Update;
using erp_univ_back.Shared.Data;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

public class UpdateUniteEnseignementIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _http;
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    public UpdateUniteEnseignementIntegrationTests(WebApplicationFactory<Program> factory)
    {
        var dbName = "DbTest_UE_Update_" + Guid.NewGuid().ToString();

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

    private async Task<Guid> CreerEtablissementAsync(string nom = "Univ UE Update Test")
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
    public async Task Put_DoitRetourner200OK_QuandMiseAJourValide()
    {
        // Arrange
        var etablissementId = await CreerEtablissementAsync();
        var fnId = await CreerFiliereNiveauAsync(etablissementId, "INF", "L1");
        var ue = await CreerUeAsync(etablissementId, fnId, "Algorithmique Initiale", 5, 1);

        var updateRequest = new UpdateUniteEnseignementRequest(
            Nom: "Algorithmique Avancée",
            Credit: 6,
            Semestre: 2
        );

        using var message = new HttpRequestMessage(HttpMethod.Put, $"/api/unites-enseignement/{ue.Id}");
        message.Headers.Add("X-EtablissementId", etablissementId.ToString());
        message.Content = JsonContent.Create(updateRequest);

        // Act
        var response = await _http.SendAsync(message);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await response.Content.ReadFromJsonAsync<UpdateUniteEnseignementResponse>(_jsonOptions);
        updated.Should().NotBeNull();
        updated!.Id.Should().Be(ue.Id);
        updated.Nom.Should().Be("Algorithmique Avancée");
        updated.Credit.Should().Be(6);
        updated.Semestre.Should().Be(2);
        updated.Code.Should().Be(ue.Code); // Code inchangé si non fourni
    }

    [Fact]
    public async Task Put_DoitRetourner200OK_QuandCodeEstModifieAvecCodeUnique()
    {
        // Arrange
        var etablissementId = await CreerEtablissementAsync();
        var fnId = await CreerFiliereNiveauAsync(etablissementId, "INF", "L1");
        var ue = await CreerUeAsync(etablissementId, fnId, "Maths", 4, 1);

        var updateRequest = new UpdateUniteEnseignementRequest(
            Nom: "Maths Appliquées",
            Credit: 4,
            Semestre: 1,
            Code: "UE-INF-L1-MATHS"
        );

        using var message = new HttpRequestMessage(HttpMethod.Put, $"/api/unites-enseignement/{ue.Id}");
        message.Headers.Add("X-EtablissementId", etablissementId.ToString());
        message.Content = JsonContent.Create(updateRequest);

        // Act
        var response = await _http.SendAsync(message);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await response.Content.ReadFromJsonAsync<UpdateUniteEnseignementResponse>(_jsonOptions);
        updated!.Code.Should().Be("UE-INF-L1-MATHS");
    }

    [Fact]
    public async Task Put_DoitRetourner400BadRequest_QuandChampsInvalides()
    {
        // Arrange
        var etablissementId = await CreerEtablissementAsync();
        var fnId = await CreerFiliereNiveauAsync(etablissementId, "INF", "L1");
        var ue = await CreerUeAsync(etablissementId, fnId, "Web", 3, 1);

        var updateRequest = new UpdateUniteEnseignementRequest(
            Nom: "",
            Credit: 0,
            Semestre: 3
        );

        using var message = new HttpRequestMessage(HttpMethod.Put, $"/api/unites-enseignement/{ue.Id}");
        message.Headers.Add("X-EtablissementId", etablissementId.ToString());
        message.Content = JsonContent.Create(updateRequest);

        // Act
        var response = await _http.SendAsync(message);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Put_DoitRetourner404NotFound_QuandUeInexistante()
    {
        // Arrange
        var etablissementId = await CreerEtablissementAsync();
        var updateRequest = new UpdateUniteEnseignementRequest("Inexistant", 4, 1);

        using var message = new HttpRequestMessage(HttpMethod.Put, $"/api/unites-enseignement/{Guid.NewGuid()}");
        message.Headers.Add("X-EtablissementId", etablissementId.ToString());
        message.Content = JsonContent.Create(updateRequest);

        // Act
        var response = await _http.SendAsync(message);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Put_DoitRetourner409Conflict_QuandCodeExisteDejaPourAutreUE()
    {
        // Arrange
        var etablissementId = await CreerEtablissementAsync();
        var fnId = await CreerFiliereNiveauAsync(etablissementId, "INF", "L1");
        var ue1 = await CreerUeAsync(etablissementId, fnId, "UE 1", 4, 1);
        var ue2 = await CreerUeAsync(etablissementId, fnId, "UE 2", 4, 1);

        // Tentative d'affecter le code de ue1 à ue2
        var updateRequest = new UpdateUniteEnseignementRequest(
            Nom: "UE 2 Modifiee",
            Credit: 4,
            Semestre: 1,
            Code: ue1.Code
        );

        using var message = new HttpRequestMessage(HttpMethod.Put, $"/api/unites-enseignement/{ue2.Id}");
        message.Headers.Add("X-EtablissementId", etablissementId.ToString());
        message.Content = JsonContent.Create(updateRequest);

        // Act
        var response = await _http.SendAsync(message);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Put_DoitRetourner409Conflict_QuandNomExisteDejaDansMemeClasseEtSemestre()
    {
        // Arrange
        var etablissementId = await CreerEtablissementAsync();
        var fnId = await CreerFiliereNiveauAsync(etablissementId, "GEST", "L2");
        var ue1 = await CreerUeAsync(etablissementId, fnId, "Comptabilite", 5, 1);
        var ue2 = await CreerUeAsync(etablissementId, fnId, "Marketing", 4, 1);

        // Tentative de renommer ue2 avec le même nom et semestre que ue1
        var updateRequest = new UpdateUniteEnseignementRequest(
            Nom: "Comptabilite",
            Credit: 4,
            Semestre: 1
        );

        using var message = new HttpRequestMessage(HttpMethod.Put, $"/api/unites-enseignement/{ue2.Id}");
        message.Headers.Add("X-EtablissementId", etablissementId.ToString());
        message.Content = JsonContent.Create(updateRequest);

        // Act
        var response = await _http.SendAsync(message);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Put_DoitIsolerDonnees_EntreEtablissements()
    {
        // Arrange
        var etabA = await CreerEtablissementAsync("Univ A");
        var etabB = await CreerEtablissementAsync("Univ B");

        var fnIdA = await CreerFiliereNiveauAsync(etabA, "DROIT", "L1");
        var ueA = await CreerUeAsync(etabA, fnIdA, "Droit Civil", 5, 1);

        // Tenant B tente de modifier l'UE du Tenant A
        var updateRequest = new UpdateUniteEnseignementRequest("Droit Modifie", 6, 1);

        using var message = new HttpRequestMessage(HttpMethod.Put, $"/api/unites-enseignement/{ueA.Id}");
        message.Headers.Add("X-EtablissementId", etabB.ToString());
        message.Content = JsonContent.Create(updateRequest);

        // Act
        var response = await _http.SendAsync(message);

        // Assert (doit être NotFound à cause du filtre multi-tenant)
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}

