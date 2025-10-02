using OpenAI.Chat;
using AIAgentConversation.Models;

namespace AIAgentConversation.Services;

public class OpenAIService : IOpenAIService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<OpenAIService> _logger;
    private readonly ChatClient _chatClient;

    public OpenAIService(IConfiguration configuration, ILogger<OpenAIService> logger)
    {
        _configuration = configuration;
        _logger = logger;
        
        var apiKey = _configuration["OpenAI:ApiKey"];
        var model = _configuration["OpenAI:Model"] ?? "gpt-3.5-turbo";
        
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            _logger.LogWarning("OpenAI API key is not configured. Service will fail at runtime.");
            apiKey = "dummy-key"; // Prevent null reference
        }
        
        // Use OpenAI SDK with proper OpenAI client
        var client = new OpenAI.OpenAIClient(apiKey);
        _chatClient = client.GetChatClient(model);
    }

    public async Task<string> GenerateResponseAsync(string personality, string topic, string history, string politenessLevel = "medium", ConversationPhase phase = ConversationPhase.Conversation)
    {
        try
        {
            _logger.LogInformation("Calling OpenAI with personality: {Personality}", personality);
            
            var maxTokens = _configuration.GetValue<int>("OpenAI:MaxTokens", 500);
            var temperature = _configuration.GetValue<float>("OpenAI:Temperature", 0.7f);
            
            // Determine conversation depth based on history length to adjust creativity
            var messageCount = string.IsNullOrEmpty(history) ? 0 : history.Split('\n', StringSplitOptions.RemoveEmptyEntries).Length;
            var adjustedTemperature = temperature;
            
            // Increase temperature slightly as conversation progresses for more dynamic responses
            if (messageCount >= 2 && messageCount < 4)
            {
                adjustedTemperature = Math.Min(temperature + 0.1f, 1.0f);
            }
            else if (messageCount >= 4)
            {
                adjustedTemperature = Math.Min(temperature + 0.15f, 1.0f);
            }
            
            var chatOptions = new ChatCompletionOptions
            {
                MaxOutputTokenCount = maxTokens,
                Temperature = adjustedTemperature
            };
            
            // Build enhanced prompt with system and user messages for better context
            var messages = new List<ChatMessage>();
            
            // Adjust tone based on politeness level
            var toneGuidance = politenessLevel?.ToLower() switch
            {
                "low" => "Be direct and assertive. Challenge points you disagree with. " +
                        "Avoid excessive politeness or overly agreeable language. " +
                        "Focus on your perspective and don't hesitate to express disagreement or skepticism.",
                "high" => "Be respectful and courteous. Acknowledge others' points graciously. " +
                         "Use phrases like 'I appreciate your perspective' or 'That's a valid point'. " +
                         "Maintain a collaborative and warm tone throughout.",
                _ => "Be balanced in tone - neither overly polite nor too confrontational. " +
                     "Engage thoughtfully with respect but without excessive pleasantries."
            };
            
            // Build phase-specific system prompt
            var phaseGuidance = phase switch
            {
                ConversationPhase.Introduction => 
                    "This is the INTRODUCTION phase. Introduce yourself briefly and share your initial perspective on the topic. " +
                    "Keep it concise (2-3 sentences). Set the tone for the discussion ahead.",
                
                ConversationPhase.Conversation => 
                    "This is the CONVERSATION phase. Engage deeply with the points made. " +
                    "Challenge ideas, build on arguments, or present counterpoints. " +
                    "Provide responses that are 2-4 sentences long, balancing depth with conciseness. " +
                    "Build upon previous points when continuing the conversation.",
                
                ConversationPhase.Conclusion => 
                    "This is the CONCLUSION phase. Summarize your key points from the conversation. " +
                    "Reflect on what was discussed and provide a thoughtful closing statement (2-3 sentences). " +
                    "You may acknowledge valid points made by the other agent while reinforcing your perspective.",
                
                _ => "Engage thoughtfully in this conversation."
            };
            
            // System message to set up the agent's role and constraints
            var systemPrompt = $"You are {personality}. You are engaging in a thoughtful conversation about {topic}. " +
                             $"Stay true to your personality traits while being engaging and substantive. " +
                             $"{toneGuidance} " +
                             $"{phaseGuidance}";
            messages.Add(new SystemChatMessage(systemPrompt));
            
            // User message with the conversation context (phase-specific)
            string userPrompt;
            if (string.IsNullOrEmpty(history))
            {
                // First message - introduction phase
                userPrompt = phase switch
                {
                    ConversationPhase.Introduction => 
                        $"Introduce yourself and share your initial perspective on: {topic}. " +
                        $"Keep it brief and engaging as this is just the introduction.",
                    _ => 
                        $"Begin a conversation on the topic: {topic}. Share your initial perspective based on your personality."
                };
            }
            else
            {
                // Continuing conversation - provide history and ask for response
                userPrompt = phase switch
                {
                    ConversationPhase.Conclusion => 
                        $"Here is the conversation so far:\n{history}\n\n" +
                        $"Now provide your CONCLUSION. Summarize your key points and provide a thoughtful closing statement.",
                    _ => 
                        $"Here is the conversation so far:\n{history}\n\n" +
                        $"Respond to the conversation above, addressing points made while staying in character as {personality}."
                };
            }
            messages.Add(new UserChatMessage(userPrompt));
            
            var completion = await _chatClient.CompleteChatAsync(messages, chatOptions);
            
            var response = completion.Value.Content[0].Text;
            
            _logger.LogInformation("OpenAI response received, length: {Length}, temperature: {Temp}", response.Length, adjustedTemperature);
            
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling OpenAI API");
            throw;
        }
    }
}
