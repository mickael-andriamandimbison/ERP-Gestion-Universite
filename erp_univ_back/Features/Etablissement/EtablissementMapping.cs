using Mapster;
using erp_univ_back.Shared.Entities;
using erp_univ_back.Features.Etablissement.CreateEtablissement;

namespace erp_univ_back.Features.Etablissement;

public class EtablissementMapping : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        // mapping pour la creation d'etablissement
        config.NewConfig<EtablissementRequest, EtablissementEntity>()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.DateCreation)
            .Ignore(dest => dest.DateModification);

    }
}
