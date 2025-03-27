using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Reflection.Emit;
using System.Web;
using Microsoft.AspNet.Identity.EntityFramework;
using Sem3EProjectOnlineCPFH.Models.Auth;
using Sem3EProjectOnlineCPFH.Models.Data;


namespace Sem3EProjectOnlineCPFH.Models
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext() : base("OnlineRPFH") { }

        // create a new instance of the ApplicationDbContext
        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        // Add a new DbSet
        public DbSet<UserProfile> UserProfiles { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Vacancy> Vacancies { get; set; }
        public DbSet<Applicant> Applicants { get; set; }
        public DbSet<Applicant_Vacancy> ApplicantVacancies { get; set; }
        public DbSet<Interview> Interviews { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // Vacancy - Department (1-n)
            modelBuilder.Entity<Vacancy>()
                .HasRequired(v => v.Department)
                .WithMany()
                .HasForeignKey(v => v.DepartmentId)
                .WillCascadeOnDelete(false);

            // Applicant_Vacancy (Many-to-Many Applicant và Vacancy)
            modelBuilder.Entity<Applicant_Vacancy>()
                .HasRequired(av => av.Applicant)
                .WithMany(a => a.ApplicantVacancies)
                .HasForeignKey(av => av.ApplicantId)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<Applicant_Vacancy>()
                .HasRequired(av => av.Vacancy)
                .WithMany(v => v.ApplicantVacancies)
                .HasForeignKey(av => av.VacancyId)
                .WillCascadeOnDelete(true);

            // Interview - Applicant (1-n)
            modelBuilder.Entity<Interview>()
                .HasRequired(i => i.Applicant)
                .WithMany()
                .HasForeignKey(i => i.ApplicantId)
                .WillCascadeOnDelete(true);

            // Interview - Vacancy (1-n)
            modelBuilder.Entity<Interview>()
                .HasRequired(i => i.Vacancy)
                .WithMany()
                .HasForeignKey(i => i.VacancyId)
                .WillCascadeOnDelete(false);

            base.OnModelCreating(modelBuilder);
        }
    }
}