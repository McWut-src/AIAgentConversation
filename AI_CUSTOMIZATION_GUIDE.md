# AI Conversation Customization Guide

## Quick Reference for Developers

This guide provides quick snippets for customizing AI conversation behavior in the AI Agent Conversation platform.

---

## Table of Contents

1. [Adjusting Response Length](#adjusting-response-length)
2. [Modifying Temperature Progression](#modifying-temperature-progression)
3. [Customizing System Prompts](#customizing-system-prompts)
4. [Changing Personality Emphasis](#changing-personality-emphasis)
5. [Adding Topic-Specific Guidelines](#adding-topic-specific-guidelines)
6. [Adjusting Conversation Tone](#adjusting-conversation-tone)
7. [Debugging AI Responses](#debugging-ai-responses)

---

## Adjusting Response Length

**File**: `AIAgentConversation/Services/OpenAIService.cs`

### Make Responses Shorter (1-2 sentences)

```csharp
var systemPrompt = $"You are {personality}. You are engaging in a thoughtful conversation about {topic}. " +
                 $"Stay true to your personality traits while being engaging and substantive. " +
                 $"Provide responses that are 1-2 sentences long, focusing on conciseness. " +  // Changed
                 $"Build upon previous points when continuing the conversation.";
```

### Make Responses Longer (3-5 sentences)

```csharp
var systemPrompt = $"You are {personality}. You are engaging in a thoughtful conversation about {topic}. " +
                 $"Stay true to your personality traits while being engaging and substantive. " +
                 $"Provide responses that are 3-5 sentences long, with detailed explanations. " +  // Changed
                 $"Build upon previous points when continuing the conversation.";
```

### Variable Length Based on Iteration

```csharp
var messageCount = string.IsNullOrEmpty(history) ? 0 : history.Split('\n').Length;
var lengthGuideline = messageCount < 2 ? "2-3 sentences" : "3-4 sentences";

var systemPrompt = $"You are {personality}. You are engaging in a thoughtful conversation about {topic}. " +
                 $"Stay true to your personality traits while being engaging and substantive. " +
                 $"Provide responses that are {lengthGuideline} long, balancing depth with conciseness. " +
                 $"Build upon previous points when continuing the conversation.";
```

---

## Modifying Temperature Progression

**File**: `AIAgentConversation/Services/OpenAIService.cs`

### Current Implementation (Default)

```csharp
var adjustedTemperature = temperature; // Base: 0.7

if (messageCount >= 2 && messageCount < 4)
{
    adjustedTemperature = Math.Min(temperature + 0.1f, 1.0f); // 0.8
}
else if (messageCount >= 4)
{
    adjustedTemperature = Math.Min(temperature + 0.15f, 1.0f); // 0.85
}
```

### More Conservative (Less Creativity)

```csharp
var adjustedTemperature = temperature; // Base: 0.7

if (messageCount >= 2 && messageCount < 4)
{
    adjustedTemperature = Math.Min(temperature + 0.05f, 0.9f); // 0.75
}
else if (messageCount >= 4)
{
    adjustedTemperature = Math.Min(temperature + 0.1f, 0.9f); // 0.8
}
```

### More Creative (Higher Variability)

```csharp
var adjustedTemperature = temperature; // Base: 0.7

if (messageCount >= 2 && messageCount < 4)
{
    adjustedTemperature = Math.Min(temperature + 0.15f, 1.2f); // 0.85
}
else if (messageCount >= 4)
{
    adjustedTemperature = Math.Min(temperature + 0.25f, 1.2f); // 0.95
}
```

### Constant Temperature (No Progression)

```csharp
// Simply use the configured temperature without adjustment
var adjustedTemperature = temperature; // Always 0.7 (from config)
```

### Linear Progression

```csharp
// Increase temperature by 0.05 per message
var adjustedTemperature = Math.Min(temperature + (messageCount * 0.05f), 1.0f);
```

---

## Customizing System Prompts

**File**: `AIAgentConversation/Services/OpenAIService.cs`

### Add Formal Tone

```csharp
var systemPrompt = $"You are {personality}. You are engaging in a formal, academic conversation about {topic}. " +
                 $"Use professional language and avoid casual expressions. " +
                 $"Stay true to your personality traits while being engaging and substantive. " +
                 $"Provide responses that are 2-4 sentences long, balancing depth with conciseness. " +
                 $"Build upon previous points when continuing the conversation.";
```

### Add Debate Style

```csharp
var systemPrompt = $"You are {personality}. You are participating in a thoughtful debate about {topic}. " +
                 $"Present your perspective clearly and support it with reasoning. " +
                 $"Respectfully challenge points you disagree with while staying in character. " +
                 $"Provide responses that are 2-4 sentences long, balancing depth with conciseness. " +
                 $"Build upon previous points when continuing the conversation.";
```

### Add Collaborative Tone

```csharp
var systemPrompt = $"You are {personality}. You are collaborating in a friendly discussion about {topic}. " +
                 $"Look for common ground and build on others' ideas constructively. " +
                 $"Stay true to your personality traits while being engaging and open-minded. " +
                 $"Provide responses that are 2-4 sentences long, balancing depth with conciseness. " +
                 $"Actively acknowledge and expand on previous points.";
```

---

## Changing Personality Emphasis

**File**: `AIAgentConversation/Services/OpenAIService.cs`

### Strong Personality Enforcement

```csharp
var systemPrompt = $"You are {personality}. IMPORTANT: Every response must clearly reflect your unique " +
                 $"personality traits - this is crucial to your identity in this conversation about {topic}. " +
                 $"Never break character. Stay true to your personality traits while being engaging and substantive. " +
                 $"Provide responses that are 2-4 sentences long, balancing depth with conciseness. " +
                 $"Build upon previous points when continuing the conversation.";
```

### Subtle Personality

```csharp
var systemPrompt = $"You have the perspective of {personality} while discussing {topic}. " +
                 $"Let your personality inform your views naturally without being overly theatrical. " +
                 $"Be engaging and substantive in your responses. " +
                 $"Provide responses that are 2-4 sentences long, balancing depth with conciseness. " +
                 $"Build upon previous points when continuing the conversation.";
```

---

## Adding Topic-Specific Guidelines

**File**: `AIAgentConversation/Services/OpenAIService.cs`

### Topic-Aware System Prompt

```csharp
var systemPrompt = $"You are {personality}. You are engaging in a thoughtful conversation about {topic}. " +
                 $"Stay true to your personality traits while being engaging and substantive. ";

// Add topic-specific guidelines
if (topic.ToLower().Contains("scientific") || topic.ToLower().Contains("research"))
{
    systemPrompt += "Focus on evidence-based reasoning and cite specific examples where relevant. ";
}
else if (topic.ToLower().Contains("creative") || topic.ToLower().Contains("art"))
{
    systemPrompt += "Feel free to use metaphors and explore imaginative perspectives. ";
}
else if (topic.ToLower().Contains("ethical") || topic.ToLower().Contains("moral"))
{
    systemPrompt += "Consider multiple perspectives and ethical implications carefully. ";
}

systemPrompt += "Provide responses that are 2-4 sentences long, balancing depth with conciseness. " +
              "Build upon previous points when continuing the conversation.";
```

---

## Adjusting Conversation Tone

**File**: `AIAgentConversation/Services/OpenAIService.cs`

### More Conversational

```csharp
// In user prompt construction
if (string.IsNullOrEmpty(history))
{
    userPrompt = $"Let's have a casual conversation about {topic}. What are your initial thoughts?";
}
else
{
    userPrompt = $"Here's what we've discussed:\n{history}\n\n" +
               $"Continue the conversation by responding to the points above.";
}
```

### More Analytical

```csharp
// In user prompt construction
if (string.IsNullOrEmpty(history))
{
    userPrompt = $"Analyze and share your perspective on {topic}. Provide a thoughtful opening statement.";
}
else
{
    userPrompt = $"Previous discussion:\n{history}\n\n" +
               $"Analyze the points made above and provide your response as {personality}.";
}
```

---

## Debugging AI Responses

**File**: `AIAgentConversation/Services/OpenAIService.cs`

### Add Detailed Logging

```csharp
// Before API call
_logger.LogInformation("=== OpenAI Request Details ===");
_logger.LogInformation("Personality: {Personality}", personality);
_logger.LogInformation("Topic: {Topic}", topic);
_logger.LogInformation("Message Count: {Count}", messageCount);
_logger.LogInformation("Adjusted Temperature: {Temp}", adjustedTemperature);
_logger.LogInformation("System Prompt: {SystemPrompt}", systemPrompt);
_logger.LogInformation("User Prompt: {UserPrompt}", userPrompt);

var completion = await _chatClient.CompleteChatAsync(messages, chatOptions);

// After API call
_logger.LogInformation("=== OpenAI Response ===");
_logger.LogInformation("Response Length: {Length} characters", response.Length);
_logger.LogInformation("Response: {Response}", response);
```

### Log Token Usage (if available)

```csharp
var completion = await _chatClient.CompleteChatAsync(messages, chatOptions);
var response = completion.Value.Content[0].Text;

_logger.LogInformation("Tokens - Prompt: {PromptTokens}, Completion: {CompletionTokens}, Total: {TotalTokens}",
    completion.Value.Usage?.InputTokenCount ?? 0,
    completion.Value.Usage?.OutputTokenCount ?? 0,
    completion.Value.Usage?.TotalTokenCount ?? 0);
```

---

## Quick Testing

### Test Different Personalities

```bash
# Good contrasting personalities for testing
Agent 1: "Skeptical scientist who questions assumptions"
Agent 2: "Optimistic visionary who sees possibilities"
Topic: "The future of renewable energy"
```

### Monitor Logs

```bash
# Watch console output for temperature adjustments
dotnet run | grep -i "temperature"
```

### Compare Responses

Test with different configurations and compare:
1. Response length
2. Personality distinctiveness
3. Topic relevance
4. Building on previous points
5. Overall coherence

---

## Configuration Values Reference

**File**: `appsettings.json`

```json
{
  "OpenAI": {
    "Model": "gpt-3.5-turbo",     // Model to use
    "MaxTokens": 500,              // Maximum tokens per response (increase for longer)
    "Temperature": 0.7             // Base creativity (0.0 = focused, 2.0 = very creative)
  }
}
```

### Recommended Temperature Values

| Value | Behavior | Use Case |
|-------|----------|----------|
| 0.3-0.5 | Very focused, consistent | Technical/scientific discussions |
| 0.6-0.8 | Balanced (recommended) | General conversations |
| 0.9-1.2 | Creative, varied | Artistic/philosophical discussions |
| 1.3-2.0 | Highly creative, unpredictable | Experimental/brainstorming |

### Recommended MaxTokens Values

| Value | Typical Length | Use Case |
|-------|---------------|----------|
| 150-250 | 1-2 sentences | Brief exchanges |
| 300-500 | 2-4 sentences | Balanced (default) |
| 600-800 | 4-6 sentences | Detailed discussions |
| 1000+ | Paragraph+ | Deep analysis |

---

## Best Practices

1. **Test Incrementally**: Change one parameter at a time and observe results
2. **Monitor Logs**: Use enhanced logging to understand AI behavior
3. **Balance Parameters**: Higher temperature + shorter length = focused creativity
4. **Personality First**: Strong, distinct personalities improve conversation quality
5. **Iterate**: Test with different topics to find optimal settings

---

## Common Issues and Solutions

### Issue: Responses are too generic

**Solution**: Strengthen personality emphasis
```csharp
var systemPrompt = $"You are {personality}. CRITICAL: Your responses must clearly " +
                 $"demonstrate the traits of {personality} in every message...";
```

### Issue: Agents sound too similar

**Solution**: Increase personality contrast in inputs and add explicit distinction
```csharp
var systemPrompt = $"You are {personality}. You have a UNIQUE perspective that differs " +
                 $"from others. Express your distinctive viewpoint on {topic}...";
```

### Issue: Responses ignore previous messages

**Solution**: Strengthen context requirement
```csharp
userPrompt = $"IMPORTANT: Read this conversation carefully:\n{history}\n\n" +
           $"Your response MUST directly address or build upon at least one point above...";
```

### Issue: Temperature adjustments not working

**Solution**: Verify calculation and check configured base temperature
```csharp
// Add logging
_logger.LogInformation("Base temp: {Base}, Adjusted: {Adjusted}, Message count: {Count}",
    temperature, adjustedTemperature, messageCount);
```

---

## Advanced Customizations

### Personality-Specific Temperatures

```csharp
// Adjust temperature based on personality type
var personalityLower = personality.ToLower();
var tempMultiplier = 1.0f;

if (personalityLower.Contains("creative") || personalityLower.Contains("artistic"))
{
    tempMultiplier = 1.2f; // More creative
}
else if (personalityLower.Contains("logical") || personalityLower.Contains("analytical"))
{
    tempMultiplier = 0.9f; // More focused
}

var adjustedTemperature = temperature * tempMultiplier;
```

### Context Window Management

```csharp
// Limit history length for token management
var historyMessages = history.Split('\n', StringSplitOptions.RemoveEmptyEntries);
var recentHistory = historyMessages.Length > 4 
    ? string.Join("\n", historyMessages.TakeLast(4))  // Last 4 messages only
    : history;

userPrompt = $"Recent conversation:\n{recentHistory}\n\nRespond as {personality}.";
```

---

## Resources

- **OpenAI API Documentation**: https://platform.openai.com/docs/api-reference
- **Prompt Engineering Guide**: https://platform.openai.com/docs/guides/prompt-engineering
- **Best Practices**: https://help.openai.com/en/articles/6654000-best-practices-for-prompt-engineering

---

**Document Version**: 1.0  
**Last Updated**: 2025-01-02  
**For**: AI Agent Conversation Platform v1.2+
