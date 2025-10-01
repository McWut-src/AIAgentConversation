using AIAgentConversation.Data;
using AIAgentConversation.Models;
using AIAgentConversation.Models.DTOs;
using AIAgentConversation.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AIAgentConversation.Controllers;

[ApiController]
[Route("api/conversation")]
public class ConversationController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IOpenAIService _openAIService;
    private readonly ILogger<ConversationController> _logger;

    public ConversationController(
        ApplicationDbContext context,
        IOpenAIService openAIService,
        ILogger<ConversationController> logger)
    {
        _context = context;
        _openAIService = openAIService;
        _logger = logger;
    }

    [HttpPost("init")]
    public async Task<IActionResult> Init([FromBody] InitConversationRequest request)
    {
        try
        {
            // Validate input
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Trim inputs
            request.Agent1Personality = request.Agent1Personality?.Trim() ?? string.Empty;
            request.Agent2Personality = request.Agent2Personality?.Trim() ?? string.Empty;
            request.Topic = request.Topic?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(request.Agent1Personality) ||
                string.IsNullOrWhiteSpace(request.Agent2Personality) ||
                string.IsNullOrWhiteSpace(request.Topic))
            {
                return BadRequest(new { error = "Invalid input", message = "All fields are required" });
            }

            // Create conversation
            var conversation = new Conversation
            {
                Id = Guid.NewGuid(),
                Agent1Personality = request.Agent1Personality,
                Agent2Personality = request.Agent2Personality,
                Topic = request.Topic,
                IterationCount = 3,
                Status = "InProgress",
                StartTime = DateTime.UtcNow
            };

            _context.Conversations.Add(conversation);

            // Call OpenAI for Agent 1 with empty history
            var messageContent = await _openAIService.GenerateResponseAsync(
                request.Agent1Personality,
                request.Topic,
                "");

            // Save first message
            var message = new Message
            {
                Id = Guid.NewGuid(),
                ConversationId = conversation.Id,
                AgentType = "A1",
                IterationNumber = 1,
                Content = messageContent,
                Timestamp = DateTime.UtcNow
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Conversation {ConversationId} initialized with first message from A1", conversation.Id);

            // Return response with isOngoing=true
            return Ok(new ConversationResponse
            {
                ConversationId = conversation.Id,
                Message = messageContent,
                AgentType = "A1",
                IterationNumber = 1,
                IsOngoing = true,
                TotalMessages = 1
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing conversation");
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }

    [HttpPost("follow")]
    public async Task<IActionResult> Follow([FromBody] FollowConversationRequest request)
    {
        try
        {
            // Validate input
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Retrieve conversation with messages
            var conversation = await _context.Conversations
                .Include(c => c.Messages)
                .FirstOrDefaultAsync(c => c.Id == request.ConversationId);

            if (conversation == null)
                return NotFound(new { error = "Conversation not found" });

            if (conversation.Status == "Completed")
                return BadRequest(new { error = "Conversation already completed" });

            // Calculate next agent and iteration
            var messageCount = conversation.Messages.Count;
            var nextAgent = messageCount % 2 == 1 ? "A2" : "A1";
            var iterationNumber = (messageCount / 2) + 1;

            // Build concatenated history
            var history = string.Join("\n",
                conversation.Messages
                    .OrderBy(m => m.Timestamp)
                    .Select(m => $"{m.AgentType}: {m.Content}"));

            // Get next agent's personality
            var personality = nextAgent == "A1"
                ? conversation.Agent1Personality
                : conversation.Agent2Personality;

            // Call OpenAI for next agent
            var messageContent = await _openAIService.GenerateResponseAsync(
                personality,
                conversation.Topic,
                history);

            // Save new message
            var message = new Message
            {
                Id = Guid.NewGuid(),
                ConversationId = conversation.Id,
                AgentType = nextAgent,
                IterationNumber = iterationNumber,
                Content = messageContent,
                Timestamp = DateTime.UtcNow
            };

            _context.Messages.Add(message);

            // Check completion
            var totalMessages = messageCount + 1;
            var isOngoing = totalMessages < 6;

            if (totalMessages == 6)
            {
                conversation.Status = "Completed";
                conversation.EndTime = DateTime.UtcNow;
                _logger.LogInformation("Conversation {ConversationId} completed", conversation.Id);
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Added message {MessageCount} ({AgentType}) to conversation {ConversationId}",
                totalMessages, nextAgent, conversation.Id);

            return Ok(new ConversationResponse
            {
                ConversationId = conversation.Id,
                Message = messageContent,
                AgentType = nextAgent,
                IterationNumber = iterationNumber,
                IsOngoing = isOngoing,
                TotalMessages = totalMessages
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error continuing conversation");
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(Guid id)
    {
        try
        {
            // Retrieve conversation with messages
            var conversation = await _context.Conversations
                .Include(c => c.Messages)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (conversation == null)
                return NotFound(new { error = "Conversation not found" });

            // Only return if Status == "Completed"
            if (conversation.Status != "Completed")
                return NotFound(new { error = "Conversation not completed" });

            // Format messages as markdown
            var markdown = string.Join("\n",
                conversation.Messages
                    .OrderBy(m => m.Timestamp)
                    .Select(m => $"**{m.AgentType}:** {m.Content}"));

            _logger.LogInformation("Retrieved completed conversation {ConversationId}", id);

            return Ok(new
            {
                conversationId = conversation.Id,
                markdown = markdown,
                agent1Personality = conversation.Agent1Personality,
                agent2Personality = conversation.Agent2Personality,
                topic = conversation.Topic,
                status = conversation.Status,
                messageCount = conversation.Messages.Count
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving conversation");
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }
}
