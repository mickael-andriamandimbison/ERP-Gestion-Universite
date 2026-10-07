namespace erp_univ_back.Features.Etablissement.Update;

{
    public interface IUpdateService
    {
        Task ExecuteAsync(UpdateEtablissementRequest request);
    }

    public class UpdateService(ErpUnivDbContext db) : IUpdateService
    {
        public async Task ExecuteAsync(UpdateEtablissementRequest request)
        {
            var etablissement = await db.Etablissements.FindAsync(request.Id);
            if (etablissement is null)
            {
                throw new NotFoundException("Etablissement non trouvé");
            }
            etablissement.Nom = request.Nom;
            etablissement.Code = request.Code;
            etablissement.DateModification = DateTime.Now;
            await db.SaveChangesAsync(ct);
        }
    }
}