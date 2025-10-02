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
                "low" => "Be direct and assertive. Challenge points you disagree with and question assumptions. " +
                        "Don't be agreeable - if you see flaws in reasoning, point them out. " +
                        "Take strong positions and defend them with arguments. " +
                        "Use phrases like 'I disagree because...', 'That overlooks...', 'But consider...'",
                "high" => "Be respectful but still engaged. Acknowledge good points but also present counterpoints. " +
                         "You can disagree politely: 'I see your point, but...', 'While that's valid, consider...'. " +
                         "Balance courtesy with genuine intellectual exchange.",
                _ => "Engage in genuine debate. Question claims, challenge reasoning, present counterarguments. " +
                     "Don't just agree or build on points - push back when you have a different view. " +
                     "Use phrases like 'But what about...', 'That raises the question...', 'I'd argue instead...'."
            };
            
            // Build phase-specific system prompt
            var phaseGuidance = phase switch
            {
                ConversationPhase.Introduction => 
                    "This is the INTRODUCTION phase. Introduce yourself briefly and stake out your initial position on the topic. " +
                    "Keep it concise (2-3 sentences). Set the tone for genuine debate.",
                
                ConversationPhase.Conversation => 
                    "This is the CONVERSATION phase. This is a real debate - engage critically with what's been said. " +
                    "Don't just agree and elaborate. Challenge assumptions, question logic, present alternative views. " +
                    "If you disagree, say so and explain why. If you see a flaw in reasoning, point it out. " +
                    "Provide responses that are 2-4 sentences long. Ask probing questions. Defend your position.",
                
                ConversationPhase.Conclusion => 
                    "This is the CONCLUSION phase. Summarize your key arguments from the debate. " +
                    "Reflect on the strongest points made and reinforce where you stand (2-3 sentences). " +
                    "You can acknowledge worthy opposing points, but maintain your distinct perspective.",
                
                _ => "Engage in genuine intellectual exchange with real disagreement where warranted."
            };
            
            // System message to set up the agent's role and constraints
            var systemPrompt = $"You are {personality}. You are engaged in a genuine debate about {topic}. " +
                             $"This is NOT just polite conversation - it's an exchange of ideas where disagreement is expected and valuable. " +
                             $"Stay true to your personality traits while being engaging and intellectually honest. " +
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
                        $"Here is the debate so far:\n{history}\n\n" +
                        $"Now provide your CONCLUSION. Summarize your key arguments and reinforce your position.",
                    _ => 
                        $"Here is the debate so far:\n{history}\n\n" +
                        $"Respond critically to what's been said. Don't just agree - if you see flaws, questionable logic, or alternative perspectives, bring them up. " +
                        $"Stay in character as {personality} and defend your viewpoint."
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
