// Global variables
let conversationId = null;
let isConversationOngoing = false;

// Initialize when page loads
document.addEventListener('DOMContentLoaded', function() {
    const startButton = document.getElementById('start-button');
    startButton.addEventListener('click', startConversation);
});

// Helper function to create message bubble
function createMessageBubble(message, agentType) {
    const div = document.createElement('div');
    div.className = agentType === 'A1' ? 'message-bubble-a1' : 'message-bubble-a2';
    div.textContent = message;
    return div;
}

// Helper function to create waiting indicator
function createWaitingIndicator(side) {
    const div = document.createElement('div');
    div.className = `waiting-indicator waiting-${side}`;
    div.textContent = '...';
    return div;
}

// Display error message
function displayError(message) {
    console.error('Error:', message);
    const container = document.getElementById('conversation-container');
    const errorDiv = document.createElement('div');
    errorDiv.className = 'error-message';
    errorDiv.textContent = `Error: ${message}`;
    container.appendChild(errorDiv);
    
    // Re-enable start button
    const startButton = document.getElementById('start-button');
    startButton.disabled = false;
}

// Start conversation
async function startConversation() {
    try {
        // Get and validate input values
        const agent1Personality = document.getElementById('agent1-personality').value.trim();
        const agent2Personality = document.getElementById('agent2-personality').value.trim();
        const topic = document.getElementById('topic').value.trim();
        
        if (!agent1Personality || !agent2Personality || !topic) {
            alert('Please fill in all fields');
            return;
        }
        
        // Disable button and clear container
        const startButton = document.getElementById('start-button');
        startButton.disabled = true;
        
        const container = document.getElementById('conversation-container');
        container.innerHTML = '';
        
        // Show initial waiting indicator (left for A1)
        const waitingIndicator = createWaitingIndicator('left');
        container.appendChild(waitingIndicator);
        
        // Call init API endpoint
        const response = await fetch('/api/conversation/init', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                agent1Personality: agent1Personality,
                agent2Personality: agent2Personality,
                topic: topic
            })
        });
        
        if (!response.ok) {
            const errorData = await response.json();
            throw new Error(errorData.message || 'Failed to initialize conversation');
        }
        
        const data = await response.json();
        
        // Store conversationId
        conversationId = data.conversationId;
        isConversationOngoing = data.isOngoing;
        
        // Remove waiting indicator
        container.removeChild(waitingIndicator);
        
        // Append A1 message bubble (left, blue)
        const messageBubble = createMessageBubble(data.message, data.agentType);
        container.appendChild(messageBubble);
        
        // If ongoing, show next waiting indicator and continue
        if (data.isOngoing) {
            const nextWaitingIndicator = createWaitingIndicator('right');
            container.appendChild(nextWaitingIndicator);
            await continueConversation();
        } else {
            await displayMarkdown();
        }
    } catch (error) {
        displayError(error.message);
    }
}

// Continue conversation
async function continueConversation() {
    try {
        const container = document.getElementById('conversation-container');
        
        // Call follow API endpoint
        const response = await fetch('/api/conversation/follow', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                conversationId: conversationId
            })
        });
        
        if (!response.ok) {
            const errorData = await response.json();
            throw new Error(errorData.message || 'Failed to continue conversation');
        }
        
        const data = await response.json();
        
        // Remove waiting indicator (last child)
        const waitingIndicator = container.lastChild;
        if (waitingIndicator && waitingIndicator.className.includes('waiting-indicator')) {
            container.removeChild(waitingIndicator);
        }
        
        // Append message bubble (A1 left blue or A2 right green)
        const messageBubble = createMessageBubble(data.message, data.agentType);
        container.appendChild(messageBubble);
        
        // Check ongoing flag and recurse
        if (data.isOngoing) {
            // Determine next waiting indicator side
            const nextSide = data.agentType === 'A1' ? 'right' : 'left';
            const nextWaitingIndicator = createWaitingIndicator(nextSide);
            container.appendChild(nextWaitingIndicator);
            
            // Recursively call continueConversation
            await continueConversation();
        } else {
            // Conversation completed, display markdown
            await displayMarkdown();
        }
    } catch (error) {
        displayError(error.message);
    }
}

// Display markdown
async function displayMarkdown() {
    try {
        // Call get API endpoint
        const response = await fetch(`/api/conversation/${conversationId}`, {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json'
            }
        });
        
        if (!response.ok) {
            const errorData = await response.json();
            throw new Error(errorData.message || 'Failed to retrieve conversation');
        }
        
        const data = await response.json();
        
        // Hide bubble container, show markdown container
        const conversationContainer = document.getElementById('conversation-container');
        const markdownContainer = document.getElementById('markdown-container');
        
        conversationContainer.style.display = 'none';
        markdownContainer.style.display = 'block';
        
        // Display markdown with line breaks preserved
        // Replace \n with <br> for proper display while keeping text safe
        const safeMarkdown = data.markdown
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/\n/g, '<br>');
        markdownContainer.innerHTML = safeMarkdown;
        
        // Re-enable start button for new conversation
        const startButton = document.getElementById('start-button');
        startButton.disabled = false;
    } catch (error) {
        displayError(error.message);
    }
}
