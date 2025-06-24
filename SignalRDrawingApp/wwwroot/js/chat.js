// Global Chat Logic (separate from drawing logic)
let chatUserName = null;

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
    msgDiv.innerHTML = `<span style='color:#6cf;font-weight:bold;'>${user}:</span> <span>${message}</span>`;
    document.getElementById('chat-messages').appendChild(msgDiv);
    document.getElementById('chat-messages').scrollTop = document.getElementById('chat-messages').scrollHeight;
}

// Receive chat message
chatConnection.on('ReceiveMessage', (user, message) => {
    appendMessage(user, message);
});

chatConnection.start().then(() => {
    chatUserName = getUserName();
    if (chatUserName) {
        chatConnection.invoke('JoinChat', chatUserName);
    }
}).catch(err => console.error(err.toString()));

// Send message
document.addEventListener('DOMContentLoaded', function () {
    const chatForm = document.getElementById('chat-form');
    if (chatForm) {
        chatForm.addEventListener('submit', function (e) {
            e.preventDefault();
            const input = document.getElementById('chat-input');
            const msg = input.value.trim();
            if (msg.length > 0 && chatUserName) {
                chatConnection.invoke('SendMessage', chatUserName, msg);
                input.value = '';
            }
        });
    }
});

// Optional: auto-join chat after login in canvas.js
window.addEventListener('userNameSet', function(e) {
    chatUserName = e.detail;
    chatConnection.invoke('JoinChat', chatUserName);
});
