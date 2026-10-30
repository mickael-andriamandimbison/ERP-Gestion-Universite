using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using erp_univ_back.Features.AnneeAcademique.Create;
using erp_univ_back.Features.AnneeAcademique.GetAll;
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

namespace erp_univ_back.Tests.Features.AnneeAcademique.GetAll;

public class GetAllAnneeAcademiqueIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _http;
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    public GetAllAnneeAcademiqueIntegrationTests(WebApplicationFactory<Program> factory)
    {
        var dbName = "DbTest_AnneeAcademique_GetAll_" + Guid.NewGuid().ToString();

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

    private async Task<Guid> CreerEtablissementAsync(string nom = "Univ Annee GetAll")
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
    public async Task GetAll_DoitRetourner200OK_AvecListeFiltreeEtOrdonnee()
    {
        // Arrange
        var etablissementId = await CreerEtablissementAsync();
        await CreerAnneeAsync(etablissementId, "2023-2024", 2023, estActive: false);
        await CreerAnneeAsync(etablissementId, "2024-2025", 2024, estActive: true);

        using var message = new HttpRequestMessage(HttpMethod.Get, "/api/annees-academiques");
        message.Headers.Add("X-EtablissementId", etablissementId.ToString());

        // Act
        var response = await _http.SendAsync(message);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var list = await response.Content.ReadFromJsonAsync<List<GetAllAnneeAcademiqueResponse>>(_jsonOptions);
        list.Should().NotBeNull();
        list!.Should().HaveCount(2);

        // L'année active doit être la première de la liste
        list.First().EstActive.Should().BeTrue();
        list.First().Libelle.Should().Be("2024-2025");
    }

    [Fact]
    public async Task GetAll_DoitRetourner200OK_AvecFiltreRecherche()
    {
        // Arrange
        var etablissementId = await CreerEtablissementAsync();
        await CreerAnneeAsync(etablissementId, "2021-2022", 2021);
        await CreerAnneeAsync(etablissementId, "2022-2023", 2022);

        using var message = new HttpRequestMessage(HttpMethod.Get, "/api/annees-academiques?recherche=2021");
        message.Headers.Add("X-EtablissementId", etablissementId.ToString());

        // Act
        var response = await _http.SendAsync(message);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var list = await response.Content.ReadFromJsonAsync<List<GetAllAnneeAcademiqueResponse>>(_jsonOptions);
        list.Should().NotBeNull();
        list!.Should().ContainSingle(a => a.Libelle == "2021-2022");
    }

    [Fact]
    public async Task GetAll_DoitRetourner400BadRequest_QuandRechercheDepasseLongueurMax()
    {
        // Arrange
        var etablissementId = await CreerEtablissementAsync();
        var rechercheAbusive = new string('A', 101);

        using var message = new HttpRequestMessage(HttpMethod.Get, $"/api/annees-academiques?recherche={rechercheAbusive}");
        message.Headers.Add("X-EtablissementId", etablissementId.ToString());

        // Act
        var response = await _http.SendAsync(message);

        // Assert : Protection serveur
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetAll_DoitIsolerDonneesEntreEtablissements()
    {
        // Arrange : Multi-tenant data segregation
        var etablissement1 = await CreerEtablissementAsync("Univ Nord");
        var etablissement2 = await CreerEtablissementAsync("Univ Sud");

        await CreerAnneeAsync(etablissement1, "2020-2021", 2020);
        await CreerAnneeAsync(etablissement2, "2025-2026", 2025);

        using var message1 = new HttpRequestMessage(HttpMethod.Get, "/api/annees-academiques");
        message1.Headers.Add("X-EtablissementId", etablissement1.ToString());

        // Act
        var response1 = await _http.SendAsync(message1);

        // Assert : L'établissement 1 ne voit QUE son année
        response1.StatusCode.Should().Be(HttpStatusCode.OK);
        var list1 = await response1.Content.ReadFromJsonAsync<List<GetAllAnneeAcademiqueResponse>>(_jsonOptions);
        list1.Should().ContainSingle(a => a.Libelle == "2020-2021");
        list1.Should().NotContain(a => a.Libelle == "2025-2026");
    }
}

