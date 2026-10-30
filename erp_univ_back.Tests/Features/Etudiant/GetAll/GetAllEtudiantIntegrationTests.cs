using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using erp_univ_back.Features.Etablissement;
using erp_univ_back.Features.Etablissement.Create;
using erp_univ_back.Features.Etudiant.Create;
using erp_univ_back.Features.Etudiant.GetAll;
using erp_univ_back.Shared.Data;
using erp_univ_back.Shared.Responses;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

namespace erp_univ_back.Tests.Features.Etudiant.GetAll;

public class GetAllEtudiantIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _http;
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    public GetAllEtudiantIntegrationTests(WebApplicationFactory<Program> factory)
    {
        var dbName = "DbTest_Etudiant_GetAll_" + Guid.NewGuid().ToString();

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

    private async Task<Guid> CreerEtablissementAsync(string nom = "Univ Etudiant GetAll")
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

    private async Task<CreateEtudiantResponse> CreerEtudiantAsync(Guid etablissementId, string nom, string telephone, string email)
    {
        var request = new CreateEtudiantRequest(
            Nom: nom,
            Prenom: "Test",
            DateNaissance: new DateOnly(2003, 5, 20),
            LieuNaissance: "Antananarivo",
            NumeroTelephone: telephone,
            Email: email,
            Parents: null
        );

        using var message = new HttpRequestMessage(HttpMethod.Post, "/api/etudiants");
        message.Headers.Add("X-EtablissementId", etablissementId.ToString());
        message.Content = JsonContent.Create(request);

        var response = await _http.SendAsync(message);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await response.Content.ReadFromJsonAsync<CreateEtudiantResponse>(_jsonOptions);
        return created!;
    }

    [Fact]
    public async Task GetAll_DoitRetourner200OK_AvecPaginationValide()
    {
        // Arrange
        var etablissementId = await CreerEtablissementAsync();
        await CreerEtudiantAsync(etablissementId, "Etudiant Un", "+261340101010", "etu1@univ.mg");
        await CreerEtudiantAsync(etablissementId, "Etudiant Deux", "+261340202020", "etu2@univ.mg");
        await CreerEtudiantAsync(etablissementId, "Etudiant Trois", "+261340303030", "etu3@univ.mg");

        using var message = new HttpRequestMessage(HttpMethod.Get, "/api/etudiants?page=1&taillePage=2&champTri=nom");
        message.Headers.Add("X-EtablissementId", etablissementId.ToString());

        // Act
        var response = await _http.SendAsync(message);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<PagedResponse<GetAllEtudiantResponse>>(_jsonOptions);
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
        var uniqueNom = "Zafindrasoa";
        await CreerEtudiantAsync(etablissementId, uniqueNom, "+261349988776", "zafy@univ.mg");

        using var message = new HttpRequestMessage(HttpMethod.Get, $"/api/etudiants?recherche={uniqueNom}");
        message.Headers.Add("X-EtablissementId", etablissementId.ToString());

        // Act
        var response = await _http.SendAsync(message);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<PagedResponse<GetAllEtudiantResponse>>(_jsonOptions);
        result.Should().NotBeNull();
        result!.Elements.Should().Contain(e => e.Nom == uniqueNom);
    }

    [Theory]
    [InlineData("page=0")]
    [InlineData("taillePage=500")]
    [InlineData("champTri=inconnu")]
    public async Task GetAll_DoitRetourner400BadRequest_QuandParametresRequeteInvalides(string queryInvalide)
    {
        // Arrange
        var etablissementId = await CreerEtablissementAsync();

        using var message = new HttpRequestMessage(HttpMethod.Get, $"/api/etudiants?{queryInvalide}");
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
        var etablissement2 = await CreerEtablissementAsync("Univ Mahajanga");

        await CreerEtudiantAsync(etablissement1, "EtuTana", "+261341122334", "tana@univ.mg");
        await CreerEtudiantAsync(etablissement2, "EtuMajunga", "+261349988771", "majunga@univ.mg");

        using var message1 = new HttpRequestMessage(HttpMethod.Get, "/api/etudiants");
        message1.Headers.Add("X-EtablissementId", etablissement1.ToString());

        // Act
        var response1 = await _http.SendAsync(message1);

        // Assert : Seul l'étudiant du tenant 1 est retourné
        response1.StatusCode.Should().Be(HttpStatusCode.OK);
        var result1 = await response1.Content.ReadFromJsonAsync<PagedResponse<GetAllEtudiantResponse>>(_jsonOptions);
        result1!.Elements.Should().Contain(e => e.Nom == "EtuTana");
        result1.Elements.Should().NotContain(e => e.Nom == "EtuMajunga");
    }
}

