using System.ComponentModel.DataAnnotations;

namespace AIAgentConversation.Models;

public class Message
{
    public Guid Id { get; set; }
    
    [Required]
    public Guid ConversationId { get; set; }
    
    [Required]
    [MaxLength(10)]
    public string AgentType { get; set; } = string.Empty;
    
    public int IterationNumber { get; set; }
    
    public ConversationPhase Phase { get; set; }
    
    [Required]
    public string Content { get; set; } = string.Empty;
    
    public DateTime Timestamp { get; set; }
    
    public Conversation Conversation { get; set; } = null!;
}
