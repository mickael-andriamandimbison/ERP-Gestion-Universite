namespace erp_univ_back.Features.Matiere;

using erp_univ_back.Features.Matiere.Create;
using erp_univ_back.Features.Matiere.GetById;
using erp_univ_back.Features.Matiere.Update;
using Mapster;

public class MatiereMapping : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<CreateMatiereRequest, MatiereEntity>()
            .Map(dest => dest.Nom, src => src.Nom.Trim())
            .Map(dest => dest.Code, src => src.Code.Trim())
            .Map(dest => dest.Coefficient, src => src.Coefficient)
            .Map(dest => dest.UniteEnseignementId, src => src.UniteEnseignementId)
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.DateCreation)
            .Ignore(dest => dest.DateModification)
            .Ignore(dest => dest.EtablissementId)
            .Ignore(dest => dest.Etablissement);

        config.NewConfig<MatiereEntity, CreateMatiereResponse>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Nom, src => src.Nom)
            .Map(dest => dest.Code, src => src.Code)
            .Map(dest => dest.Coefficient, src => src.Coefficient)
            .Map(dest => dest.UniteEnseignementId, src => src.UniteEnseignementId)
            .Map(dest => dest.DateCreation, src => src.DateCreation);

        config.NewConfig<MatiereEntity, UpdateMatiereResponse>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Nom, src => src.Nom)
            .Map(dest => dest.Code, src => src.Code)
            .Map(dest => dest.Coefficient, src => src.Coefficient)
            .Map(dest => dest.UniteEnseignementId, src => src.UniteEnseignementId)
            .Map(dest => dest.DateModification, src => src.DateModification);

        config.NewConfig<MatiereEntity, GetMatiereByIdResponse>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Nom, src => src.Nom)
            .Map(dest => dest.Code, src => src.Code)
            .Map(dest => dest.Coefficient, src => src.Coefficient)
            .Map(dest => dest.UniteEnseignementId, src => src.UniteEnseignementId)
            .Map(dest => dest.UniteEnseignementNom, src => src.UniteEnseignement.Nom)
            .Map(dest => dest.UniteEnseignementCode, src => src.UniteEnseignement.Code)
            .Map(dest => dest.FiliereId, src => src.UniteEnseignement.FiliereNiveau.FiliereId)
            .Map(dest => dest.FiliereNom, src => src.UniteEnseignement.FiliereNiveau.Filiere.Nom)
            .Map(dest => dest.NiveauId, src => src.UniteEnseignement.FiliereNiveau.NiveauId)
            .Map(dest => dest.NiveauNom, src => src.UniteEnseignement.FiliereNiveau.Niveau.Nom)
            .Map(dest => dest.DateCreation, src => src.DateCreation);
    }
}
