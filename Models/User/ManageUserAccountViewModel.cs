using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace Sem3EProjectOnlineCPFH.Models.user
{
	public class ManageUserAccountViewModel
	{
		public string Id { get; set; }
		public string UserName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }        
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool isActive { get; set; }
        public string RoleName { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}