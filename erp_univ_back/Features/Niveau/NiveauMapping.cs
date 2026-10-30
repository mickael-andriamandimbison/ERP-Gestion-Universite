namespace erp_univ_back.Features.Niveau;

using erp_univ_back.Features.Niveau.Create;
using erp_univ_back.Features.Niveau.GetAll;
using erp_univ_back.Features.Niveau.GetById;
using erp_univ_back.Features.Niveau.Update;
using Mapster;

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

        config.NewConfig<UpdateNiveauRequest, NiveauEntity>()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.DateCreation)
            .Ignore(dest => dest.DateModification)
            .Ignore(dest => dest.EtablissementId)
            .Ignore(dest => dest.Etablissement)
            .Ignore(dest => dest.FiliereNiveaux);

        config.NewConfig<NiveauEntity, CreateNiveauResponse>();
        config.NewConfig<NiveauEntity, GetAllNiveauResponse>();
        config.NewConfig<NiveauEntity, GetNiveauByIdResponse>();
        config.NewConfig<NiveauEntity, UpdateNiveauResponse>();
    }
}
