namespace erp_univ_back.Features.UniteEnseignement;

using erp_univ_back.Features.UniteEnseignement.Create;
using erp_univ_back.Features.UniteEnseignement.GetById;
using erp_univ_back.Features.UniteEnseignement.Update;
using Mapster;

public class UniteEnseignementMapping : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<UniteEnseignementEntity, CreateUniteEnseignementResponse>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Code, src => src.Code)
            .Map(dest => dest.Nom, src => src.Nom)
            .Map(dest => dest.Credit, src => src.Credit)
            .Map(dest => dest.Semestre, src => src.Semestre)
            .Map(dest => dest.FiliereNiveauId, src => src.FiliereNiveauId)
            .Map(dest => dest.FiliereNiveauLibelle, src => 
                (src.FiliereNiveau.LibelleAffiche != null 
                    ? src.FiliereNiveau.LibelleAffiche.Replace(" - ", "-") 
                    : (src.FiliereNiveau.Filiere != null && src.FiliereNiveau.Niveau != null 
                        ? $"{src.FiliereNiveau.Filiere.Nom.Trim()}-{src.FiliereNiveau.Niveau.Code.Trim()}".Replace(" - ", "-") 
                        : "")))
            .Map(dest => dest.DateCreation, src => src.DateCreation);

        config.NewConfig<UniteEnseignementEntity, UpdateUniteEnseignementResponse>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Code, src => src.Code)
            .Map(dest => dest.Nom, src => src.Nom)
            .Map(dest => dest.Credit, src => src.Credit)
            .Map(dest => dest.Semestre, src => src.Semestre)
            .Map(dest => dest.FiliereNiveauId, src => src.FiliereNiveauId)
            .Map(dest => dest.FiliereNiveauLibelle, src => 
                (src.FiliereNiveau.LibelleAffiche != null 
                    ? src.FiliereNiveau.LibelleAffiche.Replace(" - ", "-") 
                    : (src.FiliereNiveau.Filiere != null && src.FiliereNiveau.Niveau != null 
                        ? $"{src.FiliereNiveau.Filiere.Nom.Trim()}-{src.FiliereNiveau.Niveau.Code.Trim()}".Replace(" - ", "-") 
                        : "")))
            .Map(dest => dest.DateModification, src => src.DateModification);

        config.NewConfig<UniteEnseignementEntity, GetUniteEnseignementByIdResponse>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Code, src => src.Code)
            .Map(dest => dest.Nom, src => src.Nom)
            .Map(dest => dest.Credit, src => src.Credit)
            .Map(dest => dest.Semestre, src => src.Semestre)
            .Map(dest => dest.FiliereNiveauId, src => src.FiliereNiveauId)
            .Map(dest => dest.FiliereNiveauLibelle, src => 
                (src.FiliereNiveau.LibelleAffiche != null 
                    ? src.FiliereNiveau.LibelleAffiche.Replace(" - ", "-") 
                    : (src.FiliereNiveau.Filiere != null && src.FiliereNiveau.Niveau != null 
                        ? $"{src.FiliereNiveau.Filiere.Nom.Trim()}-{src.FiliereNiveau.Niveau.Code.Trim()}".Replace(" - ", "-") 
                        : "")))
            .Map(dest => dest.FiliereId, src => src.FiliereNiveau.FiliereId)
            .Map(dest => dest.FiliereNom, src => src.FiliereNiveau.Filiere.Nom)
            .Map(dest => dest.NiveauId, src => src.FiliereNiveau.NiveauId)
            .Map(dest => dest.NiveauNom, src => src.FiliereNiveau.Niveau.Nom)
            .Map(dest => dest.NiveauCode, src => src.FiliereNiveau.Niveau.Code)
            .Map(dest => dest.DateCreation, src => src.DateCreation);
    }
}
