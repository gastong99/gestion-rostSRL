using System.ComponentModel.DataAnnotations;

namespace TransformadoresApp.Models.Catalogs
{
    public enum ItemType
    {
        [Display(Name = "Materia Prima")]
        RawMaterial = 1,

        [Display(Name = "Producto Terminado")]
        FinishedProduct = 2,

        [Display(Name = "Producto Comercial")]
        CommercialProduct = 3,

        [Display(Name = "Servicio")]
        Service = 4
    }
}