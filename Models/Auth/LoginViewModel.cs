using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Sem3EProjectOnlineCPFH.Models.Auth
{
	public class LoginViewModel
	{
        [Required(ErrorMessage = "Email is required")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; }
        public bool RememberMe { get; set; }
    }
}