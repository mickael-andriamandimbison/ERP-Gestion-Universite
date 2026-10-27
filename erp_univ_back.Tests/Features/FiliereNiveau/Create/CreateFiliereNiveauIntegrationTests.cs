using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using erp_univ_back.Features.Etablissement;
using erp_univ_back.Features.Etablissement.Create;
using erp_univ_back.Features.Filiere.Create;
using erp_univ_back.Features.FiliereNiveau.Create;
using erp_univ_back.Features.Niveau.Create;
using erp_univ_back.Shared.Data;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

namespace erp_univ_back.Tests.Features.FiliereNiveau.Create;

public class CreateFiliereNiveauIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _http;
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    public CreateFiliereNiveauIntegrationTests(WebApplicationFactory<Program> factory)
    {
        var dbName = "DbTest_FiliereNiveau_Create_" + Guid.NewGuid().ToString();

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

    private async Task<Guid> CreerEtablissementAsync(string nom = "Univ FiliereNiveau")
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

    private async Task<CreateFiliereResponse> CreerFiliereAsync(Guid etablissementId, string code = "INFO", string nom = "Informatique")
    {
        var request = new CreateFiliereRequest(code, nom, "Description test");

        using var message = new HttpRequestMessage(HttpMethod.Post, "/api/filieres");
        message.Headers.Add("X-EtablissementId", etablissementId.ToString());
        message.Content = JsonContent.Create(request);

        var response = await _http.SendAsync(message);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await response.Content.ReadFromJsonAsync<CreateFiliereResponse>(_jsonOptions);
        return created!;
    }

    private async Task<CreateNiveauResponse> CreerNiveauAsync(Guid etablissementId, string code = "L1", string nom = "Licence 1", int ordre = 1)
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
    public async Task Post_DoitRetourner201Created_QuandRequeteEstValide()
    {
        // Arrange
        var etablissementId = await CreerEtablissementAsync();
        var filiere = await CreerFiliereAsync(etablissementId, "GL", "Genie Logiciel");
        var niveau = await CreerNiveauAsync(etablissementId, "L3", "Licence 3", 3);

        var request = new CreateFiliereNiveauRequest(filiere.Id, niveau.Id);

        using var message = new HttpRequestMessage(HttpMethod.Post, "/api/filiere-niveaux");
        message.Headers.Add("X-EtablissementId", etablissementId.ToString());
        message.Content = JsonContent.Create(request);

        // Act
        var response = await _http.SendAsync(message);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var created = await response.Content.ReadFromJsonAsync<CreateFiliereNiveauResponse>(_jsonOptions);
        created.Should().NotBeNull();
        created!.Id.Should().NotBeEmpty();
        created.FiliereId.Should().Be(filiere.Id);
        created.FiliereNom.Should().Be("Genie Logiciel");
        created.NiveauId.Should().Be(niveau.Id);
        created.NiveauNom.Should().Be("Licence 3");
        created.LibelleAffiche.Should().Be("Genie Logiciel-L3");
    }

    [Fact]
    public async Task Post_DoitRetourner400BadRequest_QuandChampsObligatoiresVides()
    {
        // Arrange
        var etablissementId = await CreerEtablissementAsync();
        var request = new CreateFiliereNiveauRequest(Guid.Empty, Guid.Empty);

        using var message = new HttpRequestMessage(HttpMethod.Post, "/api/filiere-niveaux");
        message.Headers.Add("X-EtablissementId", etablissementId.ToString());
        message.Content = JsonContent.Create(request);

        // Act
        var response = await _http.SendAsync(message);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Post_DoitRetourner404NotFound_QuandFiliereInexistante()
    {
        // Arrange
        var etablissementId = await CreerEtablissementAsync();
        var niveau = await CreerNiveauAsync(etablissementId, "M1", "Master 1", 4);
        var request = new CreateFiliereNiveauRequest(Guid.NewGuid(), niveau.Id);

        using var message = new HttpRequestMessage(HttpMethod.Post, "/api/filiere-niveaux");
        message.Headers.Add("X-EtablissementId", etablissementId.ToString());
        message.Content = JsonContent.Create(request);

        // Act
        var response = await _http.SendAsync(message);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Post_DoitRetourner404NotFound_QuandNiveauInexistant()
    {
        // Arrange
        var etablissementId = await CreerEtablissementAsync();
        var filiere = await CreerFiliereAsync(etablissementId, "RT", "Reseaux et Telecoms");
        var request = new CreateFiliereNiveauRequest(filiere.Id, Guid.NewGuid());

        using var message = new HttpRequestMessage(HttpMethod.Post, "/api/filiere-niveaux");
        message.Headers.Add("X-EtablissementId", etablissementId.ToString());
        message.Content = JsonContent.Create(request);

        // Act
        var response = await _http.SendAsync(message);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Post_DoitRetourner409Conflict_QuandAssociationExisteDeja()
    {
        // Arrange
        var etablissementId = await CreerEtablissementAsync();
        var filiere = await CreerFiliereAsync(etablissementId, "MED", "Medecine");
        var niveau = await CreerNiveauAsync(etablissementId, "P1", "Premiere Annee", 1);

        var request = new CreateFiliereNiveauRequest(filiere.Id, niveau.Id);

        using var firstMsg = new HttpRequestMessage(HttpMethod.Post, "/api/filiere-niveaux");
        firstMsg.Headers.Add("X-EtablissementId", etablissementId.ToString());
        firstMsg.Content = JsonContent.Create(request);
        var firstResponse = await _http.SendAsync(firstMsg);
        firstResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        // Act : Deuxième tentative d'association identique
        using var secondMsg = new HttpRequestMessage(HttpMethod.Post, "/api/filiere-niveaux");
        secondMsg.Headers.Add("X-EtablissementId", etablissementId.ToString());
        secondMsg.Content = JsonContent.Create(request);
        var secondResponse = await _http.SendAsync(secondMsg);

        // Assert
        secondResponse.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Post_DoitRetourner404NotFound_QuandFiliereAppartientAUnAutreEtablissement()
    {
        // Arrange : Multi-tenant test
        var etablissement1 = await CreerEtablissementAsync("Univ 1");
        var etablissement2 = await CreerEtablissementAsync("Univ 2");

        var filiereEtab1 = await CreerFiliereAsync(etablissement1, "FIL1", "Filiere Etab 1");
        var niveauEtab2 = await CreerNiveauAsync(etablissement2, "NIV2", "Niveau Etab 2", 1);

        // Tentative de lier une filière d'Etab 1 dans l'Etab 2
        var request = new CreateFiliereNiveauRequest(filiereEtab1.Id, niveauEtab2.Id);

        using var message = new HttpRequestMessage(HttpMethod.Post, "/api/filiere-niveaux");
        message.Headers.Add("X-EtablissementId", etablissement2.ToString());
        message.Content = JsonContent.Create(request);

        // Act
        var response = await _http.SendAsync(message);

        // Assert : La filière d'Etab 1 est invisible pour Etab 2 -> 404
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}

