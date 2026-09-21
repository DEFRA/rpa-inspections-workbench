using Moq;
using RPA.Inspections.Workbench.DAL;
using RPA.Inspections.Workbench.Models;
using RPA.Inspections.Workbench.Tests.Data.Mock;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPA.Inspections.Workbench.Tests.DAL.Mock
{
    public class MockWorkbenchContext
    {

        public Mock<IWorkbenchContext> MockContext { get; set; }

        //Workbench Items

        public virtual Mock<DbSet<Breed>> MockBreed { get; set; }

        public virtual Mock<DbSet<Cattle>> MockCattle { get; set; }

        public virtual Mock<DbSet<Control>> MockControl { get; set; }

        public virtual Mock<DbSet<Holding>> MockHolding { get; set; }

        public virtual Mock<DbSet<Inspector>> MockInspector { get; set; }

        public virtual Mock<DbSet<LinkedHolding>> MockLinkedHolding { get; set; }

        public virtual Mock<DbSet<PackRequested>> MockPackRequested { get; set; }

        public virtual Mock<DbSet<SchemeNameDrop>> MockSchemeNameDrop { get; set; }

        public virtual Mock<DbSet<BcmsRequested>> MockBcmsRequested { get; set; }

        public MockWorkbenchContext(bool setMocks = true)
        {
            if (setMocks)
            {
                SetMocks();
            }
        }

        public void SetMocks()
        {
            SetMockContext();
            SetMockBreed();
            SetMockCattle();
            SetMockControl();
            SetMockHolding();
            SetMockInspector();
            SetMockLinkedHolding();
            SetMockPackRequested();
            SetMockSchemeNameDrop();
            SetMockBcmsRequested();

        }



        public void SetMockBreed()
        {
            MockBreed = new Mock<DbSet<Breed>>().SetupData(BreedData.Data());
            MockContext.Setup(x => x.Breed).Returns(MockBreed.Object);
        }

        public void SetMockCattle()
        {
            MockCattle = new Mock<DbSet<Cattle>>().SetupData(CattleData.Data());
            MockContext.Setup(x => x.Cattle).Returns(MockCattle.Object);
        }

        public void SetMockControl()
        {
            MockControl = new Mock<DbSet<Control>>().SetupData(ControlData.Data());
            MockContext.Setup(x => x.Control).Returns(MockControl.Object);
        }

        public void SetMockHolding()
        {
            MockHolding = new Mock<DbSet<Holding>>().SetupData(HoldingData.Data());
            MockContext.Setup(x => x.Holding).Returns(MockHolding.Object);
        }

        public void SetMockInspector()
        {
            MockInspector = new Mock<DbSet<Inspector>>().SetupData(InspectorData.Data());
            MockContext.Setup(x => x.Inspector).Returns(MockInspector.Object);
        }

        public void SetMockLinkedHolding()
        {
            MockLinkedHolding = new Mock<DbSet<LinkedHolding>>().SetupData(LinkedHoldingData.Data());
            MockContext.Setup(x => x.LinkedHolding).Returns(MockLinkedHolding.Object);
        }

        public void SetMockPackRequested()
        {
            MockPackRequested = new Mock<DbSet<PackRequested>>().SetupData(PackRequestedData.Data());
            MockContext.Setup(x => x.PackRequested).Returns(MockPackRequested.Object);
        }

        public void SetMockSchemeNameDrop()
        {
            MockSchemeNameDrop = new Mock<DbSet<SchemeNameDrop>>().SetupData(SchemeNameDropData.Data());
            MockContext.Setup(x => x.SchemeNameDrop).Returns(MockSchemeNameDrop.Object);
        }

        public void SetMockBcmsRequested()
        {
            MockBcmsRequested = new Mock<DbSet<BcmsRequested>>().SetupData(BcmsRequestedData.Data());
            MockContext.Setup(x => x.BcmsRequested).Returns(MockBcmsRequested.Object);
        }

        public void SetMockContext()
        {
            MockContext = new Mock<IWorkbenchContext>();
        }
    }
}
