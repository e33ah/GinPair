using Xunit;
using GinPair.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using GinPair.Models;
using GinPair.Data;

namespace GinPair.Tests
{
    public class HomeControllerTests
    {
        private DbContextOptions<GinPairDbContext> GetDbContextOptions()
        {
            return new DbContextOptionsBuilder<GinPairDbContext>().UseInMemoryDatabase(databaseName: "TestDatabase").Options;
        }

        [Fact]
        public async Task Index_Returns_ViewResult_With_ViewModel()
        {
            // Arrange - configure mock in-memory db
            var options = GetDbContextOptions();
            using var mockContext = new GinPairDbContext(options);
            var controller = new HomeController(mockContext);

            // Act - call Index action of HomeController
            var result = await controller.Index(null);

            // Assert result of the Index action is of type ViewResult (subclass of ActionResult class) and model is a Pairing VM
            var viewResult = Assert.IsType<ViewResult>(result); 
            var modelResult = Assert.IsType<PairingVM>(viewResult.Model); 
            Assert.Null(viewResult.ViewName); // ViewName will be null if default View is being returned (Index)
        }
        [Fact]
        public async Task Index_With_EmptySearch_Returns_ViewResult()
        {
            var options = GetDbContextOptions();
            using var mockContext = new GinPairDbContext(options);
            var controller = new HomeController(mockContext);

            var result = await controller.Index("");

            // Assert 
            var viewResult = Assert.IsType<ViewResult>(result); 
            Assert.Null(viewResult.ViewName); 
        }
        [Fact]
        public async Task Index_WithNoResults_Returns_Message()
        {
            var options = GetDbContextOptions();
            using var context = new GinPairDbContext(options);

            context.Gins.AddRange(new List<Gin>
            {
                new Gin {GinId = 1, GinName = "TestName1", Distillery = "TestDis1"},
                new Gin {GinId = 2, GinName = "TestName2", Distillery = "TestDis2"}
            });
            context.SaveChanges();

            var controller = new HomeController(context);

            var result = await controller.Index("NonExistant");

            var viewResult = Assert.IsType<ViewResult>(result);
            var modelResult = Assert.IsType<PairingVM>(viewResult.Model);
            Assert.Equal(
                "Sorry, a gin matching \"NonExistant\" was not found!<br>Try searching again, or <a href='/Home/AddGnt/'>add it</a> to our collection.", 
                modelResult.Message
                );
        }
    }
}


