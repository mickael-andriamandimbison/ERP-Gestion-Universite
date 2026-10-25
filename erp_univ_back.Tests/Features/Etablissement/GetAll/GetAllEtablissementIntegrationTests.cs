using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using erp_univ_back.Features.Etablissement;
using erp_univ_back.Features.Etablissement.Create;
using erp_univ_back.Features.Etablissement.GetAll;
using erp_univ_back.Shared.Data;
using erp_univ_back.Shared.Responses;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

namespace erp_univ_back.Tests.Features.Etablissement.GetAll;

public class GetAllEtablissementIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _http;
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    public GetAllEtablissementIntegrationTests(WebApplicationFactory<Program> factory)
    {
        var dbName = "DbTest_Etablissement_GetAll_" + Guid.NewGuid().ToString();

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

    private CreateEtablissementRequest GetCreateRequest(string nom, string ville, string email, string telephone) => new(
        Nom: nom,
        Adresse: "Campus Universitaire",
        CodePostal: "101",
        Ville: ville,
        Pays: "Madagascar",
        Telephone: telephone,
        Email: email,
        SiteWeb: "https://www.univ.mg",
        Logo: "logo.png",
        Sigle: "U",
        Description: "Description",
        Statut: StatusEtablissement.Actif
    );

    [Fact]
    public async Task Get_DoitRetourner200OK_AvecPaginationEtElementsValides()
    {
        // Arrange : Création de 3 établissements
        await _http.PostAsJsonAsync("/api/etablissements", GetCreateRequest("Univ Pagination 1", "Antananarivo", "p1@univ.mg", "+261340000051"));
        await _http.PostAsJsonAsync("/api/etablissements", GetCreateRequest("Univ Pagination 2", "Fianarantsoa", "p2@univ.mg", "+261340000052"));
        await _http.PostAsJsonAsync("/api/etablissements", GetCreateRequest("Univ Pagination 3", "Mahajanga", "p3@univ.mg", "+261340000053"));

        // Act : Demande de la première page avec taille de 2
        var response = await _http.GetAsync("/api/etablissements?page=1&taillePage=2&champTri=nom");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<PagedResponse<GetAllEtablissementResponse>>(_jsonOptions);
        result.Should().NotBeNull();
        result!.Page.Should().Be(1);
        result.TaillePage.Should().Be(2);
        result.Elements.Should().HaveCount(2);
        result.TotalElements.Should().BeGreaterThanOrEqualTo(3);
        result.TotalPages.Should().BeGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task Get_DoitRetourner200OK_AvecFiltreRecherche()
    {
        // Arrange
        var uniqueNom = "Institut Superieur Specialisé Antsiranana";
        await _http.PostAsJsonAsync("/api/etablissements", GetCreateRequest(uniqueNom, "Antsiranana", "iss@univ.mg", "+261340000060"));

        // Act : Recherche ciblée sur le nom
        var response = await _http.GetAsync("/api/etablissements?recherche=Antsiranana");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<PagedResponse<GetAllEtablissementResponse>>(_jsonOptions);
        result.Should().NotBeNull();
        result!.Elements.Should().Contain(e => e.Nom == uniqueNom);
    }

    [Theory]
    [InlineData("page=0")]
    [InlineData("taillePage=500")]
    [InlineData("champTri=champ_interdit")]
    public async Task Get_DoitRetourner400BadRequest_QuandParametresRequeteInvalides(string queryParamInvalide)
    {
        // Act : Envoi d'une requête avec paramètre abusif ou non autorisé
        var response = await _http.GetAsync($"/api/etablissements?{queryParamInvalide}");

        // Assert : Doit être rejeté en 400 Bad Request par FluentValidation pour protéger le serveur
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}

