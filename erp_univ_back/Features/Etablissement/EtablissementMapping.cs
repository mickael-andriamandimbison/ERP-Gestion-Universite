using Mapster;
using erp_univ_back.Features.Etablissement.Create;

namespace erp_univ_back.Features.Etablissement
{

    public class EtablissementMapping : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<EtablissementRequest, EtablissementEntity>()
                .Ignore(dest => dest.Id)
                .Ignore(dest => dest.DateCreation)
                .Ignore(dest => dest.DateModification);

        }
    }
}

