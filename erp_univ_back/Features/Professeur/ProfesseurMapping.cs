using erp_univ_back.Features.Professeur.Create;
using erp_univ_back.Features.Professeur.Update;
using Mapster;

namespace erp_univ_back.Features.Professeur
{
    public class ProfesseurMapping : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<CreateProfRequest, ProfesseurEntity>()
                .Ignore(dest => dest.Id)
                .Ignore(dest => dest.DateCreation)
                .Ignore(dest => dest.DateModification);

            config.NewConfig<UpdateProfRequest, ProfesseurEntity>()
                .Ignore(dest => dest.Id)
                .Ignore(dest => dest.Matricule)
                .Ignore(dest => dest.DateCreation)
                .Ignore(dest => dest.DateModification)
                .Ignore(dest => dest.EtablissementId)
                .Ignore(dest => dest.Etablissement);
        }
    }
}