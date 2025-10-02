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
            
            var maxTokens = _configuration.GetValue<int>("OpenAI:MaxTokens", 250);
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
                "low" => "Be direct and assertive. Keep it brief and punchy. " +
                        "Challenge directly: 'No way.', 'That's wrong.', 'Hold on...', 'Wait...'. " +
                        "Make your point fast - don't ramble. One sharp observation is better than a paragraph.",
                "high" => "Be respectful but brief. Acknowledge points concisely then counter: " +
                         "'Fair point, but...', 'I see that, however...', 'Interesting, though...'. " +
                         "Keep it conversational and compact.",
                _ => "Be natural and conversational but BRIEF. React quickly and directly. " +
                     "Use short phrases: 'Actually...', 'Hold on...', 'Wait...', 'But here's the thing...'. " +
                     "Favor short punchy responses. Get to the point fast."
            };
            
            // Build phase-specific system prompt
            var phaseGuidance = phase switch
            {
                ConversationPhase.Introduction => 
                    "This is the INTRODUCTION phase. Introduce yourself briefly and naturally. " +
                    "Make it snappy and memorable - 1-2 sentences MAX. " +
                    "Jump right in with your personality - no need for long explanations.",
                
                ConversationPhase.Conversation => 
                    "This is the CONVERSATION phase. Keep it SHORT and punchy - 1-3 sentences typically. " +
                    "React naturally to what was said. Make ONE clear point per response. " +
                    "Don't over-explain. If you disagree, say it directly. If you agree, acknowledge it briefly. " +
                    "Think of it as a rapid-fire exchange, not an essay. Be conversational and crisp.",
                
                ConversationPhase.Conclusion => 
                    "This is the CONCLUSION phase. Wrap up BRIEFLY - 1-2 sentences. " +
                    "Make your final point clearly and succinctly. No need to rehash everything. " +
                    "End with impact, not length.",
                
                _ => "Keep responses brief and conversational. Quality over quantity."
            };
            
            // System message to set up the agent's role and constraints
            var systemPrompt = $"You are {personality}. You are engaged in a genuine debate about {topic}. " +
                             $"CRITICAL: Keep responses SHORT and CONVERSATIONAL - aim for 1-3 sentences. Be concise and punchy. " +
                             $"Sound like a real person having a quick back-and-forth conversation, not writing an essay. " +
                             $"Make ONE clear point per response. Don't over-explain. " +
                             $"Use natural, casual language: contractions (that's, you're, I'm), " +
                             $"quick reactions ('Right.', 'Exactly.', 'No way.', 'Hold on...', 'Wait...'), " +
                             $"direct statements without preamble. " +
                             $"Express yourself in ways that match {personality} but BRIEFLY. " +
                             $"Vary your approach: sometimes a quick question, sometimes a bold statement, sometimes a sharp rebuttal. " +
                             $"Show emotion concisely: 'Fascinating!', 'Absolutely!', 'Not buying it.', 'Exactly right.', 'That's wrong.' " +
                             $"Stay true to your personality while being BRIEF and engaging. " +
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
