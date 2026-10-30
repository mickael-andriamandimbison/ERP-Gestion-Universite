using erp_univ_back.Features.Professeur.Create;
using erp_univ_back.Features.Professeur.GetAll;
using erp_univ_back.Features.Professeur.GetById;
using erp_univ_back.Features.Professeur.Update;
using Mapster;

namespace erp_univ_back.Features.Professeur;

public class ProfesseurMapping : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<CreateProfesseurRequest, ProfesseurEntity>()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.DateCreation)
            .Ignore(dest => dest.DateModification);

        config.NewConfig<UpdateProfesseurRequest, ProfesseurEntity>()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.Matricule)
            .Ignore(dest => dest.DateCreation)
            .Ignore(dest => dest.DateModification)
            .Ignore(dest => dest.EtablissementId)
            .Ignore(dest => dest.Etablissement);

        config.NewConfig<ProfesseurEntity, CreateProfesseurResponse>();
        config.NewConfig<ProfesseurEntity, GetAllProfesseurResponse>();
        config.NewConfig<ProfesseurEntity, GetProfesseurByIdResponse>();
        config.NewConfig<ProfesseurEntity, UpdateProfesseurResponse>();
    }
}