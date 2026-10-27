using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using erp_univ_back.Features.Etablissement;
using erp_univ_back.Features.Etablissement.Create;
using erp_univ_back.Features.Etudiant;
using erp_univ_back.Features.Etudiant.Create;
using erp_univ_back.Shared.Data;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

namespace erp_univ_back.Tests.Features.Etudiant.Create;

public class CreateEtudiantIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _http;
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    public CreateEtudiantIntegrationTests(WebApplicationFactory<Program> factory)
    {
        var dbName = "DbTest_Etudiant_Create_" + Guid.NewGuid().ToString();

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

    private async Task<Guid> CreerEtablissementAsync(string nom = "Univ Etudiant")
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

    private CreateEtudiantRequest GetValidRequest() => new(
        Nom: "Rakoto",
        Prenom: "Jean",
        DateNaissance: new DateOnly(2005, 5, 12),
        LieuNaissance: "Antananarivo",
        NumeroTelephone: "+261341234567",
        Email: "jean.rakoto@univ.mg",
        Parents:
        [
            new CreateParentDto(
                Nom: "Rakoto",
                Prenom: "Pierre",
                NumeroTelephone: "+261331122334",
                Profession: "Enseignant",
                Role: Role.Pere,
                Email: "pierre.rakoto@parent.mg"
            )
        ]
    );

    [Fact]
    public async Task Post_DoitRetourner201Created_QuandRequeteEstValide()
    {
        // Arrange : Multi-tenant context
        var etablissementId = await CreerEtablissementAsync();
        var request = GetValidRequest();

        using var message = new HttpRequestMessage(HttpMethod.Post, "/api/etudiants");
        message.Headers.Add("X-EtablissementId", etablissementId.ToString());
        message.Content = JsonContent.Create(request);

        // Act
        var response = await _http.SendAsync(message);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var result = await response.Content.ReadFromJsonAsync<CreateEtudiantResponse>(_jsonOptions);
        result.Should().NotBeNull();
        result!.Id.Should().NotBeEmpty();
        result.Matricule.Should().StartWith("ETU-");
    }

    [Fact]
    public async Task Post_DoitRetourner400BadRequest_QuandNomEstVide()
    {
        // Arrange
        var etablissementId = await CreerEtablissementAsync();
        var invalidRequest = GetValidRequest() with { Nom = string.Empty };

        using var message = new HttpRequestMessage(HttpMethod.Post, "/api/etudiants");
        message.Headers.Add("X-EtablissementId", etablissementId.ToString());
        message.Content = JsonContent.Create(invalidRequest);

        // Act
        var response = await _http.SendAsync(message);

        // Assert : Interception par FluentValidation avant le service
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Post_DoitRetourner409Conflict_QuandTelephoneOuEmailExisteDeja()
    {
        // Arrange
        var etablissementId = await CreerEtablissementAsync();
        var request = GetValidRequest();

        // 1ère création réussie
        using var firstMsg = new HttpRequestMessage(HttpMethod.Post, "/api/etudiants");
        firstMsg.Headers.Add("X-EtablissementId", etablissementId.ToString());
        firstMsg.Content = JsonContent.Create(request);
        var firstResponse = await _http.SendAsync(firstMsg);
        firstResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        // 2ème création avec le même numéro de téléphone
        var duplicatePhoneRequest = request with { Email = "autre.email@univ.mg" };
        using var dupMsg = new HttpRequestMessage(HttpMethod.Post, "/api/etudiants");
        dupMsg.Headers.Add("X-EtablissementId", etablissementId.ToString());
        dupMsg.Content = JsonContent.Create(duplicatePhoneRequest);

        // Act
        var duplicateResponse = await _http.SendAsync(dupMsg);

        // Assert
        duplicateResponse.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Post_DoitAccepterMemeEmailOuTelephone_PourDeuxEtablissementsDifferents()
    {
        // Arrange : Deux établissements distincts
        var etablissement1 = await CreerEtablissementAsync("Univ 1");
        var etablissement2 = await CreerEtablissementAsync("Univ 2");
        var request = GetValidRequest();

        // Act Tenant 1
        using var msg1 = new HttpRequestMessage(HttpMethod.Post, "/api/etudiants");
        msg1.Headers.Add("X-EtablissementId", etablissement1.ToString());
        msg1.Content = JsonContent.Create(request);
        var resp1 = await _http.SendAsync(msg1);

        // Act Tenant 2
        using var msg2 = new HttpRequestMessage(HttpMethod.Post, "/api/etudiants");
        msg2.Headers.Add("X-EtablissementId", etablissement2.ToString());
        msg2.Content = JsonContent.Create(request);
        var resp2 = await _http.SendAsync(msg2);

        // Assert : Étanchéité multi-tenant
        resp1.StatusCode.Should().Be(HttpStatusCode.Created);
        resp2.StatusCode.Should().Be(HttpStatusCode.Created);
    }
}