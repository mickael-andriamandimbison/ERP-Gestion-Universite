namespace erp_univ_back.Features.FiliereNiveau;

using erp_univ_back.Features.FiliereNiveau.Create;
using erp_univ_back.Features.FiliereNiveau.GetByFiliere;
using Mapster;

public class FiliereNiveauMapping : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<FiliereNiveauEntity, FiliereNiveauResponse>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.NiveauId, src => src.NiveauId)
            .Map(dest => dest.NiveauCode, src => src.Niveau.Code)
            .Map(dest => dest.NiveauNom, src => src.Niveau.Nom)
            .Map(dest => dest.Ordre, src => src.Niveau.Ordre)
            .Map(dest => dest.LibelleAffiche, src => src.LibelleAffiche ?? "");

        config.NewConfig<FiliereNiveauEntity, CreateFiliereNiveauResponse>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.FiliereId, src => src.FiliereId)
            .Map(dest => dest.FiliereNom, src => src.Filiere.Nom)
            .Map(dest => dest.NiveauId, src => src.NiveauId)
            .Map(dest => dest.NiveauNom, src => src.Niveau.Nom)
            .Map(dest => dest.LibelleAffiche, src => src.LibelleAffiche ?? "")
            .Map(dest => dest.DateCreation, src => src.DateCreation);
    }
}
