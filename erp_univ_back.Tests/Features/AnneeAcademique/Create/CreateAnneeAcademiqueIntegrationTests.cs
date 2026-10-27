using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using erp_univ_back.Features.AnneeAcademique.Create;
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

namespace erp_univ_back.Tests.Features.AnneeAcademique.Create;

public class CreateAnneeAcademiqueIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _http;
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    public CreateAnneeAcademiqueIntegrationTests(WebApplicationFactory<Program> factory)
    {
        var dbName = "DbTest_AnneeAcademique_Create_" + Guid.NewGuid().ToString();

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

    private async Task<Guid> CreerEtablissementAsync(string nom = "Univ Annee Academique")
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
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await response.Content.ReadFromJsonAsync<CreateEtablissementResponse>(_jsonOptions);
        return created!.Id;
    }

    private CreateAnneeAcademiqueRequest GetValidRequest() => new(
        Libelle: "2025-2026",
        DateDebut: new DateOnly(2025, 9, 1),
        DateFin: new DateOnly(2026, 6, 30),
        EstActive: false
    );

    [Fact]
    public async Task Post_DoitRetourner201Created_QuandRequeteEstValide()
    {
        // Arrange : Multi-tenant context
        var etablissementId = await CreerEtablissementAsync();
        using var message = new HttpRequestMessage(HttpMethod.Post, "/api/annees-academiques");
        message.Headers.Add("X-EtablissementId", etablissementId.ToString());
        message.Content = JsonContent.Create(GetValidRequest());

        // Act
        var response = await _http.SendAsync(message);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var result = await response.Content.ReadFromJsonAsync<CreateAnneeAcademiqueResponse>(_jsonOptions);
        result.Should().NotBeNull();
        result!.Id.Should().NotBeEmpty();
        result.Libelle.Should().Be("2025-2026");
        result.DateDebut.Should().Be(new DateOnly(2025, 9, 1));
        result.DateFin.Should().Be(new DateOnly(2026, 6, 30));

        // Surcharge SaveChangesAsync : DateCreation assignée automatiquement
        result.DateCreation.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task Post_DoitRetourner400BadRequest_QuandLibelleEstInvalide()
    {
        // Arrange
        var etablissementId = await CreerEtablissementAsync();
        var invalidRequest = GetValidRequest() with { Libelle = "invalide" };

        using var message = new HttpRequestMessage(HttpMethod.Post, "/api/annees-academiques");
        message.Headers.Add("X-EtablissementId", etablissementId.ToString());
        message.Content = JsonContent.Create(invalidRequest);

        // Act
        var response = await _http.SendAsync(message);

        // Assert : Interception FluentValidation avant le service
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Post_DoitRetourner409Conflict_QuandLibelleExisteDejaPourMemeEtablissement()
    {
        // Arrange
        var etablissementId = await CreerEtablissementAsync();
        var request = GetValidRequest();

        // 1ère création réussie
        using var firstMsg = new HttpRequestMessage(HttpMethod.Post, "/api/annees-academiques");
        firstMsg.Headers.Add("X-EtablissementId", etablissementId.ToString());
        firstMsg.Content = JsonContent.Create(request);
        var firstResponse = await _http.SendAsync(firstMsg);
        firstResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        // 2ème tentative avec même libellé
        using var duplicateMsg = new HttpRequestMessage(HttpMethod.Post, "/api/annees-academiques");
        duplicateMsg.Headers.Add("X-EtablissementId", etablissementId.ToString());
        duplicateMsg.Content = JsonContent.Create(request);

        // Act
        var duplicateResponse = await _http.SendAsync(duplicateMsg);

        // Assert
        duplicateResponse.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Post_DoitAccepterMemeLibelle_PourDeuxEtablissementsDifferents()
    {
        // Arrange : Deux tenants isolés
        var etablissement1 = await CreerEtablissementAsync("Univ Un");
        var etablissement2 = await CreerEtablissementAsync("Univ Deux");
        var request = GetValidRequest();

        // Act pour Tenant 1
        using var msg1 = new HttpRequestMessage(HttpMethod.Post, "/api/annees-academiques");
        msg1.Headers.Add("X-EtablissementId", etablissement1.ToString());
        msg1.Content = JsonContent.Create(request);
        var resp1 = await _http.SendAsync(msg1);

        // Act pour Tenant 2
        using var msg2 = new HttpRequestMessage(HttpMethod.Post, "/api/annees-academiques");
        msg2.Headers.Add("X-EtablissementId", etablissement2.ToString());
        msg2.Content = JsonContent.Create(request);
        var resp2 = await _http.SendAsync(msg2);

        // Assert : Étanchéité multi-tenant garantie, pas de conflit inter-établissements
        resp1.StatusCode.Should().Be(HttpStatusCode.Created);
        resp2.StatusCode.Should().Be(HttpStatusCode.Created);
    }
}

