using MCBA.Data;
using Microsoft.EntityFrameworkCore;

namespace MCBA.BackgroundServices;
//  background service for billpay

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

        var billPay = await context.BillPays.ToListAsync(cancellationToken);
        foreach (var bill in billPay)
        {
            _logger.LogInformation(bill.ToString());
        }

        await context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("BillPay Background Service work complete.");
    }
}