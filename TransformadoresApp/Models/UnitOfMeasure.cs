using System.ComponentModel.DataAnnotations;

namespace TransformadoresApp.Models
{
    public class UnitOfMeasure
    {
        public int UnitOfMeasureId { get; set; }

        [Required(ErrorMessage = "El nombre de la unidad es obligatorio.")]
        [StringLength(50, ErrorMessage = "El nombre de la unidad no puede tener más de 50 caracteres.")]
        [Display(Name = "Nombre")]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "La abreviatura es obligatoria.")]
        [StringLength(10, ErrorMessage = "La abreviatura no puede tener más de 10 caracteres.")]
        [Display(Name = "Abreviatura")]
        public string Abbreviation { get; set; } = null!;
    }
}