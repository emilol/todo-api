using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TodoApi.Data;

namespace TodoApi
{
    public class Startup
    {
        public Startup(IHostingEnvironment env, IConfiguration configuration)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddConfiguration(configuration)
                .AddEnvironmentVariables();

            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddMvc();

            var settings = Configuration.GetSection("Database").Get<DatabaseSettings>();

            if (settings.UseInMemoryDatabase)
            {
                services.AddEntityFrameworkInMemoryDatabase()
                    .AddDbContext<TodoDbContext>(options =>
                    {
                        options.UseInMemoryDatabase(settings.DatabaseName);
                    });
            }
            else
            {
                services.AddEntityFrameworkSqlServer()
                    .AddDbContext<TodoDbContext>(options =>
                    {
                        options.UseSqlServer(
                            Configuration.GetConnectionString(settings.ConnectionStringName),
                            builder => builder.UseRowNumberForPaging()
                        );
                    });
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, TodoDbContext context)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            app.UseMvc();
        }
    }
}
