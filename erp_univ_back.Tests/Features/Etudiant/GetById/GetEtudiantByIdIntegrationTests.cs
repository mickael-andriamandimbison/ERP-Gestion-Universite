using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using erp_univ_back.Features.Etablissement;
using erp_univ_back.Features.Etablissement.Create;
using erp_univ_back.Features.Etudiant.Create;
using erp_univ_back.Features.Etudiant.GetById;
using erp_univ_back.Shared.Data;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

namespace erp_univ_back.Tests.Features.Etudiant.GetById;

public class GetEtudiantByIdIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _http;
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    public GetEtudiantByIdIntegrationTests(WebApplicationFactory<Program> factory)
    {
        var dbName = "DbTest_Etudiant_GetById_" + Guid.NewGuid().ToString();

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

    private async Task<Guid> CreerEtablissementAsync(string nom = "Univ Etudiant GetById")
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
            Prenom: "Luc",
            DateNaissance: new DateOnly(2004, 7, 10),
            LieuNaissance: "Antsirabe",
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
    public async Task GetById_DoitRetourner200OK_QuandEtudiantExiste()
    {
        // Arrange
        var etablissementId = await CreerEtablissementAsync();
        var etudiant = await CreerEtudiantAsync(etablissementId, "Andria", "+261343334455", "andria@univ.mg");

        using var message = new HttpRequestMessage(HttpMethod.Get, $"/api/etudiants/{etudiant.Id}");
        message.Headers.Add("X-EtablissementId", etablissementId.ToString());

        // Act
        var response = await _http.SendAsync(message);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<GetEtudiantByIdResponse>(_jsonOptions);
        result.Should().NotBeNull();
        result!.Id.Should().Be(etudiant.Id);
        result.Nom.Should().Be("Andria");
        result.Matricule.Should().Be(etudiant.Matricule);
        result.Email.Should().Be("andria@univ.mg");
    }

    [Fact]
    public async Task GetById_DoitRetourner404NotFound_QuandEtudiantInexistant()
    {
        // Arrange
        var etablissementId = await CreerEtablissementAsync();
        using var message = new HttpRequestMessage(HttpMethod.Get, $"/api/etudiants/{Guid.NewGuid()}");
        message.Headers.Add("X-EtablissementId", etablissementId.ToString());

        // Act
        var response = await _http.SendAsync(message);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetById_DoitRetourner404NotFound_QuandConsultationDepuisAutreEtablissement()
    {
        // Arrange : Isolation multi-tenant
        var etablissement1 = await CreerEtablissementAsync("Univ 1");
        var etablissement2 = await CreerEtablissementAsync("Univ 2");

        var etudiantEtab1 = await CreerEtudiantAsync(etablissement1, "EtuSecret", "+261347778899", "secret@univ.mg");

        using var message = new HttpRequestMessage(HttpMethod.Get, $"/api/etudiants/{etudiantEtab1.Id}");
        message.Headers.Add("X-EtablissementId", etablissement2.ToString()); // Requête émise par le tenant 2

        // Act
        var response = await _http.SendAsync(message);

        // Assert : Non trouvé pour le tenant 2
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}

