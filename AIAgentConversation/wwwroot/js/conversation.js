// Global variables
let conversationId = null;
let isConversationOngoing = false;

// Initialize when page loads
document.addEventListener('DOMContentLoaded', function() {
    const startButton = document.getElementById('start-button');
    startButton.addEventListener('click', startConversation);
    
    // Add keyboard shortcut - Enter to start conversation
    const inputFields = [
        document.getElementById('agent1-personality'),
        document.getElementById('agent2-personality'),
        document.getElementById('topic')
    ];
    
    inputFields.forEach(field => {
        field.addEventListener('keypress', function(e) {
            if (e.key === 'Enter' && !startButton.disabled) {
                startConversation();
            }
        });
        
        // Add real-time validation feedback
        field.addEventListener('input', function() {
            validateInputField(field);
            updateStartButtonState();
        });
    });
    
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
    
    // Set up rotating placeholders
    setupRotatingPlaceholders();
    
    // Setup theme toggle
    setupThemeToggle();
    
    // Setup politeness slider
    setupPolitenessSlider();
    
    // Setup pre-defined options
    setupPredefinedOptions();
    
    // Setup random button
    setupRandomButton();
    
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

// Validate input field
function validateInputField(field) {
    const value = field.value.trim();
    const minLength = 3;
    
    if (value.length === 0) {
        field.style.borderColor = '#ced4da';
        field.style.backgroundColor = '';
    } else if (value.length < minLength) {
        field.style.borderColor = '#dc3545';
        field.style.backgroundColor = '#fff5f5';
    } else {
        field.style.borderColor = '#28a745';
        field.style.backgroundColor = '#f0fff4';
    }
}

// Update start button state based on input validity
function updateStartButtonState() {
    const agent1 = document.getElementById('agent1-personality').value.trim();
    const agent2 = document.getElementById('agent2-personality').value.trim();
    const topic = document.getElementById('topic').value.trim();
    const startButton = document.getElementById('start-button');
    
    const allValid = agent1.length >= 3 && agent2.length >= 3 && topic.length >= 3;
    
    if (!isConversationOngoing) {
        startButton.style.opacity = allValid ? '1' : '0.6';
    }
}

// Display error message
function displayError(message) {
    console.error('Error:', message);
    const container = document.getElementById('conversation-container');
    const errorDiv = document.createElement('div');
    errorDiv.className = 'error-message';
    errorDiv.innerHTML = `<strong>Error:</strong> ${message}<br><small>Please refresh the page to try again.</small>`;
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
        const politenessSlider = document.getElementById('politeness-slider');
        const politenessLevel = ['low', 'medium', 'high'][parseInt(politenessSlider.value)];
        
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
        // Ensure container is always visible
        container.style.display = 'block';
        
        // Add progress indicator
        addProgressIndicator(container, 1);

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
                topic: topic,
                politenessLevel: politenessLevel
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
        
        // Update progress indicator
        updateProgressIndicator(container, data.totalMessages);
        
        // Check ongoing flag and recurse
        if (data.isOngoing) {
            // Determine next waiting indicator side
            const nextSide = data.agentType === 'A1' ? 'right' : 'left';
            const nextWaitingIndicator = createWaitingIndicator(nextSide);
            container.appendChild(nextWaitingIndicator);
            
            // Recursively call continueConversation
            await continueConversation();
        } else {
            // Conversation completed - keep bubbles visible and enable export button
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
    
    // Keep conversation container visible with all bubbles
    const container = document.getElementById('conversation-container');
    container.style.display = 'block';
    
    // Add completion indicator
    addCompletionIndicator();
    
    // Add copy to clipboard button
    addCopyButton();
}

// Add completion indicator
function addCompletionIndicator() {
    const container = document.getElementById('conversation-container');
    const completionDiv = document.createElement('div');
    completionDiv.className = 'completion-indicator';
    completionDiv.innerHTML = '✓ Conversation completed successfully';
    container.appendChild(completionDiv);
}

// Add copy to clipboard button
function addCopyButton() {
    const container = document.getElementById('conversation-container');
    const copyButton = document.createElement('button');
    copyButton.className = 'copy-button';
    copyButton.innerHTML = '📋 Copy Conversation';
    copyButton.onclick = copyConversationToClipboard;
    container.appendChild(copyButton);
}

// Copy conversation to clipboard
function copyConversationToClipboard() {
    const container = document.getElementById('conversation-container');
    const bubbles = container.querySelectorAll('.message-bubble-a1, .message-bubble-a2');
    
    let text = 'AI Agent Conversation\n';
    text += '======================\n\n';
    
    bubbles.forEach((bubble, index) => {
        const agentType = bubble.className.includes('a1') ? 'Agent 1' : 'Agent 2';
        text += `${agentType}: ${bubble.textContent}\n\n`;
    });
    
    navigator.clipboard.writeText(text).then(() => {
        // Show success feedback
        const copyButton = container.querySelector('.copy-button');
        const originalText = copyButton.innerHTML;
        copyButton.innerHTML = '✓ Copied!';
        copyButton.style.backgroundColor = '#28a745';
        
        setTimeout(() => {
            copyButton.innerHTML = originalText;
            copyButton.style.backgroundColor = '';
        }, 2000);
    }).catch(err => {
        console.error('Failed to copy:', err);
        alert('Failed to copy to clipboard');
    });
}

// Add progress indicator
function addProgressIndicator(container, currentMessage) {
    const progressDiv = document.createElement('div');
    progressDiv.className = 'progress-indicator';
    progressDiv.id = 'progress-indicator';
    progressDiv.innerHTML = `<span class="progress-text">Message ${currentMessage} of 6</span>
        <div class="progress-bar-container">
            <div class="progress-bar" style="width: ${(currentMessage / 6) * 100}%"></div>
        </div>`;
    container.insertBefore(progressDiv, container.firstChild);
}

// Update progress indicator
function updateProgressIndicator(container, currentMessage) {
    const progressIndicator = document.getElementById('progress-indicator');
    if (progressIndicator) {
        progressIndicator.querySelector('.progress-text').textContent = `Message ${currentMessage} of 6`;
        progressIndicator.querySelector('.progress-bar').style.width = `${(currentMessage / 6) * 100}%`;
    }
}

// Pre-defined personalities and topics for datalist and random selection
const predefinedOptions = {
    agent1: [
        'Logical analyst who values data and evidence',
        'Professional debater who uses structured arguments',
        'Skeptical scientist who questions everything',
        'Pragmatic problem-solver focused on solutions',
        'Critical thinker who challenges assumptions',
        'Detail-oriented researcher who cites sources',
        'Strategic thinker who considers long-term implications',
        'Analytical engineer who focuses on technical accuracy',
        'Evidence-based investigator who questions claims',
        'Systematic methodologist who follows structured reasoning'
    ],
    agent2: [
        'Creative thinker who uses metaphors and storytelling',
        'Enthusiastic optimist who sees opportunities',
        'Philosophical poet who explores deep meanings',
        'Intuitive dreamer with big-picture thinking',
        'Empathetic communicator who values emotions',
        'Artistic visionary who thinks outside the box',
        'Passionate advocate who champions causes',
        'Imaginative storyteller who makes connections',
        'Holistic observer who sees patterns everywhere',
        'Inspirational speaker who motivates others'
    ],
    topic: [
        'The future of artificial intelligence',
        'Climate change and sustainability',
        'The meaning of consciousness',
        'Work-life balance in modern society',
        'The impact of social media on relationships',
        'The ethics of genetic engineering',
        'Universal basic income and automation',
        'Space exploration and colonization',
        'The role of education in the 21st century',
        'Privacy in the digital age',
        'The future of democracy',
        'Renewable energy and technology',
        'Mental health awareness and support',
        'Cultural diversity and globalization',
        'The impact of virtual reality on society'
    ]
};

// Setup rotating placeholders for better UX
function setupRotatingPlaceholders() {
    const placeholderExamples = {
        agent1: predefinedOptions.agent1.slice(0, 5),
        agent2: predefinedOptions.agent2.slice(0, 5),
        topic: predefinedOptions.topic.slice(0, 5)
    };
    
    let currentIndex = Math.floor(Math.random() * placeholderExamples.agent1.length);
    
    function rotatePlaceholders() {
        document.getElementById('agent1-personality').placeholder = 
            `e.g., ${placeholderExamples.agent1[currentIndex]}`;
        document.getElementById('agent2-personality').placeholder = 
            `e.g., ${placeholderExamples.agent2[currentIndex]}`;
        document.getElementById('topic').placeholder = 
            `e.g., ${placeholderExamples.topic[currentIndex]}`;
        
        currentIndex = (currentIndex + 1) % placeholderExamples.agent1.length;
    }
    
    // Set initial placeholders
    rotatePlaceholders();
    
    // Rotate every 5 seconds
    setInterval(rotatePlaceholders, 5000);
}

// Setup theme toggle for dark mode
function setupThemeToggle() {
    const themeToggle = document.getElementById('theme-toggle');
    
    // Check for saved theme preference or default to light mode
    const currentTheme = localStorage.getItem('theme') || 'light';
    if (currentTheme === 'dark') {
        document.body.classList.add('dark-mode');
        themeToggle.textContent = '☀️';
    }
    
    themeToggle.addEventListener('click', function() {
        document.body.classList.toggle('dark-mode');
        
        // Update button icon and save preference
        if (document.body.classList.contains('dark-mode')) {
            themeToggle.textContent = '☀️';
            localStorage.setItem('theme', 'dark');
        } else {
            themeToggle.textContent = '🌙';
            localStorage.setItem('theme', 'light');
        }
    });
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

// Setup politeness slider
function setupPolitenessSlider() {
    const slider = document.getElementById('politeness-slider');
    const valueDisplay = document.getElementById('politeness-value');
    const description = document.getElementById('politeness-description');
    
    const politenessLevels = {
        0: {
            label: 'Low (Direct)',
            description: 'Agents will be direct and assertive, challenging each other without excessive politeness'
        },
        1: {
            label: 'Medium',
            description: 'Balanced tone - neither overly polite nor confrontational'
        },
        2: {
            label: 'High (Courteous)',
            description: 'Agents will be very respectful and courteous, acknowledging points graciously'
        }
    };
    
    function updatePolitenessDisplay() {
        const level = parseInt(slider.value);
        valueDisplay.textContent = politenessLevels[level].label;
        description.textContent = politenessLevels[level].description;
    }
    
    slider.addEventListener('input', updatePolitenessDisplay);
    
    // Initialize display
    updatePolitenessDisplay();
}

// Populate datalists with pre-defined options
function setupPredefinedOptions() {
    // Populate agent1 datalist
    const agent1Datalist = document.getElementById('agent1-suggestions');
    predefinedOptions.agent1.forEach(option => {
        const optionElement = document.createElement('option');
        optionElement.value = option;
        agent1Datalist.appendChild(optionElement);
    });
    
    // Populate agent2 datalist
    const agent2Datalist = document.getElementById('agent2-suggestions');
    predefinedOptions.agent2.forEach(option => {
        const optionElement = document.createElement('option');
        optionElement.value = option;
        agent2Datalist.appendChild(optionElement);
    });
    
    // Populate topic datalist
    const topicDatalist = document.getElementById('topic-suggestions');
    predefinedOptions.topic.forEach(option => {
        const optionElement = document.createElement('option');
        optionElement.value = option;
        topicDatalist.appendChild(optionElement);
    });
}

// Setup random button to randomize all inputs
function setupRandomButton() {
    const randomButton = document.getElementById('random-button');
    
    randomButton.addEventListener('click', function() {
        // Don't allow randomization during an active conversation
        if (isConversationOngoing) {
            return;
        }
        
        // Randomly select from pre-defined options
        const randomAgent1 = predefinedOptions.agent1[Math.floor(Math.random() * predefinedOptions.agent1.length)];
        const randomAgent2 = predefinedOptions.agent2[Math.floor(Math.random() * predefinedOptions.agent2.length)];
        const randomTopic = predefinedOptions.topic[Math.floor(Math.random() * predefinedOptions.topic.length)];
        const randomPoliteness = Math.floor(Math.random() * 3); // 0, 1, or 2
        
        // Set the values
        const agent1Input = document.getElementById('agent1-personality');
        const agent2Input = document.getElementById('agent2-personality');
        const topicInput = document.getElementById('topic');
        const politenessSlider = document.getElementById('politeness-slider');
        
        agent1Input.value = randomAgent1;
        agent2Input.value = randomAgent2;
        topicInput.value = randomTopic;
        politenessSlider.value = randomPoliteness;
        
        // Trigger validation and update displays
        validateInputField(agent1Input);
        validateInputField(agent2Input);
        validateInputField(topicInput);
        updateStartButtonState();
        
        // Update politeness display
        const valueDisplay = document.getElementById('politeness-value');
        const description = document.getElementById('politeness-description');
        const politenessLevels = {
            0: {
                label: 'Low (Direct)',
                description: 'Agents will be direct and assertive, challenging each other without excessive politeness'
            },
            1: {
                label: 'Medium',
                description: 'Balanced tone - neither overly polite nor confrontational'
            },
            2: {
                label: 'High (Courteous)',
                description: 'Agents will be very respectful and courteous, acknowledging points graciously'
            }
        };
        valueDisplay.textContent = politenessLevels[randomPoliteness].label;
        description.textContent = politenessLevels[randomPoliteness].description;
        
        // Add a visual feedback - spin the dice
        randomButton.style.transform = 'rotate(360deg)';
        setTimeout(() => {
            randomButton.style.transform = '';
        }, 300);
    });
}
