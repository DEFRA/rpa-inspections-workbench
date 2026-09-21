namespace RPA.Inspections.Workbench.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    using System.Diagnostics.CodeAnalysis;

    [ExcludeFromCodeCoverage]
    public partial class schemenamedrop2 : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("IW.Holding", "SchemeNameDrop_SchemeNameDropId", "IW.SchemeNameDrop");
            DropIndex("IW.Holding", new[] { "SchemeNameDrop_SchemeNameDropId" });
            RenameColumn(table: "IW.Holding", name: "SchemeNameDrop_SchemeNameDropId", newName: "SchemeNameDropId");
            AlterColumn("IW.Holding", "SchemeNameDropId", c => c.Int());
            CreateIndex("IW.Holding", "SchemeNameDropId");
            AddForeignKey("IW.Holding", "SchemeNameDropId", "IW.SchemeNameDrop", "SchemeNameDropId", cascadeDelete: true);
            DropColumn("IW.Holding", "SchemeId");
        }
        
        public override void Down()
        {
            AddColumn("IW.Holding", "SchemeId", c => c.Int(nullable: false));
            DropForeignKey("IW.Holding", "SchemeNameDropId", "IW.SchemeNameDrop");
            DropIndex("IW.Holding", new[] { "SchemeNameDropId" });
            AlterColumn("IW.Holding", "SchemeNameDropId", c => c.Int());
            RenameColumn(table: "IW.Holding", name: "SchemeNameDropId", newName: "SchemeNameDrop_SchemeNameDropId");
            CreateIndex("IW.Holding", "SchemeNameDrop_SchemeNameDropId");
            AddForeignKey("IW.Holding", "SchemeNameDrop_SchemeNameDropId", "IW.SchemeNameDrop", "SchemeNameDropId");
        }
    }
}
