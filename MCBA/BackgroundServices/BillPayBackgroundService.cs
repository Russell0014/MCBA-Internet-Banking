using MCBA.Data;
using MCBA.Services;

namespace MCBA.BackgroundServices;
//  background service for BillPay

public class BillPayBackgroundService : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<BillPayBackgroundService> _logger;

    public BillPayBackgroundService(IServiceProvider services, ILogger<BillPayBackgroundService> logger)
    {
        _services = services;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("BillPay Background Service is running.");

        while (!cancellationToken.IsCancellationRequested)
        {
            await BillPayAsync(cancellationToken);

            _logger.LogInformation("BillPay Background Service is waiting a minute.");

            await Task.Delay(TimeSpan.FromMinutes(1), cancellationToken);
        }
    }

    private async Task BillPayAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("BillPay Background Service is working.");

        using var scope = _services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DatabaseContext>();

        // Instantiate BillPayService and process all pending bill payments
        var billPayService = new BillPayService(context);
        await billPayService.PayBillsAsync(cancellationToken);

        _logger.LogInformation("BillPay Background Service work complete.");
    }
}