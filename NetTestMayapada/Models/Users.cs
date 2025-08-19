using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace NetTestMayapada.Models
{
    public class Users: IdentityUser
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserNumber { get; set; }

        public string FullName { get; set; }
        public string? PhotoProfile { get; set; }
        public string? Level { get; set; }
        public bool IsActive { get; set; }
    }
}
