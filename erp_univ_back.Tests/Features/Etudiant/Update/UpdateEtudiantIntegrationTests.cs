using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using erp_univ_back.Features.Etablissement;
using erp_univ_back.Features.Etablissement.Create;
using erp_univ_back.Features.Etudiant.Create;
using erp_univ_back.Features.Etudiant.Update;
using erp_univ_back.Shared.Data;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

namespace erp_univ_back.Tests.Features.Etudiant.Update;

public class UpdateEtudiantIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _http;
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    public UpdateEtudiantIntegrationTests(WebApplicationFactory<Program> factory)
    {
        var dbName = "DbTest_Etudiant_Update_" + Guid.NewGuid().ToString();

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

    private async Task<Guid> CreerEtablissementAsync(string nom = "Univ Etudiant Update")
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
            DateNaissance: new DateOnly(2003, 1, 1),
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
    public async Task Put_DoitRetourner200OK_QuandRequeteEstValide()
    {
        // Arrange
        var etablissementId = await CreerEtablissementAsync();
        var etudiant = await CreerEtudiantAsync(etablissementId, "Rabe", "+261341112233", "rabe@univ.mg");

        var updateRequest = new UpdateEtudiantRequest(
            Nom: "Rabe Modifie",
            Prenom: "Paul",
            DateNaissance: new DateOnly(2003, 2, 2),
            LieuNaissance: "Toamasina",
            NumeroCin: "101999888777",
            NumeroTelephone: "+261349998877",
            Email: "rabe.modifie@univ.mg"
        );

        using var message = new HttpRequestMessage(HttpMethod.Put, $"/api/etudiants/{etudiant.Id}");
        message.Headers.Add("X-EtablissementId", etablissementId.ToString());
        message.Content = JsonContent.Create(updateRequest);

        // Act
        var response = await _http.SendAsync(message);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<UpdateEtudiantResponse>(_jsonOptions);
        result.Should().NotBeNull();
        result!.Id.Should().Be(etudiant.Id);
        result.Nom.Should().Be("Rabe Modifie");
        result.NumeroTelephone.Should().Be("+261349998877");
        result.DateModification.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task Put_DoitRetourner400BadRequest_QuandValidationEchoue()
    {
        // Arrange
        var etablissementId = await CreerEtablissementAsync();
        var etudiant = await CreerEtudiantAsync(etablissementId, "Rasoa", "+261342223344", "rasoa@univ.mg");

        var invalidRequest = new UpdateEtudiantRequest(
            Nom: "Rasoa123", // Chiffres interdits
            Prenom: "Marie",
            DateNaissance: new DateOnly(2003, 1, 1),
            LieuNaissance: "Antsirabe",
            NumeroCin: null,
            NumeroTelephone: "+261342223344",
            Email: "rasoa@univ.mg"
        );

        using var message = new HttpRequestMessage(HttpMethod.Put, $"/api/etudiants/{etudiant.Id}");
        message.Headers.Add("X-EtablissementId", etablissementId.ToString());
        message.Content = JsonContent.Create(invalidRequest);

        // Act
        var response = await _http.SendAsync(message);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Put_DoitRetourner404NotFound_QuandEtudiantInexistant()
    {
        // Arrange
        var etablissementId = await CreerEtablissementAsync();
        var updateRequest = new UpdateEtudiantRequest(
            Nom: "Inconnu",
            Prenom: "Test",
            DateNaissance: new DateOnly(2000, 1, 1),
            LieuNaissance: "Inconnu",
            NumeroCin: null,
            NumeroTelephone: "+261340000000",
            Email: "inconnu@univ.mg"
        );

        using var message = new HttpRequestMessage(HttpMethod.Put, $"/api/etudiants/{Guid.NewGuid()}");
        message.Headers.Add("X-EtablissementId", etablissementId.ToString());
        message.Content = JsonContent.Create(updateRequest);

        // Act
        var response = await _http.SendAsync(message);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Put_DoitRetourner409Conflict_QuandTelephoneAppartientADejaUnAutreEtudiant()
    {
        // Arrange : Deux étudiants créés pour le même établissement
        var etablissementId = await CreerEtablissementAsync();
        var e1 = await CreerEtudiantAsync(etablissementId, "EtudiantAlpha", "+261340000001", "etu1@univ.mg");
        var e2 = await CreerEtudiantAsync(etablissementId, "EtudiantBeta", "+261340000002", "etu2@univ.mg");

        // Tenter d'attribuer le numéro de e1 à e2
        var conflictRequest = new UpdateEtudiantRequest(
            Nom: "EtudiantBeta",
            Prenom: "Test",
            DateNaissance: new DateOnly(2003, 1, 1),
            LieuNaissance: "Antananarivo",
            NumeroCin: null,
            NumeroTelephone: "+261340000001", // Déjà pris par e1
            Email: "etu2_nouveau@univ.mg"
        );

        using var message = new HttpRequestMessage(HttpMethod.Put, $"/api/etudiants/{e2.Id}");
        message.Headers.Add("X-EtablissementId", etablissementId.ToString());
        message.Content = JsonContent.Create(conflictRequest);

        // Act
        var response = await _http.SendAsync(message);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Put_DoitRetourner404NotFound_QuandTentativeModificationSurAutreEtablissement()
    {
        // Arrange : Sécurité multi-tenant
        var etablissement1 = await CreerEtablissementAsync("Univ A");
        var etablissement2 = await CreerEtablissementAsync("Univ B");

        var etudiantEtab1 = await CreerEtudiantAsync(etablissement1, "EtudiantEtab", "+261345556677", "etu.etab1@univ.mg");

        var updateRequest = new UpdateEtudiantRequest(
            Nom: "EtudiantEtab",
            Prenom: "Hacker",
            DateNaissance: new DateOnly(2003, 1, 1),
            LieuNaissance: "Antananarivo",
            NumeroCin: null,
            NumeroTelephone: "+261345556677",
            Email: "etu.etab1@univ.mg"
        );

        using var message = new HttpRequestMessage(HttpMethod.Put, $"/api/etudiants/{etudiantEtab1.Id}");
        message.Headers.Add("X-EtablissementId", etablissement2.ToString()); // Mauvais tenant
        message.Content = JsonContent.Create(updateRequest);

        // Act
        var response = await _http.SendAsync(message);

        // Assert : Invisible pour l'établissement 2
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
