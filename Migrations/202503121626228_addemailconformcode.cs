namespace Sem3EProjectOnlineCPFH.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addemailconformcode : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "EmailConfirmationCode", c => c.String());
            AddColumn("dbo.AspNetUsers", "CodeExpirationTime", c => c.DateTime());
        }
        
        public override void Down()
        {
            DropColumn("dbo.AspNetUsers", "CodeExpirationTime");
            DropColumn("dbo.AspNetUsers", "EmailConfirmationCode");
        }
    }
}
