namespace Sem3EProjectOnlineCPFH.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class finalupdatev7 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Applicant_Vacancy", "Approver", c => c.String());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Applicant_Vacancy", "Approver", c => c.Int(nullable: false));
        }
    }
}
