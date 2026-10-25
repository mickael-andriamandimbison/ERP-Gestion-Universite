namespace erp_univ_back.Features.Etablissement;

using erp_univ_back.Features.Etablissement.Create;
using erp_univ_back.Features.Etablissement.GetAll;
using erp_univ_back.Features.Etablissement.GetById;
using erp_univ_back.Features.Etablissement.Update;
using Mapster;

public class EtablissementMapping : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<CreateEtablissementRequest, EtablissementEntity>()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.DateCreation)
            .Ignore(dest => dest.DateModification)
            .Ignore(dest => dest.Etudiants)
            .Ignore(dest => dest.Professeurs)
            .Ignore(dest => dest.AnneeAcademiques)
            .Ignore(dest => dest.Filieres)
            .Ignore(dest => dest.Niveaux)
            .Ignore(dest => dest.FiliereNiveaux)
            .Ignore(dest => dest.Parents)
            .Ignore(dest => dest.Inscriptions);

        config.NewConfig<UpdateEtablissementRequest, EtablissementEntity>()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.DateCreation)
            .Ignore(dest => dest.DateModification)
            .Ignore(dest => dest.Etudiants)
            .Ignore(dest => dest.Professeurs)
            .Ignore(dest => dest.AnneeAcademiques)
            .Ignore(dest => dest.Filieres)
            .Ignore(dest => dest.Niveaux)
            .Ignore(dest => dest.FiliereNiveaux)
            .Ignore(dest => dest.Parents)
            .Ignore(dest => dest.Inscriptions);

        config.NewConfig<EtablissementEntity, CreateEtablissementResponse>();
        config.NewConfig<EtablissementEntity, GetAllEtablissementResponse>();
        config.NewConfig<EtablissementEntity, GetEtablissementByIdResponse>();
        config.NewConfig<EtablissementEntity, UpdateEtablissementResponse>();
    }
}
