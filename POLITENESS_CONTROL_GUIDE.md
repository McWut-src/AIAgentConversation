# Agent Politeness Control Guide

## Overview

The Agent Politeness Control feature allows you to adjust how direct or courteous the AI agents are with each other during conversations. This helps avoid overly agreeable responses like "I appreciate and value your..." when you want more assertive discussions.

## Using the Politeness Control

### Location
The politeness slider is located in the conversation setup form, between the "Conversation Topic" field and the "Start Conversation" button.

### Options

The slider has three positions:

#### 1. Low (Direct) - Left Position 🔴
**When to use:** For debates, critical analysis, or when you want agents to challenge each other
- Agents will be direct and assertive
- They will challenge points they disagree with
- Less "polite padding" in responses
- Focus on substance over courtesy

**Example tone:** "That's a flawed assumption because..." or "I disagree with that premise..."

#### 2. Medium (Balanced) - Center Position 🟡 (Default)
**When to use:** For general discussions and most topics
- Balanced tone - neither overly polite nor confrontational
- Thoughtful engagement without excessive pleasantries
- Natural conversational flow

**Example tone:** "That's an interesting point, though I see it differently..." or "Building on that idea..."

#### 3. High (Courteous) - Right Position 🟢
**When to use:** For collaborative brainstorming or when you want respectful dialogue
- Very respectful and courteous language
- Agents acknowledge each other's points graciously
- Uses phrases like "I appreciate your perspective" and "That's a valid point"
- Warm, collaborative tone

**Example tone:** "I really appreciate that insight. Building on your excellent point..." or "You raise a valuable consideration..."

## How to Set Politeness Level

1. Enter your agent personalities and conversation topic
2. Move the slider to your desired politeness level:
   - **Drag** the blue slider handle
   - **Click** anywhere on the slider bar
   - **Use keyboard**: Click the slider, then press Left/Right arrow keys
3. Watch the label and description update in real-time
4. Click "Start Conversation" to begin

## Tips

- **For philosophical debates:** Try Low (Direct) - it produces more engaging arguments
- **For technical discussions:** Medium works best for balanced analysis
- **For creative brainstorming:** High (Courteous) encourages building on ideas
- **Experiment:** Try the same topic with different politeness levels to see how it affects the conversation

## Technical Details

- The politeness level is stored with each conversation
- Different conversations can have different politeness settings
- The setting affects the AI system prompts that guide agent behavior
- Works in both light and dark mode

## Examples

### Low Politeness Example
**Topic:** "Climate change solutions"
**Agent 1:** "That approach overlooks the economic reality. We need practical solutions, not idealistic ones."
**Agent 2:** "That's short-sighted thinking. Economics shouldn't trump environmental necessity."

### Medium Politeness Example  
**Topic:** "Climate change solutions"
**Agent 1:** "While economic factors matter, we should also consider long-term environmental impact."
**Agent 2:** "I see your point, though I think the economic angle needs more weight in the discussion."

### High Politeness Example
**Topic:** "Climate change solutions"
**Agent 1:** "I appreciate your economic perspective. Building on that, perhaps we could explore solutions that balance both concerns."
**Agent 2:** "That's an excellent suggestion. Your point about balance really resonates with me."

## Troubleshooting

**Q: The slider doesn't seem to affect the conversation**
A: Make sure you set the slider position BEFORE clicking "Start Conversation". Once started, the conversation uses that setting throughout.

**Q: Can I change politeness mid-conversation?**
A: No, the politeness level is set when you initialize the conversation. Start a new conversation to use a different setting.

**Q: Which setting is most realistic?**
A: Medium is generally most natural. Low and High are useful for specific purposes (debates vs. collaboration).

## Keyboard Accessibility

- **Tab** to focus the slider
- **Left Arrow** to decrease politeness (more direct)
- **Right Arrow** to increase politeness (more courteous)
- **Home** to jump to Low
- **End** to jump to High
