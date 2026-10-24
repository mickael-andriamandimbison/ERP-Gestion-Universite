using erp_univ_back.Features.Etudiant.Create;
using erp_univ_back.Features.Etudiant.GetAll;
using erp_univ_back.Features.Etudiant.GetById;
using erp_univ_back.Features.Etudiant.Update;
using Mapster;

namespace erp_univ_back.Features.Etudiant;

public class EtudiantMapping : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<EtudiantEntity, GetAllEtudiantResponse>()
            .Map(dest => dest.NumeroTelephone, src => src.NumeroTelphone);

        config.NewConfig<EtudiantEntity, GetEtudiantByIdResponse>()
            .Map(dest => dest.NumeroTelephone, src => src.NumeroTelphone);

        config.NewConfig<EtudiantEntity, UpdateEtudiantResponse>()
            .Map(dest => dest.NumeroTelephone, src => src.NumeroTelphone);

        config.NewConfig<CreateEtudiantRequest, EtudiantEntity>()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.DateCreation)
            .Ignore(dest => dest.DateModification)
            .Ignore(dest => dest.Matricule)
            .Map(dest => dest.EtudiantParents, src => src.Parents);

        config.NewConfig<UpdateEtudiantRequest, EtudiantEntity>()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.DateCreation)
            .Ignore(dest => dest.DateModification)
            .Ignore(dest => dest.Matricule)
            .Ignore(dest => dest.EtablissementId)
            .Ignore(dest => dest.Etablissement)
            .Ignore(dest => dest.EtudiantParents)
            .Map(dest => dest.NumeroTelphone, src => src.NumeroTelephone);

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