namespace Sem3EProjectOnlineCPFH.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class finalupdatev6 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Interviews", "Status", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Interviews", "Status", c => c.String());
        }
    }
}
