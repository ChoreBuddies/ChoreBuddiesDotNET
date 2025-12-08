using ChoreBuddies.Backend.Domain;
using ChoreBuddies.Backend.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace ChoreBuddies.Tests.Infrastructure;

public abstract class BaseIntegrationTest : IClassFixture<IntegrationTestWebAppFactory>
{
    private readonly IServiceScope _scope;
    protected readonly ChoreBuddiesDbContext DbContext;
    protected readonly UserManager<AppUser> UserManager;

    protected BaseIntegrationTest(IntegrationTestWebAppFactory factory)
    {
        _scope = factory.Services.CreateScope();

        DbContext = _scope.ServiceProvider.GetRequiredService<ChoreBuddiesDbContext>();
        UserManager = _scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
    }
}
