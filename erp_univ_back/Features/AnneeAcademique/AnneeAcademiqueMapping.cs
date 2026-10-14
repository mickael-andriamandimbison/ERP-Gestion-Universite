using erp_univ_back.Features.AnneeAcademique.Create;
using erp_univ_back.Features.AnneeAcademique.GetAll;
using erp_univ_back.Features.AnneeAcademique.GetActive;
using erp_univ_back.Features.AnneeAcademique.Update;
using Mapster;

namespace erp_univ_back.Features.AnneeAcademique
{
    public class AnneeAcademiqueMapping : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<CreateAnneeAcademiqueRequest, AnneeAcademiqueEntity>()
                .Ignore(dest => dest.Id)
                .Ignore(dest => dest.DateCreation)
                .Ignore(dest => dest.DateModification)
                .Ignore(dest => dest.EtablissementId)
                .Ignore(dest => dest.Etablissement);

            config.NewConfig<UpdateAnneeAcademiqueRequest, AnneeAcademiqueEntity>()
                .Ignore(dest => dest.Id)
                .Ignore(dest => dest.EstActive)
                .Ignore(dest => dest.DateCreation)
                .Ignore(dest => dest.DateModification)
                .Ignore(dest => dest.EtablissementId)
                .Ignore(dest => dest.Etablissement);

            config.NewConfig<AnneeAcademiqueEntity, CreateAnneeAcademiqueResponse>();
            config.NewConfig<AnneeAcademiqueEntity, AnneeAcademiqueGetAllResponse>();
            config.NewConfig<AnneeAcademiqueEntity, GetActiveAnneeAcademiqueResponse>();
            config.NewConfig<AnneeAcademiqueEntity, UpdateAnneeAcademiqueResponse>();
        }
    }
}
