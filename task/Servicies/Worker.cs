using Microsoft.Extensions.Options;
using System.Runtime;
using task.Model.Settings;
using task.Repository;

namespace task.Servicies;

public class Worker(ILogger<Worker> logger, IOptions<WorkerSettings> wSettings, IServiceScopeFactory serviceScopeFactory) : BackgroundService
{
    private readonly ILogger<Worker> _logger = logger;
    private readonly WorkerSettings _wSettings = wSettings.Value;
    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                var runat = _wSettings.RunAt;

                _logger.LogInformation($"Worker running at: {DateTimeOffset.Now}");

                await DoWorkAsync(stoppingToken);

                var waitTime =  runat.ToTimeSpan() - DateTime.Now.TimeOfDay;
                _logger.LogInformation($"Worker stop at: {DateTimeOffset.Now} delay {waitTime}");
                await Task.Delay(waitTime.Ticks < 0 ? waitTime.Add(TimeSpan.FromHours(24)) : waitTime, stoppingToken);

                //if(runat.Hour == DateTime.Now.TimeOfDay.Hours)
                //{
                //    _logger.LogInformation($"Worker running at: {DateTimeOffset.Now}");

                //    await DoWorkAsync(stoppingToken);

                //    var waitTime = runat.ToTimeSpan() - DateTime.Now.TimeOfDay;
                //    _logger.LogInformation($"Worker stop at: {DateTimeOffset.Now} delay {waitTime}");
                //    await Task.Delay(waitTime, stoppingToken);
                //}
            }
        }
    }

    private async Task DoWorkAsync(CancellationToken stoppingToken)
    {
        await Task.Run(() =>
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var dbRepository = scope.ServiceProvider.GetRequiredService<IDbRepository>();
            dbRepository.ImportOfficess("task.files.terminals.json", stoppingToken);
        }, stoppingToken);
    }
}
