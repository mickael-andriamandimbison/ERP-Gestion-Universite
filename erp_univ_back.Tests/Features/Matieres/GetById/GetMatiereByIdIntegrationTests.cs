namespace erp_univ_back.Tests.Features.Matieres.GetById;

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using erp_univ_back.Features.Etablissement;
using erp_univ_back.Features.Etablissement.Create;
using erp_univ_back.Features.Filiere.Create;
using erp_univ_back.Features.FiliereNiveau.Create;
using erp_univ_back.Features.Matiere.Create;
using erp_univ_back.Features.Matiere.GetById;
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

public class GetMatiereByIdIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _http;
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    public GetMatiereByIdIntegrationTests(WebApplicationFactory<Program> factory)
    {
        var dbName = "DbTest_Matiere_GetById_" + Guid.NewGuid().ToString();

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

    private async Task<Guid> CreerEtablissementAsync(string nom = "Univ Matiere GetById Test")
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

    private async Task<(CreateFiliereResponse Filiere, CreateNiveauResponse Niveau, CreateUniteEnseignementResponse Ue)> CreerUeAsync(
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

        var ueReq = new CreateUniteEnseignementRequest("UE Programmation Web", 6, 1, fn!.Id);
        using var ueMsg = new HttpRequestMessage(HttpMethod.Post, "/api/unites-enseignement");
        ueMsg.Headers.Add("X-EtablissementId", etablissementId.ToString());
        ueMsg.Content = JsonContent.Create(ueReq);
        var ueRes = await _http.SendAsync(ueMsg);
        var ue = await ueRes.Content.ReadFromJsonAsync<CreateUniteEnseignementResponse>(_jsonOptions);

        return (filiere, niveau!, ue!);
    }

    private async Task<CreateMatiereResponse> CreerMatiereAsync(
        Guid etablissementId, Guid ueId, string nom = "JavaScript", string code = "JS", int coeff = 3)
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
    public async Task ObtenirParId_DoitRetourner200EtDonneesCompletes_QuandMatiereExiste()
    {
        // Arrange
        var etablissementId = await CreerEtablissementAsync();
        var (filiere, niveau, ue) = await CreerUeAsync(etablissementId);
        var matiere = await CreerMatiereAsync(etablissementId, ue.Id, "Framework Angular", "NG", 4);

        using var getMsg = new HttpRequestMessage(HttpMethod.Get, $"/api/matieres/{matiere.Id}");
        getMsg.Headers.Add("X-EtablissementId", etablissementId.ToString());

        // Act
        var response = await _http.SendAsync(getMsg);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<GetMatiereByIdResponse>(_jsonOptions);
        result.Should().NotBeNull();
        result!.Id.Should().Be(matiere.Id);
        result.Nom.Should().Be("Framework Angular");
        result.Code.Should().Be("NG-INF-L1");
        result.Coefficient.Should().Be(4);
        result.UniteEnseignementId.Should().Be(ue.Id);
        result.UniteEnseignementNom.Should().Be("UE Programmation Web");
        result.FiliereId.Should().Be(filiere.Id);
        result.FiliereNom.Should().Be("Informatique INF");
        result.NiveauId.Should().Be(niveau.Id);
        result.NiveauNom.Should().Be("Licence L1");
    }

    [Fact]
    public async Task ObtenirParId_DoitRetourner404_QuandMatiereInexistante()
    {
        // Arrange
        var etablissementId = await CreerEtablissementAsync();
        using var getMsg = new HttpRequestMessage(HttpMethod.Get, $"/api/matieres/{Guid.NewGuid()}");
        getMsg.Headers.Add("X-EtablissementId", etablissementId.ToString());

        // Act
        var response = await _http.SendAsync(getMsg);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ObtenirParId_DoitIsolerDonnees_EntreEtablissements()
    {
        // Arrange
        var etabA = await CreerEtablissementAsync("Univ A");
        var etabB = await CreerEtablissementAsync("Univ B");

        var (_, _, ueA) = await CreerUeAsync(etabA);
        var matiereA = await CreerMatiereAsync(etabA, ueA.Id, "Cryptographie", "CRYPTO", 3);

        // Établissement B tente d'accéder à la matière de l'Établissement A
        using var getMsg = new HttpRequestMessage(HttpMethod.Get, $"/api/matieres/{matiereA.Id}");
        getMsg.Headers.Add("X-EtablissementId", etabB.ToString());

        // Act
        var response = await _http.SendAsync(getMsg);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}

