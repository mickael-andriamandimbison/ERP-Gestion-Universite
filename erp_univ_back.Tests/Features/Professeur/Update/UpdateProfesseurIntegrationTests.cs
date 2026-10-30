using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using erp_univ_back.Features.Etablissement;
using erp_univ_back.Features.Etablissement.Create;
using erp_univ_back.Features.Professeur.Create;
using erp_univ_back.Features.Professeur.Update;
using erp_univ_back.Shared.Data;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

namespace erp_univ_back.Tests.Features.Professeur.Update;

public class UpdateProfesseurIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _http;
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    public UpdateProfesseurIntegrationTests(WebApplicationFactory<Program> factory)
    {
        var dbName = "DbTest_Professeur_Update_" + Guid.NewGuid().ToString();

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

    private async Task<Guid> CreerEtablissementAsync(string nom = "Univ Professeur Update")
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
            Prenom: "PrenomInitial",
            DateNaissance: new DateOnly(1985, 2, 20),
            LieuNaissance: "Fianarantsoa",
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
    public async Task Put_DoitRetourner200OK_QuandRequeteEstValide()
    {
        // Arrange
        var etablissementId = await CreerEtablissementAsync();
        var prof = await CreerProfesseurAsync(etablissementId, "Rasolofo", "+261341112233", "rasolofo@univ.mg", "101111222333");

        var updateRequest = new UpdateProfesseurRequest(
            Nom: "RasolofoModifie",
            Prenom: "NouveauPrenom",
            DateNaissance: new DateOnly(1984, 5, 10),
            LieuNaissance: "Majunga",
            NumeroTelephone: "+261349998877",
            Email: "rasolofo.nouveau@univ.mg",
            NumeroCin: "101999888777"
        );

        using var message = new HttpRequestMessage(HttpMethod.Put, $"/api/professeurs/{prof.Id}");
        message.Headers.Add("X-EtablissementId", etablissementId.ToString());
        message.Content = JsonContent.Create(updateRequest);

        // Act
        var response = await _http.SendAsync(message);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var updated = await response.Content.ReadFromJsonAsync<UpdateProfesseurResponse>(_jsonOptions);
        updated.Should().NotBeNull();
        updated!.Id.Should().Be(prof.Id);
        updated.Nom.Should().Be("RasolofoModifie");
        updated.Prenom.Should().Be("NouveauPrenom");
        updated.NumeroTelephone.Should().Be("+261349998877");
        updated.Email.Should().Be("rasolofo.nouveau@univ.mg");
        updated.NumeroCin.Should().Be("101999888777");
    }

    [Fact]
    public async Task Put_DoitRetourner400BadRequest_QuandChampObligatoireVide()
    {
        // Arrange
        var etablissementId = await CreerEtablissementAsync();
        var prof = await CreerProfesseurAsync(etablissementId, "Razafy", "+261342223344", "razafy@univ.mg", "102222333444");

        var updateRequest = new UpdateProfesseurRequest(
            Nom: "", // Invalide
            Prenom: "Valid",
            DateNaissance: new DateOnly(1980, 1, 1),
            LieuNaissance: "Tana",
            NumeroTelephone: "+261340001122",
            Email: "prof@univ.mg",
            NumeroCin: "103333444555"
        );

        using var message = new HttpRequestMessage(HttpMethod.Put, $"/api/professeurs/{prof.Id}");
        message.Headers.Add("X-EtablissementId", etablissementId.ToString());
        message.Content = JsonContent.Create(updateRequest);

        // Act
        var response = await _http.SendAsync(message);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Put_DoitRetourner404NotFound_QuandProfesseurInexistant()
    {
        // Arrange
        var etablissementId = await CreerEtablissementAsync();
        var updateRequest = new UpdateProfesseurRequest(
            Nom: "Inconnu",
            Prenom: "Prof",
            DateNaissance: new DateOnly(1980, 1, 1),
            LieuNaissance: "Tana",
            NumeroTelephone: "+261340001122",
            Email: "inconnu@univ.mg",
            NumeroCin: "104444555666"
        );

        using var message = new HttpRequestMessage(HttpMethod.Put, $"/api/professeurs/{Guid.NewGuid()}");
        message.Headers.Add("X-EtablissementId", etablissementId.ToString());
        message.Content = JsonContent.Create(updateRequest);

        // Act
        var response = await _http.SendAsync(message);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Put_DoitRetourner404NotFound_QuandModificationDepuisAutreEtablissement()
    {
        // Arrange : Multi-tenant test
        var etablissement1 = await CreerEtablissementAsync("Univ 1");
        var etablissement2 = await CreerEtablissementAsync("Univ 2");

        var profEtab1 = await CreerProfesseurAsync(etablissement1, "ProfUn", "+261341122445", "prof1@univ.mg", "105555666777");

        var updateRequest = new UpdateProfesseurRequest(
            Nom: "HackedName",
            Prenom: "Hacked",
            DateNaissance: new DateOnly(1980, 1, 1),
            LieuNaissance: "Tana",
            NumeroTelephone: "+261349999999",
            Email: "hacked@univ.mg",
            NumeroCin: "106666777888"
        );

        // Act : Tentative de modification depuis le tenant 2
        using var message = new HttpRequestMessage(HttpMethod.Put, $"/api/professeurs/{profEtab1.Id}");
        message.Headers.Add("X-EtablissementId", etablissement2.ToString());
        message.Content = JsonContent.Create(updateRequest);

        var response = await _http.SendAsync(message);

        // Assert : Invisible pour le tenant 2
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Put_DoitRetourner409Conflict_QuandEmailExisteChezAutreProfesseur()
    {
        // Arrange
        var etablissementId = await CreerEtablissementAsync();
        var prof1 = await CreerProfesseurAsync(etablissementId, "ProfAlpha", "+261340000001", "alpha@univ.mg", "107777888999");
        var prof2 = await CreerProfesseurAsync(etablissementId, "ProfBeta", "+261340000002", "beta@univ.mg", "108888999000");

        // Act : Tentative de mettre l'email de prof1 sur prof2
        var updateRequest = new UpdateProfesseurRequest(
            Nom: prof2.Nom,
            Prenom: prof2.Prenom,
            DateNaissance: new DateOnly(1980, 1, 1),
            LieuNaissance: "Toliara",
            NumeroTelephone: "+261340000003",
            Email: prof1.Email, // Conflit !
            NumeroCin: "109999000111"
        );

        using var message = new HttpRequestMessage(HttpMethod.Put, $"/api/professeurs/{prof2.Id}");
        message.Headers.Add("X-EtablissementId", etablissementId.ToString());
        message.Content = JsonContent.Create(updateRequest);

        var response = await _http.SendAsync(message);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }
}

