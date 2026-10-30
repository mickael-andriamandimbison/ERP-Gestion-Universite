namespace erp_univ_back.Tests.Features.Matieres.Create;

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

public class CreateMatiereIntegrationTest : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _http;
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    public CreateMatiereIntegrationTest(WebApplicationFactory<Program> factory)
    {
        var dbName = "DbTest_Matiere_Create_" + Guid.NewGuid().ToString();

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

    private async Task<Guid> CreerEtablissementAsync(string nom = "Univ Matiere Test")
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

    private async Task<(CreateFiliereResponse Filiere, CreateNiveauResponse Niveau, CreateFiliereNiveauResponse FiliereNiveau)> CreerFiliereNiveauAsync(
        Guid etablissementId, string filiereCode = "INF", string niveauCode = "L1")
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

        return (filiere, niveau!, fn!);
    }

    private async Task<CreateUniteEnseignementResponse> CreerUeAsync(
        Guid etablissementId, Guid fnId, string nom = "UE Algorithmique", int credit = 6, int semestre = 1)
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
    public async Task CreerMatiere_DoitRetourner201EtDonnees_QuandRequeteEstValide()
    {
        // Arrange
        var etablissementId = await CreerEtablissementAsync();
        var (_, _, fn) = await CreerFiliereNiveauAsync(etablissementId, "INF", "L1");
        var ue = await CreerUeAsync(etablissementId, fn.Id, "UE Programmation", 6, 1);

        var request = new CreateMatiereRequest("Algorithmique C", "ALGO", 3, ue.Id);
        using var msg = new HttpRequestMessage(HttpMethod.Post, "/api/matieres");
        msg.Headers.Add("X-EtablissementId", etablissementId.ToString());
        msg.Content = JsonContent.Create(request);

        // Act
        var response = await _http.SendAsync(msg);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await response.Content.ReadFromJsonAsync<CreateMatiereResponse>(_jsonOptions);
        created.Should().NotBeNull();
        created!.Id.Should().NotBeEmpty();
        created.Nom.Should().Be("Algorithmique C");
        created.Code.Should().Be("ALGO-INF-L1");
        created.Coefficient.Should().Be(3);
        created.UniteEnseignementId.Should().Be(ue.Id);
    }

    [Fact]
    public async Task CreerMatiere_DoitRetourner409Conflict_QuandMemeNomExisteDansMemeUe()
    {
        // Arrange
        var etablissementId = await CreerEtablissementAsync();
        var (_, _, fn) = await CreerFiliereNiveauAsync(etablissementId, "INF", "L1");
        var ue = await CreerUeAsync(etablissementId, fn.Id, "UE Bases de Données", 5, 1);

        // 1ère matière
        var request1 = new CreateMatiereRequest("SQL Avancé", "SQL1", 2, ue.Id);
        using var msg1 = new HttpRequestMessage(HttpMethod.Post, "/api/matieres");
        msg1.Headers.Add("X-EtablissementId", etablissementId.ToString());
        msg1.Content = JsonContent.Create(request1);
        var response1 = await _http.SendAsync(msg1);
        response1.StatusCode.Should().Be(HttpStatusCode.Created);

        // 2ème matière avec le même nom (insensible à la casse) dans la même UE
        var request2 = new CreateMatiereRequest("  sql avancé  ", "SQL2", 3, ue.Id);
        using var msg2 = new HttpRequestMessage(HttpMethod.Post, "/api/matieres");
        msg2.Headers.Add("X-EtablissementId", etablissementId.ToString());
        msg2.Content = JsonContent.Create(request2);

        // Act
        var response2 = await _http.SendAsync(msg2);

        // Assert
        response2.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task CreerMatiere_DoitReussir_QuandMemeNomExisteDansUneAutreUe()
    {
        // Arrange
        var etablissementId = await CreerEtablissementAsync();
        var (_, _, fn) = await CreerFiliereNiveauAsync(etablissementId, "INF", "L1");
        var ue1 = await CreerUeAsync(etablissementId, fn.Id, "UE Informatique 1", 5, 1);
        var ue2 = await CreerUeAsync(etablissementId, fn.Id, "UE Informatique 2", 5, 2);

        // Matière dans UE1
        var request1 = new CreateMatiereRequest("Projet Web", "PRJ1", 2, ue1.Id);
        using var msg1 = new HttpRequestMessage(HttpMethod.Post, "/api/matieres");
        msg1.Headers.Add("X-EtablissementId", etablissementId.ToString());
        msg1.Content = JsonContent.Create(request1);
        var res1 = await _http.SendAsync(msg1);
        res1.StatusCode.Should().Be(HttpStatusCode.Created);

        // Même nom dans UE2 (avec code différent) -> Doit réussir car c'est une autre UE
        var request2 = new CreateMatiereRequest("Projet Web", "PRJ2", 2, ue2.Id);
        using var msg2 = new HttpRequestMessage(HttpMethod.Post, "/api/matieres");
        msg2.Headers.Add("X-EtablissementId", etablissementId.ToString());
        msg2.Content = JsonContent.Create(request2);

        // Act
        var res2 = await _http.SendAsync(msg2);

        // Assert
        res2.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task CreerMatiere_DoitRetourner409Conflict_QuandCodeExisteDejaDansEtablissement()
    {
        // Arrange
        var etablissementId = await CreerEtablissementAsync();
        var (_, _, fn) = await CreerFiliereNiveauAsync(etablissementId, "INF", "L1");
        var ue1 = await CreerUeAsync(etablissementId, fn.Id, "UE Programmation 1", 5, 1);
        var ue2 = await CreerUeAsync(etablissementId, fn.Id, "UE Programmation 2", 5, 2);

        // 1ère matière avec code "PROG" -> PROG-INF-L1
        var request1 = new CreateMatiereRequest("Langage Java", "PROG", 2, ue1.Id);
        using var msg1 = new HttpRequestMessage(HttpMethod.Post, "/api/matieres");
        msg1.Headers.Add("X-EtablissementId", etablissementId.ToString());
        msg1.Content = JsonContent.Create(request1);
        var res1 = await _http.SendAsync(msg1);
        res1.StatusCode.Should().Be(HttpStatusCode.Created);

        // 2ème matière avec même code "PROG" -> tente de générer le même code PROG-INF-L1
        var request2 = new CreateMatiereRequest("Langage Python", "PROG", 2, ue2.Id);
        using var msg2 = new HttpRequestMessage(HttpMethod.Post, "/api/matieres");
        msg2.Headers.Add("X-EtablissementId", etablissementId.ToString());
        msg2.Content = JsonContent.Create(request2);

        // Act
        var res2 = await _http.SendAsync(msg2);

        // Assert
        res2.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task CreerMatiere_DoitRetourner404NotFound_QuandUniteEnseignementInexistante()
    {
        // Arrange
        var etablissementId = await CreerEtablissementAsync();
        var request = new CreateMatiereRequest("Matiere Orpheline", "ORPH", 1, Guid.NewGuid());
        using var msg = new HttpRequestMessage(HttpMethod.Post, "/api/matieres");
        msg.Headers.Add("X-EtablissementId", etablissementId.ToString());
        msg.Content = JsonContent.Create(request);

        // Act
        var response = await _http.SendAsync(msg);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreerMatiere_DoitRetourner400BadRequest_QuandValidationEchoue()
    {
        // Arrange
        var etablissementId = await CreerEtablissementAsync();
        var request = new CreateMatiereRequest("", "CODE", 0, Guid.Empty);
        using var msg = new HttpRequestMessage(HttpMethod.Post, "/api/matieres");
        msg.Headers.Add("X-EtablissementId", etablissementId.ToString());
        msg.Content = JsonContent.Create(request);

        // Act
        var response = await _http.SendAsync(msg);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreerMatiere_DoitIsolerDonnees_EntreEtablissements()
    {
        // Arrange
        var etabA = await CreerEtablissementAsync("Univ A");
        var etabB = await CreerEtablissementAsync("Univ B");

        var (_, _, fnA) = await CreerFiliereNiveauAsync(etabA, "INF", "L1");
        var ueA = await CreerUeAsync(etabA, fnA.Id, "UE Algorithmes", 6, 1);

        // Etablissement B tente de créer une matière rattachée à l'UE de l'Etablissement A
        var request = new CreateMatiereRequest("Piratage UE", "PIR", 2, ueA.Id);
        using var msg = new HttpRequestMessage(HttpMethod.Post, "/api/matieres");
        msg.Headers.Add("X-EtablissementId", etabB.ToString());
        msg.Content = JsonContent.Create(request);

        // Act
        var response = await _http.SendAsync(msg);

        // Assert : Etablissement B ne voit pas l'UE de l'Etablissement A -> 404
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}

