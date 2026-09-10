using Application.DTOs.Email;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces;

public interface IEmailBackgroundTaskQueue
{
    Task EnqueueAsync(EmailDto task, CancellationToken ct = default);
    Task<EmailDto> DequeueAsync(CancellationToken ct);
}
