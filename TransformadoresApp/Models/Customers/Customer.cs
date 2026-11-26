using System.ComponentModel.DataAnnotations;

namespace TransformadoresApp.Models.Customers
{
    public class Customer
    {
        // =====================
        // IDENTIFICACIÓN
        // =====================

        public int Id { get; set; }

        [Required(ErrorMessage = "El código es obligatorio.")]
        [StringLength(20, ErrorMessage = "El código no puede superar los 20 caracteres.")]
        [RegularExpression(
        @"^[\p{L}\p{N}._/-]+$",
        ErrorMessage = "El código solo puede contener letras, números, puntos, guiones y barras.")]
        public string Code { get; set; } = string.Empty;

        [Required(ErrorMessage = "El tipo de cliente es obligatorio.")]
        public CustomerType CustomerType { get; set; }


        // =====================
        // PERSONA FÍSICA
        // =====================

        [StringLength(
        100,
        MinimumLength = 3,
        ErrorMessage = "El nombre debe tener al menos 3 caracteres.")]
        [RegularExpression(
        @"^[\p{L}\p{M}]+(?:[ '\u2019-][\p{L}\p{M}]+)*$",
        ErrorMessage = "El nombre contiene caracteres no permitidos.")]
        public string? FirstName { get; set; }

        [StringLength(
        100,
        MinimumLength = 2,
        ErrorMessage = "El apellido debe tener al menos 2 caracteres.")]
        [RegularExpression(
        @"^[\p{L}\p{M}]+(?:[ '\u2019-][\p{L}\p{M}]+)*$",
        ErrorMessage = "El apellido contiene caracteres no permitidos.")]
        public string? LastName { get; set; }

        [StringLength(20)]
        [RegularExpression(
            @"^(?=.*\d)[\p{L}\p{N}\s./-]{5,20}$",
            ErrorMessage = "El documento debe tener entre 5 y 20 caracteres y contener al menos un número.")]
        public string? DocumentNumber { get; set; }


        // =====================
        // EMPRESA
        // =====================

        [StringLength(150)]
        public string? BusinessName { get; set; }

        [StringLength(150)]
        public string? FantasyName { get; set; }

        [StringLength(150)]
        [RegularExpression(
        @"^[\p{L}\p{M}]+(?:[ '\u2019-][\p{L}\p{M}]+)*$",
        ErrorMessage = "El nombre del contacto contiene caracteres no permitidos.")]
        public string? ContactName { get; set; }


        // =====================
        // DATOS FISCALES
        // =====================

        [StringLength(20)]
        [RegularExpression(
            @"^(?:\d{11}|\d{2}-\d{8}-\d)$",
            ErrorMessage = "El CUIT/CUIL debe contener 11 números. Podés ingresarlo con o sin guiones.")]
        public string? TaxId { get; set; }


        // =====================
        // CONTACTO
        // =====================

        [StringLength(150)]
        [EmailAddress(ErrorMessage = "Ingresá un correo electrónico válido.")]
        public string? Email { get; set; }

        [StringLength(50)]
        [RegularExpression(
            @"^(?=(?:\D*\d){8,15}\D*$)[0-9+().\s-]+$",
            ErrorMessage = "El teléfono debe contener entre 8 y 15 números. Podés utilizar +, espacios, guiones y paréntesis.")]
        public string? Phone { get; set; }


        // =====================
        // DOMICILIO
        // =====================

        [StringLength(200)]
        [RegularExpression(
            @"^(?=.*[\p{L}\p{M}])[\p{L}\p{M}\d\s.,#'’ºª°/-]+$",
            ErrorMessage = "La dirección debe contener letras y solo puede incluir números y caracteres habituales de un domicilio.")]
        public string? Address { get; set; }

        [StringLength(100)]
        [RegularExpression(
            @"^(?=.*[\p{L}\p{M}])[\p{L}\p{M}\s.'’-]+$",
            ErrorMessage = "La ciudad debe contener letras y no puede incluir números.")]
        public string? City { get; set; }

        [StringLength(100)]
        [RegularExpression(
            @"^(?=.*[\p{L}\p{M}])[\p{L}\p{M}\s.'’-]+$",
            ErrorMessage = "La provincia debe contener letras y no puede incluir números.")]
        public string? Province { get; set; }

        [StringLength(100)]
        [RegularExpression(
            @"^(?=.*[\p{L}\p{M}])[\p{L}\p{M}\s.'’-]+$",
            ErrorMessage = "El país debe contener letras y no puede incluir números.")]
        public string? Country { get; set; }


        // =====================
        // INFORMACIÓN ADICIONAL
        // =====================

        [StringLength(500)]
        public string? Notes { get; set; }


        // =====================
        // ESTADO
        // =====================

        public bool IsActive { get; set; } = true;
    }
}