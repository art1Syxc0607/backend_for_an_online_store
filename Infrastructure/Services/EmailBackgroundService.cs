using Application.DTOs.Email;
using Application.Interfaces;
using Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Infrastructure.Services;

public class EmailBackgroundService : BackgroundService, IEmailBackgroundService
{
    private readonly IEmailBackgroundTaskQueue _taskQueue;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<EmailBackgroundService> _logger;

    public EmailBackgroundService(IEmailBackgroundTaskQueue taskQueue, ILogger<EmailBackgroundService> logger,
        IServiceProvider serviceProvider)
    {
        _taskQueue = taskQueue;
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    // Метод для добавления задачи в очередь (вызывается из бизнес-логики)
    public async Task Enqueue(EmailDto task)
    {
        await _taskQueue.EnqueueAsync(task);
        _logger.LogInformation("Email task enqueued for {To}", task.To);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("📧 Email Background Service started");

        // Читаем из очереди бесконечно
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
                    var task = await _taskQueue.DequeueAsync(stoppingToken);
                    await emailService.SendEmailAsync(task);
                    _logger.LogInformation("Email sent to {To}", task.To);
                }
            }
            catch (OperationCanceledException)
            {
                break; // Нормальное завершение
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email, reason - {ex.Message}", ex.Message);
                // Можно добавить повторные попытки (Retry)
            }
        }

        _logger.LogInformation("📧 Email Background Service stopped");
    }
}
