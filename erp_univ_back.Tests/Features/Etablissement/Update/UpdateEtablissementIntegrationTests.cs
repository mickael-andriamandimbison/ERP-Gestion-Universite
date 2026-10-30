using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using erp_univ_back.Features.Etablissement;
using erp_univ_back.Features.Etablissement.Create;
using erp_univ_back.Features.Etablissement.Update;
using erp_univ_back.Shared.Data;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

namespace erp_univ_back.Tests.Features.Etablissement.Update;

public class UpdateEtablissementIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _http;
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    public UpdateEtablissementIntegrationTests(WebApplicationFactory<Program> factory)
    {
        var dbName = "DbTest_Etablissement_Update_" + Guid.NewGuid().ToString();

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

    private CreateEtablissementRequest GetCreateRequest(string nom, string email, string telephone) => new(
        Nom: nom,
        Adresse: "Campus Universitaire",
        CodePostal: "101",
        Ville: "Antananarivo",
        Pays: "Madagascar",
        Telephone: telephone,
        Email: email,
        SiteWeb: "https://www.univ.mg",
        Logo: "logo.png",
        Sigle: "UNIV",
        Description: "Description initiale",
        Statut: StatusEtablissement.Actif
    );

    private UpdateEtablissementRequest GetUpdateRequest() => new(
        Nom: "Université Modifiée",
        Adresse: "Nouvelle Adresse",
        CodePostal: "102",
        Ville: "Fianarantsoa",
        Pays: "Madagascar",
        Telephone: "+261320000000",
        Email: "contact@univ-modifiee.mg",
        SiteWeb: "https://www.univ-modifiee.mg",
        Logo: "logo_modifie.png",
        Sigle: "UM",
        Description: "Description mise à jour",
        Statut: StatusEtablissement.Suspendu
    );

    [Fact]
    public async Task Put_DoitRetourner200OK_QuandRequeteEstValide()
    {
        // Arrange : Création préalable
        var createResponse = await _http.PostAsJsonAsync("/api/etablissements", GetCreateRequest("Univ Initiale", "initial@univ.mg", "+261340000010"));
        var created = await createResponse.Content.ReadFromJsonAsync<CreateEtablissementResponse>(_jsonOptions);
        created.Should().NotBeNull();

        var updateRequest = GetUpdateRequest();

        // Act
        var response = await _http.PutAsJsonAsync($"/api/etablissements/{created!.Id}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<UpdateEtablissementResponse>(_jsonOptions);
        result.Should().NotBeNull();
        result!.Id.Should().Be(created.Id);
        result.Nom.Should().Be(updateRequest.Nom);
        result.Ville.Should().Be(updateRequest.Ville);
        result.Statut.Should().Be(StatusEtablissement.Suspendu);

        // Surcharge SaveChangesAsync : DateCreation reste intacte, DateModification est actualisée
        result.DateCreation.Should().Be(created.DateCreation);
        result.DateModification.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task Put_DoitRetourner400BadRequest_QuandChampObligatoireEstVide()
    {
        // Arrange
        var invalidRequest = GetUpdateRequest() with { Nom = string.Empty };

        // Act
        var response = await _http.PutAsJsonAsync($"/api/etablissements/{Guid.NewGuid()}", invalidRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Put_DoitRetourner404NotFound_QuandEtablissementInexistant()
    {
        // Arrange
        var validRequest = GetUpdateRequest();

        // Act
        var response = await _http.PutAsJsonAsync($"/api/etablissements/{Guid.NewGuid()}", validRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Put_DoitRetourner409Conflict_QuandNomAppartientADejaUnAutreEtablissement()
    {
        // Arrange : Création de deux établissements distincts
        var resp1 = await _http.PostAsJsonAsync("/api/etablissements", GetCreateRequest("Univ Alpha", "alpha@univ.mg", "+261340000021"));
        var resp2 = await _http.PostAsJsonAsync("/api/etablissements", GetCreateRequest("Univ Beta", "beta@univ.mg", "+261340000022"));

        var etablissement2 = await resp2.Content.ReadFromJsonAsync<CreateEtablissementResponse>(_jsonOptions);

        // Act : Tenter de modifier Univ Beta pour prendre le nom de Univ Alpha
        var conflictRequest = GetUpdateRequest() with { Nom = "Univ Alpha", Email = "unique_beta@univ.mg", Telephone = "+261340000099" };
        var updateResponse = await _http.PutAsJsonAsync($"/api/etablissements/{etablissement2!.Id}", conflictRequest);

        // Assert
        updateResponse.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }
}

