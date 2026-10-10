namespace erp_univ_back.Features.Etudiant.GetAll
{
    public record GetAllEtudiantRequest();

    public record EtudiantGetAllResponse(
        string Prenom,
        string Email,
        string NumeroTelephone
    );
}