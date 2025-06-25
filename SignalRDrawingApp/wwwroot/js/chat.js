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

function appendMessage(user, message) {
    const msgDiv = document.createElement('div');
    msgDiv.className = 'chat-message';
    msgDiv.innerHTML = `<span style='color:#6cf;font-weight:bold;'>${user}:</span> <span>${message}</span>`;
    document.getElementById('chat-messages').appendChild(msgDiv);
    document.getElementById('chat-messages').scrollTop = document.getElementById('chat-messages').scrollHeight;
}

// Toggle chat sidebar visibility
function toggleChat() {
    const appContainer = document.querySelector('.app-container');
    const chatToggleIcon = document.getElementById('chat-toggle-icon');
    
    if (chatVisible) {
        // Hide chat
        appContainer.classList.add('chat-hidden');
        chatToggleIcon.textContent = '❯';
    } else {
        // Show chat
        appContainer.classList.remove('chat-hidden');
        chatToggleIcon.textContent = '❮';
    }
    
    chatVisible = !chatVisible;
}

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
    }
});

// Auto-join chat after login in canvas.js
window.addEventListener('userNameSet', function(e) {
    chatUserName = e.detail;
    chatConnection.invoke('JoinChat', chatUserName);
});
