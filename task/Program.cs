using DataLayer;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using task;
using task.Model.Settings;
using task.Repository;
using task.Servicies;

internal class Program
{
    private static async Task Main(string[] args)
    {
       await CreateHostBuilder(args).Build().RunAsync();
    }

    public static IHostBuilder CreateHostBuilder(string[] args)
    {
        var builder = Host.CreateDefaultBuilder(args);
        builder.UseWindowsService();
        builder.ConfigureWebHostDefaults(webHostBuilder =>
        {
            webHostBuilder
                .UseStartup<Startup>()
                .UseKestrel();
        });
        builder.ConfigureServices((hostContext, services) =>
        {
            services.Configure<WorkerSettings>(hostContext.Configuration.GetSection("Worker"));
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(options =>
            {
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                options.IncludeXmlComments(xmlPath);
            });

            var postgrecs = hostContext.Configuration.GetConnectionString("DellinDictionaryConnection");
            services.AddDbContext<DellinDictionaryDbContext>(options =>
                options.UseNpgsql(postgrecs, b=> b.MigrationsAssembly("DataLayer"))
            );

            services.AddTransient<IJsonRepository, JsonRepository>();
            services.AddTransient<IDbRepository, EfRepository>();
            services.AddHostedService<Worker>();
        });

        return builder;
    }
}