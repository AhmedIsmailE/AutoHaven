using System.ComponentModel.DataAnnotations;

namespace AutoHaven.ViewModel
{
    public class LoginUserViewModel
    {
        [Required]
        public string EmailOrPhone { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public bool RememberMe { get; set; }

    }
}
