using System.ComponentModel.DataAnnotations;

namespace TransformadoresApp.ViewModels;

public class CreateUserViewModel
{
    [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
    [EmailAddress(ErrorMessage = "Ingresá un correo electrónico válido.")]
    [StringLength(256, ErrorMessage = "El correo no puede superar los 256 caracteres.")]
    [Display(Name = "Correo electrónico")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [StringLength(100, MinimumLength = 12, ErrorMessage = "La contraseña debe tener entre 12 y 100 caracteres.")]
    [DataType(DataType.Password)]
    [Display(Name = "Contraseña temporal")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Seleccioná un rol.")]
    [RegularExpression("^(Administrador|Operador)$", ErrorMessage = "El rol seleccionado no es válido.")]
    [Display(Name = "Rol")]
    public string Role { get; set; } = "Operador";
}
