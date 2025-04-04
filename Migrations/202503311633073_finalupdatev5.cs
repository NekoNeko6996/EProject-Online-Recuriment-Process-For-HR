namespace Sem3EProjectOnlineCPFH.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class finalupdatev5 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Applicants", "Status", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Applicants", "Status", c => c.String());
        }
    }
}
