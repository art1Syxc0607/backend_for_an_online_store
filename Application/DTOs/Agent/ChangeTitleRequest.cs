using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Agent;

public class ChangeTitleRequest
{
    [Required(ErrorMessage = "NewTitle is required")]
    [MaxLength(200, ErrorMessage = "NewTitle is too long (max 200)")]
    public string NewTitle { get; set; } = string.Empty;
}