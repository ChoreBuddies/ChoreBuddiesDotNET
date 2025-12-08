using ChoreBuddies.Backend;
using ChoreBuddies.Backend.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.MsSql;

namespace ChoreBuddies.Tests.Infrastructure;

public class IntegrationTestWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public MsSqlContainer DbContainer = new MsSqlBuilder()
        .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
        .WithPortBinding(1435, true)
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(s => s.ServiceType == typeof(DbContextOptions<ChoreBuddiesDbContext>));

            if (descriptor is not null)
            {
                services.Remove(descriptor);
            }

            services.AddDbContext<ChoreBuddiesDbContext>(options =>
            {
                options.UseSqlServer(DbContainer.GetConnectionString());
                new DbSeeder().SetUpDbSeeding(options);
            });
        });
    }
    public Task InitializeAsync()
    {
        return DbContainer.StartAsync();
    }

    public new Task DisposeAsync()
    {
        return DbContainer.DisposeAsync().AsTask();
    }
}
