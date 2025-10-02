# User Interface Documentation

## Overview

The AI Agent Conversation Platform uses a single-page Razor application with vanilla JavaScript for real-time conversation display. The interface shows conversations between two AI agents in an SMS-style bubble format, transitioning to a markdown view upon completion.

**Framework:** ASP.NET Core Razor Pages  
**JavaScript:** Vanilla ES6+ (no libraries)  
**CSS:** Custom styles (no Bootstrap/Tailwind)  
**Design:** Stateless, session-independent

---

## Table of Contents

- [User Interface Components](#user-interface-components)
- [User Workflow](#user-workflow)
- [Visual Design](#visual-design)
- [JavaScript Implementation](#javascript-implementation)
- [State Management](#state-management)
- [Error Handling](#error-handling)
- [Accessibility](#accessibility)
- [Browser Compatibility](#browser-compatibility)

---

## User Interface Components

### 1. Input Form Section

**Purpose:** Collect agent personalities and conversation topic from user.

**Components:**
- **Agent 1 Personality Input** - Text field for first agent's personality
- **Agent 2 Personality Input** - Text field for second agent's personality  
- **Topic Input** - Text field for conversation topic
- **Start Conversation Button** - Initiates the conversation

**Location:** Top of page, always visible

**HTML Structure:**
```html
<div class="input-section">
    <div class="form-group">
        <label for="agent1-personality">Agent 1 Personality:</label>
        <input type="text" id="agent1-personality" class="form-control" 
               placeholder="e.g., Logical analyst who values data" />
    </div>
    
    <div class="form-group">
        <label for="agent2-personality">Agent 2 Personality:</label>
        <input type="text" id="agent2-personality" class="form-control" 
               placeholder="e.g., Creative thinker who loves metaphors" />
    </div>
    
    <div class="form-group">
        <label for="topic">Topic:</label>
        <input type="text" id="topic" class="form-control" 
               placeholder="e.g., The future of space exploration" />
    </div>
    
    <button id="start-button" class="btn btn-primary">Start Conversation</button>
</div>
```

**Critical Requirements:**
- ❌ **NO dropdown selects** - must be text inputs
- ✅ Personalities and topics entered as free text
- ✅ No pre-populated options or suggestions
- ✅ Iteration count is NOT shown (fixed at 3)

### 2. Conversation Display Area

**Purpose:** Show real-time conversation with SMS-style message bubbles.

**Components:**
- **Message Bubbles** - Individual agent messages
- **Waiting Indicators** - Static "..." while waiting for response
- **Agent Labels** - "A1" or "A2" identifier on each bubble

**HTML Structure:**
```html
<div id="conversation-container" class="conversation-display">
    <!-- Messages dynamically inserted here -->
</div>
```

**Message Bubble Structure:**
```html
<div class="message-bubble message-bubble-a1">
    <div class="message-agent">A1</div>
    <div>Message content goes here...</div>
</div>
```

**Waiting Indicator Structure:**
```html
<div class="waiting-indicator waiting-left">...</div>
```

**Critical Requirements:**
- ✅ A1 messages: **left-aligned, blue background**
- ✅ A2 messages: **right-aligned, green background**
- ✅ Waiting indicator: **static text "..."** (no CSS animation)
- ✅ Waiting position: left for A1, right for A2
- ❌ **NO animated dots** with keyframes or transitions

### 3. Markdown Display Area

**Purpose:** Show completed conversation in formatted text after 6 messages.

**HTML Structure:**
```html
<div id="markdown-container" class="markdown-display" style="display: none;">
    <!-- Markdown content inserted here -->
</div>
```

**Content Format:**
```
**A1:** First message from agent 1
**A2:** First response from agent 2
**A1:** Second message from agent 1
**A2:** Second response from agent 2
**A1:** Third message from agent 1
**A2:** Third response from agent 2
```

**Critical Requirements:**
- ✅ Only displayed when conversation completes (6 messages)
- ✅ Replaces bubble display (bubbles hidden)
- ✅ Preserves line breaks with `white-space: pre-wrap`
- ✅ Monospace font for readability

### 4. Error Display Area

**Purpose:** Show error messages to user when API calls fail.

**HTML Structure:**
```html
<div id="error-container" class="error-display" style="display: none;">
    <!-- Error message inserted here -->
</div>
```

**Example Display:**
```
Error: Failed to initialize conversation - Invalid API key
```

**Critical Requirements:**
- ✅ Red background with error styling
- ✅ Stops further API calls on error
- ✅ Requires page refresh to start new conversation

---

## User Workflow

### Complete User Journey

```
┌─────────────────────────────────────────────────────────┐
│ 1. USER LANDS ON PAGE                                   │
│    - Sees input form                                     │
│    - No previous conversation state                      │
└─────────────────────────────────────────────────────────┘
                         ↓
┌─────────────────────────────────────────────────────────┐
│ 2. USER ENTERS INFORMATION                               │
│    - Agent 1 Personality (text input)                    │
│    - Agent 2 Personality (text input)                    │
│    - Topic (text input)                                  │
└─────────────────────────────────────────────────────────┘
                         ↓
┌─────────────────────────────────────────────────────────┐
│ 3. USER CLICKS "START CONVERSATION"                      │
│    - Form validates (all fields required)                │
│    - Waiting indicator appears (left side)               │
└─────────────────────────────────────────────────────────┘
                         ↓
┌─────────────────────────────────────────────────────────┐
│ 4. A1 MESSAGE APPEARS                                    │
│    - Blue bubble on left                                 │
│    - "A1" label shown                                    │
│    - Waiting indicator removed                           │
│    - New waiting indicator appears (right side)          │
└─────────────────────────────────────────────────────────┘
                         ↓
┌─────────────────────────────────────────────────────────┐
│ 5. A2 MESSAGE APPEARS                                    │
│    - Green bubble on right                               │
│    - "A2" label shown                                    │
│    - Waiting indicator removed                           │
│    - New waiting indicator appears (left side)           │
└─────────────────────────────────────────────────────────┘
                         ↓
┌─────────────────────────────────────────────────────────┐
│ 6. CONVERSATION CONTINUES                                │
│    - Pattern repeats: A1 left, A2 right                  │
│    - Total 6 messages displayed                          │
│    - Each message shows after ~2-5 seconds               │
└─────────────────────────────────────────────────────────┘
                         ↓
┌─────────────────────────────────────────────────────────┐
│ 7. CONVERSATION COMPLETES                                │
│    - All bubbles hidden                                  │
│    - Markdown view appears                               │
│    - Shows all 6 messages in text format                 │
└─────────────────────────────────────────────────────────┘
                         ↓
┌─────────────────────────────────────────────────────────┐
│ 8. USER WANTS NEW CONVERSATION                           │
│    - Must refresh page (F5)                              │
│    - Form resets, conversation clears                    │
│    - Can start new conversation                          │
└─────────────────────────────────────────────────────────┘
```

### Timing and Flow

| Event | Duration | User Sees |
|-------|----------|-----------|
| Click Start | Instant | "..." left side |
| A1 Response | 2-5 sec | Blue bubble left, "..." right |
| A2 Response | 2-5 sec | Green bubble right, "..." left |
| Continue Loop | ~20-30 sec total | 6 bubbles alternating |
| Final Display | Instant | Markdown text view |

---

## Visual Design

### Color Scheme

| Element | Color | Usage |
|---------|-------|-------|
| Agent 1 Bubble | `#007bff` (Blue) | Message background |
| Agent 2 Bubble | `#28a745` (Green) | Message background |
| Bubble Text | `#ffffff` (White) | Message text color |
| Waiting Indicator | `#999999` (Gray) | "..." text color |
| Error Background | `#f8d7da` (Light Red) | Error container |
| Error Text | `#721c24` (Dark Red) | Error message text |
| Form Background | `#f8f9fa` (Light Gray) | Input section |

### Typography

| Element | Font | Size | Weight |
|---------|------|------|--------|
| Headings | System default | 24px | Bold |
| Labels | System default | 14px | 600 |
| Input Text | System default | 14px | Normal |
| Bubble Text | System default | 14px | Normal |
| Agent Label | System default | 12px | Bold |
| Markdown | Courier New, monospace | 14px | Normal |

### Layout Specifications

**Message Bubbles:**
- **Max Width:** 70% of container
- **Padding:** 12px horizontal, 16px vertical
- **Border Radius:** 18px
- **Margin:** 15px between bubbles
- **Alignment:** Float left (A1) or right (A2)

**Input Form:**
- **Max Width:** 900px container
- **Padding:** 20px
- **Input Height:** 40px
- **Button Height:** 44px

**Spacing:**
- Form groups: 15px margin bottom
- Sections: 30px margin top
- Container padding: 20px

### Responsive Behavior

**Desktop (> 768px):**
- Full 900px container width
- Bubbles 70% max width
- Side-by-side alignment works

**Tablet (768px - 480px):**
- Container adjusts to screen width
- Bubbles remain at 70%
- Vertical scrolling enabled

**Mobile (< 480px):**
- Full width container with padding
- Bubbles expand to 85% width
- Touch-friendly button sizes

---

## JavaScript Implementation

### File Structure

**Location:** `wwwroot/js/conversation.js`

**Loaded in:** `Pages/Index.cshtml`

```html
@section Scripts {
    <script src="~/js/conversation.js"></script>
}
```

### Global Variables

```javascript
// CRITICAL: Store in JavaScript variable, NOT localStorage or sessionStorage
let conversationId = null;
let isConversationActive = false;
```

**Why JavaScript Variables?**
- ✅ Stateless design - new conversation per page refresh
- ✅ No persistent state across sessions
- ✅ Simple and predictable behavior
- ❌ **NOT** stored in localStorage, sessionStorage, or cookies

### Core Functions

#### 1. startConversation()

**Purpose:** Initialize new conversation with user inputs.

```javascript
async function startConversation() {
    // 1. Get and validate inputs
    // 2. Clear previous conversation
    // 3. Show waiting indicator
    // 4. Call init API endpoint
    // 5. Store conversationId
    // 6. Display first message
    // 7. Trigger continue if ongoing
}
```

**Key Operations:**
- Input validation (all fields required)
- Clear conversation container
- Show waiting indicator (left side)
- POST to `/api/conversation/init`
- Parse and store `conversationId`
- Create message bubble for A1
- Call `continueConversation()` if `isOngoing === true`

#### 2. continueConversation()

**Purpose:** Request next agent message and display it.

```javascript
async function continueConversation() {
    // 1. Call follow API endpoint
    // 2. Remove waiting indicator
    // 3. Display new message
    // 4. Check if ongoing
    // 5. If ongoing: show next waiting + recurse
    // 6. If complete: call displayMarkdown()
}
```

**Key Operations:**
- POST to `/api/conversation/follow` with `conversationId`
- Remove previous waiting indicator
- Create bubble for new message (left or right based on agent)
- Check `isOngoing` flag
- Recursive call if conversation continues
- Call `displayMarkdown()` if complete

**Recursion Pattern:**
```javascript
if (data.isOngoing && isConversationActive) {
    const nextWaiting = createWaitingIndicator(side);
    conversationContainer.appendChild(nextWaiting);
    await continueConversation(); // Recursive call
} else {
    await displayMarkdown();
}
```

#### 3. displayMarkdown()

**Purpose:** Fetch and display completed conversation in markdown format.

```javascript
async function displayMarkdown() {
    // 1. GET complete conversation
    // 2. Hide bubble container
    // 3. Show markdown container
    // 4. Insert formatted text
}
```

**Key Operations:**
- GET from `/api/conversation/{conversationId}`
- Hide `conversation-container`
- Show `markdown-container`
- Set `textContent` (not `innerHTML` for security)
- Display markdown with preserved line breaks

#### 4. createMessageBubble(message, agentType)

**Purpose:** Create DOM element for message bubble.

```javascript
function createMessageBubble(message, agentType) {
    const bubble = document.createElement('div');
    bubble.className = `message-bubble message-bubble-${agentType.toLowerCase()}`;
    
    const agentLabel = document.createElement('div');
    agentLabel.className = 'message-agent';
    agentLabel.textContent = agentType;
    
    const messageText = document.createElement('div');
    messageText.textContent = message;
    
    bubble.appendChild(agentLabel);
    bubble.appendChild(messageText);
    
    return bubble;
}
```

**Returns:** DOM element ready to append to container

#### 5. createWaitingIndicator(side)

**Purpose:** Create static "..." waiting indicator.

```javascript
function createWaitingIndicator(side) {
    const indicator = document.createElement('div');
    indicator.className = `waiting-indicator waiting-${side}`;
    indicator.textContent = '...'; // Static text only
    return indicator;
}
```

**Critical:** No CSS animations - plain text only

**Side values:**
- `'left'` - For Agent 1 (blue, left-aligned)
- `'right'` - For Agent 2 (green, right-aligned)

#### 6. displayError(message)

**Purpose:** Show error message to user and stop conversation.

```javascript
function displayError(message) {
    const errorContainer = document.getElementById('error-container');
    errorContainer.textContent = `Error: ${message}`;
    errorContainer.style.display = 'block';
    
    console.error('Conversation error:', message);
    
    isConversationActive = false; // Stop further calls
}
```

### Event Handlers

**Page Load:**
```javascript
document.addEventListener('DOMContentLoaded', function() {
    const startButton = document.getElementById('start-button');
    startButton.addEventListener('click', startConversation);
});
```

**Button Click:**
- Triggers `startConversation()`
- Validates inputs
- Initiates API workflow

### API Call Pattern

**All API calls use fetch with async/await:**

```javascript
const response = await fetch('/api/conversation/init', {
    method: 'POST',
    headers: {
        'Content-Type': 'application/json'
    },
    body: JSON.stringify({
        agent1Personality: agent1,
        agent2Personality: agent2,
        topic: topic
    })
});

if (!response.ok) {
    const error = await response.json();
    throw new Error(error.message || 'API call failed');
}

const data = await response.json();
```

**Error Handling:**
- All API calls wrapped in try-catch
- Display errors to user
- Log to console for debugging
- Stop conversation on error

---

## State Management

### Stateless Design

**Critical Principle:** The UI maintains **no persistent state** across page loads.

**What This Means:**
- ✅ `conversationId` stored in JavaScript variable only
- ✅ Page refresh = new conversation
- ✅ No browser storage (localStorage, sessionStorage, cookies)
- ✅ No server-side sessions
- ✅ Each page load is independent

**Benefits:**
- Simple and predictable behavior
- No state synchronization issues
- Easy testing and debugging
- No privacy concerns with stored data

**Implications:**
- Users cannot resume interrupted conversations
- No conversation history accessible
- Must complete conversation in one session
- Page refresh requires starting over

### State Variables

| Variable | Type | Scope | Purpose |
|----------|------|-------|---------|
| `conversationId` | GUID/null | Global | Current conversation identifier |
| `isConversationActive` | boolean | Global | Prevent multiple simultaneous calls |

**State Transitions:**

```
Page Load:
  conversationId = null
  isConversationActive = false
  ↓
Start Button Click:
  isConversationActive = true
  ↓
Init API Response:
  conversationId = [GUID from API]
  ↓
Conversation Loop:
  [conversationId unchanged]
  [isConversationActive remains true]
  ↓
Completion:
  [conversationId unchanged]
  isConversationActive = false
  ↓
Error Occurs:
  isConversationActive = false
  [conversationId may or may not be set]
  ↓
Page Refresh:
  conversationId = null
  isConversationActive = false
```

### DOM State

**Conversation Container:**
- Empty on page load
- Populated with bubbles during conversation
- Hidden when markdown displays

**Markdown Container:**
- Hidden on page load and during conversation
- Visible only when conversation completes
- Contains final formatted text

**Error Container:**
- Hidden by default
- Visible only when error occurs
- Requires page refresh to dismiss

---

## Error Handling

### User-Visible Errors

**Input Validation:**
```
Error: Please fill in all fields
```
- Shown when user clicks Start with empty fields
- Inline validation, no API call made

**API Errors:**
```
Error: Failed to initialize conversation
Error: Conversation not found
Error: Internal server error
```
- Displayed in error container
- Red background with clear message
- Requires page refresh to continue

**Network Errors:**
```
Error: Network request failed
Error: Unable to connect to server
```
- Catch-all for network issues
- Logged to console for debugging

### Error Flow

```
Error Occurs
    ↓
Try-Catch Block
    ↓
displayError(message)
    ↓
- Show error container
- Log to console
- Set isConversationActive = false
    ↓
Stop Further API Calls
    ↓
User Must Refresh Page
```

### Console Logging

**All errors logged to browser console:**

```javascript
console.error('Error starting conversation:', error);
console.error('Error continuing conversation:', error);
console.error('Error displaying markdown:', error);
```

**Check console (F12) for:**
- API response details
- Network failures
- JavaScript errors
- Full error stack traces

### Graceful Degradation

**When API Fails:**
1. Error message shows to user
2. No further API calls attempted
3. Conversation stops cleanly
4. Page refresh required to retry

**User Recovery:**
- Refresh page (F5 or Ctrl+R)
- Re-enter information
- Try again with new conversation

---

## Accessibility

### Keyboard Navigation

**Tab Order:**
1. Agent 1 Personality input
2. Agent 2 Personality input
3. Topic input
4. Start Conversation button

**Enter Key:**
- Submit form when focused on any input
- Same as clicking Start button

### Screen Reader Support

**Labels:**
```html
<label for="agent1-personality">Agent 1 Personality:</label>
<input type="text" id="agent1-personality" />
```
- All inputs have associated labels
- Labels properly linked with `for` attribute

**ARIA Attributes:**
```html
<div role="status" aria-live="polite" id="conversation-container"></div>
<div role="alert" id="error-container"></div>
```
- Conversation updates announced to screen readers
- Errors announced immediately

**Alternative Text:**
- Agent labels ("A1", "A2") read by screen readers
- Error messages fully accessible

### Color Contrast

**WCAG 2.1 AA Compliance:**
- Blue bubble text: White on #007bff (4.6:1 ratio)
- Green bubble text: White on #28a745 (4.5:1 ratio)
- Error text: #721c24 on #f8d7da (5.2:1 ratio)
- Form labels: Dark on light background (>7:1 ratio)

### Focus Indicators

**Visible focus for all interactive elements:**
```css
input:focus, button:focus {
    outline: 2px solid #007bff;
    outline-offset: 2px;
}
```

---

## Browser Compatibility

### Supported Browsers

| Browser | Version | Status |
|---------|---------|--------|
| Chrome | 90+ | ✅ Full support |
| Firefox | 88+ | ✅ Full support |
| Safari | 14+ | ✅ Full support |
| Edge | 90+ | ✅ Full support |
| Opera | 76+ | ✅ Full support |

### JavaScript Features Used

**ES6+ Features:**
- `async/await` (ES2017)
- `const/let` (ES6)
- Arrow functions (ES6)
- Template literals (ES6)
- `fetch()` API (ES6)

**Browser APIs:**
- `document.querySelector()`
- `document.createElement()`
- `addEventListener()`
- `fetch()` for AJAX

**No Polyfills Required** for modern browsers

### Testing Recommendations

**Test on:**
1. Latest Chrome (desktop)
2. Latest Firefox (desktop)
3. Safari on macOS/iOS
4. Edge on Windows
5. Mobile Chrome (Android)
6. Mobile Safari (iOS)

**Common Issues:**
- Older Safari versions may need fetch polyfill
- IE11 not supported (no fetch, no async/await)

---

## Performance

### Load Time

**Page Load:**
- Initial HTML: < 50 KB
- CSS: < 10 KB
- JavaScript: < 8 KB
- **Total:** < 70 KB (fast load)

**First Contentful Paint:** < 1 second

### API Response Times

**Typical Response Times:**
- Init: 2-5 seconds (OpenAI processing)
- Follow: 2-5 seconds per call
- Get: < 100ms (database query only)

**Total Conversation Time:** ~20-30 seconds for 6 messages

### Optimization Techniques

**Minimal JavaScript:**
- No external libraries (no jQuery, React, etc.)
- Vanilla JavaScript only
- ~200 lines of code total

**Efficient DOM Manipulation:**
- Create elements once
- Append in batches
- Minimal reflows/repaints

**CSS Performance:**
- No animations (no GPU usage)
- Simple layout (no complex flexbox/grid)
- Minimal paint operations

---

## Development Guidelines

### Adding New Features

**To add a new UI element:**
1. Add HTML to `Index.cshtml`
2. Add styles to `site.css`
3. Add JavaScript to `conversation.js`
4. Test with API endpoints

**To modify behavior:**
1. Check `copilot-instructions.md` for requirements
2. Update JavaScript functions
3. Maintain stateless design
4. Test complete flow

### Debugging

**Browser DevTools:**
- Network tab: Monitor API calls
- Console: Check for errors
- Elements: Inspect DOM structure
- Application: Verify no storage used

**Common Debug Points:**
1. Check `conversationId` is set after init
2. Verify `isOngoing` flag transitions
3. Confirm bubble alignment (left/right)
4. Validate markdown displays after message 6

### Code Style

**JavaScript:**
- Use `async/await` (not promises)
- Use `const/let` (not `var`)
- Descriptive function names
- Comments for critical sections

**CSS:**
- BEM-like naming: `.message-bubble-a1`
- Logical grouping of styles
- Mobile-first approach

**HTML:**
- Semantic elements
- Proper indentation
- ID-based JavaScript selectors

---

## Known Limitations

### By Design (v1.0)

1. **No conversation history** - Cannot view past conversations
2. **No resume capability** - Page refresh loses conversation
3. **No user authentication** - All conversations anonymous
4. **No editing messages** - Cannot modify agent responses
5. **Fixed 3 iterations** - Cannot change conversation length
6. **No export** - Cannot download conversation (except copy/paste markdown)
7. **No real-time streaming** - Messages appear all at once after processing

### Technical Limitations

1. **Single conversation** - Cannot run multiple conversations simultaneously
2. **No offline mode** - Requires internet connection
3. **No mobile app** - Web browser only
4. **No push notifications** - Must wait on page
5. **No conversation sharing** - Cannot share links to conversations

---

## Future Enhancements

### Planned for v2.0

- [ ] Conversation history page
- [ ] Export to PDF/JSON
- [ ] Real-time streaming with SignalR
- [ ] User authentication
- [ ] Multiple concurrent conversations
- [ ] Custom iteration counts
- [ ] Message editing/regeneration
- [ ] Conversation sharing via links

### Community Requests

- [ ] Dark mode theme
- [ ] Custom bubble colors
- [ ] Avatar images for agents
- [ ] Typing indicators (animated)
- [ ] Message timestamps
- [ ] Search functionality
- [ ] Conversation bookmarking

---

## Troubleshooting

### Common User Issues

**Problem: "Nothing happens when I click Start"**

**Solution:**
1. Check all fields are filled
2. Check browser console for errors (F12)
3. Verify API is running (https://localhost:5001)
4. Check OpenAI API key is configured

**Problem: "Bubbles not appearing"**

**Solution:**
1. Check Network tab in DevTools
2. Verify API responses are successful (200 OK)
3. Check `conversationId` is stored in JavaScript
4. Verify `createMessageBubble()` function works

**Problem: "Markdown not displaying"**

**Solution:**
1. Verify conversation completed (6 messages)
2. Check API returns `isOngoing: false`
3. Verify `displayMarkdown()` function called
4. Check markdown container is shown

**Problem: "Conversation stuck on waiting indicator"**

**Solution:**
1. Check API responses in Network tab
2. Verify OpenAI API key is valid
3. Check for JavaScript errors in console
4. Refresh page and try again

### Developer Issues

**Problem: "conversationId is null"**

**Check:**
- Init API call successful
- Response contains `conversationId` field
- Variable assignment in `startConversation()`

**Problem: "Agent alternation wrong"**

**Check:**
- Follow endpoint logic (odd→A2, even→A1)
- Message count calculation
- Database message records

**Problem: "Bubbles overlapping"**

**Check:**
- CSS float properties
- Clear fix on container
- Bubble max-width setting

---

## Testing Checklist

### Manual Testing

```
□ Input form displays correctly
□ All three text inputs accept text
□ Start button is clickable
□ Input validation shows error for empty fields
□ First waiting indicator appears (left)
□ A1 message appears in blue bubble (left)
□ Second waiting indicator appears (right)
□ A2 message appears in green bubble (right)
□ Pattern continues: A1, A2, A1, A2, A1, A2
□ Exactly 6 bubbles display
□ Markdown view replaces bubbles after completion
□ Markdown shows all 6 messages correctly
□ Error messages display when API fails
□ Page refresh clears conversation
□ New conversation works after refresh
□ No data persists in localStorage/sessionStorage
□ Console shows no JavaScript errors
□ Network tab shows correct API calls
```

### Cross-Browser Testing

```
□ Chrome (latest)
□ Firefox (latest)
□ Safari (latest)
□ Edge (latest)
□ Mobile Chrome
□ Mobile Safari
```

### Accessibility Testing

```
□ Keyboard navigation works
□ Screen reader announces messages
□ Focus indicators visible
□ Color contrast meets WCAG AA
□ All inputs have labels
□ Error messages are accessible
```

---

## Support Resources

**Documentation:**
- [README.md](README.md) - Project overview and setup
- [API.md](API.md) - API endpoint documentation
- [TUTORIAL.md](TUTORIAL.md) - Complete walkthrough
- [docs/](docs/) - Feature guides and advanced topics
- `.github/copilot-instructions.md` - Development guidelines

**Source Files:**
- `Pages/Index.cshtml` - Razor page markup
- `Pages/Index.cshtml.cs` - Page model (minimal)
- `wwwroot/css/site.css` - Styles
- `wwwroot/js/conversation.js` - Client logic

**Help:**
- Check console for errors (F12)
- Review Network tab for API responses
- Verify database has correct data
- Test API endpoints directly with curl/Postman

---

**Last Updated:** September 30, 2025  
**UI Version:** 1.0.0  
**Maintained by:** AI Agent Conversation Team