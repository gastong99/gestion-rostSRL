namespace TransformadoresApp.ViewModels
{
    public class UserWithRolesViewModel
    {
        public string UserId { get; set; } = string.Empty;
        public string? Email { get; set; }
        public List<string> Roles { get; set; } = new();
    }
}