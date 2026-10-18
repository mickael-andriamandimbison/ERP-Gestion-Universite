namespace erp_univ_back.Features.Niveau;

using erp_univ_back.Features.Niveau.Create;
using erp_univ_back.Features.Niveau.GetById;
using Mapster;
using GetAllResponse = erp_univ_back.Features.Niveau.GetAll.NiveauResponse;
using UpdateResponse = erp_univ_back.Features.Niveau.Update.NiveauResponse;

public class NiveauMapping : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<CreateNiveauRequest, NiveauEntity>()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.DateCreation)
            .Ignore(dest => dest.DateModification)
            .Ignore(dest => dest.EtablissementId)
            .Ignore(dest => dest.Etablissement)
            .Ignore(dest => dest.FiliereNiveaux);

        config.NewConfig<Update.UpdateNiveauRequest, NiveauEntity>()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.DateCreation)
            .Ignore(dest => dest.DateModification)
            .Ignore(dest => dest.EtablissementId)
            .Ignore(dest => dest.Etablissement)
            .Ignore(dest => dest.FiliereNiveaux);

        config.NewConfig<NiveauEntity, CreateNiveauResponse>();
        config.NewConfig<NiveauEntity, GetAllResponse>();
        config.NewConfig<NiveauEntity, UpdateResponse>();
        config.NewConfig<NiveauEntity, NiveauDetailResponse>();
    }
}
