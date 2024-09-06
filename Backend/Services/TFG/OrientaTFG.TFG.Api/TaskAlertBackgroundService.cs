using OrientaTFG.TFG.Core;

namespace OrientaTFG.TFG.Api;

public class TaskAlertBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<TaskAlertBackgroundService> _logger;

    public TaskAlertBackgroundService(IServiceScopeFactory serviceScopeFactory, ILogger<TaskAlertBackgroundService> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Task Alert Background Service is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var tfgManager = scope.ServiceProvider.GetRequiredService<ITFGManager>();

                    _logger.LogInformation($"Checking and sending task alerts at {DateTime.Now}");
                    await tfgManager.CheckAndSendTaskAlerts();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while executing the task alert background service.");
            }

            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }

        _logger.LogInformation("Task Alert Background Service is stopping.");
    }
}
