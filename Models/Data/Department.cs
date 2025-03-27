using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Sem3EProjectOnlineCPFH.Models.Data
{
    public class Department
    {
        [Key]
        public int DepartmentId { get; set; }

        [Required, MaxLength(50)]
        public string Name { get; set; }
    }
}