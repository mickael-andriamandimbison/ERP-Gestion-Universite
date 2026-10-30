using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using erp_univ_back.Features.Etablissement;
using erp_univ_back.Features.Etablissement.Create;
using erp_univ_back.Features.Professeur.Create;
using erp_univ_back.Features.Professeur.GetAll;
using erp_univ_back.Shared.Data;
using erp_univ_back.Shared.Responses;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

namespace erp_univ_back.Tests.Features.Professeur.GetAll;

public class GetAllProfesseurIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _http;
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    public GetAllProfesseurIntegrationTests(WebApplicationFactory<Program> factory)
    {
        var dbName = "DbTest_Professeur_GetAll_" + Guid.NewGuid().ToString();

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

    private async Task<Guid> CreerEtablissementAsync(string nom = "Univ Professeur GetAll")
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

    private async Task<CreateProfesseurResponse> CreerProfesseurAsync(Guid etablissementId, string nom, string telephone, string email, string cin)
    {
        var request = new CreateProfesseurRequest(
            Nom: nom,
            Prenom: "PrenomTest",
            DateNaissance: new DateOnly(1980, 5, 14),
            LieuNaissance: "Antananarivo",
            NumeroTelephone: telephone,
            Email: email,
            NumeroCin: cin
        );

        using var message = new HttpRequestMessage(HttpMethod.Post, "/api/professeurs");
        message.Headers.Add("X-EtablissementId", etablissementId.ToString());
        message.Content = JsonContent.Create(request);

        var response = await _http.SendAsync(message);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await response.Content.ReadFromJsonAsync<CreateProfesseurResponse>(_jsonOptions);
        return created!;
    }

    [Fact]
    public async Task GetAll_DoitRetourner200OK_AvecPaginationValide()
    {
        // Arrange
        var etablissementId = await CreerEtablissementAsync();
        await CreerProfesseurAsync(etablissementId, "Professeur Un", "+261340101011", "prof1@univ.mg", "101000111222");
        await CreerProfesseurAsync(etablissementId, "Professeur Deux", "+261340202022", "prof2@univ.mg", "102000111222");
        await CreerProfesseurAsync(etablissementId, "Professeur Trois", "+261340303033", "prof3@univ.mg", "103000111222");

        using var message = new HttpRequestMessage(HttpMethod.Get, "/api/professeurs?page=1&taillePage=2&champTri=nom");
        message.Headers.Add("X-EtablissementId", etablissementId.ToString());

        // Act
        var response = await _http.SendAsync(message);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<PagedResponse<GetAllProfesseurResponse>>(_jsonOptions);
        result.Should().NotBeNull();
        result!.Page.Should().Be(1);
        result.TaillePage.Should().Be(2);
        result.Elements.Should().HaveCount(2);
        result.TotalElements.Should().BeGreaterThanOrEqualTo(3);
        result.TotalPages.Should().BeGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task GetAll_DoitRetourner200OK_AvecFiltreRecherche()
    {
        // Arrange
        var etablissementId = await CreerEtablissementAsync();
        var uniqueNom = "Ratsimbazafy";
        await CreerProfesseurAsync(etablissementId, uniqueNom, "+261349988771", "unique@univ.mg", "109888777666");

        using var message = new HttpRequestMessage(HttpMethod.Get, $"/api/professeurs?recherche={uniqueNom}");
        message.Headers.Add("X-EtablissementId", etablissementId.ToString());

        // Act
        var response = await _http.SendAsync(message);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<PagedResponse<GetAllProfesseurResponse>>(_jsonOptions);
        result.Should().NotBeNull();
        result!.Elements.Should().Contain(p => p.Nom == uniqueNom);
    }

    [Theory]
    [InlineData("page=0")]
    [InlineData("taillePage=500")]
    [InlineData("champTri=inconnu")]
    public async Task GetAll_DoitRetourner400BadRequest_QuandParametresRequeteInvalides(string queryInvalide)
    {
        // Arrange
        var etablissementId = await CreerEtablissementAsync();

        using var message = new HttpRequestMessage(HttpMethod.Get, $"/api/professeurs?{queryInvalide}");
        message.Headers.Add("X-EtablissementId", etablissementId.ToString());

        // Act
        var response = await _http.SendAsync(message);

        // Assert : Protection serveur
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetAll_DoitIsolerDonneesEntreEtablissements()
    {
        // Arrange : Ségrégation multi-tenant
        var etablissement1 = await CreerEtablissementAsync("Univ Antananarivo");
        var etablissement2 = await CreerEtablissementAsync("Univ Toliara");

        await CreerProfesseurAsync(etablissement1, "ProfTana", "+261341111111", "tana@univ.mg", "101111111111");
        await CreerProfesseurAsync(etablissement2, "ProfToliara", "+261342222222", "toliara@univ.mg", "102222222222");

        using var message1 = new HttpRequestMessage(HttpMethod.Get, "/api/professeurs");
        message1.Headers.Add("X-EtablissementId", etablissement1.ToString());

        // Act
        var response1 = await _http.SendAsync(message1);

        // Assert : Seul le professeur du tenant 1 est retourné
        response1.StatusCode.Should().Be(HttpStatusCode.OK);
        var result1 = await response1.Content.ReadFromJsonAsync<PagedResponse<GetAllProfesseurResponse>>(_jsonOptions);
        result1!.Elements.Should().Contain(p => p.Nom == "ProfTana");
        result1.Elements.Should().NotContain(p => p.Nom == "ProfToliara");
    }
}

