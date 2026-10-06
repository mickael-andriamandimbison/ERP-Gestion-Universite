
namespace erp_univ_back.Shared.Entities;

using System.ComponentModel.DataAnnotations;

public abstract class BaseEntity
{

    [Key]
    [Required]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public DateTime DateCreation { get; set; } = DateTime.UtcNow;

    [Required]
    public DateTime DateModification { get; set; } = DateTime.UtcNow;
}