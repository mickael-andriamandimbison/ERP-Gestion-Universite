namespace erp_univ_back.Features.Etudiant.Create;

public record CreateEtudiantRequest(
    string Nom,
    string? Prenom,
    DateOnly DateNaissance,
    string LieuNaissance,
    string NumeroTelephone,
    string Email,
    List<CreateParentDto>? Parents = null
)
{
    public List<CreateParentDto> Parents { get; init; } = Parents ?? [];
}

public record CreateParentDto(
    string Nom,
    string? Prenom,
    string NumeroTelephone,
    string Profession,
    Role Role,
    string? Email = null
);

public record CreateEtudiantResponse(
    Guid Id,
    string Matricule
);
