using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using erp_univ_back.Features.Etablissement;
using erp_univ_back.Features.Etablissement.Create;
using erp_univ_back.Features.Professeur.Create;
using erp_univ_back.Features.Professeur.GetById;
using erp_univ_back.Shared.Data;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

namespace erp_univ_back.Tests.Features.Professeur.GetById;

public class GetProfesseurByIdIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _http;
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    public GetProfesseurByIdIntegrationTests(WebApplicationFactory<Program> factory)
    {
        var dbName = "DbTest_Professeur_GetById_" + Guid.NewGuid().ToString();

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

    private async Task<Guid> CreerEtablissementAsync(string nom = "Univ Professeur GetById")
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
            Prenom: "TestPrenom",
            DateNaissance: new DateOnly(1978, 11, 23),
            LieuNaissance: "Antsiranana",
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
    public async Task GetById_DoitRetourner200OK_QuandProfesseurExiste()
    {
        // Arrange
        var etablissementId = await CreerEtablissementAsync();
        var prof = await CreerProfesseurAsync(etablissementId, "Ramaroson", "+261345566778", "ramaroson@univ.mg", "101234123456");

        using var message = new HttpRequestMessage(HttpMethod.Get, $"/api/professeurs/{prof.Id}");
        message.Headers.Add("X-EtablissementId", etablissementId.ToString());

        // Act
        var response = await _http.SendAsync(message);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<GetProfesseurByIdResponse>(_jsonOptions);
        result.Should().NotBeNull();
        result!.Id.Should().Be(prof.Id);
        result.Nom.Should().Be("Ramaroson");
        result.Email.Should().Be("ramaroson@univ.mg");
        result.NumeroTelephone.Should().Be("+261345566778");
        result.NumeroCin.Should().Be("101234123456");
    }

    [Fact]
    public async Task GetById_DoitRetourner404NotFound_QuandProfesseurInexistant()
    {
        // Arrange
        var etablissementId = await CreerEtablissementAsync();

        using var message = new HttpRequestMessage(HttpMethod.Get, $"/api/professeurs/{Guid.NewGuid()}");
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

        var profEtab1 = await CreerProfesseurAsync(etablissement1, "ProfPrive", "+261346677889", "prive@univ.mg", "101987654321");

        // Act : Tentative d'accès depuis le tenant 2
        using var message = new HttpRequestMessage(HttpMethod.Get, $"/api/professeurs/{profEtab1.Id}");
        message.Headers.Add("X-EtablissementId", etablissement2.ToString());

        var response = await _http.SendAsync(message);

        // Assert : Invisible pour le tenant 2
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}

