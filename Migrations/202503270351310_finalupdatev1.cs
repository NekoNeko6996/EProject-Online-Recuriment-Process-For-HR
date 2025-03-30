namespace Sem3EProjectOnlineCPFH.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class finalupdatev1 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Applicants",
                c => new
                    {
                        ApplicantId = c.String(nullable: false, maxLength: 128),
                        FullName = c.String(nullable: false, maxLength: 50),
                        Email = c.String(nullable: false, maxLength: 256),
                        PhoneNumber = c.String(),
                        CVPath = c.String(),
                        AvatarPath = c.String(),
                        Status = c.String(),
                        CreatedAt = c.DateTime(nullable: false),
                        AttachedVacancies = c.String(),
                    })
                .PrimaryKey(t => t.ApplicantId);
            
            CreateTable(
                "dbo.Applicant_Vacancy",
                c => new
                    {
                        ApplicantVacancyId = c.String(nullable: false, maxLength: 128),
                        ApplicantId = c.String(nullable: false, maxLength: 128),
                        VacancyId = c.String(nullable: false, maxLength: 128),
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
                        VacancyId = c.String(nullable: false, maxLength: 128),
                        OwnerId = c.String(maxLength: 128),
                        Title = c.String(nullable: false, maxLength: 100),
                        Description = c.String(),
                        DepartmentId = c.String(nullable: false, maxLength: 128),
                        Status = c.String(),
                        NumberOfPostions = c.Int(nullable: false),
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
                        DepartmentId = c.String(nullable: false, maxLength: 128),
                        DepartmentName = c.String(nullable: false, maxLength: 50),
                    })
                .PrimaryKey(t => t.DepartmentId);
            
            CreateTable(
                "dbo.AspNetUsers",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        FirstName = c.String(nullable: false, maxLength: 50),
                        LastName = c.String(nullable: false, maxLength: 50),
                        CreatedAt = c.DateTime(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        Email = c.String(maxLength: 256),
                        EmailConfirmed = c.Boolean(nullable: false),
                        PasswordHash = c.String(),
                        SecurityStamp = c.String(),
                        PhoneNumber = c.String(),
                        PhoneNumberConfirmed = c.Boolean(nullable: false),
                        TwoFactorEnabled = c.Boolean(nullable: false),
                        LockoutEndDateUtc = c.DateTime(),
                        LockoutEnabled = c.Boolean(nullable: false),
                        AccessFailedCount = c.Int(nullable: false),
                        UserName = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.UserName, unique: true, name: "UserNameIndex");
            
            CreateTable(
                "dbo.AspNetUserClaims",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(nullable: false, maxLength: 128),
                        ClaimType = c.String(),
                        ClaimValue = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.Interviews",
                c => new
                    {
                        InterviewId = c.String(nullable: false, maxLength: 128),
                        ApplicantId = c.String(nullable: false, maxLength: 128),
                        VacancyId = c.String(nullable: false, maxLength: 128),
                        ScheduledDate = c.DateTime(nullable: false),
                        InterviewerId = c.String(nullable: false, maxLength: 128),
                        InterviewMethod = c.String(),
                        InterviewURL = c.String(),
                        StartTime = c.Time(nullable: false, precision: 7),
                        EndTime = c.Time(nullable: false, precision: 7),
                        Status = c.String(),
                        Result = c.String(),
                    })
                .PrimaryKey(t => t.InterviewId)
                .ForeignKey("dbo.Applicants", t => t.ApplicantId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetUsers", t => t.InterviewerId)
                .ForeignKey("dbo.Vacancies", t => t.VacancyId)
                .Index(t => t.ApplicantId)
                .Index(t => t.VacancyId)
                .Index(t => t.InterviewerId);
            
            CreateTable(
                "dbo.AspNetUserLogins",
                c => new
                    {
                        LoginProvider = c.String(nullable: false, maxLength: 128),
                        ProviderKey = c.String(nullable: false, maxLength: 128),
                        UserId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.LoginProvider, t.ProviderKey, t.UserId })
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.AspNetUserRoles",
                c => new
                    {
                        UserId = c.String(nullable: false, maxLength: 128),
                        RoleId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.UserId, t.RoleId })
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetRoles", t => t.RoleId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.RoleId);
            
            CreateTable(
                "dbo.UserProfiles",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        AvatarUrl = c.String(),
                        Bio = c.String(),
                        SocialAccount1 = c.String(),
                        SocialAccount2 = c.String(),
                        SocialAccount3 = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.Id)
                .Index(t => t.Id);
            
            CreateTable(
                "dbo.AspNetRoles",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Name = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Name, unique: true, name: "RoleNameIndex");
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AspNetUserRoles", "RoleId", "dbo.AspNetRoles");
            DropForeignKey("dbo.Applicant_Vacancy", "VacancyId", "dbo.Vacancies");
            DropForeignKey("dbo.Vacancies", "OwnerId", "dbo.AspNetUsers");
            DropForeignKey("dbo.UserProfiles", "Id", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserRoles", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserLogins", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Interviews", "VacancyId", "dbo.Vacancies");
            DropForeignKey("dbo.Interviews", "InterviewerId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Interviews", "ApplicantId", "dbo.Applicants");
            DropForeignKey("dbo.AspNetUserClaims", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Vacancies", "DepartmentId", "dbo.Departments");
            DropForeignKey("dbo.Applicant_Vacancy", "ApplicantId", "dbo.Applicants");
            DropIndex("dbo.AspNetRoles", "RoleNameIndex");
            DropIndex("dbo.UserProfiles", new[] { "Id" });
            DropIndex("dbo.AspNetUserRoles", new[] { "RoleId" });
            DropIndex("dbo.AspNetUserRoles", new[] { "UserId" });
            DropIndex("dbo.AspNetUserLogins", new[] { "UserId" });
            DropIndex("dbo.Interviews", new[] { "InterviewerId" });
            DropIndex("dbo.Interviews", new[] { "VacancyId" });
            DropIndex("dbo.Interviews", new[] { "ApplicantId" });
            DropIndex("dbo.AspNetUserClaims", new[] { "UserId" });
            DropIndex("dbo.AspNetUsers", "UserNameIndex");
            DropIndex("dbo.Vacancies", new[] { "DepartmentId" });
            DropIndex("dbo.Vacancies", new[] { "OwnerId" });
            DropIndex("dbo.Applicant_Vacancy", new[] { "VacancyId" });
            DropIndex("dbo.Applicant_Vacancy", new[] { "ApplicantId" });
            DropTable("dbo.AspNetRoles");
            DropTable("dbo.UserProfiles");
            DropTable("dbo.AspNetUserRoles");
            DropTable("dbo.AspNetUserLogins");
            DropTable("dbo.Interviews");
            DropTable("dbo.AspNetUserClaims");
            DropTable("dbo.AspNetUsers");
            DropTable("dbo.Departments");
            DropTable("dbo.Vacancies");
            DropTable("dbo.Applicant_Vacancy");
            DropTable("dbo.Applicants");
        }
    }
}
