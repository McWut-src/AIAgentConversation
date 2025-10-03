# GitHub Copilot Instructions for AI Agent Conversation Project

## Project Overview

This is a .NET 8 web application where two AI agents (powered by OpenAI GPT-3.5-turbo) engage in a conversation. The application follows established patterns and architectural guidelines.

**Critical Files to Reference:**
- `README.md` - Complete unified documentation (contains all project information, API docs, UI guide, AI behavior, development workflow, and more)

## Architecture Overview

```
Single-Page Razor Application
    ↓
ASP.NET Core API (3 endpoints)
    ↓
OpenAI GPT-3.5-turbo Service
    ↓
Entity Framework Core
    ↓
Local SQL Server (LocalDB/Express)
```

**Technology Stack:**
- .NET 8 / ASP.NET Core 8
- Razor Pages (single-page app)
- Entity Framework Core 8
- SQL Server LocalDB/Express
- OpenAI API (GPT-3.5-turbo)
- Serilog (console-only logging)
- Vanilla JavaScript (no frameworks)
- Plain CSS (no Bootstrap/Tailwind)

## Critical Requirements (NON-NEGOTIABLE)

### Database Schema

**ALWAYS use simplified schema with string fields:**

```csharp
public class Conversation
{
    public Guid Id { get; set; }
    
    // ⚠️ CRITICAL: Store as strings, NOT foreign keys
    [Required, MaxLength(500)]
    public string Agent1Personality { get; set; }
    
    [Required, MaxLength(500)]
    public string Agent2Personality { get; set; }
    
    [Required, MaxLength(1000)]
    public string Topic { get; set; }
    
    // ⚠️ CRITICAL: Fixed at 3, not configurable
    public int IterationCount { get; set; } = 3;
    
    [Required, MaxLength(50)]
    public string Status { get; set; } // "InProgress", "Completed", "Failed"
    
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    
    // Navigation property
    public ICollection<Message> Messages { get; set; }
}

public class Message
{
    public Guid Id { get; set; }
    
    [Required]
    public Guid ConversationId { get; set; }
    
    [Required, MaxLength(10)]
    public string AgentType { get; set; } // "A1" or "A2"
    
    public int IterationNumber { get; set; } // 1, 2, or 3
    
    [Required]
    public string Content { get; set; }
    
    public DateTime Timestamp { get; set; }
    
    // Navigation property
    public Conversation Conversation { get; set; }
}
```

**❌ NEVER create separate Personalities or Topics tables**  
**❌ NEVER add navigation properties to non-existent tables**  
**❌ NEVER make IterationCount configurable**

### OpenAI Integration

**ALWAYS use this EXACT prompt format:**

```csharp
// ⚠️ CRITICAL: This exact format is required
var prompt = $"You are {personality}. Respond to the conversation on {topic}: {history}";
```

**History format must be simple concatenation:**

```csharp
// ⚠️ CORRECT: Simple string concatenation
var history = string.Join("\n", messages.Select(m => $"{m.AgentType}: {m.Content}"));

// ❌ WRONG: Do not use structured formats or JSON
```

**OpenAI Configuration:**
- Model: `gpt-3.5-turbo` (REQUIRED, not gpt-4)
- MaxTokens: 500
- Temperature: 0.7

### API Endpoint Requirements

#### 1. POST /api/conversation/init

**Purpose:** Create conversation, call Agent 1, return first message

```csharp
// Request DTO
public class InitConversationRequest
{
    [Required, MaxLength(500)]
    public string Agent1Personality { get; set; }
    
    [Required, MaxLength(500)]
    public string Agent2Personality { get; set; }
    
    [Required, MaxLength(1000)]
    public string Topic { get; set; }
    
    // ❌ NO iteration count parameter - always fixed at 3
}

// Logic flow:
// 1. Validate inputs (required, trim whitespace)
// 2. Create Conversation entity (IterationCount = 3, Status = "InProgress")
// 3. Call OpenAI with: "You are {Agent1Personality}. Respond to the conversation on {Topic}: "
// 4. Save Message (AgentType = "A1", IterationNumber = 1)
// 5. Return response with isOngoing = true
```

#### 2. POST /api/conversation/follow

**Purpose:** Continue conversation with next agent response

```csharp
// Request DTO
public class FollowConversationRequest
{
    [Required]
    public Guid ConversationId { get; set; }
}

// ⚠️ CRITICAL: Agent alternation logic
var messageCount = conversation.Messages.Count;
var nextAgent = messageCount % 2 == 1 ? "A2" : "A1"; // Odd=A2, Even=A1
var iterationNumber = (messageCount / 2) + 1; // Calculate iteration

// ⚠️ CRITICAL: History concatenation
var history = string.Join("\n", 
    conversation.Messages
        .OrderBy(m => m.Timestamp)
        .Select(m => $"{m.AgentType}: {m.Content}"));

// Call OpenAI with next agent's personality
var personality = nextAgent == "A1" 
    ? conversation.Agent1Personality 
    : conversation.Agent2Personality;

// ⚠️ CRITICAL: Completion check
var totalMessages = messageCount + 1; // After saving new message
var isOngoing = totalMessages < 6;

if (totalMessages == 6)
{
    conversation.Status = "Completed";
    conversation.EndTime = DateTime.UtcNow;
}
```

**Message Sequence (MUST follow this pattern):**
1. A1 - Iteration 1 (from init)
2. A2 - Iteration 1 (1st follow call)
3. A1 - Iteration 2 (2nd follow call)
4. A2 - Iteration 2 (3rd follow call)
5. A1 - Iteration 3 (4th follow call)
6. A2 - Iteration 3 (5th follow call, isOngoing=false)

#### 3. GET /api/conversation/{id}

**Purpose:** Return completed conversation in markdown

```csharp
// ⚠️ CRITICAL: Only return if Status == "Completed"
if (conversation.Status != "Completed")
{
    return NotFound("Conversation not completed");
}

// ⚠️ CRITICAL: Markdown format
var markdown = string.Join("\n", 
    conversation.Messages
        .OrderBy(m => m.Timestamp)
        .Select(m => $"**{m.AgentType}:** {m.Content}"));

// Example output:
// **A1:** First message
// **A2:** Second message
// **A1:** Third message
// ...
```

### UI Requirements

#### Index.cshtml Structure

```html
<!-- ⚠️ CRITICAL: Use text inputs, NOT dropdowns -->
<input type="text" id="agent1-personality" placeholder="Agent 1 Personality" />
<input type="text" id="agent2-personality" placeholder="Agent 2 Personality" />
<input type="text" id="topic" placeholder="Topic" />
<button id="start-button">Start Conversation</button>

<div id="conversation-container"></div>
<div id="markdown-container" style="display: none;"></div>
```

#### JavaScript Requirements

```javascript
// ⚠️ CRITICAL: Store conversationId in variable, NOT localStorage/sessionStorage
let conversationId = null;

// ⚠️ CRITICAL: Static waiting indicator (plain text)
function createWaitingIndicator(side) {
    const div = document.createElement('div');
    div.className = `waiting-indicator waiting-${side}`;
    div.textContent = '...'; // Plain text, no animation
    return div;
}

// Bubble creation
function createMessageBubble(message, agentType) {
    const div = document.createElement('div');
    div.className = agentType === 'A1' 
        ? 'message-bubble-a1'  // Left-aligned
        : 'message-bubble-a2'; // Right-aligned
    div.textContent = message;
    return div;
}

// ⚠️ CRITICAL: Loop follow calls while isOngoing === true
async function continueConversation() {
    const response = await fetch('/api/conversation/follow', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ conversationId })
    });
    
    const data = await response.json();
    
    // Remove waiting indicator
    // Append message bubble
    
    if (data.isOngoing) {
        // Show next waiting indicator
        // Recursively call continueConversation()
    } else {
        // Call displayMarkdown()
    }
}
```

#### CSS Requirements

```css
/* ⚠️ CRITICAL: No CSS animation for waiting indicator */
.waiting-indicator {
    /* Simple text styling only */
    color: #999;
    font-size: 14px;
}

/* ⚠️ CRITICAL: A1 left, A2 right alignment */
.message-bubble-a1 {
    background-color: #007bff;
    color: white;
    text-align: left;
    margin-right: auto; /* Left align */
}

.message-bubble-a2 {
    background-color: #28a745;
    color: white;
    text-align: right;
    margin-left: auto; /* Right align */
}
```

### Logging Requirements

**ALWAYS use Serilog console-only logging:**

```csharp
// Program.cs configuration
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("System", LogEventLevel.Warning)
    .WriteTo.Console()
    .CreateLogger();

// ❌ NEVER add file sinks or other outputs in v1.0
```

**Log errors in all services:**

```csharp
try
{
    // Your code
}
catch (Exception ex)
{
    _logger.LogError(ex, "Error in operation: {OperationName}", operationName);
    throw; // Re-throw for controller error handling
}
```

## Code Generation Guidelines

### When Creating Entities

1. Use string fields for personalities and topics
2. Add proper data annotations ([Required], [MaxLength])
3. Set default values (IterationCount = 3)
4. Include navigation properties for EF relationships
5. Use Guid for primary keys

### When Creating Services

1. Define interface first (IServiceName)
2. Implement with constructor dependency injection
3. Inject IConfiguration, ILogger<T>, DbContext as needed
4. Use async/await for all I/O operations
5. Wrap in try-catch with Serilog error logging

### When Creating API Controllers

1. Add [ApiController] and [Route] attributes
2. Use [HttpPost], [HttpGet] attributes on actions
3. Accept DTOs with validation attributes
4. Return IActionResult with proper status codes
5. Use async Task<IActionResult> for all actions
6. Validate ModelState.IsValid first
7. Log errors and return meaningful messages

### When Creating UI Components

1. Use semantic HTML
2. Add proper id attributes for JavaScript
3. Use vanilla JavaScript (no jQuery/React)
4. Store state in JavaScript variables only
5. Use fetch API for AJAX calls
6. Handle errors gracefully with user feedback

## Common Mistakes to Avoid

### ❌ DON'T Create Separate Tables

```csharp
// ❌ WRONG: Do not create these
public class Personality { }
public class Topic { }
```

### ❌ DON'T Make Iterations Configurable

```csharp
// ❌ WRONG: Do not accept iteration count
public class InitConversationRequest
{
    public int IterationCount { get; set; } // ❌ WRONG
}
```

### ❌ DON'T Use Complex Prompt Formats

```csharp
// ❌ WRONG: Complex structured format
var prompt = new {
    role = "system",
    content = personality,
    messages = history
};

// ✅ CORRECT: Simple string concatenation
var prompt = $"You are {personality}. Respond to the conversation on {topic}: {history}";
```

### ❌ DON'T Use Dropdowns in UI

```html
<!-- ❌ WRONG: Dropdown -->
<select id="agent1-personality">
    <option>Professional Debater</option>
</select>

<!-- ✅ CORRECT: Text input -->
<input type="text" id="agent1-personality" />
```

### ❌ DON'T Add CSS Animations

```css
/* ❌ WRONG: Animated dots */
@keyframes blink {
    0%, 100% { opacity: 0; }
    50% { opacity: 1; }
}

/* ✅ CORRECT: Static text */
.waiting-indicator {
    color: #999;
}
```

### ❌ DON'T Use Session Storage

```javascript
// ❌ WRONG: Persistent storage
localStorage.setItem('conversationId', id);
sessionStorage.setItem('conversationId', id);

// ✅ CORRECT: JavaScript variable
let conversationId = id;
```

## Verification Checklist

Before marking any task as complete, verify:

- [ ] Conversation entity has string fields (not FK)
- [ ] No Personality or Topic tables exist
- [ ] IterationCount is fixed at 3
- [ ] OpenAI prompt matches exact format
- [ ] History uses simple concatenation
- [ ] Agent alternation: odd→A2, even→A1
- [ ] isOngoing=false only at message 6
- [ ] Markdown format: `**A1:** msg\n**A2:** msg`
- [ ] UI uses text inputs (no dropdowns)
- [ ] Waiting indicator is static text (no animation)
- [ ] ConversationId in JS variable (no storage)
- [ ] Stateless design (no session management)
- [ ] Serilog console-only (no file logging)
- [ ] Using gpt-3.5-turbo model
- [ ] Exactly 6 messages per conversation

## Development Workflow

When working on new features or fixes:

1. **Understand the requirement** completely
2. **Review README.md** for complete documentation (API, UI, coding guidelines, etc.)
3. **Follow patterns and guidelines** from README.md
4. **Generate code** following patterns in this document
5. **Test the implementation** thoroughly
6. **Update documentation** if needed

## Example Task Implementation

**Task: E2-F4-T14 - Implement POST /api/conversation/init endpoint**

```csharp
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
            // Validate
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Trim inputs
            request.Agent1Personality = request.Agent1Personality?.Trim();
            request.Agent2Personality = request.Agent2Personality?.Trim();
            request.Topic = request.Topic?.Trim();

            // Create conversation
            var conversation = new Conversation
            {
                Id = Guid.NewGuid(),
                Agent1Personality = request.Agent1Personality,
                Agent2Personality = request.Agent2Personality,
                Topic = request.Topic,
                IterationCount = 3, // Fixed
                Status = "InProgress",
                StartTime = DateTime.UtcNow
            };

            _context.Conversations.Add(conversation);

            // Call OpenAI for A1
            var prompt = $"You are {request.Agent1Personality}. Respond to the conversation on {request.Topic}: ";
            var messageContent = await _openAIService.GenerateResponseAsync(
                request.Agent1Personality,
                request.Topic,
                "");

            // Save message
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

            // Return response
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
}
```

## Questions to Ask Yourself

Before generating code, ask:

1. Does this create any extra database tables? (Should be NO)
2. Is IterationCount fixed at 3? (Should be YES)
3. Does the prompt match the exact format? (Should be YES)
4. Am I using simple string concatenation for history? (Should be YES)
5. Does agent alternation follow odd/even logic? (Should be YES)
6. Is the UI using text inputs? (Should be YES)
7. Is conversationId in a JS variable? (Should be YES)
8. Am I using localStorage/sessionStorage? (Should be NO)
9. Is the waiting indicator animated? (Should be NO)
10. Is this stateless? (Should be YES)

## Getting Help

If uncertain about any requirement:

1. Check `README.md` for complete documentation (contains all project information)
2. Review appropriate sections in README.md for technical details (API, UI, AI behavior, development workflow, etc.)
3. Review existing code for established patterns

## Final Notes

- **Follow existing patterns** - Maintain consistency with codebase
- **Documentation is key** - Keep docs updated with code changes
- **Test thoroughly** - Verify functionality before committing
- **Ask for clarification** - When in doubt, ask rather than assume
- **Quality over speed** - Write maintainable, well-tested code

**When in doubt, consult the documentation and follow established patterns.**
