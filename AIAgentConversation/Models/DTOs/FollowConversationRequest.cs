using System.ComponentModel.DataAnnotations;

namespace AIAgentConversation.Models.DTOs;

public class FollowConversationRequest
{
    [Required]
    public Guid ConversationId { get; set; }
}
