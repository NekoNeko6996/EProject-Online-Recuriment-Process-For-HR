using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Sem3EProjectOnlineCPFH.Models.User
{
	public class UserId
	{
		[Required]
		public int Id { get; set; }
	}
}