using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using erp_univ_back.Features.AnneeAcademique.Create;
using erp_univ_back.Features.AnneeAcademique.Update;
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

namespace erp_univ_back.Tests.Features.AnneeAcademique.Update;

public class UpdateAnneeAcademiqueIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _http;
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    public UpdateAnneeAcademiqueIntegrationTests(WebApplicationFactory<Program> factory)
    {
        var dbName = "DbTest_AnneeAcademique_Update_" + Guid.NewGuid().ToString();

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

    private async Task<Guid> CreerEtablissementAsync(string nom = "Univ Annee Update")
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

    private async Task<CreateAnneeAcademiqueResponse> CreerAnneeAsync(Guid etablissementId, string libelle, int anneeDebut)
    {
        var request = new CreateAnneeAcademiqueRequest(
            Libelle: libelle,
            DateDebut: new DateOnly(anneeDebut, 9, 1),
            DateFin: new DateOnly(anneeDebut + 1, 6, 30),
            EstActive: false
        );

        using var message = new HttpRequestMessage(HttpMethod.Post, "/api/annees-academiques");
        message.Headers.Add("X-EtablissementId", etablissementId.ToString());
        message.Content = JsonContent.Create(request);

        var response = await _http.SendAsync(message);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await response.Content.ReadFromJsonAsync<CreateAnneeAcademiqueResponse>(_jsonOptions);
        return created!;
    }

    [Fact]
    public async Task Put_DoitRetourner200OK_QuandRequeteEstValide()
    {
        // Arrange
        var etablissementId = await CreerEtablissementAsync();
        var annee = await CreerAnneeAsync(etablissementId, "2025-2026", 2025);

        var updateRequest = new UpdateAnneeAcademiqueRequest(
            Libelle: "2025/2026",
            DateDebut: new DateOnly(2025, 10, 1),
            DateFin: new DateOnly(2026, 7, 15)
        );

        using var message = new HttpRequestMessage(HttpMethod.Put, $"/api/annees-academiques/{annee.Id}");
        message.Headers.Add("X-EtablissementId", etablissementId.ToString());
        message.Content = JsonContent.Create(updateRequest);

        // Act
        var response = await _http.SendAsync(message);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<UpdateAnneeAcademiqueResponse>(_jsonOptions);
        result.Should().NotBeNull();
        result!.Id.Should().Be(annee.Id);
        result.Libelle.Should().Be("2025/2026");
        result.DateDebut.Should().Be(new DateOnly(2025, 10, 1));
        result.DateModification.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task Put_DoitRetourner400BadRequest_QuandLibelleEstInvalide()
    {
        // Arrange
        var etablissementId = await CreerEtablissementAsync();
        var annee = await CreerAnneeAsync(etablissementId, "2025-2026", 2025);

        var invalidRequest = new UpdateAnneeAcademiqueRequest(
            Libelle: "2025-2028", // Années non consécutives
            DateDebut: new DateOnly(2025, 9, 1),
            DateFin: new DateOnly(2028, 6, 30)
        );

        using var message = new HttpRequestMessage(HttpMethod.Put, $"/api/annees-academiques/{annee.Id}");
        message.Headers.Add("X-EtablissementId", etablissementId.ToString());
        message.Content = JsonContent.Create(invalidRequest);

        // Act
        var response = await _http.SendAsync(message);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Put_DoitRetourner404NotFound_QuandAnneeInexistante()
    {
        // Arrange
        var etablissementId = await CreerEtablissementAsync();
        var updateRequest = new UpdateAnneeAcademiqueRequest(
            Libelle: "2025-2026",
            DateDebut: new DateOnly(2025, 9, 1),
            DateFin: new DateOnly(2026, 6, 30)
        );

        using var message = new HttpRequestMessage(HttpMethod.Put, $"/api/annees-academiques/{Guid.NewGuid()}");
        message.Headers.Add("X-EtablissementId", etablissementId.ToString());
        message.Content = JsonContent.Create(updateRequest);

        // Act
        var response = await _http.SendAsync(message);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Put_DoitRetourner409Conflict_QuandLibelleExisteDejaPourMemeEtablissement()
    {
        // Arrange : Création de deux années distinctes pour le même établissement
        var etablissementId = await CreerEtablissementAsync();
        var annee1 = await CreerAnneeAsync(etablissementId, "2025-2026", 2025);
        var annee2 = await CreerAnneeAsync(etablissementId, "2026-2027", 2026);

        // Tenter de renommer annee2 avec le libellé de annee1
        var conflictRequest = new UpdateAnneeAcademiqueRequest(
            Libelle: "2025-2026",
            DateDebut: new DateOnly(2025, 9, 1),
            DateFin: new DateOnly(2026, 6, 30)
        );

        using var message = new HttpRequestMessage(HttpMethod.Put, $"/api/annees-academiques/{annee2.Id}");
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
        var etablissement1 = await CreerEtablissementAsync("Univ Propriétaire");
        var etablissement2 = await CreerEtablissementAsync("Univ Usurpatrice");

        var anneeEtab1 = await CreerAnneeAsync(etablissement1, "2025-2026", 2025);

        // Tentative d'update par l'Établissement 2 sur l'année de l'Établissement 1
        var updateRequest = new UpdateAnneeAcademiqueRequest(
            Libelle: "2025/2026",
            DateDebut: new DateOnly(2025, 9, 1),
            DateFin: new DateOnly(2026, 6, 30)
        );

        using var message = new HttpRequestMessage(HttpMethod.Put, $"/api/annees-academiques/{anneeEtab1.Id}");
        message.Headers.Add("X-EtablissementId", etablissement2.ToString()); // Mauvais tenant
        message.Content = JsonContent.Create(updateRequest);

        // Act
        var response = await _http.SendAsync(message);

        // Assert : Le QueryFilter doit rendre l'année invisible pour le tenant 2 -> 404 Not Found
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}

