using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace Sem3EProjectOnlineCPFH.Models.Auth
{
	public class ProfileSettingsViewModel
	{
        public ProfileViewModel ProfileUpdate { get; set; }
        public ChangePasswordViewModel ChangePassword { get; set; }
    }
}