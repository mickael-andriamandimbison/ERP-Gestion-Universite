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

namespace erp_univ_back.Tests.Features.Etablissement.Create;

public class CreateEtablissementIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _http;
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    public CreateEtablissementIntegrationTests(WebApplicationFactory<Program> factory)
    {
        var dbName = "DbTest_Etablissement_Create_" + Guid.NewGuid().ToString();

        var application = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.RemoveAll(typeof(DbContextOptions<ErpUnivDbContext>));
                services.RemoveAll(typeof(DbContextOptions));
                services.RemoveAll(typeof(IDbContextOptionsConfiguration<ErpUnivDbContext>));
                services.RemoveAll(typeof(ErpUnivDbContext));

                // 2. Base In-Memory isolée avec nom stable
                services.AddDbContext<ErpUnivDbContext>(options =>
                {
                    options.UseInMemoryDatabase(dbName);
                });
            });
        });

        _http = application.CreateClient();
    }

    private CreateEtablissementRequest GetValidRequest() => new(
        Nom: "Université d'Antananarivo",
        Adresse: "Campus Universitaire Ambohitsaina",
        CodePostal: "101",
        Ville: "Antananarivo",
        Pays: "Madagascar",
        Telephone: "+261340000000",
        Email: "contact@univ-antananarivo.mg",
        SiteWeb: "https://www.univ-antananarivo.mg",
        Logo: "logo_univ.png",
        Sigle: "UA",
        Description: "La plus grande université de Madagascar.",
        Statut: StatusEtablissement.Actif
    );

    [Fact]
    public async Task Post_DoitRetourner201Created_QuandRequeteEstValide()
    {
        // Arrange
        var request = GetValidRequest();

        // Act
        var response = await _http.PostAsJsonAsync("/api/etablissements", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var result = await response.Content.ReadFromJsonAsync<CreateEtablissementResponse>(_jsonOptions);
        result.Should().NotBeNull();
        result!.Id.Should().NotBeEmpty();
        result.Nom.Should().Be(request.Nom);
        result.Email.Should().Be(request.Email);
        result.Telephone.Should().Be(request.Telephone);
        result.Statut.Should().Be(StatusEtablissement.Actif);

        // Validation de la surcharge SaveChangesAsync (assignation automatique des dates d'audit)
        result.DateCreation.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
        result.DateModification.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task Post_DoitRetourner400BadRequest_QuandChampObligatoireEstVide()
    {
        // Arrange : Requête invalide (Nom vide) interceptée par FluentValidation avant le service
        var invalidRequest = GetValidRequest() with { Nom = string.Empty };

        // Act
        var response = await _http.PostAsJsonAsync("/api/etablissements", invalidRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Post_DoitRetourner409Conflict_QuandEtablissementExisteDeja()
    {
        // Arrange : On crée d'abord un établissement valide en base
        var request = GetValidRequest();
        var initialResponse = await _http.PostAsJsonAsync("/api/etablissements", request);
        initialResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        // Act : Tentative de recréation avec les mêmes données uniques (Nom, Email, Téléphone)
        var duplicateResponse = await _http.PostAsJsonAsync("/api/etablissements", request);

        // Assert : Doit retourner un conflit 409
        duplicateResponse.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }
}
