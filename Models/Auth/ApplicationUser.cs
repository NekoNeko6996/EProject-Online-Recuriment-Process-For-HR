using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNet.Identity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity.EntityFramework;
using Sem3EProjectOnlineCPFH.Models;

namespace Sem3EProjectOnlineCPFH.Models.Auth
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(50, ErrorMessage = "Tên không được vượt quá 50 ký tự.")]
        public string FirstName { get; set; }

        [Required]
        [StringLength(50)]
        public string LastName { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;

        //
        public virtual UserProfile UserProfile { get; set; }

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);

            // Thêm Role vào Claims
            var roles = await manager.GetRolesAsync(this.Id);
            foreach (var role in roles)
            {
                userIdentity.AddClaim(new Claim(ClaimTypes.Role, role));
            }

            return userIdentity;
        }

    }
}