using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using erp_univ_back.Features.Etablissement;
using erp_univ_back.Features.Etablissement.Create;
using erp_univ_back.Features.Professeur.Create;
using erp_univ_back.Shared.Data;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

namespace erp_univ_back.Tests.Features.Professeur.Create;

public class CreateProfesseurIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _http;
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    public CreateProfesseurIntegrationTests(WebApplicationFactory<Program> factory)
    {
        var dbName = "DbTest_Professeur_Create_" + Guid.NewGuid().ToString();

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

    private async Task<Guid> CreerEtablissementAsync(string nom = "Univ Professeur Create")
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

    private CreateProfesseurRequest GetValidRequest() => new(
        Nom: "Rabemananjara",
        Prenom: "Faly",
        DateNaissance: new DateOnly(1975, 6, 12),
        LieuNaissance: "Toamasina",
        NumeroTelephone: "+26132" + Random.Shared.Next(10000000, 99999999),
        Email: $"prof_{Guid.NewGuid():N}@univ.mg",
        NumeroCin: "10" + Random.Shared.NextInt64(1000000000, 9999999999)
    );

    [Fact]
    public async Task Post_DoitRetourner201Created_QuandRequeteEstValide()
    {
        // Arrange
        var etablissementId = await CreerEtablissementAsync();
        var request = GetValidRequest();

        using var message = new HttpRequestMessage(HttpMethod.Post, "/api/professeurs");
        message.Headers.Add("X-EtablissementId", etablissementId.ToString());
        message.Content = JsonContent.Create(request);

        // Act
        var response = await _http.SendAsync(message);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var created = await response.Content.ReadFromJsonAsync<CreateProfesseurResponse>(_jsonOptions);
        created.Should().NotBeNull();
        created!.Id.Should().NotBeEmpty();
        created.Nom.Should().Be(request.Nom);
        created.Prenom.Should().Be(request.Prenom);
        created.Email.Should().Be(request.Email);
        created.NumeroTelephone.Should().Be(request.NumeroTelephone);
        created.NumeroCin.Should().Be(request.NumeroCin);
        created.Matricule.Should().StartWith("PROF-");
    }

    [Fact]
    public async Task Post_DoitRetourner400BadRequest_QuandChampObligatoireManquant()
    {
        // Arrange
        var etablissementId = await CreerEtablissementAsync();
        var request = GetValidRequest() with { Nom = "" };

        using var message = new HttpRequestMessage(HttpMethod.Post, "/api/professeurs");
        message.Headers.Add("X-EtablissementId", etablissementId.ToString());
        message.Content = JsonContent.Create(request);

        // Act
        var response = await _http.SendAsync(message);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Post_DoitRetourner409Conflict_QuandEmailExisteDeja()
    {
        // Arrange
        var etablissementId = await CreerEtablissementAsync();
        var req1 = GetValidRequest();

        using var msg1 = new HttpRequestMessage(HttpMethod.Post, "/api/professeurs");
        msg1.Headers.Add("X-EtablissementId", etablissementId.ToString());
        msg1.Content = JsonContent.Create(req1);
        var res1 = await _http.SendAsync(msg1);
        res1.StatusCode.Should().Be(HttpStatusCode.Created);

        // Act : Même email
        var req2 = GetValidRequest() with
        {
            Email = req1.Email,
            NumeroTelephone = "+261330000000",
            NumeroCin = "999999999999"
        };

        using var msg2 = new HttpRequestMessage(HttpMethod.Post, "/api/professeurs");
        msg2.Headers.Add("X-EtablissementId", etablissementId.ToString());
        msg2.Content = JsonContent.Create(req2);
        var res2 = await _http.SendAsync(msg2);

        // Assert
        res2.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Post_DoitRetourner409Conflict_QuandTelephoneExisteDeja()
    {
        // Arrange
        var etablissementId = await CreerEtablissementAsync();
        var req1 = GetValidRequest();

        using var msg1 = new HttpRequestMessage(HttpMethod.Post, "/api/professeurs");
        msg1.Headers.Add("X-EtablissementId", etablissementId.ToString());
        msg1.Content = JsonContent.Create(req1);
        var res1 = await _http.SendAsync(msg1);
        res1.StatusCode.Should().Be(HttpStatusCode.Created);

        // Act : Même numéro de téléphone
        var req2 = GetValidRequest() with
        {
            NumeroTelephone = req1.NumeroTelephone,
            Email = "autre_email@univ.mg",
            NumeroCin = "888888888888"
        };

        using var msg2 = new HttpRequestMessage(HttpMethod.Post, "/api/professeurs");
        msg2.Headers.Add("X-EtablissementId", etablissementId.ToString());
        msg2.Content = JsonContent.Create(req2);
        var res2 = await _http.SendAsync(msg2);

        // Assert
        res2.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Post_DoitRetourner409Conflict_QuandCinExisteDeja()
    {
        // Arrange
        var etablissementId = await CreerEtablissementAsync();
        var req1 = GetValidRequest();

        using var msg1 = new HttpRequestMessage(HttpMethod.Post, "/api/professeurs");
        msg1.Headers.Add("X-EtablissementId", etablissementId.ToString());
        msg1.Content = JsonContent.Create(req1);
        var res1 = await _http.SendAsync(msg1);
        res1.StatusCode.Should().Be(HttpStatusCode.Created);

        // Act : Même numéro CIN
        var req2 = GetValidRequest() with
        {
            NumeroCin = req1.NumeroCin,
            Email = "prof2@univ.mg",
            NumeroTelephone = "+261329988776"
        };

        using var msg2 = new HttpRequestMessage(HttpMethod.Post, "/api/professeurs");
        msg2.Headers.Add("X-EtablissementId", etablissementId.ToString());
        msg2.Content = JsonContent.Create(req2);
        var res2 = await _http.SendAsync(msg2);

        // Assert
        res2.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }
}

