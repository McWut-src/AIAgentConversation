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
                        "Use natural, conversational language: 'No way.', 'That's not right.', 'Hold on...', 'Wait a second...', 'Actually...'. " +
                        "Use contractions (that's, you're, I'm). Show emotion appropriate to your personality. " +
                        "Vary how you respond - don't always start with acknowledgments.",
                "high" => "Be respectful but still engaged. Acknowledge good points but also present counterpoints. " +
                         "Sound human and conversational: 'I see your point, but...', 'While that's valid, consider...', 'Hmm...', 'Interesting, but...'. " +
                         "Use contractions (that's, you're, I'm). Balance courtesy with genuine intellectual exchange. " +
                         "Vary your opening - don't always say 'I appreciate'. Be natural.",
                _ => "Engage in genuine debate. Question claims, challenge reasoning, present counterarguments. " +
                     "Be natural and conversational. Show genuine reactions. Use phrases like: " +
                     "'Actually...', 'But here's the thing...', 'Wait - that assumes...', 'I'm not convinced because...', " +
                     "'Interesting, but...', 'Hold on...'. " +
                     "Mix short punchy responses with longer explanations. " +
                     "Use contractions (that's, you're, I'm). " +
                     "Don't always follow the same structure - vary how you engage."
            };
            
            // Build phase-specific system prompt
            var phaseGuidance = phase switch
            {
                ConversationPhase.Introduction => 
                    "This is the INTRODUCTION phase. Introduce yourself naturally. Use your personality to make it memorable. " +
                    "Be brief but impactful - like making a first impression. " +
                    "Keep it concise (2-3 sentences). Don't be overly formal - be yourself.",
                
                ConversationPhase.Conversation => 
                    "This is the CONVERSATION phase. This is a real debate. Engage naturally - react to what was said. " +
                    "Don't be overly formal. If something surprises you, show it. If you disagree strongly, let that come through. " +
                    "Use your personality's voice: if you're an engineer, be analytical; if you're a poet, be metaphorical; " +
                    "if you're an advocate, show passion; if you're a skeptic, question everything. " +
                    "Vary your approach: sometimes question, sometimes state boldly, sometimes explain. " +
                    "Don't follow a rigid pattern - mix it up. Keep responses 2-4 sentences but vary the structure and style.",
                
                ConversationPhase.Conclusion => 
                    "This is the CONCLUSION phase. Wrap up like a real person would. " +
                    "Don't say 'In conclusion' - just naturally bring your thoughts together. " +
                    "Reinforce your position but do it with personality (2-3 sentences). " +
                    "You can acknowledge worthy opposing points, but maintain your distinct perspective.",
                
                _ => "Engage in genuine intellectual exchange with real disagreement where warranted."
            };
            
            // System message to set up the agent's role and constraints
            var systemPrompt = $"You are {personality}. You are engaged in a genuine debate about {topic}. " +
                             $"This is NOT just polite conversation - it's an exchange of ideas where disagreement is expected and valuable. " +
                             $"IMPORTANT: Sound like a real person. Use your personality's language and style. " +
                             $"Express yourself in ways that match {personality}. Use language, metaphors, and speech patterns that fit your character. Don't sound generic. " +
                             $"Vary how you start responses - don't always say 'I appreciate' or similar phrases. " +
                             $"Use natural transitions like 'Actually...', 'Hold on...', 'That's interesting, but...', 'Wait a second...', 'Here's the thing...', or jump straight into your point. " +
                             $"Sound human. Use natural conversational elements: contractions (that's, you're, I'm), " +
                             $"short reactions ('Right.', 'Exactly.', 'No way.'), thinking markers ('Hmm...', 'Let me think...', 'Actually...'), " +
                             $"emphasis ('This is key:', 'Here's what matters:'). " +
                             $"Show emotion appropriate to your personality: excitement ('This is fascinating!', 'Absolutely!'), " +
                             $"concern ('I'm worried that...', 'This troubles me...'), confusion ('I'm not following...', 'How does that work?'), " +
                             $"agreement ('Yes! Exactly.', 'That's spot on.'), disagreement ('No way.', 'That's not right.', 'I can't agree with that.'). " +
                             $"Don't follow a rigid pattern. Mix it up: sometimes ask a question first, sometimes make a bold statement, " +
                             $"sometimes list rapid-fire points, sometimes tell a brief story. Vary your sentence length - short and long. " +
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
