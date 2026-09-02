using System.ComponentModel.DataAnnotations;

namespace TransformadoresApp.Models.Purchasing
{
    public class Supplier
    {
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "Código")]
        public string Code { get; set; } = string.Empty;

        [Required]
        [StringLength(150)]
        [Display(Name = "Razón Social")]
        public string BusinessName { get; set; } = string.Empty;

        [StringLength(150)]
        [Display(Name = "Nombre Comercial")]
        public string? FantasyName { get; set; }

        [StringLength(20)]
        [Display(Name = "CUIT / RUT / NIT")]
        public string? TaxId { get; set; }

        [StringLength(150)]
        [EmailAddress]
        [Display(Name = "Email")]
        public string? Email { get; set; }

        [StringLength(50)]
        [Display(Name = "Teléfono")]
        public string? Phone { get; set; }

        [StringLength(200)]
        [Display(Name = "Dirección")]
        public string? Address { get; set; }

        [StringLength(100)]
        [Display(Name = "Ciudad")]
        public string? City { get; set; }

        [StringLength(100)]
        [Display(Name = "Provincia")]
        public string? Province { get; set; }

        [StringLength(100)]
        [Display(Name = "País")]
        public string? Country { get; set; }

        [StringLength(500)]
        [Display(Name = "Observaciones")]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;
    }
}