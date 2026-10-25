using erp_univ_back.Features.Filiere.Create;
using erp_univ_back.Features.Filiere.GetAll;
using erp_univ_back.Features.Filiere.GetById;
using erp_univ_back.Features.Filiere.Update;
using Mapster;

namespace erp_univ_back.Features.Filiere;

public class FiliereMapping : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<CreateFiliereRequest, FiliereEntity>()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.DateCreation)
            .Ignore(dest => dest.DateModification)
            .Ignore(dest => dest.EtablissementId)
            .Ignore(dest => dest.Etablissement)
            .Ignore(dest => dest.FiliereNiveaux);

        config.NewConfig<UpdateFiliereRequest, FiliereEntity>()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.DateCreation)
            .Ignore(dest => dest.DateModification)
            .Ignore(dest => dest.EtablissementId)
            .Ignore(dest => dest.Etablissement)
            .Ignore(dest => dest.FiliereNiveaux);

        config.NewConfig<FiliereEntity, CreateFiliereResponse>();
        config.NewConfig<FiliereEntity, GetAllFiliereResponse>();
        config.NewConfig<FiliereEntity, GetFiliereByIdResponse>();
        config.NewConfig<FiliereEntity, UpdateFiliereResponse>();
    }
}
