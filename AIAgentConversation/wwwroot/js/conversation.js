// Global variables
let conversationId = null;
let isConversationOngoing = false;

// Initialize when page loads
document.addEventListener('DOMContentLoaded', function() {
    const startButton = document.getElementById('start-button');
    startButton.addEventListener('click', startConversation);
    
    // Setup export button dropdown handlers
    const exportButton = document.getElementById('export-button');
    const exportDropdown = document.getElementById('export-dropdown');
    
    exportButton.addEventListener('click', function(e) {
        e.stopPropagation();
        exportDropdown.style.display = exportDropdown.style.display === 'block' ? 'none' : 'block';
    });
    
    // Close dropdown when clicking outside
    document.addEventListener('click', function() {
        exportDropdown.style.display = 'none';
    });
    
    // Prevent dropdown from closing when clicking inside it
    exportDropdown.addEventListener('click', function(e) {
        e.stopPropagation();
    });
    
    // Export format buttons
    document.getElementById('export-json').addEventListener('click', () => {
        exportConversation('json');
        exportDropdown.style.display = 'none';
    });
    
    document.getElementById('export-md').addEventListener('click', () => {
        exportConversation('md');
        exportDropdown.style.display = 'none';
    });
    
    document.getElementById('export-txt').addEventListener('click', () => {
        exportConversation('txt');
        exportDropdown.style.display = 'none';
    });
    
    document.getElementById('export-xml').addEventListener('click', () => {
        exportConversation('xml');
        exportDropdown.style.display = 'none';
    });
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
        
        // Hide export button
        const exportButton = document.getElementById('export-button');
        exportButton.style.display = 'none';
        
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
            // Conversation completed, enable export button
            enableExportButton();
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
            // Conversation completed, enable export button
            enableExportButton();
        }
    } catch (error) {
        displayError(error.message);
    }
}

// Enable export button when conversation is complete
function enableExportButton() {
    const exportButton = document.getElementById('export-button');
    exportButton.style.display = 'inline-block';
    
    // Re-enable start button for new conversation
    const startButton = document.getElementById('start-button');
    startButton.disabled = false;
}

// Export conversation in selected format
async function exportConversation(format) {
    try {
        const response = await fetch(`/api/conversation/${conversationId}/export?format=${format}`, {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json'
            }
        });
        
        if (!response.ok) {
            const errorData = await response.json();
            throw new Error(errorData.message || 'Failed to export conversation');
        }
        
        // Get the content type to determine file extension
        const contentType = response.headers.get('content-type');
        let fileExtension = format.toLowerCase();
        let content;
        
        if (contentType.includes('application/json')) {
            content = await response.text();
            fileExtension = 'json';
        } else if (contentType.includes('application/xml') || contentType.includes('text/xml')) {
            content = await response.text();
            fileExtension = 'xml';
        } else {
            content = await response.text();
            if (format.toLowerCase() === 'md') {
                fileExtension = 'md';
            } else {
                fileExtension = 'txt';
            }
        }
        
        // Create a blob and download
        const blob = new Blob([content], { type: contentType });
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `conversation_${conversationId}.${fileExtension}`;
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        window.URL.revokeObjectURL(url);
        
    } catch (error) {
        displayError(error.message);
    }
}
