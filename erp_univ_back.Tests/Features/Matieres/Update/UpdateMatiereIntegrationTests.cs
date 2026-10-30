namespace erp_univ_back.Tests.Features.Matieres.Update;

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using erp_univ_back.Features.Etablissement;
using erp_univ_back.Features.Etablissement.Create;
using erp_univ_back.Features.Filiere.Create;
using erp_univ_back.Features.FiliereNiveau.Create;
using erp_univ_back.Features.Matiere.Create;
using erp_univ_back.Features.Matiere.Update;
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

public class UpdateMatiereIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _http;
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    public UpdateMatiereIntegrationTests(WebApplicationFactory<Program> factory)
    {
        var dbName = "DbTest_Matiere_Update_" + Guid.NewGuid().ToString();

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

    private async Task<Guid> CreerEtablissementAsync(string nom = "Univ Matiere Update Test")
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

    private async Task<CreateUniteEnseignementResponse> CreerUeAsync(Guid etablissementId, string filiereCode = "INF", string niveauCode = "L1")
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

        var ueReq = new CreateUniteEnseignementRequest("UE Informatique", 6, 1, fn!.Id);
        using var ueMsg = new HttpRequestMessage(HttpMethod.Post, "/api/unites-enseignement");
        ueMsg.Headers.Add("X-EtablissementId", etablissementId.ToString());
        ueMsg.Content = JsonContent.Create(ueReq);
        var ueRes = await _http.SendAsync(ueMsg);
        var ue = await ueRes.Content.ReadFromJsonAsync<CreateUniteEnseignementResponse>(_jsonOptions);

        return ue!;
    }

    private async Task<CreateMatiereResponse> CreerMatiereAsync(Guid etablissementId, Guid ueId, string nom = "Algorithmique 1", string code = "ALGO", int coeff = 3)
    {
        var request = new CreateMatiereRequest(nom, code, coeff, ueId);
        using var msg = new HttpRequestMessage(HttpMethod.Post, "/api/matieres");
        msg.Headers.Add("X-EtablissementId", etablissementId.ToString());
        msg.Content = JsonContent.Create(request);

        var response = await _http.SendAsync(msg);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await response.Content.ReadFromJsonAsync<CreateMatiereResponse>(_jsonOptions);
        return created!;
    }

    [Fact]
    public async Task MettreAJour_DoitRetourner200EtDonneesModifiees_QuandRequeteEstValide()
    {
        // Arrange
        var etablissementId = await CreerEtablissementAsync();
        var ue = await CreerUeAsync(etablissementId);
        var matiere = await CreerMatiereAsync(etablissementId, ue.Id, "Programmation Initiale", "PROG", 2);

        var updateReq = new UpdateMatiereRequest("Programmation Avancée", 4);
        using var updateMsg = new HttpRequestMessage(HttpMethod.Put, $"/api/matieres/{matiere.Id}");
        updateMsg.Headers.Add("X-EtablissementId", etablissementId.ToString());
        updateMsg.Content = JsonContent.Create(updateReq);

        // Act
        var response = await _http.SendAsync(updateMsg);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await response.Content.ReadFromJsonAsync<UpdateMatiereResponse>(_jsonOptions);
        updated.Should().NotBeNull();
        updated!.Id.Should().Be(matiere.Id);
        updated.Nom.Should().Be("Programmation Avancée");
        updated.Coefficient.Should().Be(4);
    }

    [Fact]
    public async Task MettreAJour_DoitRetourner409Conflict_QuandAutreMatiereMemeNomExisteDansMemeUe()
    {
        // Arrange
        var etablissementId = await CreerEtablissementAsync();
        var ue = await CreerUeAsync(etablissementId);
        var matiere1 = await CreerMatiereAsync(etablissementId, ue.Id, "Bases de Données", "BDD", 2);
        var matiere2 = await CreerMatiereAsync(etablissementId, ue.Id, "Systèmes", "SYS", 3);

        // Tente de renommer matiere2 en "bases de données" (insensible à la casse)
        var updateReq = new UpdateMatiereRequest("  bases de données  ", 4);
        using var updateMsg = new HttpRequestMessage(HttpMethod.Put, $"/api/matieres/{matiere2.Id}");
        updateMsg.Headers.Add("X-EtablissementId", etablissementId.ToString());
        updateMsg.Content = JsonContent.Create(updateReq);

        // Act
        var response = await _http.SendAsync(updateMsg);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task MettreAJour_DoitReussir_QuandOnGardeLeMemeNomPourLaMemeMatiere()
    {
        // Arrange
        var etablissementId = await CreerEtablissementAsync();
        var ue = await CreerUeAsync(etablissementId);
        var matiere = await CreerMatiereAsync(etablissementId, ue.Id, "Réseaux", "RES", 2);

        // Garde le même nom mais modifie le coefficient
        var updateReq = new UpdateMatiereRequest("Réseaux", 5);
        using var updateMsg = new HttpRequestMessage(HttpMethod.Put, $"/api/matieres/{matiere.Id}");
        updateMsg.Headers.Add("X-EtablissementId", etablissementId.ToString());
        updateMsg.Content = JsonContent.Create(updateReq);

        // Act
        var response = await _http.SendAsync(updateMsg);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await response.Content.ReadFromJsonAsync<UpdateMatiereResponse>(_jsonOptions);
        updated!.Coefficient.Should().Be(5);
    }

    [Fact]
    public async Task MettreAJour_DoitRetourner409Conflict_QuandCodeConflitAvecAutreMatiere()
    {
        // Arrange
        var etablissementId = await CreerEtablissementAsync();
        var ue = await CreerUeAsync(etablissementId);
        var matiere1 = await CreerMatiereAsync(etablissementId, ue.Id, "Matiere 1", "MAT1", 2);
        var matiere2 = await CreerMatiereAsync(etablissementId, ue.Id, "Matiere 2", "MAT2", 3);

        // Tente de donner à matiere2 le même code que matiere1
        var updateReq = new UpdateMatiereRequest("Matiere 2 Renommee", 3, Code: matiere1.Code);
        using var updateMsg = new HttpRequestMessage(HttpMethod.Put, $"/api/matieres/{matiere2.Id}");
        updateMsg.Headers.Add("X-EtablissementId", etablissementId.ToString());
        updateMsg.Content = JsonContent.Create(updateReq);

        // Act
        var response = await _http.SendAsync(updateMsg);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task MettreAJour_DoitRetourner404_QuandMatiereInexistante()
    {
        // Arrange
        var etablissementId = await CreerEtablissementAsync();
        var updateReq = new UpdateMatiereRequest("Inexistante", 2);
        using var updateMsg = new HttpRequestMessage(HttpMethod.Put, $"/api/matieres/{Guid.NewGuid()}");
        updateMsg.Headers.Add("X-EtablissementId", etablissementId.ToString());
        updateMsg.Content = JsonContent.Create(updateReq);

        // Act
        var response = await _http.SendAsync(updateMsg);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task MettreAJour_DoitRetourner400_QuandValidationEchoue()
    {
        // Arrange
        var etablissementId = await CreerEtablissementAsync();
        var ue = await CreerUeAsync(etablissementId);
        var matiere = await CreerMatiereAsync(etablissementId, ue.Id, "Test Val", "TVAL", 2);

        // Requête invalide (nom vide, coefficient 0)
        var updateReq = new UpdateMatiereRequest("", 0);
        using var updateMsg = new HttpRequestMessage(HttpMethod.Put, $"/api/matieres/{matiere.Id}");
        updateMsg.Headers.Add("X-EtablissementId", etablissementId.ToString());
        updateMsg.Content = JsonContent.Create(updateReq);

        // Act
        var response = await _http.SendAsync(updateMsg);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task MettreAJour_DoitIsolerDonnees_EntreEtablissements()
    {
        // Arrange
        var etabA = await CreerEtablissementAsync("Univ A");
        var etabB = await CreerEtablissementAsync("Univ B");

        var ueA = await CreerUeAsync(etabA);
        var matiereA = await CreerMatiereAsync(etabA, ueA.Id, "Securite", "SEC", 2);

        // Établissement B tente de modifier la matière de l'Établissement A
        var updateReq = new UpdateMatiereRequest("Securite Hacked", 10);
        using var updateMsg = new HttpRequestMessage(HttpMethod.Put, $"/api/matieres/{matiereA.Id}");
        updateMsg.Headers.Add("X-EtablissementId", etabB.ToString());
        updateMsg.Content = JsonContent.Create(updateReq);

        // Act
        var response = await _http.SendAsync(updateMsg);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}

