namespace erp_univ_back.Tests.Features.Inscription.Register;

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using erp_univ_back.Features.AnneeAcademique.Create;
using erp_univ_back.Features.Etablissement;
using erp_univ_back.Features.Etablissement.Create;
using erp_univ_back.Features.Etudiant;
using erp_univ_back.Features.Etudiant.Create;
using erp_univ_back.Features.Filiere.Create;
using erp_univ_back.Features.FiliereNiveau.Create;
using erp_univ_back.Features.Inscription;
using erp_univ_back.Features.Inscription.Register;
using erp_univ_back.Features.Niveau.Create;
using erp_univ_back.Shared.Data;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

public class RegisterInscriptionIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _http;
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    public RegisterInscriptionIntegrationTests(WebApplicationFactory<Program> factory)
    {
        var dbName = "DbTest_Inscription_Register_" + Guid.NewGuid().ToString();

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

    private async Task<Guid> CreerEtablissementAsync(string nom = "Univ Register Inscription")
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

    private async Task<CreateFiliereNiveauResponse> CreerClasseAsync(Guid etablissementId, string filiereCode, string niveauCode)
    {
        var filRequest = new CreateFiliereRequest(filiereCode, $"Filiere {filiereCode}");
        using var filMsg = new HttpRequestMessage(HttpMethod.Post, "/api/filieres");
        filMsg.Headers.Add("X-EtablissementId", etablissementId.ToString());
        filMsg.Content = JsonContent.Create(filRequest);
        var filRes = await _http.SendAsync(filMsg);
        var fil = await filRes.Content.ReadFromJsonAsync<CreateFiliereResponse>(_jsonOptions);

        var nivRequest = new CreateNiveauRequest(niveauCode, $"Niveau {niveauCode}", 1);
        using var nivMsg = new HttpRequestMessage(HttpMethod.Post, "/api/niveaux");
        nivMsg.Headers.Add("X-EtablissementId", etablissementId.ToString());
        nivMsg.Content = JsonContent.Create(nivRequest);
        var nivRes = await _http.SendAsync(nivMsg);
        var niv = await nivRes.Content.ReadFromJsonAsync<CreateNiveauResponse>(_jsonOptions);

        var fnRequest = new CreateFiliereNiveauRequest(fil!.Id, niv!.Id);
        using var fnMsg = new HttpRequestMessage(HttpMethod.Post, "/api/filiere-niveaux");
        fnMsg.Headers.Add("X-EtablissementId", etablissementId.ToString());
        fnMsg.Content = JsonContent.Create(fnRequest);
        var fnRes = await _http.SendAsync(fnMsg);
        var fn = await fnRes.Content.ReadFromJsonAsync<CreateFiliereNiveauResponse>(_jsonOptions);
        return fn!;
    }

    private async Task<CreateAnneeAcademiqueResponse> CreerAnneeAcademiqueAsync(Guid etablissementId, string libelle, bool estActive = true)
    {
        var parts = libelle.Split('-');
        var startYear = int.Parse(parts[0].Trim());
        var endYear = int.Parse(parts[1].Trim());

        var request = new CreateAnneeAcademiqueRequest(
            Libelle: libelle,
            DateDebut: new DateOnly(startYear, 9, 1),
            DateFin: new DateOnly(endYear, 6, 30),
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

    private CreateEtudiantRequest GetValidEtudiantRequest(string nom = "Rakoto") => new(
        Nom: nom,
        Prenom: "Faly",
        DateNaissance: new DateOnly(2004, 3, 10),
        LieuNaissance: "Antananarivo",
        NumeroTelephone: "+26134" + Random.Shared.Next(10000000, 99999999),
        Email: $"etu_{Guid.NewGuid():N}@univ.mg",
        NumeroCin: "10" + Random.Shared.NextInt64(1000000000, 9999999999),
        Parents:
        [
            new CreateParentDto(
                Nom: "RakotoRay",
                Prenom: "Jean",
                NumeroTelephone: "+26133" + Random.Shared.Next(10000000, 99999999),
                Profession: "Ingénieur",
                Role: Role.Pere,
                Email: "parent@univ.mg"
            )
        ]
    );

    [Fact]
    public async Task Register_DoitRetourner201Created_QuandRequeteEstValide()
    {
        // Arrange
        var etablissementId = await CreerEtablissementAsync();
        var classe = await CreerClasseAsync(etablissementId, "INFO", "L1");
        var annee = await CreerAnneeAcademiqueAsync(etablissementId, "2025-2026", estActive: true);

        var etudiantInfos = GetValidEtudiantRequest("Rabenandrasana");
        var request = new RegisterInscriptionRequest(etudiantInfos, classe.Id, annee.Id);

        using var message = new HttpRequestMessage(HttpMethod.Post, "/api/inscriptions/register");
        message.Headers.Add("X-EtablissementId", etablissementId.ToString());
        message.Content = JsonContent.Create(request);

        // Act
        var response = await _http.SendAsync(message);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var created = await response.Content.ReadFromJsonAsync<RegisterInscriptionResponse>(_jsonOptions);
        created.Should().NotBeNull();
        created!.Id.Should().NotBeEmpty();
        created.EtudiantId.Should().NotBeEmpty();
        created.EtudiantNom.Should().Be("Rabenandrasana");
        created.EtudiantMatricule.Should().StartWith("ETU-");
        created.FiliereNiveauId.Should().Be(classe.Id);
        created.AnneeAcademiqueId.Should().Be(annee.Id);
        created.Statut.Should().Be(StatutInscription.Inscrit);
    }

    [Fact]
    public async Task Register_DoitRetourner400BadRequest_QuandChampsObligatoiresVides()
    {
        // Arrange
        var etablissementId = await CreerEtablissementAsync();
        var request = new RegisterInscriptionRequest(null!, Guid.Empty, Guid.Empty);

        using var message = new HttpRequestMessage(HttpMethod.Post, "/api/inscriptions/register");
        message.Headers.Add("X-EtablissementId", etablissementId.ToString());
        message.Content = JsonContent.Create(request);

        // Act
        var response = await _http.SendAsync(message);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_DoitRetourner400BadRequest_QuandAnneeAcademiqueEstInactive()
    {
        // Arrange
        var etablissementId = await CreerEtablissementAsync();
        var classe = await CreerClasseAsync(etablissementId, "DROIT", "L1");
        var anneeInactive = await CreerAnneeAcademiqueAsync(etablissementId, "2021-2022", estActive: false);

        var etudiantInfos = GetValidEtudiantRequest("Razafindrakoto");
        var request = new RegisterInscriptionRequest(etudiantInfos, classe.Id, anneeInactive.Id);

        using var message = new HttpRequestMessage(HttpMethod.Post, "/api/inscriptions/register");
        message.Headers.Add("X-EtablissementId", etablissementId.ToString());
        message.Content = JsonContent.Create(request);

        // Act
        var response = await _http.SendAsync(message);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_DoitRetourner404NotFound_QuandClasseInexistante()
    {
        // Arrange
        var etablissementId = await CreerEtablissementAsync();
        var annee = await CreerAnneeAcademiqueAsync(etablissementId, "2025-2026", estActive: true);

        var etudiantInfos = GetValidEtudiantRequest("Randrianarisoa");
        var request = new RegisterInscriptionRequest(etudiantInfos, Guid.NewGuid(), annee.Id);

        using var message = new HttpRequestMessage(HttpMethod.Post, "/api/inscriptions/register");
        message.Headers.Add("X-EtablissementId", etablissementId.ToString());
        message.Content = JsonContent.Create(request);

        // Act
        var response = await _http.SendAsync(message);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Register_DoitRetourner409Conflict_QuandTelephoneEtudiantExisteDeja()
    {
        // Arrange
        var etablissementId = await CreerEtablissementAsync();
        var classe = await CreerClasseAsync(etablissementId, "AGRO", "L1");
        var annee = await CreerAnneeAcademiqueAsync(etablissementId, "2025-2026", estActive: true);

        var premierEtudiant = GetValidEtudiantRequest("PremierEtudiant");
        var premierRequest = new RegisterInscriptionRequest(premierEtudiant, classe.Id, annee.Id);

        using var msg1 = new HttpRequestMessage(HttpMethod.Post, "/api/inscriptions/register");
        msg1.Headers.Add("X-EtablissementId", etablissementId.ToString());
        msg1.Content = JsonContent.Create(premierRequest);
        var res1 = await _http.SendAsync(msg1);
        res1.StatusCode.Should().Be(HttpStatusCode.Created);

        // Act : Deuxième tentative avec le même numéro de téléphone
        var deuxiemeEtudiant = GetValidEtudiantRequest("DeuxiemeEtudiant") with
        {
            NumeroTelephone = premierEtudiant.NumeroTelephone,
            Email = "unique_email@univ.mg"
        };
        var deuxiemeRequest = new RegisterInscriptionRequest(deuxiemeEtudiant, classe.Id, annee.Id);

        using var msg2 = new HttpRequestMessage(HttpMethod.Post, "/api/inscriptions/register");
        msg2.Headers.Add("X-EtablissementId", etablissementId.ToString());
        msg2.Content = JsonContent.Create(deuxiemeRequest);
        var res2 = await _http.SendAsync(msg2);

        // Assert
        res2.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Register_DoitRetourner404NotFound_QuandClasseAppartientAUnAutreEtablissement()
    {
        // Arrange : Multi-tenant test
        var etablissement1 = await CreerEtablissementAsync("Univ 1");
        var etablissement2 = await CreerEtablissementAsync("Univ 2");

        var classeEtab1 = await CreerClasseAsync(etablissement1, "MED", "P1");
        var anneeEtab2 = await CreerAnneeAcademiqueAsync(etablissement2, "2025-2026", estActive: true);

        // Inscription tentée dans l'établissement 2 avec une classe de l'établissement 1
        var etudiantInfos = GetValidEtudiantRequest("EtudiantTenantTest");
        var request = new RegisterInscriptionRequest(etudiantInfos, classeEtab1.Id, anneeEtab2.Id);

        using var message = new HttpRequestMessage(HttpMethod.Post, "/api/inscriptions/register");
        message.Headers.Add("X-EtablissementId", etablissement2.ToString());
        message.Content = JsonContent.Create(request);

        // Act
        var response = await _http.SendAsync(message);

        // Assert : La classe d'Etab 1 est invisible pour Etab 2 -> 404
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}

