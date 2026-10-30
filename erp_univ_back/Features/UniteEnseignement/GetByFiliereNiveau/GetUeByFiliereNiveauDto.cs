namespace erp_univ_back.Features.UniteEnseignement.GetByFiliereNiveau;

public record GetUeByFiliereNiveauRequest(
    Guid? FiliereId = null,
    Guid? NiveauId = null
);

public record UniteEnseignementItemDto(
    Guid Id,
    string Code,
    string Nom,
    int Credit,
    int Semestre,
    DateTime DateCreation
);

public record SemestreUeDto(
    int Semestre,
    int TotalCredits,
    List<UniteEnseignementItemDto> UnitesEnseignement
);

public record GetUeByFiliereNiveauResponse(
    string? AnneAcademique,
    string? FiliereNom,
    Guid? FiliereId,
    string? NiveauNom,
    Guid? NiveauId,
    Guid? FiliereNiveauId,
    List<SemestreUeDto> Semestres
)
{
    public string? AnneeAcademique => AnneAcademique;
}
