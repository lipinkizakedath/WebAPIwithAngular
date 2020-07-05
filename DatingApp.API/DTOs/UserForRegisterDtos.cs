using System.ComponentModel.DataAnnotations;

namespace DatingApp.API.DTOs
{
    public class UserForRegisterDtos
    {
        [Required]
        public string username { get; set; }

        [Required]
        [StringLength(8, MinimumLength=6, ErrorMessage="Password must have a minimum 6 characters")]
        public string password { get; set; }
        
    }
}