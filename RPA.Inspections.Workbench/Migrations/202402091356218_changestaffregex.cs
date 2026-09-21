namespace RPA.Inspections.Workbench.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class changestaffregex : DbMigration
    {
        public override void Up()
        {
            AlterColumn("IW.Holding", "AssignedUser", c => c.String());
        }
        
        public override void Down()
        {
            AlterColumn("IW.Holding", "AssignedUser", c => c.String(maxLength: 7));
        }
    }
}
