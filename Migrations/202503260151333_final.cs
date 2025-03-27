namespace Sem3EProjectOnlineCPFH.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class final : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Applicants",
                c => new
                    {
                        ApplicantId = c.Int(nullable: false, identity: true),
                        FullName = c.String(nullable: false, maxLength: 50),
                        Email = c.String(nullable: false, maxLength: 256),
                        PhoneNumber = c.String(),
                        CVFile = c.String(),
                        AvatarUrl = c.String(),
                        Status = c.String(),
                        CreatedDate = c.DateTime(nullable: false),
                        AttachedVacancies = c.String(),
                    })
                .PrimaryKey(t => t.ApplicantId);
            
            CreateTable(
                "dbo.Applicant_Vacancy",
                c => new
                    {
                        ApplicantVacancyId = c.Int(nullable: false, identity: true),
                        ApplicantId = c.Int(nullable: false),
                        VacancyId = c.Int(nullable: false),
                        Approver = c.Int(nullable: false),
                        ApplyAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.ApplicantVacancyId)
                .ForeignKey("dbo.Applicants", t => t.ApplicantId, cascadeDelete: true)
                .ForeignKey("dbo.Vacancies", t => t.VacancyId, cascadeDelete: true)
                .Index(t => t.ApplicantId)
                .Index(t => t.VacancyId);
            
            CreateTable(
                "dbo.Vacancies",
                c => new
                    {
                        VacancyId = c.Int(nullable: false, identity: true),
                        OwnerId = c.String(maxLength: 128),
                        Title = c.String(nullable: false, maxLength: 100),
                        Description = c.String(),
                        DepartmentId = c.Int(nullable: false),
                        Status = c.String(),
                        NumberOfOpenings = c.Int(nullable: false),
                        Deadline = c.DateTime(nullable: false),
                        ApplicationLimit = c.Int(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                        ListOfSelectedApplicants = c.String(),
                    })
                .PrimaryKey(t => t.VacancyId)
                .ForeignKey("dbo.Departments", t => t.DepartmentId)
                .ForeignKey("dbo.AspNetUsers", t => t.OwnerId)
                .Index(t => t.OwnerId)
                .Index(t => t.DepartmentId);
            
            CreateTable(
                "dbo.Departments",
                c => new
                    {
                        DepartmentId = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 50),
                    })
                .PrimaryKey(t => t.DepartmentId);
            
            CreateTable(
                "dbo.Interviews",
                c => new
                    {
                        InterviewId = c.Int(nullable: false, identity: true),
                        ApplicantId = c.Int(nullable: false),
                        VacancyId = c.Int(nullable: false),
                        ScheduledDate = c.DateTime(nullable: false),
                        StartTime = c.Time(nullable: false, precision: 7),
                        EndTime = c.Time(nullable: false, precision: 7),
                        Status = c.String(),
                        Result = c.String(),
                    })
                .PrimaryKey(t => t.InterviewId)
                .ForeignKey("dbo.Applicants", t => t.ApplicantId, cascadeDelete: true)
                .ForeignKey("dbo.Vacancies", t => t.VacancyId)
                .Index(t => t.ApplicantId)
                .Index(t => t.VacancyId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Interviews", "VacancyId", "dbo.Vacancies");
            DropForeignKey("dbo.Interviews", "ApplicantId", "dbo.Applicants");
            DropForeignKey("dbo.Applicant_Vacancy", "VacancyId", "dbo.Vacancies");
            DropForeignKey("dbo.Vacancies", "OwnerId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Vacancies", "DepartmentId", "dbo.Departments");
            DropForeignKey("dbo.Applicant_Vacancy", "ApplicantId", "dbo.Applicants");
            DropIndex("dbo.Interviews", new[] { "VacancyId" });
            DropIndex("dbo.Interviews", new[] { "ApplicantId" });
            DropIndex("dbo.Vacancies", new[] { "DepartmentId" });
            DropIndex("dbo.Vacancies", new[] { "OwnerId" });
            DropIndex("dbo.Applicant_Vacancy", new[] { "VacancyId" });
            DropIndex("dbo.Applicant_Vacancy", new[] { "ApplicantId" });
            DropTable("dbo.Interviews");
            DropTable("dbo.Departments");
            DropTable("dbo.Vacancies");
            DropTable("dbo.Applicant_Vacancy");
            DropTable("dbo.Applicants");
        }
    }
}
