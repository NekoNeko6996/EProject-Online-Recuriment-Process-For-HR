namespace Sem3EProjectOnlineCPFH.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class finalupdatev4 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Vacancies", "Status", c => c.String(nullable: false, maxLength: 100));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Vacancies", "Status", c => c.String());
        }
    }
}
