namespace GaspApp.Models.AccountViewModels
{
    public class ModifyUserViewModel
    {
        public Guid Id { get; set; } = default!;
        public string Name { get; set; } = default!;
        public string Email { get; set; } = default!;
        public bool SuperUser { get; set; } = default!;
    }
}
