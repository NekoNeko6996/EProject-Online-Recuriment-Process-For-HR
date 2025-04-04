namespace Sem3EProjectOnlineCPFH.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class finalupdatev3 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Interviews", "InterviewMethod", c => c.String(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Interviews", "InterviewMethod", c => c.String());
        }
    }
}
