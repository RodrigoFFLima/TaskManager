using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using TaskManager.Application.Interfaces;
using TaskManager.Infrastructure.Data;

namespace TaskManager.Api.Tests.Helpers;

public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    public Mock<ITaskService> TaskServiceMock { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // Replace database migrator with no-op so startup doesn't require PostgreSQL
            services.RemoveAll<IDatabaseMigrator>();
            services.AddSingleton<IDatabaseMigrator>(new Mock<IDatabaseMigrator>().Object);

            // Replace ITaskService with mock (controller under test)
            services.RemoveAll<ITaskService>();
            services.AddScoped(_ => TaskServiceMock.Object);
        });
    }
}
