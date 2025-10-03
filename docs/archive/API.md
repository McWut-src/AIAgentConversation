# API Documentation

## Overview

The AI Agent Conversation Platform provides a RESTful API for managing conversations between two AI agents powered by OpenAI's GPT-3.5-turbo model. The API follows strict workflow requirements to ensure proper conversation flow and data integrity.

**Base URL:** `https://localhost:5001/api`  
**Content-Type:** `application/json`  
**API Version:** 1.0.0

---

## Table of Contents

- [Authentication](#authentication)
- [Endpoints](#endpoints)
  - [Initialize Conversation](#1-initialize-conversation)
  - [Continue Conversation](#2-continue-conversation)
  - [Get Complete Conversation](#3-get-complete-conversation)
- [Data Models](#data-models)
- [Error Handling](#error-handling)
- [Workflow Requirements](#workflow-requirements)
- [Examples](#examples)

---

## Authentication

**Current Version:** No authentication required (v1.0)

This is a development version intended for local use. Production deployments should implement proper authentication mechanisms.

---

## Endpoints

### 1. Initialize Conversation

Creates a new conversation between two AI agents and returns the first message from Agent 1.

**Endpoint:** `POST /api/conversation/init`

#### Request

**Headers:**
```
Content-Type: application/json
```

**Body:**
```json
{
  "agent1Personality": "string",
  "agent2Personality": "string",
  "topic": "string"
}
```

**Parameters:**

| Field | Type | Required | Max Length | Description |
|-------|------|----------|------------|-------------|
| `agent1Personality` | string | Yes | 500 | Personality description for Agent 1 |
| `agent2Personality` | string | Yes | 500 | Personality description for Agent 2 |
| `topic` | string | Yes | 1000 | Conversation topic |

**Note:** Iteration count is **fixed at 3** and cannot be configured.

#### Response

**Success (200 OK):**
```json
{
  "conversationId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "message": "Based on current technological trajectories and available data...",
  "agentType": "A1",
  "iterationNumber": 1,
  "isOngoing": true,
  "totalMessages": 1
}
```

**Response Fields:**

| Field | Type | Description |
|-------|------|-------------|
| `conversationId` | GUID | Unique identifier for the conversation |
| `message` | string | First message from Agent 1 |
| `agentType` | string | Always "A1" for init endpoint |
| `iterationNumber` | integer | Always 1 for init endpoint |
| `isOngoing` | boolean | Always true for init endpoint |
| `totalMessages` | integer | Always 1 for init endpoint |

#### Internal Process

1. **Validate** input parameters (required, trim whitespace)
2. **Create** Conversation entity in database:
   - Agent1Personality, Agent2Personality, Topic stored as strings
   - IterationCount fixed at 3
   - Status set to "InProgress"
   - StartTime set to current UTC time
3. **Generate** OpenAI prompt: `"You are {Agent1Personality}. Respond to the conversation on {Topic}: "`
4. **Call** OpenAI GPT-3.5-turbo API
5. **Save** Message entity:
   - AgentType = "A1"
   - IterationNumber = 1
   - Content = OpenAI response
6. **Return** formatted response

#### Error Responses

**400 Bad Request:**
```json
{
  "error": "Invalid input",
  "message": "All fields are required"
}
```

**500 Internal Server Error:**
```json
{
  "error": "Internal server error",
  "message": "Error calling OpenAI API: [details]"
}
```

#### Example Request

```bash
curl -X POST https://localhost:5001/api/conversation/init \
  -H "Content-Type: application/json" \
  -d '{
    "agent1Personality": "Logical analyst who values data and evidence",
    "agent2Personality": "Creative thinker who uses metaphors and storytelling",
    "topic": "The future of artificial intelligence"
  }'
```

---

### 2. Continue Conversation

Continues an existing conversation by generating the next agent's response. Agents alternate automatically based on message count.

**Endpoint:** `POST /api/conversation/follow`

#### Request

**Headers:**
```
Content-Type: application/json
```

**Body:**
```json
{
  "conversationId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
}
```

**Parameters:**

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `conversationId` | GUID | Yes | Conversation identifier from init response |

#### Response

**Success (200 OK) - Ongoing:**
```json
{
  "conversationId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "message": "Imagine AI as a vast ocean of possibilities...",
  "agentType": "A2",
  "iterationNumber": 1,
  "isOngoing": true,
  "totalMessages": 2
}
```

**Success (200 OK) - Completed:**
```json
{
  "conversationId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "message": "In conclusion, the symbiosis of logic and creativity...",
  "agentType": "A2",
  "iterationNumber": 3,
  "isOngoing": false,
  "totalMessages": 6
}
```

**Response Fields:**

| Field | Type | Description |
|-------|------|-------------|
| `conversationId` | GUID | Conversation identifier |
| `message` | string | Generated message from current agent |
| `agentType` | string | "A1" or "A2" - which agent responded |
| `iterationNumber` | integer | Current iteration (1, 2, or 3) |
| `isOngoing` | boolean | `false` when totalMessages == 6, `true` otherwise |
| `totalMessages` | integer | Total messages in conversation (2-6) |

#### Agent Alternation Logic

**Critical:** Agents alternate based on **odd/even message count**:

| Message Count (before follow) | Next Agent | Iteration |
|-------------------------------|------------|-----------|
| 1 (after init) | A2 | 1 |
| 2 | A1 | 2 |
| 3 | A2 | 2 |
| 4 | A1 | 3 |
| 5 | A2 | 3 |

**Algorithm:**
```csharp
var nextAgent = messageCount % 2 == 1 ? "A2" : "A1";
var iterationNumber = (messageCount / 2) + 1;
```

#### Internal Process

1. **Retrieve** conversation with all messages from database
2. **Validate** conversation exists and status is "InProgress"
3. **Calculate** next agent and iteration number
4. **Build history** by concatenating all messages:
   ```
   "A1: First message\nA2: Second message\nA1: Third message"
   ```
5. **Generate** OpenAI prompt: `"You are {NextAgentPersonality}. Respond to the conversation on {Topic}: {History}"`
6. **Call** OpenAI GPT-3.5-turbo API
7. **Save** new Message entity with correct AgentType and IterationNumber
8. **Check completion:**
   - If totalMessages == 6: Set Status = "Completed", EndTime = now, isOngoing = false
   - Otherwise: isOngoing = true
9. **Return** formatted response

#### Error Responses

**404 Not Found:**
```json
{
  "error": "Not found",
  "message": "Conversation not found"
}
```

**400 Bad Request:**
```json
{
  "error": "Invalid request",
  "message": "Conversation already completed"
}
```

**500 Internal Server Error:**
```json
{
  "error": "Internal server error",
  "message": "Error calling OpenAI API: [details]"
}
```

#### Example Request

```bash
curl -X POST https://localhost:5001/api/conversation/follow \
  -H "Content-Type: application/json" \
  -d '{
    "conversationId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
  }'
```

#### Complete Message Sequence

A complete conversation requires **5 follow calls** after init:

```
POST /init                  → A1-M1 (isOngoing: true)
POST /follow (1st call)     → A2-M1 (isOngoing: true)
POST /follow (2nd call)     → A1-M2 (isOngoing: true)
POST /follow (3rd call)     → A2-M2 (isOngoing: true)
POST /follow (4th call)     → A1-M3 (isOngoing: true)
POST /follow (5th call)     → A2-M3 (isOngoing: false) ← Completed
```

---

### 3. Get Complete Conversation

Retrieves a completed conversation formatted as Markdown.

**Endpoint:** `GET /api/conversation/{id}`

#### Request

**Path Parameters:**

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `id` | GUID | Yes | Conversation identifier |

**No request body required.**

#### Response

**Success (200 OK):**
```json
{
  "conversationId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "markdown": "**A1:** Based on current technological trajectories...\n**A2:** Imagine AI as a vast ocean...\n**A1:** While the metaphor is poetic...\n**A2:** But poetry and logic...\n**A1:** The data supports...\n**A2:** In conclusion, the symbiosis...",
  "agent1Personality": "Logical analyst who values data and evidence",
  "agent2Personality": "Creative thinker who uses metaphors and storytelling",
  "topic": "The future of artificial intelligence",
  "status": "Completed",
  "messageCount": 6
}
```

**Response Fields:**

| Field | Type | Description |
|-------|------|-------------|
| `conversationId` | GUID | Conversation identifier |
| `markdown` | string | Formatted conversation with **AgentType:** prefix |
| `agent1Personality` | string | Agent 1 personality used |
| `agent2Personality` | string | Agent 2 personality used |
| `topic` | string | Conversation topic |
| `status` | string | Always "Completed" for successful response |
| `messageCount` | integer | Always 6 for completed conversations |

#### Markdown Format

**Critical:** Messages are formatted as:
```
**A1:** [message content]
**A2:** [message content]
**A1:** [message content]
**A2:** [message content]
**A1:** [message content]
**A2:** [message content]
```

- Each message on a new line
- Bold markers: `**AgentType:**`
- No timestamps or extra formatting
- Messages ordered by creation time

#### Internal Process

1. **Retrieve** conversation by ID from database
2. **Validate** conversation exists
3. **Check** Status == "Completed" (return error if not)
4. **Load** all messages ordered by Timestamp
5. **Format** as Markdown: `**{AgentType}:** {Content}\n`
6. **Return** formatted response

#### Error Responses

**404 Not Found:**
```json
{
  "error": "Not found",
  "message": "Conversation not found"
}
```

**400 Bad Request:**
```json
{
  "error": "Invalid request",
  "message": "Conversation not yet completed"
}
```

**Note:** This endpoint only returns completed conversations (Status = "Completed").

#### Example Request

```bash
curl https://localhost:5001/api/conversation/3fa85f64-5717-4562-b3fc-2c963f66afa6
```

---

## Data Models

### Conversation Entity

Stored in database as complete entity (no separate Personality/Topic tables).

```json
{
  "id": "GUID",
  "agent1Personality": "string (max 500 chars)",
  "agent2Personality": "string (max 500 chars)",
  "topic": "string (max 1000 chars)",
  "iterationCount": 3,
  "status": "InProgress | Completed | Failed",
  "startTime": "datetime",
  "endTime": "datetime | null"
}
```

### Message Entity

```json
{
  "id": "GUID",
  "conversationId": "GUID (FK)",
  "agentType": "A1 | A2",
  "iterationNumber": 1 | 2 | 3,
  "content": "string",
  "timestamp": "datetime"
}
```

### Status Values

| Status | Description |
|--------|-------------|
| `InProgress` | Conversation active (messages < 6) |
| `Completed` | All 6 messages generated |
| `Failed` | Error occurred during conversation |

---

## Error Handling

### Error Response Format

All errors follow this structure:

```json
{
  "error": "Error type",
  "message": "Detailed error message"
}
```

### HTTP Status Codes

| Code | Meaning | When Used |
|------|---------|-----------|
| 200 | OK | Successful request |
| 400 | Bad Request | Invalid input, validation failure |
| 404 | Not Found | Conversation not found |
| 500 | Internal Server Error | OpenAI API error, database error |

### Common Errors

**Validation Errors:**
```json
{
  "error": "Invalid input",
  "message": "All fields are required"
}
```

**OpenAI API Errors:**
```json
{
  "error": "Internal server error",
  "message": "Error calling OpenAI API: Unauthorized"
}
```

**Conversation State Errors:**
```json
{
  "error": "Invalid request",
  "message": "Conversation already completed"
}
```

### Error Logging

All errors are logged to console using Serilog:
```
[ERR] Error initializing conversation
System.Exception: Error calling OpenAI API
   at AIAgentConversation.Services.OpenAIService...
```

---

## Workflow Requirements

### Critical Requirements (MUST Follow)

1. **Fixed Iterations:** Always 3 iterations (6 messages) - not configurable
2. **Agent Alternation:** Must follow odd/even pattern (A1, A2, A1, A2, A1, A2)
3. **Prompt Format:** Exactly `"You are {personality}. Respond to the conversation on {topic}: {history}"`
4. **History Format:** Simple concatenation `"A1: msg1\nA2: msg2\n..."`
5. **Completion Flag:** `isOngoing = false` only when totalMessages == 6
6. **Status Update:** Set to "Completed" only when 6 messages exist
7. **Markdown Format:** `**A1:** msg\n**A2:** msg\n...`

### Workflow Sequence

```
1. Client → POST /init
2. API → Create conversation, call A1, save message
3. API → Return A1-M1 with isOngoing=true
4. Client → POST /follow (1st time)
5. API → Calculate next agent (A2), build history, call A2, save message
6. API → Return A2-M1 with isOngoing=true
7. Client → POST /follow (2nd time)
8. API → Next agent (A1), call A1, save message
9. API → Return A1-M2 with isOngoing=true
   ... repeat until 6 messages ...
10. API → Return A2-M3 with isOngoing=false
11. Client → GET /conversation/{id}
12. API → Return markdown formatted conversation
```

### Iteration and Message Mapping

| Message # | Agent | Iteration | Call Type |
|-----------|-------|-----------|-----------|
| 1 | A1 | 1 | init |
| 2 | A2 | 1 | follow |
| 3 | A1 | 2 | follow |
| 4 | A2 | 2 | follow |
| 5 | A1 | 3 | follow |
| 6 | A2 | 3 | follow |

---

## Examples

### Complete Conversation Flow

#### Step 1: Initialize
```bash
POST /api/conversation/init
Content-Type: application/json

{
  "agent1Personality": "Scientist who relies on empirical evidence",
  "agent2Personality": "Philosopher who questions fundamental assumptions",
  "topic": "The nature of consciousness"
}
```

**Response:**
```json
{
  "conversationId": "abc123...",
  "message": "From a scientific perspective, consciousness emerges from neural activity...",
  "agentType": "A1",
  "iterationNumber": 1,
  "isOngoing": true,
  "totalMessages": 1
}
```

#### Step 2: First Follow
```bash
POST /api/conversation/follow
Content-Type: application/json

{
  "conversationId": "abc123..."
}
```

**Response:**
```json
{
  "conversationId": "abc123...",
  "message": "But what if consciousness is not merely reducible to physical processes?...",
  "agentType": "A2",
  "iterationNumber": 1,
  "isOngoing": true,
  "totalMessages": 2
}
```

#### Steps 3-6: Continue Following
*(Repeat follow calls 4 more times)*

#### Step 7: Final Follow
```bash
POST /api/conversation/follow
Content-Type: application/json

{
  "conversationId": "abc123..."
}
```

**Response:**
```json
{
  "conversationId": "abc123...",
  "message": "Perhaps the answer lies in embracing both perspectives...",
  "agentType": "A2",
  "iterationNumber": 3,
  "isOngoing": false,
  "totalMessages": 6
}
```

#### Step 8: Get Markdown
```bash
GET /api/conversation/abc123...
```

**Response:**
```json
{
  "conversationId": "abc123...",
  "markdown": "**A1:** From a scientific perspective...\n**A2:** But what if consciousness...\n**A1:** The empirical evidence suggests...\n**A2:** Yet evidence itself requires...\n**A1:** While philosophical inquiry...\n**A2:** Perhaps the answer lies...",
  "agent1Personality": "Scientist who relies on empirical evidence",
  "agent2Personality": "Philosopher who questions fundamental assumptions",
  "topic": "The nature of consciousness",
  "status": "Completed",
  "messageCount": 6
}
```

---

## Rate Limiting

**Current Version:** No rate limiting implemented (v1.0)

**Recommended for Production:**
- 100 requests per hour per IP
- 10 concurrent conversations per user
- OpenAI API rate limits apply

---

## Changelog

### Version 1.0.0 (September 2025)
- Initial release
- Three endpoints: init, follow, get
- Fixed 3 iterations (6 messages)
- GPT-3.5-turbo integration
- Simplified string-based schema
- Console-only logging

---

## Support

For issues or questions:
- Check the [README.md](README.md) for setup instructions
- Review [UI.md](UI.md) for frontend integration
- See [TUTORIAL.md](TUTORIAL.md) for complete walkthrough
- Consult [AI_CUSTOMIZATION_GUIDE.md](AI_CUSTOMIZATION_GUIDE.md) for AI behavior customization

---

**Last Updated:** September 30, 2025  
**API Version:** 1.0.0  
**Maintained by:** AI Agent Conversation Team