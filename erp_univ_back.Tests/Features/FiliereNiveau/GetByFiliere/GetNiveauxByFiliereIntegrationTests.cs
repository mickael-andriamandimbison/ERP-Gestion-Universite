using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using erp_univ_back.Features.Etablissement;
using erp_univ_back.Features.Etablissement.Create;
using erp_univ_back.Features.Filiere.Create;
using erp_univ_back.Features.FiliereNiveau.Create;
using erp_univ_back.Features.FiliereNiveau.GetByFiliere;
using erp_univ_back.Features.Niveau.Create;
using erp_univ_back.Shared.Data;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

namespace erp_univ_back.Tests.Features.FiliereNiveau.GetByFiliere;

public class GetNiveauxByFiliereIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _http;
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    public GetNiveauxByFiliereIntegrationTests(WebApplicationFactory<Program> factory)
    {
        var dbName = "DbTest_FiliereNiveau_GetByFiliere_" + Guid.NewGuid().ToString();

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

    private async Task<Guid> CreerEtablissementAsync(string nom = "Univ FiliereNiveau GetByFiliere")
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

    private async Task<CreateFiliereResponse> CreerFiliereAsync(Guid etablissementId, string code, string nom)
    {
        var request = new CreateFiliereRequest(code, nom, "Description");

        using var message = new HttpRequestMessage(HttpMethod.Post, "/api/filieres");
        message.Headers.Add("X-EtablissementId", etablissementId.ToString());
        message.Content = JsonContent.Create(request);

        var response = await _http.SendAsync(message);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await response.Content.ReadFromJsonAsync<CreateFiliereResponse>(_jsonOptions);
        return created!;
    }

    private async Task<CreateNiveauResponse> CreerNiveauAsync(Guid etablissementId, string code, string nom, int ordre)
    {
        var request = new CreateNiveauRequest(code, nom, ordre);

        using var message = new HttpRequestMessage(HttpMethod.Post, "/api/niveaux");
        message.Headers.Add("X-EtablissementId", etablissementId.ToString());
        message.Content = JsonContent.Create(request);

        var response = await _http.SendAsync(message);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await response.Content.ReadFromJsonAsync<CreateNiveauResponse>(_jsonOptions);
        return created!;
    }

    private async Task<CreateFiliereNiveauResponse> CreerFiliereNiveauAsync(Guid etablissementId, Guid filiereId, Guid niveauId)
    {
        var request = new CreateFiliereNiveauRequest(filiereId, niveauId);

        using var message = new HttpRequestMessage(HttpMethod.Post, "/api/filiere-niveaux");
        message.Headers.Add("X-EtablissementId", etablissementId.ToString());
        message.Content = JsonContent.Create(request);

        var response = await _http.SendAsync(message);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await response.Content.ReadFromJsonAsync<CreateFiliereNiveauResponse>(_jsonOptions);
        return created!;
    }

    [Fact]
    public async Task GetByFiliere_DoitRetourner200OK_AvecNiveauxTriesParOrdre()
    {
        // Arrange
        var etablissementId = await CreerEtablissementAsync();
        var filiere = await CreerFiliereAsync(etablissementId, "INF", "Informatique");
        var nMaster = await CreerNiveauAsync(etablissementId, "M2", "Master 2", 5);
        var nLicence = await CreerNiveauAsync(etablissementId, "L1", "Licence 1", 1);

        // Création dans le désordre pour vérifier le tri par Ordre
        await CreerFiliereNiveauAsync(etablissementId, filiere.Id, nMaster.Id);
        await CreerFiliereNiveauAsync(etablissementId, filiere.Id, nLicence.Id);

        using var message = new HttpRequestMessage(HttpMethod.Get, $"/api/filiere-niveaux/by-filiere/{filiere.Id}");
        message.Headers.Add("X-EtablissementId", etablissementId.ToString());

        // Act
        var response = await _http.SendAsync(message);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<List<GetNiveauxByFiliereResponse>>(_jsonOptions);
        result.Should().NotBeNull();
        result!.Should().HaveCount(2);
        result![0].NiveauCode.Should().Be("L1");
        result[0].Ordre.Should().Be(1);
        result[1].NiveauCode.Should().Be("M2");
        result[1].Ordre.Should().Be(5);
    }

    [Fact]
    public async Task GetByFiliere_DoitRetourner404NotFound_QuandFiliereInexistante()
    {
        // Arrange
        var etablissementId = await CreerEtablissementAsync();

        using var message = new HttpRequestMessage(HttpMethod.Get, $"/api/filiere-niveaux/by-filiere/{Guid.NewGuid()}");
        message.Headers.Add("X-EtablissementId", etablissementId.ToString());

        // Act
        var response = await _http.SendAsync(message);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetByFiliere_DoitRetourner404NotFound_QuandFiliereAppartientAUnAutreEtablissement()
    {
        // Arrange : Multi-tenant test
        var etablissement1 = await CreerEtablissementAsync("Univ 1");
        var etablissement2 = await CreerEtablissementAsync("Univ 2");

        var filiereEtab1 = await CreerFiliereAsync(etablissement1, "PHY", "Physique");
        var niveauEtab1 = await CreerNiveauAsync(etablissement1, "L1_P", "Licence 1 Physique", 1);
        await CreerFiliereNiveauAsync(etablissement1, filiereEtab1.Id, niveauEtab1.Id);

        // Act : Tentative d'accès depuis le tenant 2
        using var message = new HttpRequestMessage(HttpMethod.Get, $"/api/filiere-niveaux/by-filiere/{filiereEtab1.Id}");
        message.Headers.Add("X-EtablissementId", etablissement2.ToString());

        var response = await _http.SendAsync(message);

        // Assert : Invisible pour le tenant 2
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}

