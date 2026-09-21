using Moq;
using NUnit.Framework;
using RPA.Inspections.Workbench.DAL;
using RPA.Inspections.Workbench.Models;
using RPA.Inspections.Workbench.SL;
using RPA.Inspections.Workbench.Tests.DAL.Mock;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPA.Inspections.Workbench.Tests.SL
{
    [TestFixture]
    [Category("Breed Service")]
    public class BreedServiceTests
    {


        MockWorkbenchContext context;
        
        IBreedService breedService;

        [SetUp]

        public void Setup()
        {
            context = new MockWorkbenchContext();
            breedService = new BreedService(context.MockContext.Object);
        }

        [Test]
        public void Test_GetBreedById_Gets_Breed()
        {
            //Arrange
            //Already done in setup

   

            //Act
            var result = breedService.GetBreedById(Guid.Parse("C741904A-5C65-4D12-848D-284BF2D4AF79"));

            //Assert
           Assert.AreEqual("Mega Cow", result.breedFull);
       
        }

        [Test]
        public void Test_GetBreeds_Returns_List_of_Breeds()
        {
            //Arrange
            //Already Done in Setup


            //Act
            var result = breedService.GetBreeds(null,1,1);

            //Assert

          Assert.AreEqual(4, result.TotalItemCount);
        }

    
    }
}
