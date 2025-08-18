using System.ComponentModel.DataAnnotations;

namespace NetTestMayapada.ViewModels
{
    public class HomeViewModel
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Level { get; set; }
        public string PhotoProfile { get; set; }
    }

    public class EditProfileViewModel
    {
        [Required(ErrorMessage = "Name is required.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage = "User Level is required.")]
        public string Level { get; set; }
        
        public string? PhotoProfile { get; set; }

        //[Required(ErrorMessage = "Photo is required.")]
        public IFormFile? PhotoProfileFile { get; set; }
    }
}
