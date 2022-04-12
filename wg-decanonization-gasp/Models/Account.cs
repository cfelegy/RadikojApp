using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace GaspApp.Models
{
    [Index(nameof(Email), IsUnique = true)]
    public class Account
    {
        public Guid Id {  get; set; }
        public string Email { get; set; }
        public string DisplayName { get; set; }
        public string LoginToken { get; set; }
        public DateTimeOffset LoginTokenExpiresAt { get; set; }
        public DateTimeOffset? LastLoggedInAt { get; set; }
        public bool SuperUser { get; set; }
        public bool Disabled { get; set; }
    }
}
