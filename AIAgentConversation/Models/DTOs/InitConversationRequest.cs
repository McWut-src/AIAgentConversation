using System.ComponentModel.DataAnnotations;

namespace AIAgentConversation.Models.DTOs;

public class InitConversationRequest
{
    [Required]
    [MaxLength(500)]
    public string Agent1Personality { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(500)]
    public string Agent2Personality { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(1000)]
    public string Topic { get; set; } = string.Empty;
    
    [MaxLength(20)]
    public string? PolitenessLevel { get; set; } = "medium";
}
