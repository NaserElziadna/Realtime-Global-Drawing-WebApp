// Global Chat Logic (separate from drawing logic)
let chatUserName = null;
let chatVisible = true; // Track chat visibility state

const chatConnection = new signalR.HubConnectionBuilder()
    .withUrl("/chatHub")
    .configureLogging(signalR.LogLevel.Information)
    .build();

function getUserName() {
    if (window.userName && window.userName.length > 0) return window.userName;
    return localStorage.getItem('userName') || '';
}

// Add a function to share the current position
function shareCurrentPosition() {
    if (!chatUserName) return;
    
    const viewState = window.getCurrentViewState();
    const positionRef = `[position:${viewState.x},${viewState.y},${viewState.scale}]`;
    
    const input = document.getElementById('chat-input');
    input.value += positionRef;
    input.focus();
}

// Modify the appendMessage function to make position references clickable
function appendMessage(user, message) {
    const msgDiv = document.createElement('div');
    msgDiv.className = 'chat-message';
    
    // Parse position references in the format [position:x,y,scale]
    const positionRegex = /\[position:(-?\d+\.?\d*),(-?\d+\.?\d*),(\d+\.?\d*)\]/g;
    
    // Replace position references with clickable links
    const formattedMessage = message.replace(positionRegex, (match, x, y, scale) => {
        return `<a href="#" class="position-link" data-x="${x}" data-y="${y}" data-scale="${scale}">📍(${x}, ${y})</a>`;
    });
    
    msgDiv.innerHTML = `<span style='color:#6cf;font-weight:bold;'>${user}:</span> <span>${formattedMessage}</span>`;
    document.getElementById('chat-messages').appendChild(msgDiv);
    document.getElementById('chat-messages').scrollTop = document.getElementById('chat-messages').scrollHeight;
    
    // Add click handlers to position links
    const positionLinks = msgDiv.querySelectorAll('.position-link');
    positionLinks.forEach(link => {
        link.addEventListener('click', (e) => {
            e.preventDefault();
            const x = link.getAttribute('data-x');
            const y = link.getAttribute('data-y');
            const scale = link.getAttribute('data-scale');
            window.animateToPosition(x, y, scale);
        });
    });
}

// Toggle chat sidebar visibility
function toggleChat() {
    console.log("Toggle chat called");
    
    const appContainer = document.querySelector('.app-container');
    const chatToggleIcon = document.getElementById('chat-toggle-icon');
    const chatToggleBtn = document.getElementById('chat-toggle');
    const chatSidebar = document.getElementById('global-chat');
    const isMobile = window.innerWidth <= 768;
    
    if (!appContainer || !chatToggleIcon) {
        console.error("Required elements not found");
        return;
    }
    
    console.log("Current chat visible state:", chatVisible);
    
    if (chatVisible) {
        // Hide chat
        appContainer.classList.add('chat-hidden');
        chatToggleIcon.textContent = isMobile ? '▲' : '❯';
        console.log("Hiding chat");
    } else {
        // Show chat
        appContainer.classList.remove('chat-hidden');
        chatToggleIcon.textContent = isMobile ? '▼' : '❮';
        console.log("Showing chat");
    }
    
    chatVisible = !chatVisible;
    console.log("New chat visible state:", chatVisible);
    
    // Ensure toggle button is visible
    if (chatToggleBtn) {
        chatToggleBtn.style.display = 'flex';
        chatToggleBtn.style.visibility = 'visible';
        chatToggleBtn.style.opacity = '1';
    }
    
    // Ensure chat sidebar has proper styling
    if (chatSidebar) {
        chatSidebar.style.display = 'flex';
    }
    
    // Force a redraw of the canvas to ensure it fills the available space
    setTimeout(() => {
        // Trigger window resize event to make canvas redraw
        window.dispatchEvent(new Event('resize'));
        
        // Also explicitly call the redraw function from canvas.js if available
        if (typeof redraw === 'function') {
            redraw();
        }
    }, 300); // Wait for the CSS transition to complete
}

// Update toggle icon on resize
window.addEventListener('resize', function() {
    if (!chatVisible) return; // Only update if chat is visible
    
    const chatToggleIcon = document.getElementById('chat-toggle-icon');
    const isMobile = window.innerWidth <= 768;
    
    if (chatToggleIcon) {
        chatToggleIcon.textContent = isMobile ? '▼' : '❮';
    }
});

// Receive chat message
chatConnection.on('ReceiveMessage', (user, message) => {
    appendMessage(user, message);
});

// Handle user count updates
chatConnection.on('updateUserCount', function(count) {
    const chatUserCountElement = document.getElementById('chatUserCount');
    if (chatUserCountElement) {
        chatUserCountElement.innerHTML = `${count} 👥`;
    }
});

// Handle user list updates
chatConnection.on('UpdateUserList', function(users) {
    console.log('Connected users:', users);
});

chatConnection.start().then(() => {
    chatUserName = getUserName();
    if (chatUserName) {
        chatConnection.invoke('JoinChat', chatUserName);
    }
}).catch(err => console.error(err.toString()));

// Send message function
function sendChatMessage() {
    const input = document.getElementById('chat-input');
    const msg = input.value.trim();
    if (msg.length > 0 && chatUserName) {
        chatConnection.invoke('SendMessage', chatUserName, msg)
            .then(() => {
                console.log('Message sent successfully');
            })
            .catch(err => {
                console.error('Error sending message:', err);
            });
        input.value = '';
    }
    return false; // Prevent form submission
}

// Set up event listeners when DOM is loaded
document.addEventListener('DOMContentLoaded', function () {
    console.log('Setting up chat event listeners');
    
    // Handle send button click
    const sendButton = document.getElementById('send-message');
    if (sendButton) {
        console.log('Send button found, adding click listener');
        sendButton.addEventListener('click', function() {
            console.log('Send button clicked');
            sendChatMessage();
        });
    } else {
        console.log('Send button not found');
    }
    
    // Handle Enter key in input field
    const chatInput = document.getElementById('chat-input');
    if (chatInput) {
        chatInput.addEventListener('keypress', function(e) {
            if (e.key === 'Enter') {
                e.preventDefault();
                sendChatMessage();
            }
        });
    }
    
    // For backward compatibility, also handle form submit
    const chatForm = document.getElementById('chat-form');
    if (chatForm) {
        chatForm.addEventListener('submit', function(e) {
            e.preventDefault();
            sendChatMessage();
            return false;
        });
    }
    
    // Set up chat toggle button
    const chatToggleBtn = document.getElementById('chat-toggle');
    if (chatToggleBtn) {
        chatToggleBtn.addEventListener('click', toggleChat);
        
        // Ensure the toggle button is visible
        chatToggleBtn.style.display = 'flex';
        
        // Set the correct icon based on screen size and visibility state
        const isMobile = window.innerWidth <= 768;
        const chatToggleIcon = document.getElementById('chat-toggle-icon');
        if (chatToggleIcon) {
            chatToggleIcon.textContent = isMobile ? '▲' : '❮';
        }
    }
    
    // Set up mobile chat toggle button
    const mobileChatToggleBtn = document.getElementById('mobile-chat-toggle');
    if (mobileChatToggleBtn) {
        mobileChatToggleBtn.addEventListener('click', toggleChat);
    }
    
    // Set up share position button
    const sharePositionBtn = document.getElementById('share-position');
    if (sharePositionBtn) {
        sharePositionBtn.addEventListener('click', shareCurrentPosition);
    }
    
    // Fix for mobile devices - ensure chat toggle is visible
    setTimeout(function() {
        const chatToggleBtn = document.getElementById('chat-toggle');
        if (chatToggleBtn) {
            chatToggleBtn.style.display = 'flex';
        }
    }, 1000);
});

// Ensure chat and toggle are visible on page load
document.addEventListener('DOMContentLoaded', function() {
    // Show chat toggle immediately
    const chatToggleBtn = document.getElementById('chat-toggle');
    if (chatToggleBtn) {
        chatToggleBtn.style.display = 'flex';
        chatToggleBtn.style.visibility = 'visible';
        chatToggleBtn.style.opacity = '1';
    }
    
    // Make sure chat is properly initialized
    const appContainer = document.querySelector('.app-container');
    const isMobile = window.innerWidth <= 768;
    
    // Set initial chat state
    if (appContainer) {
        if (isMobile) {
            // Start with chat hidden on mobile
            appContainer.classList.add('chat-hidden');
            if (chatToggleBtn && document.getElementById('chat-toggle-icon')) {
                document.getElementById('chat-toggle-icon').textContent = '▲';
            }
        } else {
            // Start with chat visible on desktop
            appContainer.classList.remove('chat-hidden');
            if (chatToggleBtn && document.getElementById('chat-toggle-icon')) {
                document.getElementById('chat-toggle-icon').textContent = '❮';
            }
        }
    }
    
    // Force redraw after a short delay
    setTimeout(function() {
        window.dispatchEvent(new Event('resize'));
    }, 500);
});
