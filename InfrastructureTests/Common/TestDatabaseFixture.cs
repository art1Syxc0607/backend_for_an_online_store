using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InfrastructureTests.Common;

public class TestDatabaseFixture : IDisposable
{
    public AppDbContext Context { get; }

    public TestDatabaseFixture()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        Context = new AppDbContext(options);
    }

    // Вместо ExecuteDeleteAsync используем RemoveRange
    public void Cleanup()
    {
        Context.Categories.RemoveRange(Context.Categories);
        //Context.Products.RemoveRange(Context.Products);
        //Context.Users.RemoveRange(Context.Users);
        //Context.Orders.RemoveRange(Context.Orders);
        Context.SaveChanges();
    }

    public void Dispose()
    {
        Context.Database.EnsureDeleted();
        Context.Dispose();
    }
}