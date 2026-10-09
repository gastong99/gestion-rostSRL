using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using TransformadoresApp.Data;

namespace TransformadoresApp.Tests.Infrastructure
{
    public class TestDbContextFactory : IDisposable
    {
        private readonly SqliteConnection _connection;

        public TestDbContextFactory()
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            using var context = CreateContext();

            context.Database.EnsureCreated();
        }

        public ApplicationDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlite(_connection).Options;

            return new ApplicationDbContext(options);
        }

        public void Dispose()
        {
            _connection.Dispose();
        }
    }
}