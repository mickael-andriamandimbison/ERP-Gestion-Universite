using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using erp_univ_back.Features.Etablissement;
using erp_univ_back.Features.Etablissement.Create;
using erp_univ_back.Features.Filiere.Create;
using erp_univ_back.Features.FiliereNiveau.Create;
using erp_univ_back.Features.FiliereNiveau.GetAll;
using erp_univ_back.Features.Niveau.Create;
using erp_univ_back.Shared.Data;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

namespace erp_univ_back.Tests.Features.FiliereNiveau.GetAll;

public class GetAllFiliereNiveauIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _http;
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    public GetAllFiliereNiveauIntegrationTests(WebApplicationFactory<Program> factory)
    {
        var dbName = "DbTest_FiliereNiveau_GetAll_" + Guid.NewGuid().ToString();

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

    private async Task<Guid> CreerEtablissementAsync(string nom = "Univ FiliereNiveau GetAll")
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
    public async Task GetAll_DoitRetourner200OK_AvecListeFiliereNiveaux()
    {
        // Arrange
        var etablissementId = await CreerEtablissementAsync();
        var filiere = await CreerFiliereAsync(etablissementId, "DROIT", "Droit");
        var n1 = await CreerNiveauAsync(etablissementId, "L1_D", "Licence 1", 1);
        var n2 = await CreerNiveauAsync(etablissementId, "L2_D", "Licence 2", 2);

        var fn1 = await CreerFiliereNiveauAsync(etablissementId, filiere.Id, n1.Id);
        var fn2 = await CreerFiliereNiveauAsync(etablissementId, filiere.Id, n2.Id);

        using var message = new HttpRequestMessage(HttpMethod.Get, "/api/filiere-niveaux");
        message.Headers.Add("X-EtablissementId", etablissementId.ToString());

        // Act
        var response = await _http.SendAsync(message);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<List<FiliereNiveauDto>>(_jsonOptions);
        result.Should().NotBeNull();
        result.Should().Contain(x => x.Id == fn1.Id);
        result.Should().Contain(x => x.Id == fn2.Id);
    }

    [Fact]
    public async Task GetAll_DoitIsolerDonneesEntreEtablissements()
    {
        // Arrange : Ségrégation multi-tenant
        var etablissement1 = await CreerEtablissementAsync("Univ Antananarivo");
        var etablissement2 = await CreerEtablissementAsync("Univ Fianarantsoa");

        var f1 = await CreerFiliereAsync(etablissement1, "FIL1", "Filiere Etab 1");
        var n1 = await CreerNiveauAsync(etablissement1, "NIV1", "Niveau Etab 1", 1);
        var fn1 = await CreerFiliereNiveauAsync(etablissement1, f1.Id, n1.Id);

        var f2 = await CreerFiliereAsync(etablissement2, "FIL2", "Filiere Etab 2");
        var n2 = await CreerNiveauAsync(etablissement2, "NIV2", "Niveau Etab 2", 1);
        var fn2 = await CreerFiliereNiveauAsync(etablissement2, f2.Id, n2.Id);

        using var message = new HttpRequestMessage(HttpMethod.Get, "/api/filiere-niveaux");
        message.Headers.Add("X-EtablissementId", etablissement1.ToString());

        // Act
        var response = await _http.SendAsync(message);

        // Assert : Seules les filière-niveaux du tenant 1 sont retournées
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<List<FiliereNiveauDto>>(_jsonOptions);
        result.Should().NotBeNull();
        result.Should().Contain(x => x.Id == fn1.Id);
        result.Should().NotContain(x => x.Id == fn2.Id);
    }
}

