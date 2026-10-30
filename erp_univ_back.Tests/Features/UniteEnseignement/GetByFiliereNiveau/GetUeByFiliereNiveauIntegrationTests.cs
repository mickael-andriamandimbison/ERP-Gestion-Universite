namespace erp_univ_back.Tests.Features.UniteEnseignement.GetByFiliereNiveau;

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using erp_univ_back.Features.AnneeAcademique.Create;
using erp_univ_back.Features.Etablissement;
using erp_univ_back.Features.Etablissement.Create;
using erp_univ_back.Features.Filiere.Create;
using erp_univ_back.Features.FiliereNiveau.Create;
using erp_univ_back.Features.Niveau.Create;
using erp_univ_back.Features.UniteEnseignement.Create;
using erp_univ_back.Features.UniteEnseignement.GetByFiliereNiveau;
using erp_univ_back.Shared.Data;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

public class GetUeByFiliereNiveauIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _http;
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    public GetUeByFiliereNiveauIntegrationTests(WebApplicationFactory<Program> factory)
    {
        var dbName = "DbTest_UE_GetByFN_" + Guid.NewGuid().ToString();

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

    private async Task<Guid> CreerEtablissementAsync(string nom = "Univ UE GetByFN Test")
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

    private async Task CreerAnneeAcademiqueAsync(Guid etablissementId, int anneeDebut = 2025, bool estActive = true)
    {
        var anneeFin = anneeDebut + 1;
        var request = new CreateAnneeAcademiqueRequest(
            Libelle: $"{anneeDebut}-{anneeFin}",
            DateDebut: new DateOnly(anneeDebut, 9, 1),
            DateFin: new DateOnly(anneeFin, 6, 30),
            EstActive: estActive
        );

        using var msg = new HttpRequestMessage(HttpMethod.Post, "/api/annees-academiques");
        msg.Headers.Add("X-EtablissementId", etablissementId.ToString());
        msg.Content = JsonContent.Create(request);

        var response = await _http.SendAsync(msg);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
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

    private async Task<CreateUniteEnseignementResponse> CreerUeAsync(Guid etablissementId, Guid fnId, string nom = "UE Test", int credit = 5, int semestre = 1)
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
    public async Task Get_DoitInclureAnneeAcademiqueEtRegrouperParSemestre_QuandFiltresNonFournis()
    {
        // Arrange
        var etablissementId = await CreerEtablissementAsync();
        await CreerAnneeAcademiqueAsync(etablissementId, 2025, estActive: true);
        var (filiere, niveau, fn) = await CreerFiliereNiveauAsync(etablissementId, "INF", "L1");
        await CreerUeAsync(etablissementId, fn.Id, "Algorithmique 1", 6, 1);
        await CreerUeAsync(etablissementId, fn.Id, "Bases de Données", 4, 1);
        await CreerUeAsync(etablissementId, fn.Id, "Systèmes d Exploitation", 5, 2);

        // Appel sans paramètre
        using var message = new HttpRequestMessage(HttpMethod.Get, "/api/unites-enseignement");
        message.Headers.Add("X-EtablissementId", etablissementId.ToString());

        // Act
        var response = await _http.SendAsync(message);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<GetUeByFiliereNiveauResponse>(_jsonOptions);
        result.Should().NotBeNull();
        result!.AnneAcademique.Should().Be("2025-2026");
        result.FiliereId.Should().Be(filiere.Id);
        result.FiliereNom.Should().Be(filiere.Nom);
        result.NiveauId.Should().Be(niveau.Id);
        result.NiveauNom.Should().Be(niveau.Nom);

        // Regroupé par semestre
        result.Semestres.Should().HaveCount(2);

        var s1 = result.Semestres.First(s => s.Semestre == 1);
        s1.TotalCredits.Should().Be(10);
        s1.UnitesEnseignement.Should().HaveCount(2);

        var s2 = result.Semestres.First(s => s.Semestre == 2);
        s2.TotalCredits.Should().Be(5);
        s2.UnitesEnseignement.Should().HaveCount(1);
        s2.UnitesEnseignement.First().Nom.Should().Be("Systèmes d Exploitation");
    }

    [Fact]
    public async Task Get_DoitFiltrerParFiliereEtNiveauDansTableFiliereNiveaux_QuandFiltresFournis()
    {
        // Arrange
        var etablissementId = await CreerEtablissementAsync();
        await CreerAnneeAcademiqueAsync(etablissementId, 2024, estActive: true);
        var (filiere1, niveau1, fn1) = await CreerFiliereNiveauAsync(etablissementId, "INF", "L1");
        var (filiere2, niveau2, fn2) = await CreerFiliereNiveauAsync(etablissementId, "GEST", "L2");

        await CreerUeAsync(etablissementId, fn1.Id, "Algo INF", 5, 1);
        await CreerUeAsync(etablissementId, fn2.Id, "Compta GEST", 4, 1);

        // Appel ciblant spécifiquement la filière 2 et niveau 2
        using var message = new HttpRequestMessage(HttpMethod.Get, $"/api/unites-enseignement?filiereId={filiere2.Id}&niveauId={niveau2.Id}");
        message.Headers.Add("X-EtablissementId", etablissementId.ToString());

        // Act
        var response = await _http.SendAsync(message);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<GetUeByFiliereNiveauResponse>(_jsonOptions);
        result.Should().NotBeNull();
        result!.AnneAcademique.Should().Be("2024-2025");
        result.FiliereId.Should().Be(filiere2.Id);
        result.FiliereNom.Should().Be(filiere2.Nom);
        result.NiveauId.Should().Be(niveau2.Id);
        result.NiveauNom.Should().Be(niveau2.Nom);

        result.Semestres.Should().HaveCount(1);
        result.Semestres.First().Semestre.Should().Be(1);
        result.Semestres.First().UnitesEnseignement.Should().HaveCount(1);
        result.Semestres.First().UnitesEnseignement.First().Nom.Should().Be("Compta GEST");
    }

    [Fact]
    public async Task Get_DoitRetourner404_QuandCombinaisonFiliereNiveauInexistante()
    {
        // Arrange
        var etablissementId = await CreerEtablissementAsync();
        using var message = new HttpRequestMessage(HttpMethod.Get, $"/api/unites-enseignement?filiereId={Guid.NewGuid()}&niveauId={Guid.NewGuid()}");
        message.Headers.Add("X-EtablissementId", etablissementId.ToString());

        // Act
        var response = await _http.SendAsync(message);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Get_DoitRetournerSemestresVide_QuandAucuneUePourFiliereNiveau()
    {
        // Arrange
        var etablissementId = await CreerEtablissementAsync();
        var (filiere, niveau, _) = await CreerFiliereNiveauAsync(etablissementId, "DROIT", "L1");

        using var message = new HttpRequestMessage(HttpMethod.Get, $"/api/unites-enseignement?filiereId={filiere.Id}&niveauId={niveau.Id}");
        message.Headers.Add("X-EtablissementId", etablissementId.ToString());

        // Act
        var response = await _http.SendAsync(message);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<GetUeByFiliereNiveauResponse>(_jsonOptions);
        result.Should().NotBeNull();
        result!.Semestres.Should().BeEmpty();
    }

    [Fact]
    public async Task Get_DoitIsolerDonnees_EntreEtablissements()
    {
        // Arrange
        var etabA = await CreerEtablissementAsync("Univ A");
        var etabB = await CreerEtablissementAsync("Univ B");

        var (_, _, fnA) = await CreerFiliereNiveauAsync(etabA, "INF", "L1");
        await CreerUeAsync(etabA, fnA.Id, "Algo A", 5, 1);

        // Établissement B interroge sans avoir créé de filière ni UE
        using var message = new HttpRequestMessage(HttpMethod.Get, "/api/unites-enseignement");
        message.Headers.Add("X-EtablissementId", etabB.ToString());

        // Act
        var response = await _http.SendAsync(message);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<GetUeByFiliereNiveauResponse>(_jsonOptions);
        result!.Semestres.Should().BeEmpty();
    }
}
