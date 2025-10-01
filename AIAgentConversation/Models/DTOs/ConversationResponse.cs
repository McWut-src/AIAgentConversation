namespace AIAgentConversation.Models.DTOs;

public class ConversationResponse
{
    public Guid ConversationId { get; set; }
    public string Message { get; set; } = string.Empty;
    public string AgentType { get; set; } = string.Empty;
    public int IterationNumber { get; set; }
    public bool IsOngoing { get; set; }
    public int TotalMessages { get; set; }
}
