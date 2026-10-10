using Mapster;
using erp_univ_back.Features.Etudiant.Create;

namespace erp_univ_back.Features.Etudiant
{
    public class EtudiantMapping : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<CreateEtudiantRequest, EtudiantEntity>()
                .Ignore(dest => dest.Id)
                .Ignore(dest => dest.DateCreation)
                .Ignore(dest => dest.DateModification)
                .Ignore(dest => dest.Matricule) 
                .Map(dest => dest.EtudiantParents, src => src.Parents);

            config.NewConfig<CreateParentDto, EtudiantParentEntity>()
                .Ignore(dest => dest.Id)
                .Ignore(dest => dest.DateCreation)
                .Ignore(dest => dest.DateModification)
                .Map(dest => dest.Role, src => src.Role)
                .Map(dest => dest.Parent, src => src);

            config.NewConfig<CreateParentDto, ParentEntity>()
                .Ignore(dest => dest.Id)
                .Ignore(dest => dest.DateCreation)
                .Ignore(dest => dest.DateModification);
        }
    }
}