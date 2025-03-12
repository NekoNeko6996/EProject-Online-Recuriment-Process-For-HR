using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Sem3EProjectOnlineCPFH.Models.Auth
{
    public class VerifyEmailViewModel
    {
        public string UserId { get; set; }

        [Required]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "Code must be 6 digits.")]
        public string Code { get; set; }
    }
}