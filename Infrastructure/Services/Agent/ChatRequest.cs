using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services.Agent;

public class ChatRequest
{
    [Required]
    [MaxLength(4000)]
    public string Message { get; init; } = string.Empty;

    /// <summary>
    /// ID сессии. Если null — создаётся новый диалог.
    /// </summary>
    public string? SessionId { get; init; }

    /// <summary>
    /// Заполняется контроллером из JWT.
    /// </summary>
    //[JsonIgnore]
    public int UserId { get; init; }
};
