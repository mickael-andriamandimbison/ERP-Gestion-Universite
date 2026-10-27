using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using erp_univ_back.Features.Etablissement;
using erp_univ_back.Features.Etablissement.Create;
using erp_univ_back.Features.Filiere.Create;
using erp_univ_back.Features.Filiere.Update;
using erp_univ_back.Shared.Data;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

namespace erp_univ_back.Tests.Features.Filiere.Update;

public class UpdateFiliereIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _http;
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    public UpdateFiliereIntegrationTests(WebApplicationFactory<Program> factory)
    {
        var dbName = "DbTest_Filiere_Update_" + Guid.NewGuid().ToString();

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

    private async Task<Guid> CreerEtablissementAsync(string nom = "Univ Filiere Update")
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
        var request = new CreateFiliereRequest(code, nom, "Description initiale");

        using var message = new HttpRequestMessage(HttpMethod.Post, "/api/filieres");
        message.Headers.Add("X-EtablissementId", etablissementId.ToString());
        message.Content = JsonContent.Create(request);

        var response = await _http.SendAsync(message);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await response.Content.ReadFromJsonAsync<CreateFiliereResponse>(_jsonOptions);
        return created!;
    }

    [Fact]
    public async Task Put_DoitRetourner200OK_QuandRequeteEstValide()
    {
        // Arrange
        var etablissementId = await CreerEtablissementAsync();
        var filiere = await CreerFiliereAsync(etablissementId, "GL", "Génie Logiciel");

        var updateRequest = new UpdateFiliereRequest("GL_AVANCE", "Génie Logiciel Avancé", "Nouvelle description");

        using var message = new HttpRequestMessage(HttpMethod.Put, $"/api/filieres/{filiere.Id}");
        message.Headers.Add("X-EtablissementId", etablissementId.ToString());
        message.Content = JsonContent.Create(updateRequest);

        // Act
        var response = await _http.SendAsync(message);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<UpdateFiliereResponse>(_jsonOptions);
        result.Should().NotBeNull();
        result!.Id.Should().Be(filiere.Id);
        result.Code.Should().Be("GL_AVANCE");
        result.Nom.Should().Be("Génie Logiciel Avancé");
        result.DateModification.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task Put_DoitRetourner400BadRequest_QuandValidationEchoue()
    {
        // Arrange
        var etablissementId = await CreerEtablissementAsync();
        var filiere = await CreerFiliereAsync(etablissementId, "RT", "Réseaux");

        var invalidRequest = new UpdateFiliereRequest("", "Réseaux Modifiés", null);

        using var message = new HttpRequestMessage(HttpMethod.Put, $"/api/filieres/{filiere.Id}");
        message.Headers.Add("X-EtablissementId", etablissementId.ToString());
        message.Content = JsonContent.Create(invalidRequest);

        // Act
        var response = await _http.SendAsync(message);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Put_DoitRetourner404NotFound_QuandFiliereInexistante()
    {
        // Arrange
        var etablissementId = await CreerEtablissementAsync();
        var updateRequest = new UpdateFiliereRequest("CODE_INCONNU", "Nom", null);

        using var message = new HttpRequestMessage(HttpMethod.Put, $"/api/filieres/{Guid.NewGuid()}");
        message.Headers.Add("X-EtablissementId", etablissementId.ToString());
        message.Content = JsonContent.Create(updateRequest);

        // Act
        var response = await _http.SendAsync(message);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Put_DoitRetourner409Conflict_QuandCodeAppartientADejaUnAutreFiliere()
    {
        // Arrange : Deux filières distinctes
        var etablissementId = await CreerEtablissementAsync();
        var f1 = await CreerFiliereAsync(etablissementId, "INFO", "Informatique");
        var f2 = await CreerFiliereAsync(etablissementId, "GESTION", "Sciences de Gestion");

        // Tenter d'attribuer le code de f1 à f2
        var conflictRequest = new UpdateFiliereRequest("info", "Gestion Modifiée", null);

        using var message = new HttpRequestMessage(HttpMethod.Put, $"/api/filieres/{f2.Id}");
        message.Headers.Add("X-EtablissementId", etablissementId.ToString());
        message.Content = JsonContent.Create(conflictRequest);

        // Act
        var response = await _http.SendAsync(message);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Put_DoitRetourner404NotFound_QuandTentativeModificationSurAutreEtablissement()
    {
        // Arrange : Sécurité multi-tenant
        var etablissement1 = await CreerEtablissementAsync("Univ 1");
        var etablissement2 = await CreerEtablissementAsync("Univ 2");

        var filiereEtab1 = await CreerFiliereAsync(etablissement1, "DROIT", "Droit Privé");

        var updateRequest = new UpdateFiliereRequest("DROIT", "Droit Hacked", null);

        using var message = new HttpRequestMessage(HttpMethod.Put, $"/api/filieres/{filiereEtab1.Id}");
        message.Headers.Add("X-EtablissementId", etablissement2.ToString()); // Mauvais tenant
        message.Content = JsonContent.Create(updateRequest);

        // Act
        var response = await _http.SendAsync(message);

        // Assert : Invisible pour le tenant 2
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}

