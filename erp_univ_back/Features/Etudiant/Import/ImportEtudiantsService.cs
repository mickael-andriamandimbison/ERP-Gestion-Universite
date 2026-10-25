using System.Globalization;
using System.Text;
using ClosedXML.Excel;
using CsvHelper;
using CsvHelper.Configuration;
using erp_univ_back.Features.Etudiant.Create;
using erp_univ_back.Shared.Data;
using erp_univ_back.Shared.Exceptions;
using Microsoft.AspNetCore.Http;
using ValidationException = erp_univ_back.Shared.Exceptions.ValidationException;

namespace erp_univ_back.Features.Etudiant.Import;

public interface IImportEtudiantsService
{
    Task<ImportEtudiantReport> ExecuteAsync(IFormFile file, CancellationToken cancellationToken = default);
}

public class ImportEtudiantsService(
    ICreateEtudiantService _createService,
    ErpUnivDbContext _db
) : IImportEtudiantsService
{
    public async Task<ImportEtudiantReport> ExecuteAsync(IFormFile file, CancellationToken cancellationToken = default)
    {
        if (file == null || file.Length == 0)
            throw new BadRequestException("Aucun fichier n'a été fourni ou le fichier est vide.");

        var extension = Path.GetExtension(file.FileName)?.ToLowerInvariant();

        List<ImportEtudiantLigneDto> lignes = extension switch
        {
            ".csv" => ExtraireCsv(file.OpenReadStream()),
            ".xlsx" or ".xls" => ExtraireExcel(file.OpenReadStream()),
            _ => throw new BadRequestException($"Le format '{extension}' n'est pas supporté. Veuillez importer un fichier .csv ou .xlsx.")
        };

        var erreurs = new List<ImportLigneErreur>();
        int succes = 0;
        int echecs = 0;

        for (int i = 0; i < lignes.Count; i++)
        {
            var ligne = lignes[i];
            int numeroLigne = i + 2;

            try
            {
                if (string.IsNullOrWhiteSpace(ligne.Nom))
                    throw new ValidationException("Le nom de l'étudiant est obligatoire.");

                if (string.IsNullOrWhiteSpace(ligne.Email))
                    throw new ValidationException("L'adresse email est obligatoire.");

                if (string.IsNullOrWhiteSpace(ligne.NumeroTelephone))
                    throw new ValidationException("Le numéro de téléphone est obligatoire.");

                // Reconstruction des parents
                var parents = new List<CreateParentDto>();

                if (!string.IsNullOrWhiteSpace(ligne.NomPere) || !string.IsNullOrWhiteSpace(ligne.TelephonePere))
                {
                    parents.Add(new CreateParentDto(
                        Nom: !string.IsNullOrWhiteSpace(ligne.NomPere) ? ligne.NomPere.Trim() : "Non renseigné",
                        Prenom: null,
                        NumeroTelephone: !string.IsNullOrWhiteSpace(ligne.TelephonePere) ? ligne.TelephonePere.Trim() : "Non renseigné",
                        Profession: !string.IsNullOrWhiteSpace(ligne.ProfessionPere) ? ligne.ProfessionPere.Trim() : "Non renseignée",
                        Role: Role.Pere,
                        Email: !string.IsNullOrWhiteSpace(ligne.EmailPere) ? ligne.EmailPere.Trim() : null
                    ));
                }

                if (!string.IsNullOrWhiteSpace(ligne.NomMere) || !string.IsNullOrWhiteSpace(ligne.TelephoneMere))
                {
                    parents.Add(new CreateParentDto(
                        Nom: !string.IsNullOrWhiteSpace(ligne.NomMere) ? ligne.NomMere.Trim() : "Non renseigné",
                        Prenom: null,
                        NumeroTelephone: !string.IsNullOrWhiteSpace(ligne.TelephoneMere) ? ligne.TelephoneMere.Trim() : "Non renseigné",
                        Profession: !string.IsNullOrWhiteSpace(ligne.ProfessionMere) ? ligne.ProfessionMere.Trim() : "Non renseignée",
                        Role: Role.Mere,
                        Email: !string.IsNullOrWhiteSpace(ligne.EmailMere) ? ligne.EmailMere.Trim() : null
                    ));
                }

                var createRequest = new CreateEtudiantRequest(
                    Nom: ligne.Nom.Trim(),
                    Prenom: string.IsNullOrWhiteSpace(ligne.Prenom) ? null : ligne.Prenom.Trim(),
                    DateNaissance: ligne.DateNaissance ?? new DateOnly(2000, 1, 1),
                    LieuNaissance: !string.IsNullOrWhiteSpace(ligne.LieuNaissance) ? ligne.LieuNaissance.Trim() : "Non renseigné",
                    NumeroTelephone: ligne.NumeroTelephone.Trim(),
                    Email: ligne.Email.Trim(),
                    Parents: parents
                );

                await _createService.ExecuteAsync(createRequest, cancellationToken);
                succes++;
            }
            catch (Exception ex)
            {
                _db.ChangeTracker.Clear();

                echecs++;
                erreurs.Add(new ImportLigneErreur(
                    Ligne: numeroLigne,
                    Identifiant: !string.IsNullOrWhiteSpace(ligne.Email) ? ligne.Email : (!string.IsNullOrWhiteSpace(ligne.Nom) ? ligne.Nom : $"Ligne {numeroLigne}"),
                    Message: ex.Message
                ));
            }
        }

        return new ImportEtudiantReport(
            TotalLignes: lignes.Count,
            Succes: succes,
            Echecs: echecs,
            Erreurs: erreurs
        );
    }

    private List<ImportEtudiantLigneDto> ExtraireCsv(Stream stream)
    {
        using var memoryStream = new MemoryStream();
        stream.CopyTo(memoryStream);
        memoryStream.Position = 0;

        using var detectReader = new StreamReader(memoryStream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, leaveOpen: true);
        var premiereLigne = detectReader.ReadLine();
        var delimiter = premiereLigne != null && premiereLigne.Count(c => c == ';') > premiereLigne.Count(c => c == ',')
            ? ";"
            : ",";

        memoryStream.Position = 0;
        using var reader = new StreamReader(memoryStream, Encoding.UTF8);

        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Delimiter = delimiter,
            HasHeaderRecord = true,
            HeaderValidated = null,
            MissingFieldFound = null,
            IgnoreBlankLines = true,
            PrepareHeaderForMatch = args => args.Header.Trim().Replace(" ", "").Replace("_", "").ToLowerInvariant()
        };

        using var csv = new CsvReader(reader, config);
        csv.Context.RegisterClassMap<ImportEtudiantLigneDtoMap>();

        return csv.GetRecords<ImportEtudiantLigneDto>().ToList();
    }

    private List<ImportEtudiantLigneDto> ExtraireExcel(Stream stream)
    {
        using var workbook = new XLWorkbook(stream);
        var worksheet = workbook.Worksheets.FirstOrDefault();
        if (worksheet is null)
            return [];

        var range = worksheet.RangeUsed();
        if (range is null)
            return [];

        var firstRow = range.FirstRow();
        var headerMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        foreach (var cell in firstRow.CellsUsed())
        {
            var header = cell.GetString().Trim().Replace(" ", "").Replace("_", "").ToLowerInvariant();
            if (!string.IsNullOrEmpty(header) && !headerMap.ContainsKey(header))
            {
                headerMap[header] = cell.Address.ColumnNumber;
            }
        }

        string GetCell(IXLRangeRow row, string[] possibleKeys, int defaultCol)
        {
            foreach (var key in possibleKeys)
            {
                var normalizedKey = key.Replace(" ", "").Replace("_", "").ToLowerInvariant();
                if (headerMap.TryGetValue(normalizedKey, out var colNum))
                    return row.Cell(colNum).GetString().Trim();
            }

            if (defaultCol > 0 && defaultCol <= range.ColumnCount())
                return row.Cell(defaultCol).GetString().Trim();

            return string.Empty;
        }

        var lignes = new List<ImportEtudiantLigneDto>();
        var dataRows = range.RowsUsed().Skip(1);

        foreach (var row in dataRows)
        {
            var nom = GetCell(row, ["nom"], 1);
            var prenom = GetCell(row, ["prenom", "prénom"], 2);
            var email = GetCell(row, ["email", "mail"], 3);
            var telephone = GetCell(row, ["numerotelephone", "telephone", "numtel"], 4);
            var nomPere = GetCell(row, ["nompere", "pere"], 5);
            var telPere = GetCell(row, ["telephonepere", "telpere"], 6);
            var profPere = GetCell(row, ["professionpere", "profpere", "metierpere"], 0);
            var emailPere = GetCell(row, ["emailpere", "mailpere"], 0);
            var nomMere = GetCell(row, ["nommere", "mere"], 7);
            var telMere = GetCell(row, ["telephonemere", "telmere"], 8);
            var profMere = GetCell(row, ["professionmere", "profmere", "metiermere"], 0);
            var emailMere = GetCell(row, ["emailmere", "mailmere"], 0);
            var dateNaissStr = GetCell(row, ["datenaissance", "date_naissance"], 0);
            DateOnly? dateNaissance = DateOnly.TryParse(dateNaissStr, out var d) ? d : null;
            var lieuNaissance = GetCell(row, ["lieunaissance", "lieu_naissance"], 0);

            if (string.IsNullOrWhiteSpace(nom) && string.IsNullOrWhiteSpace(email) && string.IsNullOrWhiteSpace(telephone))
                continue;

            lignes.Add(new ImportEtudiantLigneDto(
                Nom: nom,
                Prenom: string.IsNullOrWhiteSpace(prenom) ? null : prenom,
                Email: email,
                NumeroTelephone: telephone,
                NomPere: string.IsNullOrWhiteSpace(nomPere) ? null : nomPere,
                TelephonePere: string.IsNullOrWhiteSpace(telPere) ? null : telPere,
                ProfessionPere: string.IsNullOrWhiteSpace(profPere) ? null : profPere,
                EmailPere: string.IsNullOrWhiteSpace(emailPere) ? null : emailPere,
                NomMere: string.IsNullOrWhiteSpace(nomMere) ? null : nomMere,
                TelephoneMere: string.IsNullOrWhiteSpace(telMere) ? null : telMere,
                ProfessionMere: string.IsNullOrWhiteSpace(profMere) ? null : profMere,
                EmailMere: string.IsNullOrWhiteSpace(emailMere) ? null : emailMere,
                DateNaissance: dateNaissance,
                LieuNaissance: string.IsNullOrWhiteSpace(lieuNaissance) ? null : lieuNaissance
            ));
        }

        return lignes;
    }
}

public sealed class ImportEtudiantLigneDtoMap : ClassMap<ImportEtudiantLigneDto>
{
    public ImportEtudiantLigneDtoMap()
    {
        Map(m => m.Nom).Name("Nom", "nom", "NOM");
        Map(m => m.Prenom).Name("Prenom", "prenom", "Prénom", "prénom", "PRENOM").Optional();
        Map(m => m.Email).Name("Email", "email", "Mail", "mail", "EMAIL");
        Map(m => m.NumeroTelephone).Name("NumeroTelephone", "numerotelephone", "Telephone", "telephone", "Numero Telephone", "numero_telephone");
        Map(m => m.NomPere).Name("NomPere", "nompere", "Nom Pere", "nom_pere").Optional();
        Map(m => m.TelephonePere).Name("TelephonePere", "telephonepere", "Telephone Pere", "telephone_pere").Optional();
        Map(m => m.ProfessionPere).Name("ProfessionPere", "professionpere", "Profession Pere", "profession_pere").Optional();
        Map(m => m.EmailPere).Name("EmailPere", "emailpere", "Email Pere", "email_pere").Optional();
        Map(m => m.NomMere).Name("NomMere", "nommere", "Nom Mere", "nom_mere").Optional();
        Map(m => m.TelephoneMere).Name("TelephoneMere", "telephonemere", "Telephone Mere", "telephone_mere").Optional();
        Map(m => m.ProfessionMere).Name("ProfessionMere", "professionmere", "Profession Mere", "profession_mere").Optional();
        Map(m => m.EmailMere).Name("EmailMere", "emailmere", "Email Mere", "email_mere").Optional();
        Map(m => m.DateNaissance).Name("DateNaissance", "datenaissance", "Date Naissance", "date_naissance").Optional();
        Map(m => m.LieuNaissance).Name("LieuNaissance", "lieunaissance", "Lieu Naissance", "lieu_naissance").Optional();
    }
}
