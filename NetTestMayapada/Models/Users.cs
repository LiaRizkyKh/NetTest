using Microsoft.AspNetCore.Identity;

namespace NetTestMayapada.Models
{
    public class Users: IdentityUser
    {
        public string FullName { get; set; }
        public string? PhotoProfile { get; set; }
        public string? Level { get; set; }
    }
}
