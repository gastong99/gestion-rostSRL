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
        [RegularExpression(
        @"^(?=.*\d)[A-Za-z0-9]+(?:[ .-]?[A-Za-z0-9]+)*$",
        ErrorMessage = "Ingresá un CUIT, RUT o NIT válido. Se admiten letras, números, puntos, espacios y guiones, y debe contener al menos un número.")]
        [Display(Name = "CUIT / RUT / NIT")]
        public string? TaxId { get; set; }

        [StringLength(150)]
        [EmailAddress]
        [Display(Name = "Email")]
        public string? Email { get; set; }

        [StringLength(50)]
        [RegularExpression(
            @"^(?=(?:\D*\d){8,15}\D*$)[0-9+().\s-]+$",
            ErrorMessage = "El teléfono debe contener entre 8 y 15 números. Podés utilizar +, espacios, guiones y paréntesis.")]
        [Display(Name = "Teléfono")]
        public string? Phone { get; set; }

        [StringLength(200)]
        [RegularExpression(
        @"^(?=.*[\p{L}\p{M}])[\p{L}\p{M}\d\s.,#'’ºª°/-]+$",
        ErrorMessage = "La dirección debe contener letras y solo puede incluir números y caracteres habituales de un domicilio.")]
        [Display(Name = "Dirección")]
        public string? Address { get; set; }

        [StringLength(100)]
        [RegularExpression(
        @"^(?=.*[\p{L}\p{M}])[\p{L}\p{M}\s.'’-]+$",
        ErrorMessage = "La ciudad debe contener letras y no puede incluir números.")]
        [Display(Name = "Ciudad")]
        public string? City { get; set; }

        [StringLength(100)]
        [RegularExpression(
        @"^(?=.*[\p{L}\p{M}])[\p{L}\p{M}\s.'’-]+$",
        ErrorMessage = "La provincia debe contener letras y no puede incluir números.")]
        [Display(Name = "Provincia")]
        public string? Province { get; set; }

        [StringLength(100)]
        [RegularExpression(
        @"^(?=.*[\p{L}\p{M}])[\p{L}\p{M}\s.'’-]+$",
        ErrorMessage = "El país debe contener letras y no puede incluir números.")]
        [Display(Name = "País")]
        public string? Country { get; set; }

        [StringLength(500)]
        [Display(Name = "Observaciones")]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;
    }
}