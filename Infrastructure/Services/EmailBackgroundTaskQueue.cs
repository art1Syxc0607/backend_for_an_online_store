using Application.DTOs.Email;
using Application.Interfaces;
using Domain.Exceptions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Infrastructure.Services;

public class EmailBackgroundTaskQueue : IEmailBackgroundTaskQueue
{
    private readonly Channel<EmailDto> _queue;
    private readonly ILogger<EmailBackgroundTaskQueue> _logger;

    public EmailBackgroundTaskQueue(ILogger<EmailBackgroundTaskQueue> logger, int capacity = 100)
    {
        _logger = logger;
        var options = new BoundedChannelOptions(capacity)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,  // ✅ Только один читатель (BackgroundService)
            SingleWriter = false  // ✅ Много писателей (handlers)
        };
        _queue = Channel.CreateBounded<EmailDto>(options);
    }

    public async Task EnqueueAsync(EmailDto task, CancellationToken ct = default)
    {
        if (task == null)
            throw new DomainException("EmailDto is null");

        try
        {
            await _queue.Writer.WriteAsync(task, ct);
            _logger.LogDebug("Email task enqueued for {To}", task.To);
        }
        catch (ChannelClosedException)
        {
            _logger.LogWarning("Email queue is closed");
            throw;
        }
    }

    public async Task<EmailDto> DequeueAsync(CancellationToken ct)
    {
        return await _queue.Reader.ReadAsync(ct);
    }
}
