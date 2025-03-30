using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Sem3EProjectOnlineCPFH.Models.Data
{
    public class Department
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string DepartmentId { get; set; }

        [Required, MaxLength(50)]
        public string DepartmentName { get; set; }
    }
}