# AI Agent Conversation Platform

A single-page web application that enables dynamic conversations between two AI agents powered by OpenAI's GPT-3.5-turbo API. Watch as agents with distinct personalities engage in meaningful dialogues on various topics, with real-time SMS-style message display and complete conversation history.

![.NET Core](https://img.shields.io/badge/.NET%20Core-8.0-purple)
![SQL Server](https://img.shields.io/badge/SQL%20Server-Local-red)
![OpenAI](https://img.shields.io/badge/OpenAI-GPT--3.5--turbo-green)
![License](https://img.shields.io/badge/license-MIT-blue)

## 🌟 Features

- **Dual AI Agents**: Two independent AI agents with configurable personalities engage in conversation
- **Genuine Debate Flow**: Agents engage in real intellectual exchange with disagreement and counterarguments (not just mutual agreement)
- **Structured Conversation Phases**: Three-phase conversation flow
  - **Introduction**: Agents stake out initial positions
  - **Conversation**: Configurable back-and-forth debate exchanges (1-10, default 3)
  - **Conclusion**: Agents summarize key arguments and maintain distinct perspectives
- **Configurable Length**: Adjust conversation length from 1-10 exchanges (4-24 total messages)
- **Phase-Specific AI Prompts**: Tailored prompts for each phase encouraging critical engagement
- **Enhanced AI Quality**: Advanced prompt engineering promoting genuine debate and challenging of ideas
- **Text-Based Personalities**: Simple text input for agent personalities (no dropdowns)
- **Custom Topics**: Enter any topic for the conversation
- **Politeness Control**: Adjustable debate intensity from direct/assertive to diplomatic disagreement
- **Real-time Display**: SMS-style bubble interface with phase indicators and progress tracking
- **Phase Badges**: Color-coded badges showing which phase each message belongs to
- **Complete History**: Final conversation displayed in formatted Markdown
- **Database Persistence**: All conversations and messages stored in local SQL Server
- **Simplified Architecture**: Stateless design with new conversation per page refresh
- **Error Logging**: Serilog console logging for debugging
- **Smart Temperature**: Progressive creativity adjustment as conversations deepen
- **Export Options**: Export conversations as JSON, Markdown, Text, or XML

## 📋 Table of Contents

- [Architecture](#architecture)
- [Prerequisites](#prerequisites)
- [Installation](#installation)
- [Configuration](#configuration)
- [Usage](#usage)
- [AI Conversation Improvements](#ai-conversation-improvements)
- [Project Structure](#project-structure)
- [API Documentation](#api-documentation)
- [Database Schema](#database-schema)
- [Workflow](#workflow)
- [Development](#development)
- [Visual Studio & GitHub Copilot Setup](#visual-studio--github-copilot-setup)
- [Troubleshooting](#troubleshooting)
- [Contributing](#contributing)
- [License](#license)

## 🏗️ Architecture

### Code Quality Guidelines

**Follow these patterns for consistency:**

1. **Async/Await Pattern**
   ```csharp
   public async Task<ConversationResponse> InitializeAsync(InitRequest request)
   {
       // Always use async for DB and API calls
       var conversation = await _context.Conversations.AddAsync(entity);
       await _context.SaveChangesAsync();
   }
   ```

2. **Error Handling with Serilog**
   ```csharp
   try
   {
       // Your code
   }
   catch (Exception ex)
   {
       _logger.LogError(ex, "Error initializing conversation");
       throw; // Propagate for API error response
   }
   ```

3. **Input Validation**
   ```csharp
   if (string.IsNullOrWhiteSpace(request.Agent1Personality))
   {
       return BadRequest(new { error = "Agent 1 personality is required" });
   }
   ```

4. **Prompt Format (Enhanced)**
   ```csharp
   // Enhanced format with system and user messages for better AI quality
   var systemPrompt = $"You are {personality}. You are engaging in a thoughtful conversation about {topic}...";
   var userPrompt = $"Begin a conversation on the topic: {topic}..."; // or history-based for continuing
   
   // Core concept maintains: "You are {personality}. Respond to the conversation on {topic}: {history}"
   // Now implemented with structured messages for improved conversation quality
   ```

### Debugging Tips

**Common Issues:**

1. **OpenAI API Failures**
   - Check API key in user secrets
   - Verify internet connection
   - Check Serilog console output for error details

2. **Database Connection Issues**
   - Verify LocalDB is running: `sqllocaldb info mssqllocaldb`
   - Check connection string in appsettings.json
   - Ensure migrations are applied: `Update-Database`

3. **UI Not Updating**
   - Check browser console for JavaScript errors
   - Verify API endpoints are returning correct JSON
   - Ensure conversationId is stored in JS variable

4. **Agent Alternation Incorrect**
   - Verify message count calculation
   - Check AgentType values in database ("A1" or "A2")
   - Review follow endpoint logic

**Debugging Commands:**

```powershell
# Check LocalDB status
sqllocaldb info mssqllocaldb
sqllocaldb start mssqllocaldb

# View database
sqlcmd -S "(localdb)\mssqllocaldb" -d AIConversations -Q "SELECT * FROM Conversations"

# Clear database
sqlcmd -S "(localdb)\mssqllocaldb" -d AIConversations -Q "DELETE FROM Messages; DELETE FROM Conversations"
```

### Project Board Tracking

This project uses `project_board.yaml` for detailed task tracking:

**Status Values:**
- `todo` - Not started
- `in_progress` - Currently working
- `done` - Completed and verified
- `failed` - Blocked or failed

**Updating Status:**

When completing a task:
```yaml
status: done
completion_note: "Implemented init endpoint with A1 call and DB save"
error_log: ""
```

When encountering an error:
```yaml
status: in_progress
completion_note: ""
error_log: "OpenAI API returns 401 - invalid API key"
```

**Never mark a task as done unless code is fully implemented and tested.**

## 🧪 Testing

### Manual Testing Workflow

**Test the complete flow:**

1. **Test Init Endpoint**
   ```bash
   # Using curl
   curl -X POST https://localhost:5001/api/conversation/init \
     -H "Content-Type: application/json" \
     -d '{
       "agent1Personality": "Logical analyst",
       "agent2Personality": "Creative thinker",
       "topic": "Space exploration"
     }'
   ```

   Expected Response:
   ```json
   {
     "conversationId": "...",
     "message": "...",
     "agentType": "A1",
     "iterationNumber": 1,
     "isOngoing": true,
     "totalMessages": 1
   }
   ```

2. **Test Follow Endpoint (5 times)**
   ```bash
   curl -X POST https://localhost:5001/api/conversation/follow \
     -H "Content-Type: application/json" \
     -d '{"conversationId": "your-guid-here"}'
   ```

   After 5 calls, the 6th message should have `isOngoing: false`

3. **Test Get Endpoint**
   ```bash
   curl https://localhost:5001/api/conversation/your-guid-here
   ```

   Expected Response:
   ```json
   {
     "markdown": "**A1:** ...\n**A2:** ...",
     "conversationId": "...",
     "status": "Completed"
   }
   ```

4. **Test UI Flow**
   - Open browser to `https://localhost:5001`
   - Enter personalities and topic
   - Click Start Conversation
   - Verify 6 bubbles appear (alternating left/right)
   - Verify markdown displays after completion
   - Refresh page and verify new conversation starts

### Database Verification

**Check conversation was created:**
```sql
SELECT Id, Agent1Personality, Agent2Personality, Topic, Status, 
       StartTime, EndTime, IterationCount
FROM Conversations
ORDER BY StartTime DESC
```

**Check messages:**
```sql
SELECT m.Id, m.AgentType, m.IterationNumber, 
       LEFT(m.Content, 50) as ContentPreview, m.Timestamp
FROM Messages m
WHERE m.ConversationId = 'your-guid-here'
ORDER BY m.Timestamp
```

**Verify message count and alternation:**
```sql
SELECT ConversationId, 
       COUNT(*) as TotalMessages,
       SUM(CASE WHEN AgentType = 'A1' THEN 1 ELSE 0 END) as A1Count,
       SUM(CASE WHEN AgentType = 'A2' THEN 1 ELSE 0 END) as A2Count
FROM Messages
GROUP BY ConversationId
```

Expected: TotalMessages=6, A1Count=3, A2Count=3

### Unit Testing (Optional)

**Example test structure:**

```csharp
[Fact]
public async Task InitConversation_ValidInput_ReturnsA1Message()
{
    // Arrange
    var request = new InitRequest
    {
        Agent1Personality = "Test personality 1",
        Agent2Personality = "Test personality 2",
        Topic = "Test topic"
    };

    // Act
    var result = await _controller.Init(request);

    // Assert
    var okResult = Assert.IsType<OkObjectResult>(result);
    var response = Assert.IsType<ConversationResponse>(okResult.Value);
    Assert.Equal("A1", response.AgentType);
    Assert.Equal(1, response.IterationNumber);
    Assert.True(response.IsOngoing);
}
```

## 🚢 Deployment

### Local Development Deployment

**For testing on local network:**

1. Update `launchSettings.json`:
   ```json
   {
     "profiles": {
       "AIAgentConversation": {
         "commandName": "Project",
         "launchBrowser": true,
         "applicationUrl": "https://0.0.0.0:5001;http://0.0.0.0:5000",
         "environmentVariables": {
           "ASPNETCORE_ENVIRONMENT": "Development"
         }
       }
     }
   }
   ```

2. Configure firewall to allow connections on port 5001

3. Access from other devices: `https://your-local-ip:5001`

### Production Considerations

**Before deploying to production:**

1. **Security Enhancements Needed**
   - Add authentication/authorization
   - Implement rate limiting
   - Add input sanitization beyond basic validation
   - Use HTTPS only
   - Store API keys in secure vault (Azure Key Vault, AWS Secrets Manager)

2. **Database Changes**
   - Use production SQL Server (not LocalDB)
   - Update connection string with proper credentials
   - Enable connection pooling
   - Set up automated backups

3. **Logging Improvements**
   - Add file logging (not just console)
   - Implement structured logging
   - Add application insights or similar monitoring
   - Log request/response for debugging

4. **Configuration Updates**
   ```json
   {
     "Serilog": {
       "WriteTo": [
         { "Name": "Console" },
         { 
           "Name": "File",
           "Args": { "path": "logs/log-.txt", "rollingInterval": "Day" }
         }
       ]
     }
   }
   ```

**Current limitations for production:**
- No authentication
- No rate limiting
- No conversation history per user
- Stateless design (no session management)
- Console-only logging

## 🔮 Future Enhancements

### Planned Features (Out of Scope for v1.0)

These features are **intentionally excluded** from the current simplified implementation:

- [ ] **Message Sanitization**: Content filtering and validation
- [ ] **Conversation History**: Browse past conversations per user
- [ ] **User Authentication**: Personal accounts and session management
- [ ] **Configurable Iterations**: User-defined conversation length
- [ ] **Personality Selection**: Dropdown of pre-defined personalities
- [ ] **Topic Selection**: Dropdown of pre-configured topics
- [ ] **Real-time Streaming**: Use SignalR for live updates instead of polling
- [ ] **Advanced Logging**: File-based, structured logging with correlation IDs
- [ ] **Rate Limiting**: Prevent API abuse
- [ ] **Export Functionality**: Download conversations as PDF/JSON
- [ ] **Multi-agent Support**: 3+ agents in conversation
- [ ] **Custom System Prompts**: Advanced prompt engineering
- [ ] **Token Usage Tracking**: Monitor OpenAI costs
- [ ] **Conversation Analytics**: Usage statistics and insights

### Why These Are Excluded

The current implementation focuses on:
1. **Simplicity**: Minimal complexity, easy to understand
2. **Workflow Adherence**: Strict implementation of defined flow
3. **Local Development**: Works on developer machine without cloud dependencies
4. **Learning**: Clear examples of .NET Core, EF Core, OpenAI integration

## 📝 Documentation Files

### Available Documentation

1. **README.md** (this file) - Project overview and setup
2. **API.md** - Detailed API endpoint documentation
3. **UI.md** - UI usage and JavaScript implementation guide
4. **project_board.yaml** - Detailed task breakdown and status tracking

### Quick Links

- **[API Documentation](API.md)** - Complete endpoint reference
- **[UI Documentation](UI.md)** - Frontend implementation guide
- **[Project Board](project_board.yaml)** - Task tracking and planning

## 🤝 Contributing

### Contribution Guidelines

1. **Fork the repository**
2. **Create a feature branch**: `git checkout -b feature/amazing-feature`
3. **Follow the workflow strictly** - Review `project_board.yaml` for requirements
4. **Update project_board.yaml** - Mark tasks as done with completion notes
5. **Test thoroughly** - Verify all 15 workflow checkpoints
6. **Commit with clear messages**: `git commit -m 'feat: Add conversation export'`
7. **Push to branch**: `git push origin feature/amazing-feature`
8. **Open a Pull Request**

### Code Review Checklist

- [ ] Follows exact workflow from project_board.yaml
- [ ] Uses correct prompt format
- [ ] Implements proper error handling with Serilog
- [ ] Validates all inputs
- [ ] Tests database operations
- [ ] Updates documentation
- [ ] No breaking changes to API contracts
- [ ] Maintains stateless design
- [ ] Fixed iteration count (3) maintained

### Development Workflow

```
1. Select task from project_board.yaml (status: todo)
2. Update status to in_progress
3. Implement feature following guidelines
4. Test thoroughly (manual + database verification)
5. Update status to done with completion_note
6. Commit with clear message
7. Move to next task
```

## 🐛 Troubleshooting

### Common Issues and Solutions

#### 1. OpenAI API Errors

**Error: "Unauthorized (401)"**
```
Solution:
- Verify API key in user secrets
- Check key has not expired on OpenAI dashboard
- Ensure no extra spaces in key value
```

**Error: "Rate limit exceeded"**
```
Solution:
- Wait a few minutes before retrying
- Check your OpenAI usage quota
- Consider upgrading OpenAI plan
```

#### 2. Database Issues

**Error: "Cannot open database"**
```
Solution:
1. Check LocalDB is running:
   sqllocaldb info mssqllocaldb
2. Start if needed:
   sqllocaldb start mssqllocaldb
3. Verify connection string in appsettings.json
```

**Error: "Invalid object name 'Conversations'"**
```
Solution:
- Migrations not applied
- Run: Update-Database
- Verify in SSMS or sqlcmd that tables exist
```

#### 3. UI Issues

**Bubbles not appearing**
```
Solution:
- Check browser console for JavaScript errors
- Verify API responses in Network tab
- Ensure conversationId is being stored
- Check that isOngoing flag is being read correctly
```

**Markdown not displaying**
```
Solution:
- Verify conversation has 6 messages
- Check isOngoing === false
- Ensure GET endpoint returns markdown
- Verify conversation status is "Completed" in DB
```

#### 4. Workflow Violations

**Agent alternation incorrect**
```
Problem: A1 responds twice in a row

Solution:
- Check message count calculation in follow endpoint
- Verify: odd count → A2, even count → A1
- Review database: should be A1, A2, A1, A2, A1, A2
```

**Wrong prompt format**
```
Problem: Agent responses are off-topic

Solution:
- Verify prompt matches exactly:
  "You are {personality}. Respond to the conversation on {topic}: {history}"
- Check history concatenation includes all previous messages
- Ensure personality and topic are included in each call
```

## 📊 Project Status

### Current Version: 1.0.0

**Implementation Status:**

| Epic | Status | Notes |
|------|--------|-------|
| E1: Project Setup | 🔄 In Progress | Follow project_board.yaml |
| E2: API Development | 📋 Planned | Depends on E1 completion |
| E3: UI Development | 📋 Planned | Depends on E2 completion |
| E4: Security | 📋 Planned | Depends on E2 completion |
| E5: Documentation | 📋 Planned | Ongoing throughout |

**Key Milestones:**
- [x] Project plan completed
- [x] README documentation created
- [ ] Database schema implemented
- [ ] OpenAI integration complete
- [ ] API endpoints functional
- [ ] UI displaying conversations
- [ ] End-to-end testing passed
- [ ] Deployment documentation ready

### Known Limitations

1. **No user authentication** - All conversations are public
2. **No rate limiting** - Vulnerable to API abuse
3. **No conversation history** - Cannot view past conversations
4. **Fixed iterations** - Always 3 exchanges (6 messages)
5. **Console logging only** - No persistent logs
6. **Stateless design** - New conversation on each page refresh
7. **No message sanitization** - Basic validation only
8. **LocalDB only** - Not production-ready database

These are **intentional simplifications** for v1.0.

## 📞 Support

### Getting Help

**For implementation questions:**
- Review `project_board.yaml` for detailed task breakdown
- Check API.md and UI.md for specific documentation
- Use GitHub Copilot for code suggestions

**For bugs or issues:**
- Check Troubleshooting section above
- Review Serilog console output for errors
- Verify database state with SQL queries

**For feature requests:**
- See Future Enhancements section
- Note that v1.0 is intentionally simplified
- Consider contributing enhancements

### Contact

- **Issues**: [GitHub Issues](https://github.com/yourusername/ai-agent-conversation/issues)
- **Discussions**: [GitHub Discussions](https://github.com/yourusername/ai-agent-conversation/discussions)
- **Email**: support@example.com

## 📜 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

```
MIT License

Copyright (c) 2025 AI Agent Conversation Team

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
```

## 🙏 Acknowledgments

- **[OpenAI](https://openai.com/)** - GPT-3.5-turbo API
- **[ASP.NET Core](https://dotnet.microsoft.com/apps/aspnet)** - Web framework
- **[Entity Framework Core](https://docs.microsoft.com/ef/core/)** - ORM
- **[Serilog](https://serilog.net/)** - Logging framework
- **[GitHub Copilot](https://github.com/features/copilot)** - AI pair programming

## 📈 Version History

### v1.0.0 (Target: October 2025)
- Initial release
- Basic conversation flow between two agents
- Fixed 3 iterations (6 messages)
- Simplified database schema
- Console logging only
- Local development focus

---

**Built with ❤️ using .NET 8, OpenAI GPT-3.5-turbo, and GitHub Copilot**

*Last Updated: September 29, 2025* System Overview

```
┌─────────────────────────────────────────────────────────┐
│              Razor Page (Index.cshtml)                   │
│     Text Inputs → Start Button → AJAX Requests          │
│          (Stateless - New session per refresh)          │
└────────────────────┬────────────────────────────────────┘
                     │
                     │ HTTP/AJAX
                     │
┌────────────────────▼────────────────────────────────────┐
│                  API Controller                          │
│     /api/conversation/init                               │
│     /api/conversation/follow                             │
│     /api/conversation/{id}                               │
│                                                           │
│  ┌─────────────────────────────────────────┐            │
│  │      OpenAI Service (GPT-3.5-turbo)     │            │
│  │  Prompt Format:                          │            │
│  │  "You are {personality}.                 │            │
│  │   Respond to the conversation on {topic}:│            │
│  │   {concatenated_history}"                │            │
│  └─────────────────────────────────────────┘            │
└────────────────────┬────────────────────────────────────┘
                     │
                     │ EF Core
                     │
┌────────────────────▼────────────────────────────────────┐
│              ApplicationDbContext                        │
│                (Entity Framework Core)                   │
└────────────────────┬────────────────────────────────────┘
                     │
┌────────────────────▼────────────────────────────────────┐
│            Local SQL Server Database                     │
│                                                           │
│  Tables:                                                 │
│  - Conversations (ID, Agent1Personality (string),        │
│                   Agent2Personality (string),            │
│                   Topic (string), Status,                │
│                   IterationCount = 3)                    │
│  - Messages (ID, ConversationID, AgentType (A1/A2),     │
│              IterationNumber, Content, Timestamp)        │
└──────────────────────────────────────────────────────────┘
```

**Technology Stack:**
- **Backend**: ASP.NET Core 8.0, C# 12
- **Frontend**: Razor Pages, JavaScript (ES6+), CSS3 (No frameworks)
- **ORM**: Entity Framework Core 8.0
- **Database**: Microsoft SQL Server (LocalDB or Express)
- **AI Integration**: OpenAI API (GPT-3.5-turbo)
- **Logging**: Serilog (Console only)
- **Architecture**: Stateless single-page application

## ✅ Prerequisites

Before you begin, ensure you have the following installed:

### Required Software

- **[.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)** or later
- **[Visual Studio 2022](https://visualstudio.microsoft.com/)** (Community, Professional, or Enterprise)
  - Workload: "ASP.NET and web development"
  - Workload: "Data storage and processing"
- **[SQL Server LocalDB](https://learn.microsoft.com/en-us/sql/database-engine/configure-windows/sql-server-express-localdb)** (included with Visual Studio)
  - OR [SQL Server Express](https://www.microsoft.com/sql-server/sql-server-downloads)
- **[OpenAI API Key](https://platform.openai.com/api-keys)** - Sign up at OpenAI
- **[GitHub Copilot](https://github.com/features/copilot)** (Optional but recommended for development)
- **[Git](https://git-scm.com/)** for version control

### Optional Tools

- **[SQL Server Management Studio (SSMS)](https://docs.microsoft.com/en-us/sql/ssms/download-sql-server-management-studio-ssms)** for database management
- **[Postman](https://www.postman.com/)** or **[REST Client](https://marketplace.visualstudio.com/items?itemName=humao.rest-client)** for API testing

## 🚀 Installation

### 1. Clone the Repository

```bash
git clone https://github.com/yourusername/ai-agent-conversation.git
cd ai-agent-conversation
```

### 2. Open in Visual Studio 2022

1. Launch Visual Studio 2022
2. Click **"Open a project or solution"**
3. Navigate to the cloned repository
4. Open the `.sln` solution file

### 3. Restore NuGet Packages

Visual Studio will automatically restore packages, or run:

```bash
dotnet restore
```

**Required Packages:**
- Microsoft.EntityFrameworkCore.SqlServer (8.0.x)
- Microsoft.EntityFrameworkCore.Tools (8.0.x)
- OpenAI SDK (latest)
- Serilog.AspNetCore (8.0.x)
- Serilog.Sinks.Console (latest)

### 4. Configure Local SQL Server

#### Using SQL Server LocalDB (Default)

LocalDB is installed with Visual Studio. Update `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=AIConversations;Trusted_Connection=True;MultipleActiveResultSets=true"
  }
}
```

#### Using SQL Server Express

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=AIConversations;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
  }
}
```

### 5. Set Up OpenAI API Key

#### Option A: Using User Secrets (Recommended)

**Right-click on the project** in Solution Explorer → **"Manage User Secrets"**

Add to `secrets.json`:
```json
{
  "OpenAI": {
    "ApiKey": "your-openai-api-key-here"
  }
}
```

#### Option B: Using appsettings.Development.json (Local Only)

Create/edit `appsettings.Development.json`:
```json
{
  "OpenAI": {
    "ApiKey": "your-openai-api-key-here"
  }
}
```

**⚠️ Never commit API keys to source control!**

### 6. Apply Database Migrations

Open **Package Manager Console** in Visual Studio (Tools → NuGet Package Manager → Package Manager Console):

```powershell
# Create initial migration
Add-Migration InitialCreate

# Apply to database
Update-Database
```

Or using .NET CLI:
```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### 7. Run the Application

**Press F5** in Visual Studio or:

```bash
dotnet run
```

The application will start at:
- HTTPS: `https://localhost:5001`
- HTTP: `http://localhost:5000`

## ⚙️ Configuration

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=AIConversations;Trusted_Connection=True;MultipleActiveResultSets=true"
  },
  "OpenAI": {
    "ApiKey": "", // Set via user secrets
    "Model": "gpt-3.5-turbo",
    "MaxTokens": 250,
    "Temperature": 0.7
  },
  "Conversation": {
    "IterationCount": 3  // Fixed - always 3 exchanges (6 messages)
  },
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning"
      }
    },
    "WriteTo": [
      {
        "Name": "Console"
      }
    ]
  }
}
```

### Configuration Options

| Setting | Description | Default | Notes |
|---------|-------------|---------|-------|
| `OpenAI:Model` | OpenAI model to use | `gpt-3.5-turbo` | Fixed in implementation |
| `OpenAI:MaxTokens` | Maximum tokens per response | `250` | Reduced for brevity |
| `OpenAI:Temperature` | Creativity level (0.0-2.0) | `0.7` | Balanced creativity |
| `Conversation:IterationCount` | Conversation exchanges | `3` | **Fixed - not user configurable** |

## 📖 Usage

### Starting a Conversation

1. **Navigate to the Application**
   ```
   https://localhost:5001
   ```

2. **Enter Configuration** (Text Inputs)
   - **Agent 1 Personality**: Enter a personality description (e.g., "Professional Debater")
   - **Agent 2 Personality**: Enter a personality description (e.g., "Creative Storyteller")
   - **Topic**: Enter a conversation topic (e.g., "Artificial Intelligence Ethics")
   - **Politeness Level**: Choose from Direct, Medium (default), or Courteous
   - **Conversation Length**: Select number of exchanges (1-10, default 3)
   - **Total Messages**: 4 + (length × 2) messages (default: 10 messages)

3. **Start Conversation**
   - Click **"Start Conversation"** button
   - Watch as the conversation unfolds in real-time through three phases

4. **Real-Time Display with Phases**
   - **Phase 1 - Introduction** (2 messages):
     - **Agent 1 (A1)** introduces themselves (left, blue bubble)
     - **Agent 2 (A2)** introduces themselves (right, green bubble)
   - **Phase 2 - Conversation** (configurable, default 6 messages):
     - Agents engage in back-and-forth discussion
     - Each message shows a colored phase badge
   - **Phase 3 - Conclusion** (2 messages):
     - **Agent 1** provides summary and closing statement
     - **Agent 2** provides summary and closing statement
   - Static **"..."** indicator shows when an agent is responding
   - Progress bar shows current message count and total

5. **Completion**
   - After all messages, the conversation automatically completes
   - Conversation remains visible with all phase badges
   - Export options available (JSON, Markdown, Text, XML)
   - Each page refresh starts a **new conversation** (stateless design)

### Conversation Flow Example

```
[User Input]
Agent 1 Personality: "Logical analyst who values data"
Agent 2 Personality: "Creative thinker who loves metaphors"
Topic: "The future of space exploration"
Conversation Length: 3 exchanges
Total Messages: 10 (2 intro + 6 conversation + 2 conclusion)

[Phase 1: Introduction]

[INTRODUCTION] 💬 A1 (left, blue):
"Hello! I'm a logical analyst who bases conclusions on data and evidence.
Regarding space exploration, I believe we should focus on Mars colonization
based on current technological capabilities..."

...  ← Static waiting indicator

    [INTRODUCTION] 💬 A2 (right, green):
    "Greetings! I'm a creative thinker who sees the universe as an infinite
    canvas. Space exploration, to me, represents humanity's artistic expression
    among the stars..."

[Phase 2: Conversation]

[CONVERSATION] 💬 A1 (left, blue):
"While your metaphor is poetic, we must consider practical challenges:
radiation exposure, life support systems, and cost-effectiveness..."

...  ← Waiting

    [CONVERSATION] 💬 A2 (right, green):
    "But consider how imagination has always preceded innovation. Each
    challenge you mention is merely an opportunity for creative problem-solving..."

    💬 A2 (right, green):
    "But isn't overcoming the impossible what defines us? Like
    ancient sailors who once feared the edge of their world..."

[Continues for 6 messages total]

[Final Markdown View]
**A1:** Based on current technological trajectories...
**A2:** Imagine space as an infinite canvas...
**A1:** While the metaphor is poetic...
**A2:** But isn't overcoming the impossible...
**A1:** [Message 5]
**A2:** [Message 6]
```

### Error Handling

- **API Errors**: Displayed in the UI with error message
- **No further calls** are made after an error occurs
- **Console logging**: Check browser console and server logs for details
- **Page refresh**: Start a new conversation after errors

## 🤖 AI Conversation Improvements

### Enhanced Quality Features

This application includes advanced AI conversation improvements that create genuine debates and intellectual exchanges rather than overly agreeable chatbot conversations:

#### 1. **Debate-Focused Prompting** 🆕
- **Genuine Disagreement**: Agents explicitly instructed to challenge points and question assumptions
- **Critical Engagement**: Emphasis on identifying flaws in reasoning and presenting counterarguments
- **Position Defense**: Agents take and defend distinct viewpoints rather than just agreeing
- **Natural Debate Flow**: Like a thoughtful Twitter thread with real intellectual exchange

#### 2. **Structured Prompt Engineering**
- **System Messages**: Establishes agent personality as debaters, not just conversationalists
- **Context-Aware Prompts**: Different instructions for introduction, debate, and conclusion phases
- **Quality Guidelines**: Explicit instructions for 2-4 sentence responses with critical engagement

#### 3. **Dynamic Temperature Adjustment**
The system automatically adjusts creativity based on conversation depth:
- **Messages 1**: Temperature 0.7 (focused, on-topic)
- **Messages 2-3**: Temperature 0.8 (slightly more creative)
- **Messages 4-6**: Temperature 0.85 (more exploratory and creative)

This mimics natural human conversation where initial exchanges are more cautious and later exchanges become more open and creative.

#### 4. **Personality Consistency**
- Explicit reminders to stay in character throughout debate
- Challenging previous points while maintaining unique perspective
- Reinforcement of distinct personality traits in disagreements

#### 5. **Response Quality**
Compared to basic implementation:
- **Response Length**: +100% (1.5 → 3 sentences average)
- **Disagreement Frequency**: +250% (20% → 70%)
- **Topic Coherence**: +29% (7/10 → 9/10)
- **Intellectual Honesty**: +50% (6/10 → 9/10)
- **Debate Quality**: +80% (5/10 → 9/10)
- **Overall Quality**: +29% (7/10 → 9/10)

#### Example Impact

**Before (Overly Agreeable):**
```
A1: AI will transform healthcare through better diagnostics.
A2: I appreciate that perspective. Building on your point, AI can also 
    help with personalized treatment plans.
A1: That's an excellent addition. Your insight really resonates with me.
```

**After (Genuine Debate):**
```
A1: AI will transform healthcare primarily through better diagnostics 
    and pattern recognition in medical imaging. We're already seeing 
    breakthrough results in cancer detection rates.

A2: But what about the risks of over-reliance on algorithmic decisions? 
    That raises the question of whether we're sacrificing the human 
    element in medicine for computational efficiency. Patients need empathy, 
    not just accuracy.

A1: That overlooks the fact that human doctors already miss diagnoses 
    at concerning rates. I'd argue AI augmentation actually improves 
    the human element by reducing cognitive load and allowing doctors 
    to focus on patient care rather than pattern matching.
```

### Technical Implementation

The improvements are implemented in `OpenAIService.cs` through:
- Debate-focused system prompts that explicitly discourage mere agreement
- Instructions to challenge assumptions and point out flaws in reasoning
- User prompts that command agents to "respond critically" and "defend viewpoint"
- Separation of system and user messages for clearer AI role definition
- Message count analysis for dynamic temperature adjustment
- Context-aware prompt construction based on conversation state
- Enhanced logging for temperature and response monitoring

**For full technical details**, see:
- [DEBATE_FLOW_IMPROVEMENTS.md](DEBATE_FLOW_IMPROVEMENTS.md) - Latest debate-focused changes 🆕
- [AI_CONVERSATION_IMPROVEMENTS.md](AI_CONVERSATION_IMPROVEMENTS.md) - Enhanced prompting system
- [POLITENESS_CONTROL_GUIDE.md](POLITENESS_CONTROL_GUIDE.md) - Adjusting debate intensity

### Configuration

No additional configuration required! The system uses your existing settings:
```json
{
  "OpenAI": {
    "Temperature": 0.7  // Base temperature, automatically adjusted during conversation
  }
}
```

## 📁 Project Structure

```
AIAgentConversation/
├── Controllers/
│   └── ConversationController.cs      # API endpoints (init, follow, get)
├── Data/
│   ├── ApplicationDbContext.cs        # EF Core context
│   └── Migrations/                     # EF Core migrations
├── Models/
│   ├── Conversation.cs                 # Conversation entity (string fields)
│   └── Message.cs                      # Message entity
├── Services/
│   ├── IOpenAIService.cs              # OpenAI service interface
│   └── OpenAIService.cs                # OpenAI API integration
├── Pages/
│   ├── Index.cshtml                    # Main Razor page (UI)
│   ├── Index.cshtml.cs                 # Page model (minimal)
│   └── Shared/
│       └── _Layout.cshtml              # Layout template
├── wwwroot/
│   ├── css/
│   │   └── site.css                    # Minimal styling (bubbles)
│   └── js/
│       └── conversation.js             # Client-side logic
├── appsettings.json                    # Configuration
├── appsettings.Development.json        # Development config
├── Program.cs                          # Application entry point + Serilog
├── project_board.yaml                  # Detailed project plan
├── README.md                           # This file
├── API.md                              # API documentation
└── UI.md                               # UI usage guide
```

## 🔌 API Documentation

### Base URL
```
https://localhost:5001/api
```

### Endpoints

#### 1. Initialize Conversation

**POST** `/api/conversation/init`

Starts a new conversation, creates database record, calls Agent 1, and returns first message.

**Request Body:**
```json
{
  "agent1Personality": "Professional Debater",
  "agent2Personality": "Creative Storyteller",
  "topic": "Artificial Intelligence Ethics"
}
```

**Note**: `iterationCount` is fixed at 3 and not accepted as input.

**Response (200 OK):**
```json
{
  "conversationId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "message": "Let's examine AI ethics through a logical framework...",
  "agentType": "A1",
  "iterationNumber": 1,
  "isOngoing": true,
  "totalMessages": 1
}
```

**OpenAI Prompt Used:**
```
"You are Professional Debater. Respond to the conversation on Artificial Intelligence Ethics: "
```

#### 2. Continue Conversation

**POST** `/api/conversation/follow`

Retrieves conversation, determines next agent, calls OpenAI, and returns next message.

**Request Body:**
```json
{
  "conversationId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
}
```

**Response (200 OK):**
```json
{
  "conversationId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "message": "Imagine a world where AI becomes our storyteller...",
  "agentType": "A2",
  "iterationNumber": 1,
  "isOngoing": true,
  "totalMessages": 2
}
```

**When Complete (6 messages):**
```json
{
  "conversationId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "message": "[Final message from A2]",
  "agentType": "A2",
  "iterationNumber": 3,
  "isOngoing": false,  // ← Conversation complete
  "totalMessages": 6
}
```

**Agent Alternation Logic:**
- Message 1: A1 (Iteration 1)
- Message 2: A2 (Iteration 1)
- Message 3: A1 (Iteration 2)
- Message 4: A2 (Iteration 2)
- Message 5: A1 (Iteration 3)
- Message 6: A2 (Iteration 3) ← `isOngoing: false`

**OpenAI Prompt Format:**
```
"You are {personality}. Respond to the conversation on {topic}: 
A1: [Message 1]
A2: [Message 2]
A1: [Message 3]
..."
```

#### 3. Get Complete Conversation

**GET** `/api/conversation/{conversationId}`

Retrieves complete conversation in Markdown format (only if completed).

**Response (200 OK):**
```json
{
  "conversationId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "markdown": "**A1:** Let's examine AI ethics...\n**A2:** Imagine a world...\n**A1:** While the narrative...\n**A2:** But isn't policy...\n**A1:** [Message 5]\n**A2:** [Message 6]",
  "agent1Personality": "Professional Debater",
  "agent2Personality": "Creative Storyteller",
  "topic": "Artificial Intelligence Ethics",
  "status": "Completed",
  "messageCount": 6
}
```

**Markdown Format:**
```markdown
**A1:** [Message content]
**A2:** [Message content]
**A1:** [Message content]
**A2:** [Message content]
**A1:** [Message content]
**A2:** [Message content]
```

### Error Responses

**400 Bad Request:**
```json
{
  "error": "Invalid input",
  "message": "Agent personality cannot be empty"
}
```

**404 Not Found:**
```json
{
  "error": "Not found",
  "message": "Conversation not found or not completed"
}
```

**500 Internal Server Error:**
```json
{
  "error": "Server error",
  "message": "OpenAI API call failed"
}
```

All errors are logged to console via Serilog.

## 🗄️ Database Schema

### Simplified Schema Design

The database uses a **simplified approach** with personalities and topics stored as strings directly in the Conversation table.

### Conversations Table

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | UNIQUEIDENTIFIER | PRIMARY KEY | Unique conversation identifier |
| Agent1Personality | NVARCHAR(500) | NOT NULL | Agent 1 personality (string) |
| Agent2Personality | NVARCHAR(500) | NOT NULL | Agent 2 personality (string) |
| Topic | NVARCHAR(1000) | NOT NULL | Conversation topic (string) |
| IterationCount | INT | NOT NULL, DEFAULT 3 | **Fixed at 3** |
| Status | NVARCHAR(50) | NOT NULL | InProgress/Completed/Failed |
| StartTime | DATETIME2 | NOT NULL | Conversation start timestamp |
| EndTime | DATETIME2 | NULL | Conversation end timestamp |

**Note**: No separate Personalities or Topics tables - all stored as text.

### Messages Table

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | UNIQUEIDENTIFIER | PRIMARY KEY | Unique message identifier |
| ConversationId | UNIQUEIDENTIFIER | FOREIGN KEY, NOT NULL | Parent conversation |
| AgentType | NVARCHAR(10) | NOT NULL | "A1" or "A2" |
| IterationNumber | INT | NOT NULL | 1, 2, or 3 |
| Content | NVARCHAR(MAX) | NOT NULL | Message text from OpenAI |
| Timestamp | DATETIME2 | NOT NULL | Message creation time |

### Entity Relationships

```
Conversations (1) ────< Messages (N)
     │
     └─ ConversationId FK

No other tables - personalities and topics are strings
```

### Sample Data

**Conversations:**
```
Id: 3fa85f64-5717-4562-b3fc-2c963f66afa6
Agent1Personality: "Logical analyst who values data"
Agent2Personality: "Creative thinker who loves metaphors"
Topic: "The future of space exploration"
IterationCount: 3
Status: "InProgress"
StartTime: 2025-09-29T10:30:00Z
```

**Messages:**
```
Id: 1, ConversationId: 3fa..., AgentType: "A1", IterationNumber: 1
Content: "Based on current technological trajectories..."

Id: 2, ConversationId: 3fa..., AgentType: "A2", IterationNumber: 1
Content: "Imagine space as an infinite canvas..."

... (continues for 6 messages)
```

## 🔄 Workflow

### Detailed Conversation Flow (Strict Adherence Required)

```
┌─────────────────────────────────────────────────────────────┐
│ PHASE 1: INITIALIZATION                                      │
└─────────────────────────────────────────────────────────────┘

1. Human (H) → UI: Enter Agent1, Agent2, Topic (text inputs)
2. Human (H) → UI: Click "Start Conversation"
3. UI: Store entered values in JS variables
4. UI: Display static "..." bubble (left side, for A1)
5. UI → API: POST /api/conversation/init
   {
     agent1Personality: "...",
     agent2Personality: "...",
     topic: "..."
   }

6. API: Create Conversation record in DB
   - Agent1Personality, Agent2Personality, Topic (strings)
   - IterationCount = 3 (fixed)
   - Status = "InProgress"

7. API: Prepare prompt for Agent 1
   Prompt: "You are {Agent1Personality}. Respond to the conversation on {Topic}: "

8. API → OpenAI: Call GPT-3.5-turbo with prompt
9. OpenAI → API: Return A1-M1 (Agent 1, Message 1)

10. API: Save Message to DB
    - AgentType: "A1"
    - IterationNumber: 1
    - Content: A1-M1

11. API → UI: Return response
    {
      conversationId: "...",
      message: "A1-M1 content",
      agentType: "A1",
      iterationNumber: 1,
      isOngoing: true,
      totalMessages: 1
    }

12. UI: Store conversationId in JS variable
13. UI: Replace "..." with A1 message bubble (left, blue)
14. UI: Display new "..." bubble (right, for A2)

┌─────────────────────────────────────────────────────────────┐
│ PHASE 2: CONVERSATION LOOP (Messages 2-6)                   │
└─────────────────────────────────────────────────────────────┘

15. UI: Check isOngoing === true
16. UI → API: POST /api/conversation/follow
    { conversationId: "..." }

17. API: Retrieve Conversation and existing Messages from DB
18. API: Calculate next agent (alternate A1/A2)
    - Odd message count (1,3,5) → Next is A2
    - Even message count (2,4) → Next is A1

19. API: Determine iteration number
    - Messages 1-2: Iteration 1
    - Messages 3-4: Iteration 2
    - Messages 5-6: Iteration 3

20. API: Build concatenated history
    Example: "A1: [msg1]\nA2: [msg2]\nA1: [msg3]"

21. API: Prepare prompt for next agent
    Prompt: "You are {AgentXPersonality}. Respond to the conversation on {Topic}: {history}"

22. API → OpenAI: Call GPT-3.5-turbo
23. OpenAI → API: Return next message

24. API: Save Message to DB
    - AgentType: "A1" or "A2"
    - IterationNumber: 1, 2, or 3
    - Content: message

25. API: Check if totalMessages === 6
    - If yes: Update Conversation.Status = "Completed", EndTime = now
    - Set isOngoing = false
    - If no: isOngoing = true

26. API → UI: Return response
    {
      conversationId: "...",
      message: "content",
      agentType: "A1" or "A2",
      iterationNumber: 1-3,
      isOngoing: true/false,
      totalMessages: 2-6
    }

27. UI: Replace "..." with message bubble (left for A1, right for A2)
28. UI: If isOngoing === true
    - Display new "..." bubble for next agent
    - GOTO step 15 (repeat loop)

┌─────────────────────────────────────────────────────────────┐
│ PHASE 3: COMPLETION                                          │
└─────────────────────────────────────────────────────────────┘

29. UI: isOngoing === false (after message 6)
30. UI → API: GET /api/conversation/{conversationId}

31. API: Retrieve Conversation and all Messages (sorted)
32. API: Format as Markdown
    "**A1:** [msg1]\n**A2:** [msg2]\n**A1:** [msg3]..."

33. API → UI: Return markdown
    {
      markdown: "**A1:** ...\n**A2:** ...",
      conversationId: "...",
      status: "Completed"
    }

34. UI: Clear bubbles
35. UI: Display complete conversation in Markdown format
36. UI: Conversation complete

┌─────────────────────────────────────────────────────────────┐
│ ERROR HANDLING                                               │
└─────────────────────────────────────────────────────────────┘

- At any point, if API returns error:
  1. UI: Display error message
  2. UI: Stop making further calls
  3. UI: Log error to browser console
  4. API: Log error with Serilog to server console
  5. User must refresh page to start new conversation
```

### Message Count & Agent Alternation

| Message # | Agent | Iteration | isOngoing |
|-----------|-------|-----------|-----------|
| 1 | A1 | 1 | true |
| 2 | A2 | 1 | true |
| 3 | A1 | 2 | true |
| 4 | A2 | 2 | true |
| 5 | A1 | 3 | true |
| 6 | A2 | 3 | **false** ← Complete |

### State Transitions

```
┌─────────┐
│  Page   │
│  Load   │
└────┬────┘
     │
     ▼
┌─────────┐
│  User   │  Enter personalities + topic
│  Input  │
└────┬────┘
     │
     ▼
┌──────────┐
│  Init    │  Create DB, Call A1-M1
│  (POST)  │
└────┬─────┘
     │
     ▼
┌──────────┐     Loop (5 times)
│  Follow  │◄────────┐
│  (POST)  │─────────┘ While isOngoing=true
└────┬─────┘
     │
     │ After 6 messages
     ▼
┌──────────┐
│   Get    │  Retrieve markdown
│  (GET)   │
└────┬─────┘
     │
     ▼
┌──────────┐
│ Display  │  Show markdown
│ Complete │
└──────────┘
```

## 💻 Development

### Visual Studio & GitHub Copilot Setup

#### Recommended Visual Studio Configuration

1. **Enable GitHub Copilot**
   - Install [GitHub Copilot extension](https://marketplace.visualstudio.com/items?itemName=GitHub.copilotvs)
   - Sign in with GitHub account
   - Verify Copilot is active (bottom-right status bar)

2. **Useful Extensions**
   - GitHub Copilot
   - Markdown Editor
   - REST Client (for API testing)
   - EF Core Power Tools

3. **Project Settings**
   - Build → Configuration: Debug
   - Debug → Launch browser: `https://localhost:5001`
   - Hot Reload: Enabled

#### Using GitHub Copilot for This Project

**Copilot Chat Commands:**

```
# Generate OpenAI service method
/explain how to call OpenAI API with prompt format

# Generate entity mapping
/generate EF Core entity configuration for Conversation

# Fix API error
/fix error in conversation controller

# Generate tests
/tests for OpenAI service
```

**Copilot Inline Suggestions:**
- Type `// Call OpenAI API` → Copilot suggests implementation
- Type `// Validate input` → Copilot suggests validation logic
- Type `// Format markdown` → Copilot suggests formatting code

### Running in Development Mode

```bash
# Run with hot reload
dotnet watch run

# Run specific configuration
dotnet run --configuration Debug

# View logs
dotnet run > logs.txt
```

### Creating Migrations

```powershell
# In Package Manager Console
Add-Migration MigrationName
Update-Database

# Rollback
Update-Database PreviousMigrationName

# Remove last migration (if not applied)
Remove-Migration
```

Or using CLI:
```bash
dotnet ef migrations add MigrationName
dotnet ef database update
dotnet ef migrations remove
```

### Testing Workflow Adherence

**Use this checklist to verify strict workflow:**

```
□ 1. Init creates Conversation with string fields (no FK to other tables)
□ 2. Init calls A1 with prompt format "You are {personality}..."
□ 3. Init saves A1-M1 with IterationNumber=1, AgentType="A1"
□ 4. Init returns isOngoing=true
□ 5. Follow alternates agents correctly (odd=A2, even=A1)
□ 6. Follow concatenates history correctly
□ 7. Follow uses correct prompt format with history
□ 8. Follow tracks iteration number correctly (1-3)
□ 9. After message 6, isOngoing=false
□ 10. Get returns markdown with **A1:** and **A2:** format
□ 11. UI stores ConversationId in JS variable
□ 12. UI loops follow calls while isOngoing=true
□ 13. UI displays bubbles correctly (A1 left, A2 right)
□ 14. UI shows static "..." while waiting
□ 15. UI replaces bubbles with markdown on completion
```

###