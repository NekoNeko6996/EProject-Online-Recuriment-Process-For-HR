namespace Sem3EProjectOnlineCPFH.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updateuserinfo : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.AspNetUsers", "Address");
            DropColumn("dbo.AspNetUsers", "City");
            DropColumn("dbo.AspNetUsers", "Country");
            DropColumn("dbo.AspNetUsers", "PostalCode");
            DropColumn("dbo.AspNetUsers", "EmailConfirmationCode");
            DropColumn("dbo.AspNetUsers", "CodeExpirationTime");
        }
        
        public override void Down()
        {
            AddColumn("dbo.AspNetUsers", "CodeExpirationTime", c => c.DateTime());
            AddColumn("dbo.AspNetUsers", "EmailConfirmationCode", c => c.String());
            AddColumn("dbo.AspNetUsers", "PostalCode", c => c.String(maxLength: 10));
            AddColumn("dbo.AspNetUsers", "Country", c => c.String(nullable: false, maxLength: 50));
            AddColumn("dbo.AspNetUsers", "City", c => c.String(nullable: false, maxLength: 50));
            AddColumn("dbo.AspNetUsers", "Address", c => c.String(nullable: false, maxLength: 200));
        }
    }
}
