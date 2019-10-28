using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TodoApi.Data;
using TodoApi.UnitTests.Plumbing;

namespace TodoApi.UnitTests.Controllers
{
    public class TestWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
    {
        protected Action<TodoDbContext> DatabaseSeedingDelegate { get; set; }
        protected Action<IServiceCollection> ReplaceServicesDelegate { get; set; }
        // If you prefer Autofac...
        // protected Action<ContainerBuilder> ReplaceModulesDelegate { get; set; }
        
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder
                .ConfigureAppConfiguration(Configure)
                .ConfigureTestServices(services =>
                {
                    // Add replacements here, e.g:
                    // services.Replace(ServiceDescriptor.Scoped<IService, MockedService>());

                    ReplaceAuthentication(services);
                    ReplaceServicesDelegate?.Invoke(services);
                });

            // If you prefer Autofac...
            //builder
            //    .ConfigureTestContainer<ContainerBuilder>(container =>
            //    {
            //        ReplaceModulesDelegate?.Invoke(container);
            //    });
        }


        private void ReplaceAuthentication(IServiceCollection services)
        {
            // TODO: add ability to mock user for each test
            // services.Replace(new ServiceDescriptor(typeof(IAuthenticationSchemeProvider), mock));
        }

        private static void Configure(WebHostBuilderContext arg1, IConfigurationBuilder builder)
        {
            builder
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false, true)
                .AddInMemoryCollection(new[]
                {
                    new KeyValuePair<string, string> ("Database:DatabaseName", Guid.NewGuid().ToString()) // New DB per test
                });
        }

        public TestWebApplicationFactory<TStartup> SeedDatabaseWith(
            Action<TodoDbContext> databaseSeedingDelegate)
        {
            DatabaseSeedingDelegate = databaseSeedingDelegate;
            return this;
        }

        public TestWebApplicationFactory<TStartup> ReplaceServices(
            Action<IServiceCollection> replaceServicesDelegate)
        {
            ReplaceServicesDelegate = replaceServicesDelegate;
            return this;
        }

        // If you prefer Autofac...
        //public TestWebApplicationFactory<TStartup> ReplaceModules(
        //    Action<ContainerBuilder> replaceModulesDelegate)
        //{
        //    ReplaceModulesDelegate = replaceModulesDelegate;
        //    return this;
        //}

        public TEntity Find<TEntity>(params object[] keyValues) where TEntity : class
        {
            var sp = Server.Host.Services;
            using (var scope = sp.CreateScope())
            {
                var scopedServices = scope.ServiceProvider;
                var db = scopedServices.GetRequiredService<TodoDbContext>();

                return db.Find<TEntity>(keyValues);
            }
        }
        public TEntity Find<TEntity>(Expression<Func<TEntity, bool>> predicate) where TEntity : class
        {
            var sp = Server.Host.Services;
            using (var scope = sp.CreateScope())
            {
                var scopedServices = scope.ServiceProvider;
                var db = scopedServices.GetRequiredService<TodoDbContext>();

                return db.Set<TEntity>().Where(predicate).FirstOrDefault();
            }
        }

        protected override TestServer CreateServer(IWebHostBuilder builder)
        {
            var server = base.CreateServer(builder);
            return server;
        }

        protected override void ConfigureClient(HttpClient client)
        {
            base.ConfigureClient(client);

            var sp = Server.Host.Services;

            using (var scope = sp.CreateScope())
            {
                var scopedServices = scope.ServiceProvider;
                var db = scopedServices.GetRequiredService<TodoDbContext>();

                db.ResetValueGenerators();
                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();

                DatabaseSeedingDelegate?.Invoke(db);
            }
        }
    }
}