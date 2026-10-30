namespace erp_univ_back.Features.Matiere;

using erp_univ_back.Features.Matiere.Create;
using Mapster;


public class MatierMapping : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<CreateMatiereRequest, MatiereEntity>()
        .Map(dest => dest.Nom, src => src.Nom)
        .Map(dest => dest.Code, src => src.Code)
        .Map(dest => dest.Coefficient, src => src.Coefficient)
        .Map(dest => dest.UniteEnseignementId, src => src.UniteEnseignementId)
        .Ignore(dest => dest.Id)
            .Ignore(dest => dest.DateCreation)
            .Ignore(dest => dest.DateModification)
            .Ignore(dest => dest.EtablissementId)
            .Ignore(dest => dest.Etablissement);

        
        config.NewConfig<MatiereEntity,CreateMatiereResponse>()
        .Map(dest => dest.Id, src => src.Id)
        .Map(dest => dest.Nom, src => src.Nom)
        .Map(dest => dest.Code, src => src.Code)
        .Map(dest => dest.Coefficient, src => src.Coefficient)
        .Map(dest => dest.UniteEnseignementId, src => src.UniteEnseignementId)
        .Map(dest => dest.DateCreation, src => src.DateCreation);
    }
}