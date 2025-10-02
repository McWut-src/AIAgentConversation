# Agent Politeness Control Guide

## Overview

The Agent Politeness Control feature allows you to adjust how direct or courteous the AI agents are with each other during conversations. This controls the level of debate intensity - from respectful but still challenging discourse, to direct assertive disagreement. The system now encourages genuine intellectual exchange with real disagreement where warranted, rather than overly agreeable "chatbot" responses.

## Using the Politeness Control

### Location
The politeness slider is located in the conversation setup form, between the "Conversation Topic" field and the "Start Conversation" button.

### Options

The slider has three positions:

#### 1. Low (Direct) - Left Position 🔴
**When to use:** For vigorous debates, critical analysis, or when you want agents to strongly challenge each other
- Agents will be very direct and assertive
- They will actively challenge points and question assumptions
- No polite padding - straight to the point
- Focus on taking strong positions and defending them
- Will point out flaws in reasoning directly

**Example tone:** "I disagree because...", "That overlooks...", "But consider...", "That's a flawed assumption..."

#### 2. Medium (Balanced) - Center Position 🟡 (Default)
**When to use:** For genuine debates and intellectual exchange on most topics
- Engages in real debate with disagreement when warranted
- Questions claims and challenges reasoning
- Presents counterarguments rather than just agreeing
- Pushes back when holding different views
- Natural debate flow like a thoughtful Twitter thread

**Example tone:** "But what about...", "That raises the question...", "I'd argue instead...", "While true, this ignores..."

#### 3. High (Courteous) - Right Position 🟢
**When to use:** For respectful debates or discussions where courtesy matters
- Respectful but still intellectually engaged
- Acknowledges good points while presenting counterpoints
- Disagrees politely but meaningfully
- Balances courtesy with genuine intellectual exchange
- Still debates, just more diplomatically

**Example tone:** "I see your point, but...", "While that's valid, consider...", "That's thoughtful, though I'd note...", "Respectfully, I disagree because..."

## How to Set Politeness Level

1. Enter your agent personalities and conversation topic
2. Move the slider to your desired politeness level:
   - **Drag** the blue slider handle
   - **Click** anywhere on the slider bar
   - **Use keyboard**: Click the slider, then press Left/Right arrow keys
3. Watch the label and description update in real-time
4. Click "Start Conversation" to begin

## Tips

- **For philosophical debates:** Try Low (Direct) - it produces vigorous, challenging arguments
- **For technical discussions:** Medium works best for genuine intellectual exchange with disagreement
- **For nuanced debates:** High (Courteous) provides thoughtful disagreement with diplomatic language
- **Default recommendation:** Medium now provides genuine debate rather than overly agreeable conversation
- **Experiment:** Try the same topic with different politeness levels to see how debate intensity changes

## Technical Details

- The politeness level is stored with each conversation
- Different conversations can have different politeness settings
- The setting affects the AI system prompts that guide agent behavior
- Works in both light and dark mode

## Examples

### Low Politeness Example
**Topic:** "Climate change solutions"
**Agent 1:** "That approach overlooks the economic reality. We need practical solutions, not idealistic ones that ignore market forces."
**Agent 2:** "That's short-sighted thinking. Economics shouldn't trump environmental necessity when we're talking about planetary survival."

### Medium Politeness Example  
**Topic:** "Climate change solutions"
**Agent 1:** "But what about the economic disruption? You're proposing massive changes without addressing how industries will adapt."
**Agent 2:** "That raises the question of what cost we put on environmental collapse. I'd argue the economic cost of inaction far exceeds transition costs."

### High Politeness Example
**Topic:** "Climate change solutions"
**Agent 1:** "I see your point about urgency, but we need to consider the practical economic constraints that governments face."
**Agent 2:** "While that's valid, I'd note that many economists now argue green transitions can drive growth. Respectfully, I disagree that economics should limit action."

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
