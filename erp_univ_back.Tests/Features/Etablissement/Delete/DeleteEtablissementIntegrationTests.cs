using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using erp_univ_back.Features.Etablissement;
using erp_univ_back.Features.Etablissement.Create;
using erp_univ_back.Shared.Data;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

namespace erp_univ_back.Tests.Features.Etablissement.Delete;

public class DeleteEtablissementIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _http;
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    public DeleteEtablissementIntegrationTests(WebApplicationFactory<Program> factory)
    {
        var dbName = "DbTest_Etablissement_Delete_" + Guid.NewGuid().ToString();

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
        Nom: "Université à Supprimer",
        Adresse: "Campus Ankatso",
        CodePostal: "101",
        Ville: "Antananarivo",
        Pays: "Madagascar",
        Telephone: "+261340000030",
        Email: "delete@univ.mg",
        SiteWeb: "https://www.univ.mg",
        Logo: "logo.png",
        Sigle: "UAS",
        Description: "Établissement temporaire pour test de suppression",
        Statut: StatusEtablissement.Actif
    );

    [Fact]
    public async Task Delete_DoitRetourner200OK_QuandEtablissementExiste()
    {
        // Arrange : Création préalable de l'entité
        var createResponse = await _http.PostAsJsonAsync("/api/etablissements", GetCreateRequest());
        var created = await createResponse.Content.ReadFromJsonAsync<CreateEtablissementResponse>(_jsonOptions);
        created.Should().NotBeNull();

        // Act : Suppression
        var deleteResponse = await _http.DeleteAsync($"/api/etablissements/{created!.Id}");

        // Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Vérification de la suppression effective en base via GetById
        var getResponse = await _http.GetAsync($"/api/etablissements/{created.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_DoitRetourner404NotFound_QuandEtablissementInexistant()
    {
        // Act : Tentative de suppression d'un GUID aléatoire
        var response = await _http.DeleteAsync($"/api/etablissements/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}

