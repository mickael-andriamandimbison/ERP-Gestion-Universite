using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using erp_univ_back.Features.AnneeAcademique.Create;
using erp_univ_back.Features.AnneeAcademique.SetActive;
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

namespace erp_univ_back.Tests.Features.AnneeAcademique.SetActive;

public class SetActiveAnneeAcademiqueIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _http;
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    public SetActiveAnneeAcademiqueIntegrationTests(WebApplicationFactory<Program> factory)
    {
        var dbName = "DbTest_AnneeAcademique_SetActive_" + Guid.NewGuid().ToString();

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

    private async Task<Guid> CreerEtablissementAsync(string nom = "Univ Annee SetActive")
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

    private async Task<CreateAnneeAcademiqueResponse> CreerAnneeAsync(Guid etablissementId, string libelle, int anneeDebut, bool estActive = false)
    {
        var request = new CreateAnneeAcademiqueRequest(
            Libelle: libelle,
            DateDebut: new DateOnly(anneeDebut, 9, 1),
            DateFin: new DateOnly(anneeDebut + 1, 6, 30),
            EstActive: estActive
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
    public async Task SetActive_DoitRetourner200OK_EtBasculerAnneeActive()
    {
        // Arrange
        var etablissementId = await CreerEtablissementAsync();
        var annee1 = await CreerAnneeAsync(etablissementId, "2024-2025", 2024, estActive: true);
        var annee2 = await CreerAnneeAsync(etablissementId, "2025-2026", 2025, estActive: false);

        using var message = new HttpRequestMessage(HttpMethod.Put, $"/api/annees-academiques/{annee2.Id}/set-active");
        message.Headers.Add("X-EtablissementId", etablissementId.ToString());

        // Act
        var response = await _http.SendAsync(message);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<SetActiveAnneeAcademiqueResponse>(_jsonOptions);
        result.Should().NotBeNull();
        result!.Id.Should().Be(annee2.Id);
        result.EstActive.Should().BeTrue();

        // Vérification que la consultation de l'année active renvoie désormais bien l'année 2
        using var checkActiveMsg = new HttpRequestMessage(HttpMethod.Get, "/api/annees-academiques/active");
        checkActiveMsg.Headers.Add("X-EtablissementId", etablissementId.ToString());
        var activeResponse = await _http.SendAsync(checkActiveMsg);
        activeResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var currentActive = await activeResponse.Content.ReadFromJsonAsync<CreateAnneeAcademiqueResponse>(_jsonOptions);
        currentActive!.Id.Should().Be(annee2.Id);
    }

    [Fact]
    public async Task SetActive_DoitRetourner404NotFound_QuandAnneeInexistante()
    {
        // Arrange
        var etablissementId = await CreerEtablissementAsync();
        using var message = new HttpRequestMessage(HttpMethod.Put, $"/api/annees-academiques/{Guid.NewGuid()}/set-active");
        message.Headers.Add("X-EtablissementId", etablissementId.ToString());

        // Act
        var response = await _http.SendAsync(message);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task SetActive_DoitRetourner404NotFound_QuandTentativeActivationSurAutreEtablissement()
    {
        // Arrange : Sécurité multi-tenant
        var etablissement1 = await CreerEtablissementAsync("Univ Etab1");
        var etablissement2 = await CreerEtablissementAsync("Univ Etab2");

        var anneeEtab1 = await CreerAnneeAsync(etablissement1, "2025-2026", 2025);

        using var message = new HttpRequestMessage(HttpMethod.Put, $"/api/annees-academiques/{anneeEtab1.Id}/set-active");
        message.Headers.Add("X-EtablissementId", etablissement2.ToString()); // Mauvais tenant

        // Act
        var response = await _http.SendAsync(message);

        // Assert : Invisible et inaccessible pour le tenant 2
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}

