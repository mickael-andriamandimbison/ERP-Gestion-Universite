namespace erp_univ_back.Features.Inscription;

using erp_univ_back.Features.Inscription.Create;
using erp_univ_back.Features.Inscription.GetByClasse;
using Mapster;

public class InscriptionMapping : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<CreateInscriptionRequest, InscriptionEntity>()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.DateCreation)
            .Ignore(dest => dest.DateModification)
            .Ignore(dest => dest.EtablissementId)
            .Ignore(dest => dest.Etablissement)
            .Ignore(dest => dest.Etudiant)
            .Ignore(dest => dest.FiliereNiveau)
            .Ignore(dest => dest.AnneeAcademique)
            .Map(dest => dest.Statut, _ => StatutInscription.Inscrit);

        config.NewConfig<InscriptionEntity, CreateInscriptionResponse>()
            .Map(dest => dest.EtudiantNom, src => src.Etudiant.Nom)
            .Map(dest => dest.EtudiantPrenom, src => src.Etudiant.Prenom ?? "")
            .Map(dest => dest.EtudiantMatricule, src => src.Etudiant.Matricule)
            .Map(dest => dest.ClasseLibelle, src => !string.IsNullOrWhiteSpace(src.FiliereNiveau.LibelleAffiche)
                ? src.FiliereNiveau.LibelleAffiche
                : $"{src.FiliereNiveau.Niveau.Nom} {src.FiliereNiveau.Filiere.Nom}")
            .Map(dest => dest.AnneeAcademiqueLibelle, src => src.AnneeAcademique.Libelle);

        config.NewConfig<InscriptionEntity, InscriptionByClasseResponse>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.EtudiantId, src => src.EtudiantId)
            .Map(dest => dest.Matricule, src => src.Etudiant.Matricule)
            .Map(dest => dest.Nom, src => src.Etudiant.Nom)
            .Map(dest => dest.Prenom, src => src.Etudiant.Prenom)
            .Map(dest => dest.Email, src => src.Etudiant.Email)
            .Map(dest => dest.NumeroTelephone, src => src.Etudiant.NumeroTelphone)
            .Map(dest => dest.Statut, src => src.Statut)
            .Map(dest => dest.DateInscription, src => src.DateCreation);
    }
}
