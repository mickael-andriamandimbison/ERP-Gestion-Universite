using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using erp_univ_back.Features.Etablissement;
using erp_univ_back.Features.Etablissement.Create;
using erp_univ_back.Features.Niveau.Create;
using erp_univ_back.Features.Niveau.GetAll;
using erp_univ_back.Shared.Data;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

namespace erp_univ_back.Tests.Features.Niveau.GetAll;

public class GetAllNiveauIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _http;
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    public GetAllNiveauIntegrationTests(WebApplicationFactory<Program> factory)
    {
        var dbName = "DbTest_Niveau_GetAll_" + Guid.NewGuid().ToString();

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

    private async Task<Guid> CreerEtablissementAsync(string nom = "Univ Niveau GetAll")
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
    public async Task GetAll_DoitRetourner200OK_AvecNiveauxTriesParOrdre()
    {
        // Arrange : Insertion désordonnée
        var etablissementId = await CreerEtablissementAsync();
        await CreerNiveauAsync(etablissementId, "L2", "Licence 2", 2);
        await CreerNiveauAsync(etablissementId, "L1", "Licence 1", 1);
        await CreerNiveauAsync(etablissementId, "L3", "Licence 3", 3);

        using var message = new HttpRequestMessage(HttpMethod.Get, "/api/niveaux");
        message.Headers.Add("X-EtablissementId", etablissementId.ToString());

        // Act
        var response = await _http.SendAsync(message);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var list = await response.Content.ReadFromJsonAsync<List<GetAllNiveauResponse>>(_jsonOptions);
        list.Should().NotBeNull();
        list!.Should().HaveCount(3);

        // Vérification du tri par ordre ascendant (1, puis 2, puis 3)
        list[0].Code.Should().Be("L1");
        list[1].Code.Should().Be("L2");
        list[2].Code.Should().Be("L3");
    }

    [Fact]
    public async Task GetAll_DoitIsolerNiveauxEntreEtablissements()
    {
        // Arrange : Ségrégation multi-tenant
        var etablissement1 = await CreerEtablissementAsync("Univ Antananarivo");
        var etablissement2 = await CreerEtablissementAsync("Univ Fianarantsoa");

        await CreerNiveauAsync(etablissement1, "NIV_TANA", "Niveau Tana", 1);
        await CreerNiveauAsync(etablissement2, "NIV_FIANAR", "Niveau Fianar", 1);

        using var message1 = new HttpRequestMessage(HttpMethod.Get, "/api/niveaux");
        message1.Headers.Add("X-EtablissementId", etablissement1.ToString());

        // Act
        var response1 = await _http.SendAsync(message1);

        // Assert : Seul le niveau du tenant 1 est renvoyé
        response1.StatusCode.Should().Be(HttpStatusCode.OK);
        var list1 = await response1.Content.ReadFromJsonAsync<List<GetAllNiveauResponse>>(_jsonOptions);
        list1.Should().ContainSingle(n => n.Code == "NIV_TANA");
        list1.Should().NotContain(n => n.Code == "NIV_FIANAR");
    }
}

