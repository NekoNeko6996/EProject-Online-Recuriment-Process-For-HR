using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Sem3EProjectOnlineCPFH.Models.User
{
    public class UploadAvatarViewModel
    {
        public string Id { get; set; }

        [Required(ErrorMessage = "Please choose a img.")]
        public HttpPostedFileBase Avatar { get; set; }
    }
}