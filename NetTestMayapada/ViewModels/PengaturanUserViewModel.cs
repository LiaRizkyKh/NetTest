using System.ComponentModel.DataAnnotations;

namespace NetTestMayapada.ViewModels
{
    public class UserViewModel
    {
        public int UserNumber { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Level { get; set; }
        public string PhotoProfile { get; set; }
        public bool IsActive { get; set; }
    }

    public class LoadTableUserViewModel
    {
        public int UserNumber { get; set; }
        public int number { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Level { get; set; }
        public string PhotoProfile { get; set; }
        public bool IsActive { get; set; }
    }

    public class CreateUserViewModel
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Level { get; set; }
        public string Password { get; set; }
        public IFormFile PhotoProfileFile { get; set; }
        public bool IsActive { get; set; }
    }

    public class EditUserViewModel
    {
        public string Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Level { get; set; }
        public string Password { get; set; }
        public IFormFile PhotoProfileFile { get; set; }
        public bool IsActive { get; set; }
    }
}
