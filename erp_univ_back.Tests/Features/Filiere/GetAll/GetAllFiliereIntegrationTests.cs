using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using erp_univ_back.Features.Etablissement;
using erp_univ_back.Features.Etablissement.Create;
using erp_univ_back.Features.Filiere.Create;
using erp_univ_back.Features.Filiere.GetAll;
using erp_univ_back.Shared.Data;
using erp_univ_back.Shared.Responses;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

namespace erp_univ_back.Tests.Features.Filiere.GetAll;

public class GetAllFiliereIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _http;
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    public GetAllFiliereIntegrationTests(WebApplicationFactory<Program> factory)
    {
        var dbName = "DbTest_Filiere_GetAll_" + Guid.NewGuid().ToString();

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

    private async Task<Guid> CreerEtablissementAsync(string nom = "Univ Filiere GetAll")
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

    private async Task<CreateFiliereResponse> CreerFiliereAsync(Guid etablissementId, string code, string nom, string? description = "Description filiere")
    {
        var request = new CreateFiliereRequest(code, nom, description);

        using var message = new HttpRequestMessage(HttpMethod.Post, "/api/filieres");
        message.Headers.Add("X-EtablissementId", etablissementId.ToString());
        message.Content = JsonContent.Create(request);

        var response = await _http.SendAsync(message);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await response.Content.ReadFromJsonAsync<CreateFiliereResponse>(_jsonOptions);
        return created!;
    }

    [Fact]
    public async Task GetAll_DoitRetourner200OK_AvecPaginationValide()
    {
        // Arrange
        var etablissementId = await CreerEtablissementAsync();
        await CreerFiliereAsync(etablissementId, "FIL_A", "Filiere Alpha");
        await CreerFiliereAsync(etablissementId, "FIL_B", "Filiere Beta");
        await CreerFiliereAsync(etablissementId, "FIL_C", "Filiere Gamma");

        using var message = new HttpRequestMessage(HttpMethod.Get, "/api/filieres?page=1&taillePage=2&champTri=code");
        message.Headers.Add("X-EtablissementId", etablissementId.ToString());

        // Act
        var response = await _http.SendAsync(message);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<PagedResponse<GetAllFiliereResponse>>(_jsonOptions);
        result.Should().NotBeNull();
        result!.Page.Should().Be(1);
        result.TaillePage.Should().Be(2);
        result.Elements.Should().HaveCount(2);
        result.TotalElements.Should().BeGreaterThanOrEqualTo(3);
        result.TotalPages.Should().BeGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task GetAll_DoitRetourner200OK_AvecFiltreRecherche()
    {
        // Arrange
        var etablissementId = await CreerEtablissementAsync();
        var uniqueNom = "Biotechnologie Speciale";
        await CreerFiliereAsync(etablissementId, "BIO_SPEC", uniqueNom);

        using var message = new HttpRequestMessage(HttpMethod.Get, $"/api/filieres?recherche={uniqueNom}");
        message.Headers.Add("X-EtablissementId", etablissementId.ToString());

        // Act
        var response = await _http.SendAsync(message);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<PagedResponse<GetAllFiliereResponse>>(_jsonOptions);
        result.Should().NotBeNull();
        result!.Elements.Should().Contain(f => f.Nom == uniqueNom);
    }

    [Theory]
    [InlineData("page=0")]
    [InlineData("taillePage=500")]
    [InlineData("champTri=inconnu")]
    public async Task GetAll_DoitRetourner400BadRequest_QuandParametresRequeteInvalides(string queryInvalide)
    {
        // Arrange
        var etablissementId = await CreerEtablissementAsync();

        using var message = new HttpRequestMessage(HttpMethod.Get, $"/api/filieres?{queryInvalide}");
        message.Headers.Add("X-EtablissementId", etablissementId.ToString());

        // Act
        var response = await _http.SendAsync(message);

        // Assert : Protection serveur
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetAll_DoitIsolerDonneesEntreEtablissements()
    {
        // Arrange : Ségrégation multi-tenant
        var etablissement1 = await CreerEtablissementAsync("Univ Antananarivo");
        var etablissement2 = await CreerEtablissementAsync("Univ Toamasina");

        await CreerFiliereAsync(etablissement1, "FIL_TANA", "Filiere Tana");
        await CreerFiliereAsync(etablissement2, "FIL_TOAM", "Filiere Toamasina");

        using var message1 = new HttpRequestMessage(HttpMethod.Get, "/api/filieres");
        message1.Headers.Add("X-EtablissementId", etablissement1.ToString());

        // Act
        var response1 = await _http.SendAsync(message1);

        // Assert : Seule la filière de l'établissement 1 est retournée
        response1.StatusCode.Should().Be(HttpStatusCode.OK);
        var result1 = await response1.Content.ReadFromJsonAsync<PagedResponse<GetAllFiliereResponse>>(_jsonOptions);
        result1!.Elements.Should().Contain(f => f.Nom == "Filiere Tana");
        result1.Elements.Should().NotContain(f => f.Nom == "Filiere Toamasina");
    }
}

