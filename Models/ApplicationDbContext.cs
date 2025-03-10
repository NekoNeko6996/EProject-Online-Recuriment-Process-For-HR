using System;
using System.Collections.Generic;
using System.Data.Entity;
using Microsoft.AspNet.Identity.EntityFramework;
using Sem3EProjectOnlineCPFH.Models.Auth;

namespace Sem3EProjectOnlineCPFH.Models
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext() : base("OnlineRPFH")
        {
        }


        // create a new instance of the ApplicationDbContext
        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }
    }
}