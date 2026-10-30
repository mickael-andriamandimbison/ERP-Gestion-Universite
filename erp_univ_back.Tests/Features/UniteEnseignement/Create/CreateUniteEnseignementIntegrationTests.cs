namespace erp_univ_back.Tests.Features.UniteEnseignement.Create;

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

public class CreateUniteEnseignementIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _http;
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    public CreateUniteEnseignementIntegrationTests(WebApplicationFactory<Program> factory)
    {
        var dbName = "DbTest_UE_Create_" + Guid.NewGuid().ToString();

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

    private async Task<Guid> CreerEtablissementAsync(string nom = "Univ UE Test")
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
        // 1. Filière
        var filiereReq = new CreateFiliereRequest(filiereCode, "Informatique " + filiereCode, null);
        using var filiereMsg = new HttpRequestMessage(HttpMethod.Post, "/api/filieres");
        filiereMsg.Headers.Add("X-EtablissementId", etablissementId.ToString());
        filiereMsg.Content = JsonContent.Create(filiereReq);
        var filiereRes = await _http.SendAsync(filiereMsg);
        var filiere = await filiereRes.Content.ReadFromJsonAsync<CreateFiliereResponse>(_jsonOptions);

        // 2. Niveau
        var niveauReq = new CreateNiveauRequest(niveauCode, "Licence " + niveauCode, 1);
        using var niveauMsg = new HttpRequestMessage(HttpMethod.Post, "/api/niveaux");
        niveauMsg.Headers.Add("X-EtablissementId", etablissementId.ToString());
        niveauMsg.Content = JsonContent.Create(niveauReq);
        var niveauRes = await _http.SendAsync(niveauMsg);
        var niveau = await niveauRes.Content.ReadFromJsonAsync<CreateNiveauResponse>(_jsonOptions);

        // 3. FilièreNiveau
        var fnReq = new CreateFiliereNiveauRequest(filiere!.Id, niveau!.Id);
        using var fnMsg = new HttpRequestMessage(HttpMethod.Post, "/api/filiere-niveaux");
        fnMsg.Headers.Add("X-EtablissementId", etablissementId.ToString());
        fnMsg.Content = JsonContent.Create(fnReq);
        var fnRes = await _http.SendAsync(fnMsg);
        var fn = await fnRes.Content.ReadFromJsonAsync<CreateFiliereNiveauResponse>(_jsonOptions);

        return fn!.Id;
    }

    [Fact]
    public async Task Post_DoitRetourner201Created_AvecCodeGenere_QuandRequeteEstValide()
    {
        // Arrange
        var etablissementId = await CreerEtablissementAsync();
        var fnId = await CreerFiliereNiveauAsync(etablissementId, "INF", "L1");

        var request = new CreateUniteEnseignementRequest(
            Nom: "Algorithmique et Programmation",
            Credit: 6,
            Semestre: 1,
            FiliereNiveauId: fnId
        );

        using var message = new HttpRequestMessage(HttpMethod.Post, "/api/unites-enseignement");
        message.Headers.Add("X-EtablissementId", etablissementId.ToString());
        message.Content = JsonContent.Create(request);

        // Act
        var response = await _http.SendAsync(message);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await response.Content.ReadFromJsonAsync<CreateUniteEnseignementResponse>(_jsonOptions);
        created.Should().NotBeNull();
        created!.Id.Should().NotBeEmpty();
        created.Code.Should().Be("UE-INF-L1-01");
        created.Nom.Should().Be("Algorithmique et Programmation");
        created.Credit.Should().Be(6);
        created.Semestre.Should().Be(1);
        created.FiliereNiveauId.Should().Be(fnId);
    }

    [Fact]
    public async Task Post_DoitRetourner201Created_AvecSuffixeIncremente_PourDeuxiemeUE()
    {
        // Arrange
        var etablissementId = await CreerEtablissementAsync();
        var fnId = await CreerFiliereNiveauAsync(etablissementId, "INF", "L1");

        // 1ère UE
        var request1 = new CreateUniteEnseignementRequest(
            Nom: "Algorithmique 1",
            Credit: 5,
            Semestre: 1,
            FiliereNiveauId: fnId
        );
        using var msg1 = new HttpRequestMessage(HttpMethod.Post, "/api/unites-enseignement");
        msg1.Headers.Add("X-EtablissementId", etablissementId.ToString());
        msg1.Content = JsonContent.Create(request1);
        var res1 = await _http.SendAsync(msg1);
        res1.StatusCode.Should().Be(HttpStatusCode.Created);
        var created1 = await res1.Content.ReadFromJsonAsync<CreateUniteEnseignementResponse>(_jsonOptions);
        created1!.Code.Should().Be("UE-INF-L1-01");

        // 2ème UE
        var request2 = new CreateUniteEnseignementRequest(
            Nom: "Mathématiques Générales",
            Credit: 4,
            Semestre: 1,
            FiliereNiveauId: fnId
        );
        using var msg2 = new HttpRequestMessage(HttpMethod.Post, "/api/unites-enseignement");
        msg2.Headers.Add("X-EtablissementId", etablissementId.ToString());
        msg2.Content = JsonContent.Create(request2);

        // Act
        var res2 = await _http.SendAsync(msg2);

        // Assert
        res2.StatusCode.Should().Be(HttpStatusCode.Created);
        var created2 = await res2.Content.ReadFromJsonAsync<CreateUniteEnseignementResponse>(_jsonOptions);
        created2!.Code.Should().Be("UE-INF-L1-02");
    }

    [Fact]
    public async Task Post_DoitRetourner400BadRequest_QuandChampsInvalides()
    {
        // Arrange
        var etablissementId = await CreerEtablissementAsync();
        var request = new CreateUniteEnseignementRequest(
            Nom: "",
            Credit: 0,
            Semestre: 3,
            FiliereNiveauId: Guid.Empty
        );

        using var message = new HttpRequestMessage(HttpMethod.Post, "/api/unites-enseignement");
        message.Headers.Add("X-EtablissementId", etablissementId.ToString());
        message.Content = JsonContent.Create(request);

        // Act
        var response = await _http.SendAsync(message);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Post_DoitRetourner404NotFound_QuandFiliereNiveauInexistant()
    {
        // Arrange
        var etablissementId = await CreerEtablissementAsync();
        var request = new CreateUniteEnseignementRequest(
            Nom: "Systèmes d'exploitation",
            Credit: 4,
            Semestre: 1,
            FiliereNiveauId: Guid.NewGuid()
        );

        using var message = new HttpRequestMessage(HttpMethod.Post, "/api/unites-enseignement");
        message.Headers.Add("X-EtablissementId", etablissementId.ToString());
        message.Content = JsonContent.Create(request);

        // Act
        var response = await _http.SendAsync(message);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Post_DoitRetourner409Conflict_QuandMemeNomEtSemestreDansMemeClasse()
    {
        // Arrange
        var etablissementId = await CreerEtablissementAsync();
        var fnId = await CreerFiliereNiveauAsync(etablissementId, "GEST", "L2");

        var request1 = new CreateUniteEnseignementRequest(
            Nom: "Comptabilité Générale",
            Credit: 4,
            Semestre: 2,
            FiliereNiveauId: fnId
        );
        using var msg1 = new HttpRequestMessage(HttpMethod.Post, "/api/unites-enseignement");
        msg1.Headers.Add("X-EtablissementId", etablissementId.ToString());
        msg1.Content = JsonContent.Create(request1);
        var res1 = await _http.SendAsync(msg1);
        res1.StatusCode.Should().Be(HttpStatusCode.Created);

        var requestDuplicate = new CreateUniteEnseignementRequest(
            Nom: "Comptabilité Générale",
            Credit: 3,
            Semestre: 2,
            FiliereNiveauId: fnId
        );
        using var msg2 = new HttpRequestMessage(HttpMethod.Post, "/api/unites-enseignement");
        msg2.Headers.Add("X-EtablissementId", etablissementId.ToString());
        msg2.Content = JsonContent.Create(requestDuplicate);

        // Act
        var res2 = await _http.SendAsync(msg2);

        // Assert
        res2.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Post_DoitIsolerDonnees_EntreEtablissements()
    {
        // Arrange
        var etabA = await CreerEtablissementAsync("Univ A");
        var etabB = await CreerEtablissementAsync("Univ B");

        var fnIdA = await CreerFiliereNiveauAsync(etabA, "DROIT", "L1");

        // Établissement B tente d'utiliser la filière-niveau de l'établissement A
        var request = new CreateUniteEnseignementRequest(
            Nom: "Introduction au Droit",
            Credit: 5,
            Semestre: 1,
            FiliereNiveauId: fnIdA
        );

        using var message = new HttpRequestMessage(HttpMethod.Post, "/api/unites-enseignement");
        message.Headers.Add("X-EtablissementId", etabB.ToString());
        message.Content = JsonContent.Create(request);

        // Act
        var response = await _http.SendAsync(message);

        // Assert (l'accès doit être refusé car fnIdA n'existe pas dans le tenant B)
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}

