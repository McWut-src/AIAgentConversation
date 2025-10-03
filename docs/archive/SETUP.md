# Setup Instructions

## Prerequisites

- .NET 8 SDK installed
- OpenAI API key (get from https://platform.openai.com/api-keys)

## Quick Start

### 1. Clone the Repository

```bash
git clone https://github.com/McWut-src/AIAgentConversation.git
cd AIAgentConversation
```

### 2. Configure OpenAI API Key

You have two options:

**Option A: User Secrets (Recommended for Development)**

```bash
cd AIAgentConversation
dotnet user-secrets init
dotnet user-secrets set "OpenAI:ApiKey" "your-openai-api-key-here"
```

**Option B: Environment Variable**

```bash
export OpenAI__ApiKey="your-openai-api-key-here"
```

### 3. Run the Application

```bash
cd AIAgentConversation
dotnet run
```

The application will start at `https://localhost:5001`

### 4. Use the Application

1. Open your browser to `https://localhost:5001`
2. Fill in the three text fields:
   - **Agent 1 Personality**: e.g., "Logical analyst who values data and evidence"
   - **Agent 2 Personality**: e.g., "Creative thinker who uses metaphors and storytelling"
   - **Conversation Topic**: e.g., "The future of artificial intelligence"
3. Click "Start Conversation"
4. Watch as the two AI agents exchange 6 messages (3 iterations)
5. View the final conversation in markdown format

## Database

The application uses SQLite in development mode (automatically created as `AIConversations.db`).

For production, update the connection string in `appsettings.json` to use SQL Server.

## Project Structure

```
AIAgentConversation/
├── Controllers/
│   └── ConversationController.cs      # API endpoints
├── Data/
│   ├── ApplicationDbContext.cs        # EF Core context
│   └── Migrations/                     # Database migrations
├── Models/
│   ├── Conversation.cs                 # Conversation entity
│   ├── Message.cs                      # Message entity
│   └── DTOs/                           # Data transfer objects
├── Services/
│   ├── IOpenAIService.cs              # OpenAI service interface
│   └── OpenAIService.cs                # OpenAI implementation
├── Pages/
│   └── Index.cshtml                    # Main UI page
├── wwwroot/
│   ├── css/site.css                    # Styles
│   └── js/conversation.js              # Client logic
└── Program.cs                          # Application entry point
```

## Key Features

- ✅ Simplified database schema (personalities and topics stored as strings)
- ✅ Fixed 3 iterations (6 messages total)
- ✅ Exact OpenAI prompt format as specified
- ✅ Simple history concatenation
- ✅ Agent alternation (A1→A2→A1→A2→A1→A2)
- ✅ Text inputs (no dropdowns)
- ✅ Static waiting indicators (no animations)
- ✅ Stateless design (no session storage)
- ✅ Serilog console-only logging

## Troubleshooting

### OpenAI API Error

If you get an error about the OpenAI API key:
1. Verify your API key is correct
2. Check you have credits available in your OpenAI account
3. Ensure the key is properly set in user secrets or environment variable

### Database Error

If you get a database error:
1. Delete the `AIConversations.db` file
2. Run `dotnet ef database update` to recreate the database

### Build Error

If you get a build error:
1. Ensure you have .NET 8 SDK installed: `dotnet --version`
2. Run `dotnet restore` to restore packages
3. Run `dotnet build` to build the project

## Documentation

- **[README.md](README.md)** - Project overview
- **[API.md](API.md)** - API endpoint documentation
- **[UI.md](UI.md)** - UI usage guide
- **[TUTORIAL.md](TUTORIAL.md)** - Complete tutorial
- **[AI_CUSTOMIZATION_GUIDE.md](AI_CUSTOMIZATION_GUIDE.md)** - AI customization guide
- **[CONVERSATION_PHASES.md](CONVERSATION_PHASES.md)** - Phase structure documentation

## Support

For issues or questions, please check the documentation files or open an issue on GitHub.
