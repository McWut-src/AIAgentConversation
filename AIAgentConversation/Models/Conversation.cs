using System.ComponentModel.DataAnnotations;

namespace AIAgentConversation.Models;

public class Conversation
{
    public Guid Id { get; set; }
    
    [Required]
    [MaxLength(500)]
    public string Agent1Personality { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(500)]
    public string Agent2Personality { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(1000)]
    public string Topic { get; set; } = string.Empty;
    
    public int IterationCount { get; set; } = 3;
    
    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = "InProgress";
    
    public DateTime StartTime { get; set; }
    
    public DateTime? EndTime { get; set; }
    
    public ICollection<Message> Messages { get; set; } = new List<Message>();
}
