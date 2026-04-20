using DataLayer;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Reflection;
using task;
using task.Model.Settings;
using task.Repository;
using task.Servicies;

internal class Program
{
    private static async Task Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .CreateLogger();

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
        builder.ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddConsole();
            }
        );
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
            services.AddLogging(configure => configure.AddSerilog());

            var postgrecs = hostContext.Configuration.GetConnectionString("DellinDictionaryConnection");
            services.AddDbContext<DellinDictionaryDbContext>(options =>
                options.UseNpgsql(postgrecs, b => b.MigrationsAssembly("DataLayer")));

            services.AddTransient<IJsonRepository, JsonRepository>();
            services.AddTransient<IDbRepository, EfRepository>();
            services.AddHostedService<Worker>();
        });

        builder.UseSerilog((hostContext, logger) =>
        {
            logger.ReadFrom.Configuration(hostContext.Configuration);
        });

        return builder;
    }
}