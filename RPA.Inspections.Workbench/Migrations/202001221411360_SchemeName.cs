namespace RPA.Inspections.Workbench.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    using System.Diagnostics.CodeAnalysis;

    [ExcludeFromCodeCoverage]
    public partial class SchemeName : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.SchemeNames",
                c => new
                    {
                        SchemeNameId = c.Int(nullable: false, identity: true),
                        Text = c.String(),
                        Active = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.SchemeNameId);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.SchemeNames");
        }
    }
}
