using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GinPair.Controllers;
using GinPair.Data;
using GinPair.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace GinPair.Tests;
public class GetGinTests
{
    private DbContextOptions<GinPairDbContext> GetDbContextOptions()
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
            new Gin {GinId = 1, GinName = "TestName1", Distillery = "TestDis1"},
            new Gin {GinId = 2, GinName = "TestName2", Distillery = "TestDis2"}
        });
        mockContext.SaveChanges();
        var controller = new GinApiController(mockContext);

        var result = await controller.MatchGin("Test");

        Assert.NotNull(result);
        var okResult = Assert.IsType<OkObjectResult>(result);
    }
}
