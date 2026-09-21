namespace RPA.Inspections.Workbench.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    using System.Diagnostics.CodeAnalysis;

    [ExcludeFromCodeCoverage]
    public partial class allownullschemedrop : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("IW.Holding", "SchemeNameDropId", "IW.SchemeNameDrop");
            DropIndex("IW.Holding", new[] { "SchemeNameDropId" });
            AlterColumn("IW.Holding", "SchemeNameDropId", c => c.Int());
            CreateIndex("IW.Holding", "SchemeNameDropId");
            AddForeignKey("IW.Holding", "SchemeNameDropId", "IW.SchemeNameDrop", "SchemeNameDropId");
        }
        
        public override void Down()
        {
            DropForeignKey("IW.Holding", "SchemeNameDropId", "IW.SchemeNameDrop");
            DropIndex("IW.Holding", new[] { "SchemeNameDropId" });
            AlterColumn("IW.Holding", "SchemeNameDropId", c => c.Int(nullable: false));
            CreateIndex("IW.Holding", "SchemeNameDropId");
            AddForeignKey("IW.Holding", "SchemeNameDropId", "IW.SchemeNameDrop", "SchemeNameDropId", cascadeDelete: true);
        }
    }
}
