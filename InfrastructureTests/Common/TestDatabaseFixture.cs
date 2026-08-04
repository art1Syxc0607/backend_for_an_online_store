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

    public void Dispose()
    {
        Context.Database.EnsureDeleted();
        Context.Dispose();
    }
}