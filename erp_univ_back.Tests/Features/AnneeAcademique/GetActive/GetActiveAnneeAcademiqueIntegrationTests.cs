using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using erp_univ_back.Features.AnneeAcademique.Create;
using erp_univ_back.Features.AnneeAcademique.GetActive;
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

namespace erp_univ_back.Tests.Features.AnneeAcademique.GetActive;

public class GetActiveAnneeAcademiqueIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _http;
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    public GetActiveAnneeAcademiqueIntegrationTests(WebApplicationFactory<Program> factory)
    {
        var dbName = "DbTest_AnneeAcademique_GetActive_" + Guid.NewGuid().ToString();

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

    private async Task<Guid> CreerEtablissementAsync(string nom = "Univ Annee GetActive")
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
    public async Task GetActive_DoitRetourner200OK_QuandAnneeActiveExiste()
    {
        // Arrange
        var etablissementId = await CreerEtablissementAsync();
        var annee = await CreerAnneeAsync(etablissementId, "2025-2026", 2025, estActive: true);

        using var message = new HttpRequestMessage(HttpMethod.Get, "/api/annees-academiques/active");
        message.Headers.Add("X-EtablissementId", etablissementId.ToString());

        // Act
        var response = await _http.SendAsync(message);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<GetActiveAnneeAcademiqueResponse>(_jsonOptions);
        result.Should().NotBeNull();
        result!.Id.Should().Be(annee.Id);
        result.Libelle.Should().Be("2025-2026");
        result.EstActive.Should().BeTrue();
    }

    [Fact]
    public async Task GetActive_DoitRetourner404NotFound_QuandAucuneAnneeActive()
    {
        // Arrange : Établissement créé avec uniquement des années inactives
        var etablissementId = await CreerEtablissementAsync();
        await CreerAnneeAsync(etablissementId, "2024-2025", 2024, estActive: false);

        using var message = new HttpRequestMessage(HttpMethod.Get, "/api/annees-academiques/active");
        message.Headers.Add("X-EtablissementId", etablissementId.ToString());

        // Act
        var response = await _http.SendAsync(message);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}

