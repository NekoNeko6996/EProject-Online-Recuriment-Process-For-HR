using Sem3EProjectOnlineCPFH.Models.Auth;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sem3EProjectOnlineCPFH.Models
{
	public class UserProfile
	{

        [Key, ForeignKey("ApplicationUser")]
        public string Id { get; set; }
        public string AvatarUrl { get; set; }
        public string Bio { get; set; }
        public string SocialAccount1 { get; set; }
        public string SocialAccount2 { get; set; }
        public string SocialAccount3 { get; set; }

        //
        public virtual ApplicationUser ApplicationUser { get; set; }
    }
}