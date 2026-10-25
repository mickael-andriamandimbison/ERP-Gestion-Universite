using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using erp_univ_back.Features.Etablissement;
using erp_univ_back.Features.Etablissement.Create;
using erp_univ_back.Features.Etablissement.GetById;
using erp_univ_back.Shared.Data;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

namespace erp_univ_back.Tests.Features.Etablissement.GetById;

public class GetEtablissementByIdIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _http;
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    public GetEtablissementByIdIntegrationTests(WebApplicationFactory<Program> factory)
    {
        var dbName = "DbTest_Etablissement_GetById_" + Guid.NewGuid().ToString();

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

    private CreateEtablissementRequest GetCreateRequest() => new(
        Nom: "Université de Toamasina",
        Adresse: "Barikadimy",
        CodePostal: "501",
        Ville: "Toamasina",
        Pays: "Madagascar",
        Telephone: "+261340000040",
        Email: "toamasina@univ.mg",
        SiteWeb: "https://www.univ-toamasina.mg",
        Logo: "logo_toamasina.png",
        Sigle: "UT",
        Description: "Université de l'Est",
        Statut: StatusEtablissement.Actif
    );

    [Fact]
    public async Task GetById_DoitRetourner200OK_QuandEtablissementExiste()
    {
        // Arrange : Création préalable de l'entité
        var createRequest = GetCreateRequest();
        var createResponse = await _http.PostAsJsonAsync("/api/etablissements", createRequest);
        var created = await createResponse.Content.ReadFromJsonAsync<CreateEtablissementResponse>(_jsonOptions);
        created.Should().NotBeNull();

        // Act
        var response = await _http.GetAsync($"/api/etablissements/{created!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<GetEtablissementByIdResponse>(_jsonOptions);
        result.Should().NotBeNull();
        result!.Id.Should().Be(created.Id);
        result.Nom.Should().Be(createRequest.Nom);
        result.Email.Should().Be(createRequest.Email);
        result.Ville.Should().Be(createRequest.Ville);
        result.Sigle.Should().Be(createRequest.Sigle);
    }

    [Fact]
    public async Task GetById_DoitRetourner404NotFound_QuandEtablissementInexistant()
    {
        // Act : Requête avec un identifiant aléatoire inexistant
        var response = await _http.GetAsync($"/api/etablissements/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}

