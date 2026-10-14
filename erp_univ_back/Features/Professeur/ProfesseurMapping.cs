
using erp_univ_back.Features.Professeur.Create;
using Mapster;

namespace erp_univ_back.Features.Professeur
{
    public class ProfesseurMapping : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<CreateProfRequest,ProfesseurEntity>()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.DateCreation)
            .Ignore(dest => dest.DateModification);
        }
    }
}