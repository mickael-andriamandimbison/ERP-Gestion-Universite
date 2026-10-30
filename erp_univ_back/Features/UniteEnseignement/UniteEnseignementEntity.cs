namespace erp_univ_back.Features.UniteEnseignement;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using erp_univ_back.Features.Etablissement;
using erp_univ_back.Features.FiliereNiveau;
using erp_univ_back.Features.Matiere;
using erp_univ_back.Shared.Entities;

[Table("unites_enseignement")]
public class UniteEnseignementEntity : BaseEntity
{
    [Required]
    [MaxLength(50)]
    public required string Code { get; set; }

    [Required]
    [MaxLength(150)]
    public required string Nom { get; set; }

    [Required]
    public int Credit { get; set; }

    [Required]
    public int Semestre { get; set; }

    [Required]
    public Guid FiliereNiveauId { get; set; }
    public FiliereNiveauEntity FiliereNiveau { get; set; } = null!;

    [Required]
    public Guid EtablissementId { get; set; }
    public EtablissementEntity Etablissement { get; set; } = null!;

    public ICollection<MatiereEntity> Matieres {get;set;} = [];
}

