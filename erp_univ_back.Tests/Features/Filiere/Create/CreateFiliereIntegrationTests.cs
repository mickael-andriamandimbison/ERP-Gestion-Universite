using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using erp_univ_back.Features.Etablissement;
using erp_univ_back.Features.Etablissement.Create;
using erp_univ_back.Features.Filiere.Create;
using erp_univ_back.Shared.Data;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace erp_univ_back.Tests.Features.Filiere.Create;

public class CreateFiliereIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _http;
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    public CreateFiliereIntegrationTests(WebApplicationFactory<Program> factory)
    {
        var dbName = "DbTest_Filiere_Create_" + Guid.NewGuid().ToString();

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

    private async Task<Guid> CreerEtablissementAsync(string nom = "Univ Filiere")
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

    private CreateFiliereRequest GetValidRequest() => new(
        Code: "GL",
        Nom: "Génie Logiciel",
        Description: "Filière informatique appliquée"
    );

    [Fact]
    public async Task Post_DoitRetourner201Created_QuandRequeteEstValide()
    {
        // Arrange
        var etablissementId = await CreerEtablissementAsync();
        var request = GetValidRequest() with { Code = "gl" }; // Minuscule pour tester ToUpperInvariant

        using var message = new HttpRequestMessage(HttpMethod.Post, "/api/filieres");
        message.Headers.Add("X-EtablissementId", etablissementId.ToString());
        message.Content = JsonContent.Create(request);

        // Act
        var response = await _http.SendAsync(message);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var result = await response.Content.ReadFromJsonAsync<CreateFiliereResponse>(_jsonOptions);
        result.Should().NotBeNull();
        result!.Id.Should().NotBeEmpty();
        result.Code.Should().Be("GL");
        result.Nom.Should().Be("Génie Logiciel");
        result.DateCreation.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task Post_DoitRetourner400BadRequest_QuandCodeOuNomVide()
    {
        // Arrange
        var etablissementId = await CreerEtablissementAsync();
        var invalidRequest = GetValidRequest() with { Code = string.Empty };

        using var message = new HttpRequestMessage(HttpMethod.Post, "/api/filieres");
        message.Headers.Add("X-EtablissementId", etablissementId.ToString());
        message.Content = JsonContent.Create(invalidRequest);

        // Act
        var response = await _http.SendAsync(message);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Post_DoitRetourner409Conflict_QuandCodeExisteDejaPourMemeEtablissement()
    {
        // Arrange
        var etablissementId = await CreerEtablissementAsync();
        var request = GetValidRequest();

        // 1ère création
        using var msg1 = new HttpRequestMessage(HttpMethod.Post, "/api/filieres");
        msg1.Headers.Add("X-EtablissementId", etablissementId.ToString());
        msg1.Content = JsonContent.Create(request);
        var resp1 = await _http.SendAsync(msg1);
        resp1.StatusCode.Should().Be(HttpStatusCode.Created);

        // 2ème création avec le même code en minuscules
        var duplicateRequest = request with { Code = "gl", Nom = "Autre nom" };
        using var msg2 = new HttpRequestMessage(HttpMethod.Post, "/api/filieres");
        msg2.Headers.Add("X-EtablissementId", etablissementId.ToString());
        msg2.Content = JsonContent.Create(duplicateRequest);

        // Act
        var resp2 = await _http.SendAsync(msg2);

        // Assert
        resp2.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Post_DoitAccepterMemeCode_PourDeuxEtablissementsDifferents()
    {
        // Arrange : Deux établissements distincts
        var etablissement1 = await CreerEtablissementAsync("Univ 1");
        var etablissement2 = await CreerEtablissementAsync("Univ 2");
        var request = GetValidRequest();

        // Act pour Tenant 1
        using var msg1 = new HttpRequestMessage(HttpMethod.Post, "/api/filieres");
        msg1.Headers.Add("X-EtablissementId", etablissement1.ToString());
        msg1.Content = JsonContent.Create(request);
        var resp1 = await _http.SendAsync(msg1);

        // Act pour Tenant 2
        using var msg2 = new HttpRequestMessage(HttpMethod.Post, "/api/filieres");
        msg2.Headers.Add("X-EtablissementId", etablissement2.ToString());
        msg2.Content = JsonContent.Create(request);
        var resp2 = await _http.SendAsync(msg2);

        // Assert : Étanchéité multi-tenant
        resp1.StatusCode.Should().Be(HttpStatusCode.Created);
        resp2.StatusCode.Should().Be(HttpStatusCode.Created);
    }
}

