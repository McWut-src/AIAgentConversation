# AI Agent Conversation Platform

A .NET 8 web application that enables dynamic conversations between two AI agents powered by OpenAI's GPT-3.5-turbo API. Watch as agents with distinct personalities engage in meaningful dialogues with genuine debate, structured conversation phases, and configurable intensity levels.

![.NET Core](https://img.shields.io/badge/.NET%20Core-8.0-purple)
![SQL Server](https://img.shields.io/badge/SQL%20Server-Local-red)
![OpenAI](https://img.shields.io/badge/OpenAI-GPT--3.5--turbo-green)
![License](https://img.shields.io/badge/license-MIT-blue)

---

## 📋 Table of Contents

- [Features](#-features)
- [Quick Start](#-quick-start)
- [Architecture](#%EF%B8%8F-architecture)
- [Installation](#-installation-guide)
- [API Documentation](#-api-documentation)
- [User Interface](#-user-interface)
- [AI Behavior & Customization](#-ai-behavior--customization)
- [Conversation Phases](#-conversation-phases)
- [Development](#-development)
- [Testing](#-testing)
- [Troubleshooting](#-troubleshooting)
- [Contributing](#-contributing)
- [Changelog](#-changelog)
- [License](#-license)

---

## 🌟 Features

### Core Features
- **Dual AI Agents**: Two independent AI agents with configurable personalities
- **Genuine Debate Flow**: Agents engage in real intellectual exchange with disagreement and counterarguments
- **Structured Conversation Phases**:
  - **Introduction**: Agents stake out initial positions (2 messages)
  - **Conversation**: Configurable back-and-forth debate exchanges (1-10, default 3)
  - **Conclusion**: Agents summarize key arguments (2 messages)
- **Configurable Length**: Adjust conversation from 1-10 exchanges (4-24 total messages)
- **Phase-Specific AI Prompts**: Tailored prompts encouraging critical engagement
- **Enhanced AI Quality**: Advanced prompt engineering promoting genuine debate
- **Text-Based Personalities**: Simple text input for agent personalities
- **Custom Topics**: Enter any topic for conversation
- **Politeness Control**: Adjustable debate intensity (5 levels from direct to diplomatic)
- **Real-time Display**: SMS-style bubble interface with phase indicators
- **Phase Badges**: Color-coded badges showing conversation phase
- **Complete History**: Final conversation displayed in formatted Markdown
- **Database Persistence**: All conversations stored in local SQL Server
- **Simplified Architecture**: Stateless design with new conversation per refresh
- **Error Logging**: Serilog console logging
- **Smart Temperature**: Progressive creativity adjustment (0.5 → 0.7 → 0.9)
- **Export Options**: Export as JSON, Markdown, Text, or XML

---

## 🚀 Quick Start

### Prerequisites

- **.NET 8 SDK** - [Download here](https://dotnet.microsoft.com/download/dotnet/8.0)
- **OpenAI API Key** - [Get your key](https://platform.openai.com/api-keys)
- **SQL Server LocalDB** - Included with Visual Studio or install separately

### Installation (5 minutes)

1. **Clone the repository**
   ```bash
   git clone https://github.com/McWut-src/AIAgentConversation.git
   cd AIAgentConversation
   ```

2. **Set up your OpenAI API key**
   ```bash
   dotnet user-secrets init --project AIAgentConversation
   dotnet user-secrets set "OpenAI:ApiKey" "your-api-key-here" --project AIAgentConversation
   ```

3. **Apply database migrations**
   ```bash
   dotnet ef database update --project AIAgentConversation
   ```

4. **Run the application**
   ```bash
   dotnet run --project AIAgentConversation
   ```

5. **Open in browser**
   - Navigate to `https://localhost:5001`
   - Enter two distinct personalities
   - Choose a topic
   - Adjust conversation length and politeness (optional)
   - Click "Start Conversation"
   - Watch the AI agents converse!

### First Conversation Example

**Try these inputs:**
- **Agent 1 Personality**: "A pragmatic software engineer who values proven solutions and best practices"
- **Agent 2 Personality**: "A forward-thinking innovator who embraces cutting-edge technologies"
- **Topic**: "The role of AI in modern software development"
- **Conversation Length**: 3 exchanges (default)
- **Politeness**: Level 3 - Balanced (default)

---

## 🏗️ Architecture

### System Overview

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
│     /api/conversation/export/{id}/{format}               │
└────────────────────┬────────────────────────────────────┘
                     │
        ┌────────────┼────────────┐
        │            │            │
┌───────▼──────┐ ┌──▼──────────┐ ┌─▼──────────────┐
│   OpenAI     │ │    EF Core  │ │     Serilog    │
│   Service    │ │   DbContext │ │    Logging     │
│ (GPT-3.5)    │ │             │ │   (Console)    │
└──────────────┘ └──────┬──────┘ └────────────────┘
                        │
                ┌───────▼────────┐
                │  SQL Server    │
                │   LocalDB      │
                └────────────────┘
```

### Technology Stack

- **.NET 8 / ASP.NET Core 8**: Modern web framework
- **Razor Pages**: Single-page application
- **Entity Framework Core 8**: Database ORM
- **SQL Server LocalDB/Express**: Local database
- **OpenAI API (GPT-3.5-turbo)**: AI conversation engine
- **Serilog**: Console-only logging
- **Vanilla JavaScript**: No frameworks (ES6+)
- **Plain CSS**: No Bootstrap/Tailwind

### Database Schema

#### Conversation Entity
```csharp
public class Conversation
{
    public Guid Id { get; set; }
    
    [Required, MaxLength(500)]
    public string Agent1Personality { get; set; }
    
    [Required, MaxLength(500)]
    public string Agent2Personality { get; set; }
    
    [Required, MaxLength(1000)]
    public string Topic { get; set; }
    
    public int ConversationLength { get; set; } // 1-10 exchanges
    
    [MaxLength(50)]
    public string PolitenessLevel { get; set; } // "Low", "Medium", "High"
    
    [Required, MaxLength(50)]
    public string Status { get; set; } // "InProgress", "Completed", "Failed"
    
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    
    public ICollection<Message> Messages { get; set; }
}
```

#### Message Entity
```csharp
public class Message
{
    public Guid Id { get; set; }
    
    [Required]
    public Guid ConversationId { get; set; }
    
    [Required, MaxLength(10)]
    public string AgentType { get; set; } // "A1" or "A2"
    
    public int IterationNumber { get; set; }
    
    [MaxLength(20)]
    public string Phase { get; set; } // "Introduction", "Conversation", "Conclusion"
    
    [Required]
    public string Content { get; set; }
    
    public DateTime Timestamp { get; set; }
    
    public Conversation Conversation { get; set; }
}
```

### Project Structure

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
│       ├── InitConversationRequest.cs
│       ├── FollowConversationRequest.cs
│       └── ConversationResponse.cs
├── Services/
│   ├── IOpenAIService.cs              # OpenAI service interface
│   └── OpenAIService.cs                # OpenAI implementation
├── Pages/
│   └── Index.cshtml                    # Main UI page
├── wwwroot/
│   ├── css/site.css                    # Styles
│   └── js/conversation.js              # Client logic
├── appsettings.json                    # Configuration
└── Program.cs                          # Application entry point
```

---

## 📦 Installation Guide

### Detailed Setup Instructions

#### Step 1: Prerequisites

1. **Install .NET 8 SDK**
   - Download from: https://dotnet.microsoft.com/download/dotnet/8.0
   - Verify installation:
     ```bash
     dotnet --version
     # Should show 8.0.x
     ```

2. **Install SQL Server LocalDB** (if not already installed)
   - Included with Visual Studio 2022
   - Or download separately from Microsoft
   - Verify it's running:
     ```bash
     sqllocaldb info mssqllocaldb
     # Start if needed:
     sqllocaldb start mssqllocaldb
     ```

3. **Get OpenAI API Key**
   - Sign up at: https://platform.openai.com/
   - Navigate to API Keys section
   - Create new secret key
   - Copy and save securely
   - **Cost**: ~$0.002 per conversation (very affordable)

#### Step 2: Clone and Configure

1. **Clone Repository**
   ```bash
   git clone https://github.com/McWut-src/AIAgentConversation.git
   cd AIAgentConversation
   ```

2. **Configure OpenAI API Key**

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

3. **Restore Dependencies**
   ```bash
   dotnet restore
   ```

#### Step 3: Database Setup

The application uses SQLite in development mode (automatically created as `AIConversations.db`).

For production, update the connection string in `appsettings.json` to use SQL Server:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=AIAgentConversation;Trusted_Connection=true;"
  }
}
```

Apply migrations:
```bash
dotnet ef database update --project AIAgentConversation
```

#### Step 4: Run the Application

```bash
dotnet run --project AIAgentConversation
```

The application will start at `https://localhost:5001`

#### Step 5: First Test

1. Open browser to `https://localhost:5001`
2. Fill in:
   - **Agent 1 Personality**: "Logical analyst who values data and evidence"
   - **Agent 2 Personality**: "Creative thinker who uses metaphors and storytelling"
   - **Topic**: "The future of artificial intelligence"
   - **Conversation Length**: 3 (default)
   - **Politeness**: Level 3 - Balanced (default)
3. Click "Start Conversation"
4. Watch the agents exchange messages
5. View final markdown output

---

## 📡 API Documentation

### Overview

**Base URL:** `https://localhost:5001/api`  
**Content-Type:** `application/json`  
**API Version:** 1.4.0

### Authentication

**Current Version:** No authentication required (v1.0+)

This is a development version intended for local use. Production deployments should implement proper authentication.

### Endpoints

#### 1. Initialize Conversation

Creates a new conversation and returns the first message from Agent 1.

**Endpoint:** `POST /api/conversation/init`

**Request Body:**
```json
{
  "agent1Personality": "string (max 500 chars, required)",
  "agent2Personality": "string (max 500 chars, required)",
  "topic": "string (max 1000 chars, required)",
  "conversationLength": "integer (1-10, optional, default: 3)",
  "politenessLevel": "string (Low/Medium/High, optional, default: Medium)"
}
```

**Example Request:**
```json
{
  "agent1Personality": "A data-driven scientist who relies on empirical evidence",
  "agent2Personality": "A holistic philosopher who questions fundamental assumptions",
  "topic": "The nature of consciousness",
  "conversationLength": 5,
  "politenessLevel": "Medium"
}
```

**Success Response (200 OK):**
```json
{
  "conversationId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "message": "From a scientific perspective...",
  "agentType": "A1",
  "iterationNumber": 1,
  "phase": "Introduction",
  "isOngoing": true,
  "totalMessages": 1
}
```

**Error Responses:**
- `400 Bad Request`: Invalid input parameters
- `500 Internal Server Error`: OpenAI API error or server error

#### 2. Continue Conversation

Gets the next message in the conversation sequence.

**Endpoint:** `POST /api/conversation/follow`

**Request Body:**
```json
{
  "conversationId": "guid (required)"
}
```

**Success Response (200 OK):**
```json
{
  "conversationId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "message": "But what if consciousness...",
  "agentType": "A2",
  "iterationNumber": 1,
  "phase": "Introduction",
  "isOngoing": true,
  "totalMessages": 2
}
```

**Agent Alternation Logic:**
- Odd message counts → A2 responds
- Even message counts → A1 responds
- Pattern: A1 → A2 → A1 → A2 → ...

**Completion Logic:**
- Total messages = 4 + (conversationLength × 2)
- For conversationLength=3: 4 + (3 × 2) = 10 messages
- `isOngoing` becomes `false` when complete

#### 3. Get Complete Conversation

Retrieves the completed conversation in markdown format.

**Endpoint:** `GET /api/conversation/{id}`

**Success Response (200 OK):**
```json
{
  "conversationId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "markdown": "**A1:** From a scientific perspective...\\n**A2:** But what if consciousness...\\n...",
  "agent1Personality": "A data-driven scientist",
  "agent2Personality": "A holistic philosopher",
  "topic": "The nature of consciousness",
  "status": "Completed",
  "messageCount": 10
}
```

**Error Responses:**
- `404 Not Found`: Conversation not found or not completed
- `500 Internal Server Error`: Server error

#### 4. Export Conversation

Exports conversation in various formats.

**Endpoint:** `GET /api/conversation/export/{id}/{format}`

**Supported Formats:**
- `json` - Structured JSON with full metadata
- `markdown` - Formatted markdown text
- `text` - Plain text format
- `xml` - XML structure

**Example:**
```
GET /api/conversation/export/3fa85f64-5717-4562-b3fc-2c963f66afa6/json
```

### Workflow Requirements

**Critical Requirements:**

1. **Configurable Iterations**: 1-10 exchanges (4-24 total messages)
2. **Agent Alternation**: Must follow odd/even pattern (A1, A2, A1, A2, ...)
3. **Prompt Format**: `"You are {personality}. Respond to the conversation on {topic}: {history}"`
4. **History Format**: Simple concatenation `"A1: msg1\\nA2: msg2\\n..."`
5. **Phase Tracking**: Messages tagged with Introduction/Conversation/Conclusion
6. **Completion Flag**: `isOngoing = false` when totalMessages reaches expected count
7. **Status Update**: Set to "Completed" when all messages generated
8. **Markdown Format**: `**A1:** msg\\n**A2:** msg\\n...`

### Error Handling

All endpoints return consistent error structure:

```json
{
  "error": "Error type",
  "message": "Detailed error message",
  "timestamp": "2025-01-02T12:00:00Z"
}
```

**Common Error Codes:**
- `400`: Validation error (missing/invalid parameters)
- `404`: Resource not found
- `500`: Server/OpenAI API error

---

## 🎨 User Interface

### Overview

The UI is a single-page Razor application with vanilla JavaScript for real-time conversation display.

**Framework:** ASP.NET Core Razor Pages  
**JavaScript:** Vanilla ES6+ (no libraries)  
**CSS:** Custom styles (no Bootstrap/Tailwind)  
**Design:** Stateless, session-independent

### User Interface Components

#### 1. Input Form Section

Collects agent personalities, topic, and conversation settings.

**Components:**
- Agent 1 Personality (text input)
- Agent 2 Personality (text input)
- Conversation Topic (text input)
- Conversation Length selector (1-10 exchanges)
- Politeness Control slider (5 levels)
- Start Conversation button

**HTML Structure:**
```html
<div class="input-section">
    <input type="text" id="agent1-personality" placeholder="Agent 1 Personality" />
    <input type="text" id="agent2-personality" placeholder="Agent 2 Personality" />
    <input type="text" id="topic" placeholder="Topic" />
    <select id="conversation-length">
        <option value="3" selected>3 exchanges (10 messages)</option>
    </select>
    <input type="range" id="politeness-level" min="1" max="5" value="3" />
    <button id="start-button">Start Conversation</button>
</div>
```

**Critical Requirements:**
- ✅ Text inputs for personalities (NOT dropdowns for personality selection)
- ✅ Free text entry for all personality and topic fields
- ✅ Conversation length configurable (1-10)
- ✅ Politeness control optional (defaults to Medium)

#### 2. Conversation Display Area

Shows real-time conversation with SMS-style message bubbles.

**Components:**
- Message bubbles (color-coded by agent)
- Phase badges (Introduction/Conversation/Conclusion)
- Waiting indicators (static "..." text)
- Agent labels ("A1" or "A2")

**Message Bubble Structure:**
```html
<div class="message-bubble message-bubble-a1">
    <span class="phase-badge phase-introduction">Introduction</span>
    <div class="message-agent">A1</div>
    <div class="message-content">Message content...</div>
</div>
```

**Critical Requirements:**
- ✅ A1 messages: **left-aligned, blue background**
- ✅ A2 messages: **right-aligned, green background**
- ✅ Waiting indicator: **static text "..."** (no CSS animations)
- ✅ Phase badges: color-coded (blue=intro, purple=conversation, green=conclusion)
- ❌ **NO animated dots** with keyframes or transitions

#### 3. Markdown Display Area

Shows completed conversation in formatted text.

**HTML Structure:**
```html
<div id="markdown-container" class="markdown-display" style="display: none;">
    <!-- Markdown content inserted here -->
</div>
```

**Format:**
```
**A1:** First message
**A2:** First response
**A1:** Second message
...
```

**Critical Requirements:**
- ✅ Displayed when conversation completes
- ✅ Replaces bubble display
- ✅ Preserves line breaks with `white-space: pre-wrap`
- ✅ Monospace font for readability

#### 4. Export Buttons

Allow downloading conversation in various formats.

**HTML Structure:**
```html
<div class="export-buttons">
    <button onclick="exportConversation('json')">Export JSON</button>
    <button onclick="exportConversation('markdown')">Export Markdown</button>
    <button onclick="exportConversation('text')">Export Text</button>
    <button onclick="exportConversation('xml')">Export XML</button>
</div>
```

### JavaScript Implementation

**File:** `wwwroot/js/conversation.js`

#### Global Variables

```javascript
let conversationId = null; // Current conversation ID (not stored)
```

**Critical:** Uses JavaScript variable, NOT localStorage/sessionStorage (stateless design)

#### Core Functions

**1. startConversation()**

Initializes a new conversation.

```javascript
async function startConversation() {
    const agent1 = document.getElementById('agent1-personality').value.trim();
    const agent2 = document.getElementById('agent2-personality').value.trim();
    const topic = document.getElementById('topic').value.trim();
    const length = document.getElementById('conversation-length').value;
    const politeness = getPolitenessLevel();
    
    // Validate inputs
    if (!agent1 || !agent2 || !topic) {
        displayError('All fields are required');
        return;
    }
    
    // Clear previous conversation
    document.getElementById('conversation-container').innerHTML = '';
    document.getElementById('markdown-container').style.display = 'none';
    
    // Show waiting indicator
    const waitingDiv = createWaitingIndicator('left');
    document.getElementById('conversation-container').appendChild(waitingDiv);
    
    try {
        const response = await fetch('/api/conversation/init', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                agent1Personality: agent1,
                agent2Personality: agent2,
                topic: topic,
                conversationLength: parseInt(length),
                politenessLevel: politeness
            })
        });
        
        const data = await response.json();
        conversationId = data.conversationId;
        
        // Remove waiting indicator
        waitingDiv.remove();
        
        // Add first message
        const bubble = createMessageBubble(data.message, data.agentType, data.phase);
        document.getElementById('conversation-container').appendChild(bubble);
        
        // Continue if ongoing
        if (data.isOngoing) {
            continueConversation();
        }
    } catch (error) {
        displayError('Failed to start conversation');
        waitingDiv.remove();
    }
}
```

**2. continueConversation()**

Recursively fetches next messages until conversation completes.

```javascript
async function continueConversation() {
    // Determine next agent side
    const messageCount = document.querySelectorAll('.message-bubble').length;
    const nextSide = messageCount % 2 === 1 ? 'right' : 'left';
    
    // Show waiting indicator
    const waitingDiv = createWaitingIndicator(nextSide);
    document.getElementById('conversation-container').appendChild(waitingDiv);
    
    try {
        const response = await fetch('/api/conversation/follow', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ conversationId })
        });
        
        const data = await response.json();
        
        // Remove waiting indicator
        waitingDiv.remove();
        
        // Add message bubble
        const bubble = createMessageBubble(data.message, data.agentType, data.phase);
        document.getElementById('conversation-container').appendChild(bubble);
        
        // Continue or finalize
        if (data.isOngoing) {
            continueConversation(); // Recursive call
        } else {
            displayMarkdown();
        }
    } catch (error) {
        displayError('Failed to continue conversation');
        waitingDiv.remove();
    }
}
```

**3. displayMarkdown()**

Fetches and displays completed conversation.

```javascript
async function displayMarkdown() {
    try {
        const response = await fetch(`/api/conversation/${conversationId}`);
        const data = await response.json();
        
        // Hide bubbles, show markdown
        document.getElementById('conversation-container').style.display = 'none';
        const markdownContainer = document.getElementById('markdown-container');
        markdownContainer.textContent = data.markdown;
        markdownContainer.style.display = 'block';
        
        // Show export buttons
        document.getElementById('export-buttons').style.display = 'block';
    } catch (error) {
        displayError('Failed to retrieve conversation');
    }
}
```

**4. createMessageBubble()**

Creates a message bubble element.

```javascript
function createMessageBubble(message, agentType, phase) {
    const div = document.createElement('div');
    div.className = agentType === 'A1' 
        ? 'message-bubble message-bubble-a1'  // Left-aligned, blue
        : 'message-bubble message-bubble-a2'; // Right-aligned, green
    
    // Phase badge
    const badge = document.createElement('span');
    badge.className = `phase-badge phase-${phase.toLowerCase()}`;
    badge.textContent = phase;
    
    // Agent label
    const agent = document.createElement('div');
    agent.className = 'message-agent';
    agent.textContent = agentType;
    
    // Content
    const content = document.createElement('div');
    content.className = 'message-content';
    content.textContent = message;
    
    div.appendChild(badge);
    div.appendChild(agent);
    div.appendChild(content);
    
    return div;
}
```

**5. createWaitingIndicator()**

Creates static waiting indicator.

```javascript
function createWaitingIndicator(side) {
    const div = document.createElement('div');
    div.className = `waiting-indicator waiting-${side}`;
    div.textContent = '...'; // Plain text, NO animation
    return div;
}
```

**6. displayError()**

Shows error message to user.

```javascript
function displayError(message) {
    const errorContainer = document.getElementById('error-container');
    errorContainer.textContent = `Error: ${message}`;
    errorContainer.style.display = 'block';
    
    setTimeout(() => {
        errorContainer.style.display = 'none';
    }, 5000);
}
```

### Visual Design

#### Color Scheme

- **Agent 1 (A1)**: Blue (#007bff) - Left-aligned
- **Agent 2 (A2)**: Green (#28a745) - Right-aligned
- **Phase Introduction**: Light blue (#cce5ff)
- **Phase Conversation**: Light purple (#e2d5f1)
- **Phase Conclusion**: Light green (#d4edda)
- **Error**: Red (#dc3545)
- **Background**: Light gray (#f8f9fa)

#### CSS Requirements

```css
/* Message Bubbles */
.message-bubble-a1 {
    background-color: #007bff;
    color: white;
    text-align: left;
    margin-right: auto; /* Left align */
    max-width: 70%;
}

.message-bubble-a2 {
    background-color: #28a745;
    color: white;
    text-align: right;
    margin-left: auto; /* Right align */
    max-width: 70%;
}

/* Waiting Indicator - NO ANIMATION */
.waiting-indicator {
    color: #999;
    font-size: 14px;
    /* NO keyframes, NO transitions */
}

.waiting-left {
    text-align: left;
}

.waiting-right {
    text-align: right;
}

/* Phase Badges */
.phase-badge {
    display: inline-block;
    padding: 2px 8px;
    border-radius: 4px;
    font-size: 12px;
    margin-bottom: 4px;
}

.phase-introduction {
    background-color: #cce5ff;
    color: #004085;
}

.phase-conversation {
    background-color: #e2d5f1;
    color: #6c2c91;
}

.phase-conclusion {
    background-color: #d4edda;
    color: #155724;
}
```

### State Management

**Stateless Design:**
- No localStorage or sessionStorage
- Conversation ID stored in JavaScript variable only
- Page refresh clears all state
- Each page load starts fresh

### Browser Compatibility

**Supported Browsers:**
- Chrome 90+
- Firefox 88+
- Edge 90+
- Safari 14+

**JavaScript Features Used:**
- ES6+ async/await
- Fetch API
- Template literals
- Arrow functions
- DOM manipulation

---

## 🤖 AI Behavior & Customization

### Overview

The AI Agent Conversation Platform uses sophisticated prompt engineering and dynamic parameters to create engaging, natural conversations with genuine debate.

### Key AI Features

- **Three-phase conversation structure** (Introduction, Conversation, Conclusion)
- **Progressive temperature adjustment** (0.5 → 0.7 → 0.9)
- **Politeness control system** (5 levels of debate intensity)
- **Phase-specific prompting** (Different guidance per phase)
- **Genuine debate encouragement** (Agents disagree and challenge ideas)

### Temperature Progression

Temperature controls creativity and variability in AI responses.

**Implementation:**

```csharp
// Phase 1: Introduction (Temperature 0.5)
// - Focused, consistent position statements
// - Clear articulation of initial views

// Phase 2: Conversation (Temperature 0.7)
// - Balanced creativity
// - Engaging debate and counterarguments

// Phase 3: Conclusion (Temperature 0.9)
// - Creative synthesis
// - Thoughtful reflection
```

**Default Settings:**
- Introduction: 0.5 (focused)
- Conversation: 0.7 (balanced)
- Conclusion: 0.9 (creative)

### Politeness Control

Adjusts debate intensity from direct/assertive to diplomatic.

**Five Politeness Levels:**

**Level 1 - Low (Direct):**
- Very direct challenges
- Strong disagreement
- Assertive language
- Example: "That's simply incorrect. The data clearly shows..."

**Level 2 - Medium-Low:**
- Direct but respectful
- Clear disagreement with explanations
- Example: "I disagree because the evidence suggests..."

**Level 3 - Medium (Balanced) - DEFAULT:**
- Balanced tone
- Respectful disagreement
- Acknowledges points before countering
- Example: "While I see your point, I believe..."

**Level 4 - Medium-High:**
- Diplomatic phrasing
- Softer disagreement
- Example: "I appreciate that perspective, though I'd suggest..."

**Level 5 - High (Courteous):**
- Very diplomatic
- Gentle disagreement
- Emphasizes common ground
- Example: "That's an interesting view. Perhaps we could also consider..."

**Setting Politeness:**

In UI:
- Slider control (1-5)
- Real-time preview of tone
- Default: Level 3 (Medium)

In API:
```json
{
  "politenessLevel": "Medium"  // "Low", "Medium", "High"
}
```

### Prompt Engineering

**System Prompt Structure:**

```csharp
string systemPrompt = $"You are {personality}. " +
    $"You are engaging in a {phase} phase of a thoughtful conversation about {topic}. " +
    $"Stay true to your personality traits while being engaging and substantive. " +
    GetPhaseGuidance(phase, politeness) +
    $"Provide responses that are 2-4 sentences long, balancing depth with conciseness. " +
    $"Build upon or challenge previous points when continuing the conversation.";
```

**Phase-Specific Guidance:**

**Introduction Phase:**
```
"This is the INTRODUCTION phase. Introduce yourself briefly and share your 
initial perspective on the topic. Keep it concise (2-3 sentences). 
Set the tone for the discussion ahead."
```

**Conversation Phase:**
```
"This is the CONVERSATION phase. Engage deeply with the points made. 
Challenge ideas, build on arguments, or present counterpoints. 
Don't just agree - intellectual debate requires disagreement and critical engagement."
```

**Conclusion Phase:**
```
"This is the CONCLUSION phase. Summarize your key points from the conversation. 
Reflect on what was discussed and provide a thoughtful closing statement (2-3 sentences). 
You may acknowledge valid points made by the other agent while reinforcing your perspective."
```

### History Concatenation

Conversation history is sent as simple text:

```csharp
string history = string.Join("\n", 
    messages.OrderBy(m => m.Timestamp)
           .Select(m => $"{m.AgentType}: {m.Content}"));

// Example output:
// A1: From a scientific perspective...
// A2: But what if we consider...
// A1: The empirical evidence shows...
```

**Critical:** Simple concatenation, NOT structured JSON or complex formats.

### Customization Guide

#### Adjust Response Length

**File:** `Services/OpenAIService.cs`

**Make responses shorter:**
```csharp
"Provide responses that are 1-2 sentences long, focusing on conciseness."
```

**Make responses longer:**
```csharp
"Provide responses that are 3-5 sentences long, with detailed explanations."
```

**Variable length:**
```csharp
var lengthGuideline = phase == "Introduction" ? "2-3 sentences" : "3-4 sentences";
```

#### Modify Temperature

**Current implementation:**
```csharp
float temperature = phase switch {
    "Introduction" => 0.5f,
    "Conversation" => 0.7f,
    "Conclusion" => 0.9f,
    _ => 0.7f
};
```

**More conservative (less creative):**
```csharp
float temperature = phase switch {
    "Introduction" => 0.3f,
    "Conversation" => 0.5f,
    "Conclusion" => 0.7f,
    _ => 0.5f
};
```

**More creative:**
```csharp
float temperature = phase switch {
    "Introduction" => 0.7f,
    "Conversation" => 0.9f,
    "Conclusion" => 1.1f,
    _ => 0.9f
};
```

#### Add Custom Prompts

**Topic-specific guidance:**
```csharp
string topicGuidance = topic.ToLower() switch {
    var t when t.Contains("science") => "Focus on empirical evidence and methodology.",
    var t when t.Contains("philosophy") => "Explore fundamental assumptions and logical reasoning.",
    var t when t.Contains("ethics") => "Consider moral implications and diverse perspectives.",
    _ => ""
};

systemPrompt += topicGuidance;
```

#### Debugging AI Responses

**Enable detailed logging:**

```csharp
_logger.LogInformation("OpenAI Request - Phase: {Phase}, Temperature: {Temp}, Politeness: {Politeness}", 
    phase, temperature, politeness);
_logger.LogInformation("System Prompt: {Prompt}", systemPrompt);
_logger.LogInformation("History: {History}", history);
_logger.LogInformation("AI Response: {Response}", response);
```

**Check logs:**
- Visual Studio Output window
- Console output when running with `dotnet run`
- Look for patterns in responses
- Adjust prompts based on observations

### Best Practices

**Choosing Personalities:**
- Make them distinct and complementary
- Avoid overly similar viewpoints
- Clear, specific personality descriptions work best
- Example: "Data-driven analyst" vs "Intuitive creative thinker"

**Choosing Topics:**
- Topics with multiple valid perspectives work best
- Avoid yes/no questions
- Complex topics = longer conversations
- Simple topics = shorter conversations (1-3 exchanges)

**Conversation Length:**
- Short (1-2 exchanges): Simple topics, quick demos
- Medium (3-5 exchanges): Most topics, balanced depth - **RECOMMENDED**
- Long (6-10 exchanges): Complex topics, deep exploration

**Politeness Level:**
- Low: Academic/professional debates where directness is valued
- Medium: General purpose, most topics - **RECOMMENDED**
- High: Sensitive topics, collaborative discussions

---

## 🔄 Conversation Phases

### Phase Structure

The platform uses a three-phase conversation flow for natural, engaging dialogues.

### Phase 1: Introduction

**Purpose**: Agents introduce themselves and state initial positions

**Duration**: 2 messages (1 from each agent)

**AI Temperature**: 0.5 (focused, consistent)

**Characteristics**:
- Brief self-introduction
- Clear initial stance on topic
- 2-3 sentences
- Sets tone for discussion

**Example:**
```
A1: "Hello! I'm a data-driven analyst who relies on empirical evidence. 
Regarding climate change, I believe the scientific consensus is clear based 
on decades of research and measurable trends."

A2: "Greetings! I'm a holistic thinker who considers interconnected systems. 
Climate change represents a complex web of natural and human factors that 
we must approach with both urgency and nuance."
```

### Phase 2: Conversation

**Purpose**: Deep intellectual engagement and debate

**Duration**: Configurable (1-10 exchanges, default 3 = 6 messages)

**AI Temperature**: 0.7 (balanced creativity)

**Characteristics**:
- Challenge opponent's ideas
- Present counterarguments
- Build on previous points
- Ask thought-provoking questions
- Maintain respectful disagreement
- 2-4 sentences per response

**Example:**
```
A1: "The data shows a clear correlation between CO2 emissions and global 
temperature rise. We need immediate policy interventions based on this 
evidence, not philosophical debates."

A2: "While I acknowledge the data, we must also consider the socioeconomic 
systems that produce those emissions. A purely technical solution ignores 
the human dimension of the problem."

A1: "Fair point, but we don't have time for slow systemic change. The IPCC 
reports give us a narrow window for action based on specific emission 
reduction targets."

A2: "Yet hasty solutions that don't account for human behavior and economic 
realities often fail. We need both urgency AND wisdom in our approach."
```

### Phase 3: Conclusion

**Purpose**: Summarize key points and provide closure

**Duration**: 2 messages (1 from each agent)

**AI Temperature**: 0.9 (creative synthesis)

**Characteristics**:
- Summarize main arguments
- Acknowledge valid opposing points
- Restate final position
- Reflect on discussion quality
- 2-3 sentences
- No forced consensus

**Example:**
```
A1: "In conclusion, while I appreciate the holistic perspective, the scientific 
data demands immediate action. We must prioritize evidence-based policy changes 
to meet our climate targets before it's too late."

A2: "To summarize, I recognize the urgency that the data presents, but sustainable 
change requires understanding and working with human systems. The most effective 
path forward combines scientific rigor with social wisdom."
```

### Configuration

**Setting Conversation Length:**

**In UI:**
- Dropdown selector: 1-10 exchanges
- Shows total message count
- Default: 3 exchanges (10 total messages)

**Via API:**
```json
POST /api/conversation/init
{
  "conversationLength": 5  // 1-10 exchanges
}
```

**Message Count Formula:**
```
Total Messages = 4 (intro + conclusion) + (conversationLength × 2)

Examples:
- Length 1: 4 + 2 = 6 messages
- Length 3: 4 + 6 = 10 messages (default)
- Length 5: 4 + 10 = 14 messages
- Length 10: 4 + 20 = 24 messages
```

### Phase Transitions

**Automatic phase detection:**

```csharp
string DeterminePhase(int messageCount, int totalExpected)
{
    if (messageCount <= 2)
        return "Introduction";
    else if (messageCount >= totalExpected - 1)
        return "Conclusion";
    else
        return "Conversation";
}
```

### Visual Indicators

**Phase badges in UI:**
- Introduction: Light blue badge
- Conversation: Light purple badge
- Conclusion: Light green badge

**Progress tracking:**
- Shows current phase
- Progress bar (optional)
- Message count display

### Benefits

1. **Natural Flow**: Mimics real human conversations
2. **Better AI Quality**: Phase-specific prompts keep AI focused
3. **Configurable Depth**: Adjust based on topic complexity
4. **Clear Structure**: Users understand conversation progress
5. **Genuine Debate**: Avoids premature agreement or conclusions

---

## 💻 Development

### Code Quality Guidelines

#### C# Style Guidelines

- Use PascalCase for public members
- Use camelCase for private fields (prefix with `_`)
- Use meaningful variable names
- Add XML comments for public APIs
- Follow async/await patterns
- Use dependency injection

**Example:**
```csharp
public class OpenAIService : IOpenAIService
{
    private readonly ILogger<OpenAIService> _logger;
    private readonly IConfiguration _configuration;
    
    /// <summary>
    /// Generates AI response for conversation
    /// </summary>
    public async Task<string> GenerateResponseAsync(
        string personality, 
        string topic, 
        string history)
    {
        // Implementation
    }
}
```

#### JavaScript Style Guidelines

- Use ES6+ features (const, let, arrow functions)
- Use async/await for asynchronous operations
- Use descriptive function names
- Add comments for complex logic
- Handle errors properly

**Example:**
```javascript
async function startConversation() {
    try {
        const response = await fetch('/api/conversation/init', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ /* data */ })
        });
        
        const data = await response.json();
        // Handle response
    } catch (error) {
        displayError('Failed to start conversation');
    }
}
```

#### Database Guidelines

- Use migrations for schema changes
- Always use parameterized queries (EF Core does this)
- Add appropriate indexes for performance
- Use foreign keys for relationships
- Include audit fields (timestamps)

### Development Workflow

**Branch Strategy:**
- `main` - Production-ready code
- `feature/*` - New features
- `fix/*` - Bug fixes
- `docs/*` - Documentation updates
- `refactor/*` - Code refactoring

**Creating a Feature:**

```bash
git checkout main
git pull origin main
git checkout -b feature/your-feature-name

# Make changes
git add .
git commit -m "feat: Add new feature"
git push origin feature/your-feature-name

# Create pull request on GitHub
```

**Commit Message Format:**

Follow conventional commits:

```
<type>(<scope>): <subject>

<body>

<footer>
```

**Types:**
- `feat`: New feature
- `fix`: Bug fix
- `docs`: Documentation
- `style`: Code style (no logic change)
- `refactor`: Code refactoring
- `test`: Tests
- `chore`: Maintenance

**Examples:**
```
feat(api): Add conversation export endpoint
fix(ui): Correct message bubble alignment
docs(readme): Update installation instructions
refactor(service): Simplify prompt generation logic
```

### Debugging Tips

**Enable Detailed Logging:**

```csharp
// Program.cs
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Debug);
```

**Check LocalDB Status:**
```bash
sqllocaldb info mssqllocaldb
sqllocaldb start mssqllocaldb
```

**View Database:**
```bash
# Using SQL Server Management Studio
# Server: (localdb)\mssqllocaldb
# Database: AIAgentConversation

# Or use Visual Studio SQL Server Object Explorer
```

**Clear Database:**
```bash
dotnet ef database drop --project AIAgentConversation
dotnet ef database update --project AIAgentConversation
```

**Common Debug Points:**
1. Check `conversationId` is set after init
2. Verify `isOngoing` flag transitions
3. Confirm bubble alignment (left/right)
4. Validate markdown displays after completion
5. Check OpenAI API key is configured
6. Verify database connection string

---

## 🧪 Testing

### Manual Testing Workflow

Before submitting changes, verify:

**API Testing:**
- [ ] All endpoints return correct status codes
- [ ] Error responses include helpful messages
- [ ] Input validation works correctly
- [ ] Database operations complete successfully
- [ ] Phase transitions work properly
- [ ] Message count calculations correct

**UI Testing:**
- [ ] Form inputs accept and validate data
- [ ] Message bubbles display correctly (A1 left, A2 right)
- [ ] Phase indicators show accurate phase
- [ ] Export buttons generate correct output
- [ ] Error messages display to user
- [ ] Waiting indicators appear/disappear correctly
- [ ] Markdown displays after completion

**Conversation Flow:**
- [ ] Introduction phase messages (2 total)
- [ ] Conversation phase shows debate
- [ ] Conclusion phase summarizes discussion
- [ ] Message count matches configuration
- [ ] Agent alternation is correct (A1→A2→A1→A2)
- [ ] Markdown output properly formatted
- [ ] Export formats work correctly

**Cross-Browser Testing:**
- [ ] Chrome (latest)
- [ ] Firefox (latest)
- [ ] Edge (latest)
- [ ] Safari (if available)

### Database Verification

**Check conversation storage:**

```sql
-- View all conversations
SELECT Id, Agent1Personality, Agent2Personality, Topic, 
       ConversationLength, Status, StartTime, EndTime
FROM Conversations
ORDER BY StartTime DESC;

-- View messages for a conversation
SELECT AgentType, Phase, IterationNumber, Content, Timestamp
FROM Messages
WHERE ConversationId = 'your-guid-here'
ORDER BY Timestamp;

-- Verify message counts
SELECT ConversationId, COUNT(*) as MessageCount, 
       MAX(Phase) as LastPhase
FROM Messages
GROUP BY ConversationId;
```

### Test Scenarios

**Scenario 1: Basic Conversation**
- Agent 1: "Logical analyst"
- Agent 2: "Creative thinker"
- Topic: "Future of AI"
- Length: 3 exchanges
- Politeness: Medium
- Expected: 10 messages, phases correct, completion successful

**Scenario 2: Short Conversation**
- Length: 1 exchange
- Expected: 6 messages total (2 intro + 2 conversation + 2 conclusion)

**Scenario 3: Long Conversation**
- Length: 10 exchanges
- Expected: 24 messages total

**Scenario 4: Edge Cases**
- Empty personality fields → Validation error
- Invalid API key → Error message
- Very long topic (>1000 chars) → Validation error

### Verification Checklist

Run through before considering work complete:

```
□ Conversation entity stores personalities/topic as strings
□ Agent alternation: A1, A2, A1, A2, A1, A2, ...
□ A1 messages on left (blue)
□ A2 messages on right (green)
□ Static "..." waiting indicator (no animation)
□ Phase badges display correctly
□ Markdown displays after all messages
□ Page refresh clears conversation (stateless)
□ Errors display in UI
□ Serilog logs to console
□ OpenAI uses gpt-3.5-turbo
□ Prompt format correct (check logs)
□ History concatenated correctly
□ Export functionality works
□ Politeness control affects tone
□ Temperature progression applied
```

---

## 🐛 Troubleshooting

### Common Issues and Solutions

#### 1. OpenAI API Errors

**Error: "Unauthorized" or "Invalid API Key"**

**Solutions:**
- Verify API key is correct (no extra spaces)
- Check you have credits in OpenAI account
- Ensure key is properly set:
  ```bash
  dotnet user-secrets list --project AIAgentConversation
  ```
- Try setting environment variable as alternative:
  ```bash
  export OpenAI__ApiKey="your-key-here"
  ```

**Error: "Rate limit exceeded"**

**Solutions:**
- Wait a few minutes and try again
- Check OpenAI dashboard for rate limits
- Reduce conversation length to use fewer API calls

**Error: "Timeout"**

**Solutions:**
- Check internet connection
- Verify OpenAI API status: https://status.openai.com/
- Increase timeout in `OpenAIService.cs`

#### 2. Database Issues

**Error: "Cannot open database"**

**Solutions:**
- Ensure LocalDB is running:
  ```bash
  sqllocaldb info mssqllocaldb
  sqllocaldb start mssqllocaldb
  ```
- Check connection string in `appsettings.json`
- Run migrations:
  ```bash
  dotnet ef database update --project AIAgentConversation
  ```

**Error: "Invalid column name"**

**Solutions:**
- Database schema out of sync with code
- Drop and recreate database:
  ```bash
  dotnet ef database drop --project AIAgentConversation
  dotnet ef database update --project AIAgentConversation
  ```

**Error: "SQLite Error"**

**Solutions:**
- Delete `AIConversations.db` file
- Restart application (will recreate database)

#### 3. UI Issues

**Problem: Messages not displaying**

**Solutions:**
- Check browser console for JavaScript errors (F12)
- Verify API endpoints are responding:
  ```bash
  curl -X POST https://localhost:5001/api/conversation/init \
    -H "Content-Type: application/json" \
    -d '{"agent1Personality":"test","agent2Personality":"test","topic":"test"}'
  ```
- Clear browser cache
- Check conversation container exists in HTML

**Problem: Waiting indicator not disappearing**

**Solutions:**
- Check API response in Network tab (F12)
- Verify `isOngoing` flag transitions correctly
- Check for JavaScript errors in console
- Ensure `continueConversation()` is called recursively

**Problem: Markdown not showing**

**Solutions:**
- Verify conversation completed (all messages generated)
- Check `GET /api/conversation/{id}` endpoint responds
- Ensure `displayMarkdown()` is called when `isOngoing = false`
- Check markdown container display style

**Problem: Export buttons not working**

**Solutions:**
- Verify conversation ID is available
- Check export endpoint is registered in controller
- Test endpoint directly in browser
- Check browser console for errors

#### 4. Build Errors

**Error: "SDK not found"**

**Solutions:**
- Install .NET 8 SDK: https://dotnet.microsoft.com/download/dotnet/8.0
- Verify installation: `dotnet --version`
- Restart terminal/IDE after installation

**Error: "Package restore failed"**

**Solutions:**
- Check internet connection
- Clear NuGet cache:
  ```bash
  dotnet nuget locals all --clear
  ```
- Restore manually:
  ```bash
  dotnet restore
  ```

**Error: "Entity Framework tools not found"**

**Solutions:**
- Install EF Core tools:
  ```bash
  dotnet tool install --global dotnet-ef
  ```
- Verify installation:
  ```bash
  dotnet ef --version
  ```

### Getting Help

If issues persist:

1. Check Visual Studio Output window for detailed errors
2. Review Serilog console output for application logs
3. Enable debug logging (set minimum level to Debug)
4. Check GitHub Issues for similar problems
5. Create new issue with:
   - Error message
   - Steps to reproduce
   - Environment details (.NET version, OS, etc.)
   - Relevant log excerpts

---

## 🤝 Contributing

Thank you for your interest in contributing!

### Code of Conduct

**Our Standards:**
- Be respectful and professional
- Provide constructive feedback
- Work collaboratively
- Be patient and understanding

### Getting Started

**Prerequisites:**
- .NET 8 SDK installed
- Visual Studio 2022 or VS Code
- Git for version control
- OpenAI API key for testing
- Familiarity with C#, ASP.NET Core, Entity Framework Core

**Setting Up:**

1. Fork the repository on GitHub
2. Clone your fork:
   ```bash
   git clone https://github.com/YOUR_USERNAME/AIAgentConversation.git
   cd AIAgentConversation
   ```
3. Set up OpenAI API key:
   ```bash
   dotnet user-secrets init --project AIAgentConversation
   dotnet user-secrets set "OpenAI:ApiKey" "your-key" --project AIAgentConversation
   ```
4. Build and run:
   ```bash
   dotnet build
   dotnet run --project AIAgentConversation
   ```
5. Verify setup by starting a test conversation

### Pull Request Process

1. **Create feature branch:**
   ```bash
   git checkout -b feature/your-feature-name
   ```

2. **Make changes:**
   - Follow coding standards
   - Add/update tests
   - Update documentation
   - Commit with meaningful messages

3. **Test thoroughly:**
   - Run all manual tests
   - Verify no regressions
   - Test edge cases

4. **Push and create PR:**
   ```bash
   git push origin feature/your-feature-name
   ```
   - Go to GitHub and create Pull Request
   - Fill out PR template
   - Link related issues

5. **PR Title Format:**
   ```
   feat: Add conversation export functionality
   fix: Correct message ordering in Phase 2
   docs: Update API documentation
   ```

6. **PR Description Template:**
   ```markdown
   ## Description
   Brief description of changes
   
   ## Changes Made
   - Added export endpoint for JSON format
   - Updated UI with export buttons
   - Added unit tests for export service
   
   ## Testing Done
   - Manual testing of all export formats
   - Verified existing functionality unchanged
   - Tested edge cases
   
   ## Related Issues
   Closes #42
   ```

### Documentation Updates

**When to Update:**
- Adding new features
- Changing existing functionality
- Fixing bugs users should know about
- Adding/modifying API endpoints
- Changing configuration options

**Documentation Files (NOW UNIFIED):**
- **README.md**: Complete unified documentation (this file)

### Code Review Checklist

Before submitting PR:

**Code Quality:**
- [ ] Follows C# coding standards
- [ ] Includes XML comments for public APIs
- [ ] No hardcoded values (use configuration)
- [ ] Error handling implemented
- [ ] Logging added appropriately

**Testing:**
- [ ] Manual testing completed
- [ ] Edge cases tested
- [ ] No regressions in existing features
- [ ] Database changes tested

**Documentation:**
- [ ] README.md updated if needed
- [ ] Code comments added for complex logic
- [ ] API changes documented

**Git:**
- [ ] Meaningful commit messages
- [ ] Commits logically organized
- [ ] No unnecessary files included
- [ ] Branch up to date with main

### Recognition

Contributors will be acknowledged in:
- GitHub Contributors page
- Release notes
- Project documentation

---

## 📜 Changelog

All notable changes to the AI Agent Conversation Platform.

### [1.4.0] - 2025-01-02

#### Added - Conversation Phases
- Three-phase conversation structure (Introduction → Conversation → Conclusion)
- Configurable conversation length (1-10 exchanges, default 3)
- Phase-specific AI prompts encouraging genuine debate
- Phase badges with color-coded visual indicators
- Progress tracking showing current phase

#### Added - AI Quality Improvements
- Genuine debate flow with disagreement and counterarguments
- Progressive temperature (0.5 → 0.7 → 0.9)
- Politeness control (5-level system)
- Smart AI responses challenging ideas constructively
- Enhanced prompt engineering

#### Added - Export Functionality
- Multiple export formats: JSON, Markdown, Plain Text, XML
- Conversation metadata in exports
- Phase information preservation
- Download buttons for easy access

#### Enhanced - User Interface
- Phase indicators with color-coded badges
- Conversation length selector (dropdown)
- Politeness slider control
- Export buttons below completed conversations
- Improved message display and spacing

#### Enhanced - Backend
- Configurable iterations (changed from fixed 3 to 1-10)
- Phase-aware message generation
- Temperature progression implementation
- Enhanced logging for AI interactions

#### Changed
- Message sequence now based on configurable exchange count (4-24 total)
- Agent alternation scales with exchange count
- Conversation completion logic updated for variable counts

### [1.0.0] - Initial Release

#### Added - Core Features
- Dual AI agents with configurable personalities
- OpenAI GPT-3.5-turbo integration
- Text-based personality inputs
- Custom topic entry
- Real-time SMS-style bubble interface
- Database persistence (SQL Server LocalDB)
- Stateless design

#### Added - API
- POST /api/conversation/init
- POST /api/conversation/follow
- GET /api/conversation/{id}

#### Added - Technical
- .NET 8 / ASP.NET Core
- Entity Framework Core
- Razor Pages single-page app
- Serilog console logging
- Simplified string-based schema

#### Design Decisions (v1.0)
- Fixed 3 iterations (initially) - made configurable in v1.4
- No user authentication (focused on core functionality)
- No rate limiting (v1.0 simplification)
- Console logging only (no file persistence)
- Stateless design (no session management)
- LocalDB only (not production-ready)

### Migration Guide

#### Upgrading from v1.0 to v1.4

**Database Changes:**
- New fields: `ConversationLength`, `PolitenessLevel`, `Phase` (in Message)
- Run migrations: `dotnet ef database update`
- Existing v1.0 conversations remain compatible

**API Changes:**
- `init` endpoint accepts `conversationLength` and `politenessLevel` (optional)
- Response includes `phase` field
- New export endpoint added

**UI Changes:**
- New conversation length selector
- New politeness control slider
- Phase badges in message display
- Export buttons after completion

**Configuration Changes:**
- OpenAI configuration in `appsettings.json`:
  - `MaxTokens`: 500 (configurable)
  - Model remains `gpt-3.5-turbo`

**Breaking Changes:**
- None - v1.0 conversations display correctly with inferred phase info

### Roadmap

#### Planned Features (Future Versions)

**v1.5 (Q2 2025):**
- User authentication and accounts
- Conversation history page
- Save and resume conversations
- Share conversations via link

**v2.0 (Q3 2025):**
- Real-time streaming with SignalR
- Multiple AI model support (GPT-4, Claude, etc.)
- Custom AI personas library
- Advanced analytics dashboard

**v2.5 (Q4 2025):**
- Multi-agent conversations (3+ agents)
- Voice input/output
- Conversation templates
- API rate limiting

**Future Considerations:**
- Mobile app (iOS/Android)
- Browser extension
- Slack/Teams integration
- PDF export with formatting
- Conversation analytics
- Sentiment analysis
- Topic suggestions based on trending themes

---

## 📄 License

MIT License

Copyright (c) 2025 AI Agent Conversation Platform

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

---

## 🙏 Acknowledgments

- **OpenAI** for GPT-3.5-turbo API
- **Microsoft** for .NET 8 and Entity Framework Core
- **GitHub Copilot** for development assistance
- **Contributors** who helped improve this project

---

## 📞 Support

### Getting Help

**Documentation:**
- This comprehensive README contains all documentation
- Check relevant sections above for specific topics

**Issues:**
- Check [GitHub Issues](https://github.com/McWut-src/AIAgentConversation/issues)
- Search for existing issues before creating new ones
- Provide detailed error messages and steps to reproduce

**Community:**
- Star the repository if you find it useful
- Share feedback and suggestions
- Contribute improvements via pull requests

### Quick Links

- **Repository**: https://github.com/McWut-src/AIAgentConversation
- **OpenAI API**: https://platform.openai.com/
- **.NET 8 SDK**: https://dotnet.microsoft.com/download/dotnet/8.0
- **License**: MIT (see above)

---

**Built with ❤️ using .NET 8, OpenAI GPT-3.5-turbo, and GitHub Copilot**

*Last Updated: January 2025*  
*Version: 1.4.0*
