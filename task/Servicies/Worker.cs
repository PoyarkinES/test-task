using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System.Runtime;
using task.Model.Settings;
using task.Repository;

namespace task.Servicies;

public class Worker(ILogger<Worker> logger, IOptions<WorkerSettings> wSettings, 
    IServiceScopeFactory serviceScopeFactory, IHostApplicationLifetime hostApplicationLifetime) : BackgroundService
{
    private readonly ILogger<Worker> _logger = logger;
    private readonly WorkerSettings _wSettings = wSettings.Value;
    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;
    private readonly IHostApplicationLifetime _hostApplicationLifetime = hostApplicationLifetime;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("Worker running at: {DateTimeOffset}", DateTimeOffset.Now);
                    var runat = _wSettings.RunAt;
                    TimeSpan waitTime;
                    if (runat.Hour == DateTime.Now.TimeOfDay.Hours)
                    {
                        await DoWorkAsync(stoppingToken);
                        waitTime = runat.ToTimeSpan() - DateTime.Now.TimeOfDay;
                        _logger.LogInformation("Worker stop at: {DateTimeOffset} delay {waitTime}", DateTimeOffset.Now, waitTime);
                        await Task.Delay(waitTime.Ticks < 0 ? waitTime.Add(TimeSpan.FromHours(24)) : waitTime, stoppingToken);
                    }
                    else
                    {
                        waitTime = runat.ToTimeSpan() - DateTime.Now.TimeOfDay;
                        _logger.LogInformation("Worker stop at: {DateTimeOffset} delay {waitTime}", DateTimeOffset.Now, waitTime);
                        await Task.Delay(waitTime.Ticks < 0 ? waitTime.Add(TimeSpan.FromHours(24)) : waitTime, stoppingToken);
                    }
                }
            }
        }
        finally {
            _logger.LogCritical("Exiting application...");
            _hostApplicationLifetime.StopApplication();
        }
    }

    private async Task DoWorkAsync(CancellationToken stoppingToken)
    {
        try
        {
            await Task.Run(() =>
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var dbRepository = scope.ServiceProvider.GetRequiredService<IDbRepository>();
                dbRepository.ImportOfficess("task.files.terminals.json", stoppingToken);
            }, stoppingToken);
        }
        catch (Exception ex) {
            _logger.LogError("Ошибка импорта {Message}", ex.Message);
        }
    }
}
