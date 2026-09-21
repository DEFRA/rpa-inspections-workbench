namespace RPA.Inspections.Workbench.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    using System.Diagnostics.CodeAnalysis;

    [ExcludeFromCodeCoverage]
    public partial class schemenamerename : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo.SchemeNames", newName: "SchemeName");
            MoveTable(name: "dbo.SchemeName", newSchema: "IW");
        }
        
        public override void Down()
        {
            MoveTable(name: "IW.SchemeName", newSchema: "dbo");
            RenameTable(name: "dbo.SchemeName", newName: "SchemeNames");
        }
    }
}
