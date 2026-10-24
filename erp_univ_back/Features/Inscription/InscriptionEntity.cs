namespace erp_univ_back.Features.Inscription;

using System.ComponentModel.DataAnnotations.Schema;
using erp_univ_back.Features.AnneeAcademique;
using erp_univ_back.Features.Etablissement;
using erp_univ_back.Features.Etudiant;
using erp_univ_back.Features.FiliereNiveau;
using erp_univ_back.Shared.Entities;

public enum StatutInscription
{
    Inscrit,
    Abandon,
    Annule
}

[Table("inscriptions")]
public class InscriptionEntity : BaseEntity
{
    public Guid EtudiantId { get; set; }
    public EtudiantEntity Etudiant { get; set; } = null!;

    public Guid FiliereNiveauId { get; set; }
    public FiliereNiveauEntity FiliereNiveau { get; set; } = null!;

    public Guid AnneeAcademiqueId { get; set; }
    public AnneeAcademiqueEntity AnneeAcademique { get; set; } = null!;

    public StatutInscription Statut { get; set; } = StatutInscription.Inscrit;

    public Guid EtablissementId { get; set; }
    public EtablissementEntity Etablissement { get; set; } = null!;
}
