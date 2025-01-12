using GinPair.Controllers;
using GinPair.Data;
using GinPair.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GinPair.Tests;
public class GetGinTests
{
    private static DbContextOptions<GinPairDbContext> GetDbContextOptions()
    {
        return new DbContextOptionsBuilder<GinPairDbContext>().UseInMemoryDatabase(databaseName: "TestDatabase").Options;
    }

    [Fact]
    public async Task MatchPartial_ReturnsOk()
    {
        var options = GetDbContextOptions();
        using var mockContext = new GinPairDbContext(options);
        mockContext.Gins.AddRange(new List<Gin>
        {
            new() {GinId = 1, GinName = "TestName1", Distillery = "TestDis1"},
            new() {GinId = 2, GinName = "TestName2", Distillery = "TestDis2"}
        });
        mockContext.SaveChanges();
        var controller = new GinApiController(mockContext);

        var result = await controller.MatchGin("Test");

        Assert.NotNull(result);
        var okResult = Assert.IsType<OkObjectResult>(result);
    }
    [Fact]
    public async Task GetPairingById_ReturnsOk()
    {
        var options = GetDbContextOptions();
        using var mockContext = new GinPairDbContext(options);
        mockContext.Pairings.AddRange(new List<Pairing>
        {
            new() {
                PairedGin = new Gin {GinId = 3, GinName = "TestName3", Distillery = "TestDis3"},
                PairedTonic = new Tonic {TonicId = 3, TonicBrand = "TestBrand3", TonicFlavour = "TestFl3"}
            }
        });
        mockContext.SaveChanges();
        var controller = new GinApiController(mockContext);

        var result = await controller.GetPairingByGinId(3);

        Assert.NotNull(result);
        var okResult = Assert.IsType<OkObjectResult>(result);
        var message = Assert.IsType<string>(okResult.Value);
    }
}
