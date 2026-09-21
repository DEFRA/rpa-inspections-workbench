namespace RPA.Inspections.Workbench.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    using System.Diagnostics.CodeAnalysis;

    [ExcludeFromCodeCoverage]
    public partial class schemenamedrop : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "IW.SchemeNameDrop",
                c => new
                    {
                        SchemeNameDropId = c.Int(nullable: false, identity: true),
                        Text = c.String(),
                        Active = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.SchemeNameDropId);
            
            AddColumn("IW.Holding", "SchemeId", c => c.Int(nullable: false));
            AddColumn("IW.Holding", "SchemeNameDrop_SchemeNameDropId", c => c.Int());
            CreateIndex("IW.Holding", "SchemeNameDrop_SchemeNameDropId");
            AddForeignKey("IW.Holding", "SchemeNameDrop_SchemeNameDropId", "IW.SchemeNameDrop", "SchemeNameDropId");
            DropTable("IW.SchemeName");
        }
        
        public override void Down()
        {
            CreateTable(
                "IW.SchemeName",
                c => new
                    {
                        SchemeNameId = c.Int(nullable: false, identity: true),
                        Text = c.String(),
                        Active = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.SchemeNameId);
            
            DropForeignKey("IW.Holding", "SchemeNameDrop_SchemeNameDropId", "IW.SchemeNameDrop");
            DropIndex("IW.Holding", new[] { "SchemeNameDrop_SchemeNameDropId" });
            DropColumn("IW.Holding", "SchemeNameDrop_SchemeNameDropId");
            DropColumn("IW.Holding", "SchemeId");
            DropTable("IW.SchemeNameDrop");
        }
    }
}
