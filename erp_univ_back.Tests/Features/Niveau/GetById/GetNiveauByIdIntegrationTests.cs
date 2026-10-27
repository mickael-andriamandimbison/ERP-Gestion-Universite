using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using erp_univ_back.Features.Etablissement;
using erp_univ_back.Features.Etablissement.Create;
using erp_univ_back.Features.Niveau.Create;
using erp_univ_back.Features.Niveau.GetById;
using erp_univ_back.Shared.Data;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

namespace erp_univ_back.Tests.Features.Niveau.GetById;

public class GetNiveauByIdIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _http;
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    public GetNiveauByIdIntegrationTests(WebApplicationFactory<Program> factory)
    {
        var dbName = "DbTest_Niveau_GetById_" + Guid.NewGuid().ToString();

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

    private async Task<Guid> CreerEtablissementAsync(string nom = "Univ Niveau GetById")
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

    private async Task<CreateNiveauResponse> CreerNiveauAsync(Guid etablissementId, string code, string nom, int ordre)
    {
        var request = new CreateNiveauRequest(code, nom, ordre);

        using var message = new HttpRequestMessage(HttpMethod.Post, "/api/niveaux");
        message.Headers.Add("X-EtablissementId", etablissementId.ToString());
        message.Content = JsonContent.Create(request);

        var response = await _http.SendAsync(message);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await response.Content.ReadFromJsonAsync<CreateNiveauResponse>(_jsonOptions);
        return created!;
    }

    [Fact]
    public async Task GetById_DoitRetourner200OK_QuandNiveauExiste()
    {
        // Arrange
        var etablissementId = await CreerEtablissementAsync();
        var niveau = await CreerNiveauAsync(etablissementId, "M1_INFO", "Master 1 Informatique", 4);

        using var message = new HttpRequestMessage(HttpMethod.Get, $"/api/niveaux/{niveau.Id}");
        message.Headers.Add("X-EtablissementId", etablissementId.ToString());

        // Act
        var response = await _http.SendAsync(message);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<GetNiveauByIdResponse>(_jsonOptions);
        result.Should().NotBeNull();
        result!.Id.Should().Be(niveau.Id);
        result.Code.Should().Be("M1_INFO");
        result.Nom.Should().Be("Master 1 Informatique");
        result.Ordre.Should().Be(4);
    }

    [Fact]
    public async Task GetById_DoitRetourner404NotFound_QuandNiveauInexistant()
    {
        // Arrange
        var etablissementId = await CreerEtablissementAsync();
        using var message = new HttpRequestMessage(HttpMethod.Get, $"/api/niveaux/{Guid.NewGuid()}");
        message.Headers.Add("X-EtablissementId", etablissementId.ToString());

        // Act
        var response = await _http.SendAsync(message);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetById_DoitRetourner404NotFound_QuandConsultationDepuisAutreEtablissement()
    {
        // Arrange : Sécurité multi-tenant
        var etablissement1 = await CreerEtablissementAsync("Univ 1");
        var etablissement2 = await CreerEtablissementAsync("Univ 2");

        var niveauEtab1 = await CreerNiveauAsync(etablissement1, "NIV_PRIV", "Niveau Prive", 1);

        using var message = new HttpRequestMessage(HttpMethod.Get, $"/api/niveaux/{niveauEtab1.Id}");
        message.Headers.Add("X-EtablissementId", etablissement2.ToString()); // Mauvais tenant

        // Act
        var response = await _http.SendAsync(message);

        // Assert : Invisible pour le tenant 2
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}

