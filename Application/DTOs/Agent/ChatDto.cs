using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Agent;

public class ChatDto
{
    [Required]
    [MaxLength(4000)]
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// ID сессии. Если null — создаётся новый диалог.
    /// </summary>
    public string? SessionId { get; set; }
}
