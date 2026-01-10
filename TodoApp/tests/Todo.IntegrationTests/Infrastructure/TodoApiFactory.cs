using System.Data.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Todo.Infrastructure.Persistence;

namespace Todo.IntegrationTests.Infrastructure;

public sealed class TodoApiFactory : WebApplicationFactory<Program>
{
    private DbConnection? _connection;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove existing DbContext registration
            var descriptor = services.SingleOrDefault(d =>
                d.ServiceType == typeof(DbContextOptions<TodoDbContext>));

            if (descriptor is not null)
                services.Remove(descriptor);

            // Create and keep open a single SQLite in-memory connection for the test host lifetime
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            services.AddDbContext<TodoDbContext>(options =>
            {
                options.UseSqlite(_connection);
            });

            // Ensure schema exists
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<TodoDbContext>();
            db.Database.EnsureCreated();
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            _connection?.Dispose();
            _connection = null;
        }
    }
}
