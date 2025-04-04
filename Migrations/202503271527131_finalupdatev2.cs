namespace Sem3EProjectOnlineCPFH.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class finalupdatev2 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Vacancies", "NumberOfPositions", c => c.Int(nullable: false));
            DropColumn("dbo.Vacancies", "NumberOfPostions");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Vacancies", "NumberOfPostions", c => c.Int(nullable: false));
            DropColumn("dbo.Vacancies", "NumberOfPositions");
        }
    }
}
