using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.OpenApi.Writers;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using NewAuthenticationWebAPI;
using NewAuthenticationWebAPI.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationTest
{
    internal class TestServer<TStartup> : WebApplicationFactory<TStartup> where TStartup : Startup
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            //builder.UseEnvironment("Development");

            builder.ConfigureServices(services =>
            {
                services.RemoveAll(typeof(DbContextOptions<AppDbContext>));

                /*var connString = GetConnectionString();
                services.AddSqlServer<AppDbContext>(connString);

                services.AddIdentity<IdentityUser, IdentityRole>()
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();

                var dbContext = GetDbContext(services);
                dbContext.Database.EnsureDeleted();*/


                services.AddDbContext<AppDbContext>(options =>
                {
                    options.UseInMemoryDatabase("InMemoryDatabaseForTesting");
                });

                services.AddIdentity<IdentityUser, IdentityRole>()
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();

                services.AddAuthentication("testScheme").AddScheme<AuthenticationSchemeOptions, TestAuthHandler>
                ("testScheme", option => { });

                var serviceProvider = new ServiceCollection().AddEntityFrameworkInMemoryDatabase()
                                                             .BuildServiceProvider();


                using (var scope = services.BuildServiceProvider().CreateScope())
                {
                    var scopedServices = scope.ServiceProvider;
                    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    var userManager = scopedServices.GetRequiredService<UserManager<IdentityUser>>();
                    var roleManager = scopedServices.GetService<RoleManager<IdentityRole>>();
                    
                    dbContext.Database.EnsureCreated();
                }
            });

        }

        private static string? GetConnectionString()
        {
            var config = new ConfigurationBuilder().AddUserSecrets<WebApplicationFactory<TStartup>>().Build();
            var connString = "Server=.\\SQLEXPRESS;Database=TestNewAuthWebAPI;Trusted_Connection=True;TrustServerCertificate=True;";
            return connString;
        }

        private static AppDbContext GetDbContext(IServiceCollection services)
        {
            var serviceProvider = services.BuildServiceProvider();
            var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            return dbContext;
        }
    }
}
