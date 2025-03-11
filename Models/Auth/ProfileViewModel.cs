using System.ComponentModel.DataAnnotations;

namespace Sem3EProjectOnlineCPFH.Models.Auth
{
    public class ProfileViewModel
    {
        public string AvatarUrl { get; set; }

        [Required, StringLength(50)]
        public string FirstName { get; set; }

        [Required, StringLength(50)]
        public string LastName { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [StringLength(250)]
        public string Bio { get; set; }

        [Phone]
        public string PhoneNumber { get; set; }

        public string SocialAccount1 { get; set; }
        public string SocialAccount2 { get; set; }
        public string SocialAccount3 { get; set; }

        public string Address { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
    }
}
