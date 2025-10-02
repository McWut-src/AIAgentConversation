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
    
    // Number of back-and-forth exchanges in conversation phase (default: 3)
    // Total messages will be: 2 (intro) + (ConversationLength * 2) + 2 (conclusion)
    [Range(1, 10)]
    public int? ConversationLength { get; set; } = 3;
}
