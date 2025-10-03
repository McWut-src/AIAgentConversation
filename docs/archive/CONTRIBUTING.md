# Contributing to AI Agent Conversation Platform

Thank you for your interest in contributing! This document provides guidelines and information for contributors.

## Table of Contents

- [Code of Conduct](#code-of-conduct)
- [Getting Started](#getting-started)
- [Development Workflow](#development-workflow)
- [Coding Standards](#coding-standards)
- [Testing Guidelines](#testing-guidelines)
- [Documentation](#documentation)
- [Pull Request Process](#pull-request-process)
- [Community](#community)

## Code of Conduct

### Our Standards

- **Be respectful**: Treat all contributors with respect and professionalism
- **Be constructive**: Provide helpful feedback and suggestions
- **Be collaborative**: Work together to improve the project
- **Be patient**: Remember that everyone is learning and contributing in their spare time

## Getting Started

### Prerequisites

Before contributing, ensure you have:
- .NET 8 SDK installed
- Visual Studio 2022 (recommended) or VS Code
- Git for version control
- OpenAI API key for testing
- Familiarity with C#, ASP.NET Core, and Entity Framework Core

### Setting Up Development Environment

1. **Fork the repository**
   ```bash
   # Fork on GitHub, then clone your fork
   git clone https://github.com/YOUR_USERNAME/AIAgentConversation.git
   cd AIAgentConversation
   ```

2. **Set up OpenAI API key**
   ```bash
   dotnet user-secrets init --project AIAgentConversation
   dotnet user-secrets set "OpenAI:ApiKey" "your-api-key-here" --project AIAgentConversation
   ```

3. **Build and run**
   ```bash
   dotnet build
   dotnet run --project AIAgentConversation
   ```

4. **Verify setup**
   - Navigate to `https://localhost:5001`
   - Start a test conversation
   - Check that all 6+ messages generate correctly

## Development Workflow

### Branch Strategy

- `main` - Production-ready code, protected branch
- `feature/*` - New features (e.g., `feature/multi-language-support`)
- `fix/*` - Bug fixes (e.g., `fix/message-ordering-issue`)
- `docs/*` - Documentation updates (e.g., `docs/update-api-guide`)
- `refactor/*` - Code refactoring (e.g., `refactor/service-layer`)

### Creating a Feature Branch

```bash
git checkout main
git pull origin main
git checkout -b feature/your-feature-name
```

### Commit Message Format

Follow conventional commits format:

```
<type>(<scope>): <subject>

<body>

<footer>
```

**Types:**
- `feat`: New feature
- `fix`: Bug fix
- `docs`: Documentation only
- `style`: Code style changes (formatting, no logic change)
- `refactor`: Code refactoring
- `test`: Adding or updating tests
- `chore`: Maintenance tasks

**Examples:**
```
feat(api): add conversation export endpoint

Adds new GET endpoint to export conversations in JSON format.
Includes proper error handling and logging.

Closes #42
```

```
fix(ui): correct message bubble alignment

Message bubbles for Agent 2 were left-aligned instead of right.
Updated CSS to properly align A2 bubbles to the right side.

Fixes #38
```

## Coding Standards

### C# Style Guidelines

**Naming Conventions:**
```csharp
// Classes, interfaces, methods: PascalCase
public class ConversationService { }
public interface IOpenAIService { }
public async Task<string> GenerateResponseAsync() { }

// Private fields: _camelCase
private readonly ILogger<ConversationController> _logger;

// Local variables, parameters: camelCase
var conversation = new Conversation();
public void ProcessMessage(string messageContent) { }

// Constants: PascalCase
public const int MaxIterations = 10;
```

**Async/Await Pattern:**
```csharp
// Always use async for I/O operations
public async Task<ConversationResponse> InitializeAsync(InitRequest request)
{
    var conversation = await _context.Conversations.AddAsync(entity);
    await _context.SaveChangesAsync();
    return response;
}
```

**Error Handling:**
```csharp
try
{
    // Your code
}
catch (Exception ex)
{
    _logger.LogError(ex, "Error in operation: {OperationName}", operationName);
    throw; // Re-throw for controller to handle
}
```

**Dependency Injection:**
```csharp
// Constructor injection
public ConversationController(
    ApplicationDbContext context,
    IOpenAIService openAIService,
    ILogger<ConversationController> logger)
{
    _context = context;
    _openAIService = openAIService;
    _logger = logger;
}
```

### JavaScript Style Guidelines

```javascript
// Use const for constants, let for variables
const MAX_RETRIES = 3;
let conversationId = null;

// Use async/await for API calls
async function startConversation() {
    try {
        const response = await fetch('/api/conversation/init', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(requestData)
        });
        
        if (!response.ok) {
            throw new Error('API call failed');
        }
        
        const data = await response.json();
        return data;
    } catch (error) {
        console.error('Error:', error);
        displayError(error.message);
    }
}

// Use descriptive function names
function createMessageBubble(message, agentType) {
    // Implementation
}
```

### Database Guidelines

**Entity Design:**
```csharp
public class Conversation
{
    [Key]
    public Guid Id { get; set; }
    
    [Required, MaxLength(500)]
    public string Agent1Personality { get; set; } = string.Empty;
    
    // Navigation properties
    public ICollection<Message> Messages { get; set; } = new List<Message>();
}
```

**Query Patterns:**
```csharp
// Use Include for related data
var conversation = await _context.Conversations
    .Include(c => c.Messages)
    .FirstOrDefaultAsync(c => c.Id == id);

// Use AsNoTracking for read-only queries
var conversations = await _context.Conversations
    .AsNoTracking()
    .Where(c => c.Status == "Completed")
    .ToListAsync();
```

## Testing Guidelines

### Manual Testing Checklist

Before submitting a PR, verify:

**API Testing:**
- [ ] All endpoints return correct status codes
- [ ] Error responses include helpful messages
- [ ] Input validation works correctly
- [ ] Database operations complete successfully

**UI Testing:**
- [ ] Form inputs accept and validate data
- [ ] Message bubbles display correctly
- [ ] Phase indicators show accurate phase
- [ ] Export buttons generate correct output
- [ ] Error messages display to user

**Cross-Browser Testing:**
- [ ] Chrome (latest)
- [ ] Firefox (latest)
- [ ] Edge (latest)
- [ ] Safari (if available)

**Conversation Flow:**
- [ ] Introduction phase messages are appropriate
- [ ] Conversation phase shows debate/discussion
- [ ] Conclusion phase summarizes discussion
- [ ] Message count matches configuration
- [ ] Agent alternation is correct
- [ ] Markdown output is properly formatted

### Test Scenarios

**Basic Conversation:**
1. Enter two distinct personalities
2. Enter a specific topic
3. Start conversation
4. Verify all phases complete
5. Check markdown output

**Edge Cases:**
1. Very long personality descriptions (near 500 char limit)
2. Unicode characters in inputs
3. Special characters in topic
4. Minimum exchange count (1)
5. Maximum exchange count (10)

## Documentation

### When to Update Documentation

Update documentation when you:
- Add new features
- Change existing functionality
- Fix bugs that users should know about
- Add or modify API endpoints
- Change configuration options

### Documentation Files

- **README.md**: Project overview, features, quick start
- **API.md**: Complete API endpoint reference
- **UI.md**: User interface guide and JavaScript implementation
- **SETUP.md**: Detailed setup instructions
- **TUTORIAL.md**: Step-by-step tutorial
- **CHANGELOG.md**: Version history and changes
- **CONTRIBUTING.md**: This file

### Documentation Style

- Use clear, concise language
- Include code examples where appropriate
- Use proper markdown formatting
- Test all code examples
- Keep examples up-to-date with codebase

## Pull Request Process

### Before Submitting

1. **Test thoroughly**: Verify your changes work as expected
2. **Update documentation**: Reflect your changes in relevant docs
3. **Follow coding standards**: Ensure code matches project style
4. **Write clear commits**: Use conventional commit format
5. **Rebase on main**: Ensure your branch is up-to-date

### Submitting a Pull Request

1. **Push your branch**
   ```bash
   git push origin feature/your-feature-name
   ```

2. **Create PR on GitHub**
   - Go to the repository
   - Click "New Pull Request"
   - Select your branch
   - Fill out the PR template

3. **PR Title Format**
   ```
   feat: Add conversation export functionality
   fix: Correct message ordering in Phase 2
   docs: Update API documentation for v1.4
   ```

4. **PR Description Template**
   ```markdown
   ## Description
   Brief description of what this PR does
   
   ## Changes Made
   - Added export endpoint for JSON format
   - Updated UI with export buttons
   - Added unit tests for export service
   
   ## Testing Done
   - Manual testing of all export formats
   - Verified existing functionality unchanged
   - Tested with edge cases
   
   ## Related Issues
   Closes #42
   
   ## Screenshots (if applicable)
   [Include screenshots for UI changes]
   ```

### Code Review Process

1. **Automated checks**: CI/CD runs (if configured)
2. **Reviewer assigned**: Maintainer reviews code
3. **Feedback addressed**: Make requested changes
4. **Approval**: Maintainer approves PR
5. **Merge**: PR merged to main branch

### After Merge

- Delete your feature branch (locally and on GitHub)
- Pull latest main to stay updated
- Celebrate your contribution! 🎉

## Community

### Getting Help

- **Documentation**: Check docs first
- **GitHub Discussions**: Ask questions
- **GitHub Issues**: Report bugs or request features

### Reporting Bugs

Use the bug report template:

```markdown
**Describe the bug**
A clear description of what the bug is.

**To Reproduce**
Steps to reproduce:
1. Go to '...'
2. Click on '...'
3. See error

**Expected behavior**
What you expected to happen.

**Screenshots**
If applicable, add screenshots.

**Environment:**
- OS: [e.g., Windows 11]
- Browser: [e.g., Chrome 120]
- .NET Version: [e.g., 8.0]
```

### Feature Requests

Use the feature request template:

```markdown
**Is your feature request related to a problem?**
A clear description of the problem.

**Describe the solution you'd like**
What you want to happen.

**Describe alternatives considered**
Other solutions you've considered.

**Additional context**
Any other context or screenshots.
```

## Recognition

Contributors are recognized in:
- GitHub contributors list
- Release notes
- Project documentation

Thank you for contributing to AI Agent Conversation Platform!

---

**Questions?** Open a GitHub Discussion or reach out to the maintainers.
