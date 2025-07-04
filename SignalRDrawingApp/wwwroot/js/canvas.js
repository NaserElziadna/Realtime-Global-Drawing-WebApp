﻿// SignalR connection setup
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/drawingHub")
    .configureLogging(signalR.LogLevel.Information)
    .build();

// Start the SignalR connection
connection.start().then(() => {
    // Request the board state from the server after connecting
    connection.invoke('SendBoard');
}).catch(err => console.error(err.toString()));

var canvas = document.getElementById("whiteboard");
document.oncontextmenu = function () {
    return false;
}
var context = canvas.getContext("2d");

const urlParams = new URLSearchParams(window.location.search);
const roomCode = urlParams.get('code') || '';

var canvasHistory = [];
var strokeHistory = [];
var actionHistory = [];
const darkBackgroundColour = '#15171a';
const lightBackgroundColour = '#fff'
var backgroundColour = darkBackgroundColour;

// Fill Window Width and Height
redraw();
window.addEventListener("resize", (event) => {
    redraw();
});

var drawing = false;
var shiftDown = false;
var ctrlDown = false;
var panning = false;
var rightMouseDown = false;
let penColour = '#f8f9fa';

var scale = 1;

var canvasWidth = canvas.width;
var canvasHeight = canvas.height;


// The scaled width of the screen (ie not the pixels)
function xUnitsScaled() {
    return canvasWidth / scale;
}
// The scaled height of the screen (ie not the pixels)
function yUnitsScaled() {
    return canvasHeight / scale;
}

document.addEventListener('keydown', event => {
    if (event.key == "Shift") {
        if (!shiftDown) canvas.style.cursor = 'grab';
        shiftDown = true;
    }
    if (event.key == "Control") ctrlDown = true;
    if (event.key == "z") {
        if (ctrlDown) undoLast();
    }
})
document.addEventListener('keyup', event => {
    if (event.key == "Shift") {
        canvas.style.cursor = 'crosshair';
        shiftDown = false;
    }
    if (event.key == "Control") ctrlDown = false;
})

canvas.addEventListener('wheel', (event) => {
    // Prevent default scroll behavior when interacting with canvas
    event.preventDefault();
    
    const deltaY = event.deltaY;
    const scaleAmount = -deltaY / 500;
    scale = scale * (1 + scaleAmount);

    // Get mouse position relative to canvas
    const rect = canvas.getBoundingClientRect();
    var distX = (event.clientX - rect.left) / canvasWidth;
    var distY = (event.clientY - rect.top) / canvasHeight;

    const unitsZoomedX = xUnitsScaled() * scaleAmount;
    const unitsZoomedY = yUnitsScaled() * scaleAmount;

    const unitsAddLeft = unitsZoomedX * distX;
    const unitsAddTop = unitsZoomedY * distY;

            offsetX -= unitsAddLeft;
        offsetY -= unitsAddTop;

        redraw();
        updateZoomDisplay();
}, { passive: false })


// Mouse Event Handlers
canvas.addEventListener('mousedown', onMouseDown, false);
canvas.addEventListener('mouseup', onMouseUp, false);
canvas.addEventListener('mouseout', onMouseUp, false);
canvas.addEventListener('mousemove', throttle(onMouseMove, 25), false);

// Touch Event Handlers with improved handling
canvas.addEventListener('touchstart', onTouchStart, { passive: false });
canvas.addEventListener('touchend', onTouchEnd, { passive: false });
canvas.addEventListener('touchcancel', onTouchEnd, { passive: false });
canvas.addEventListener('touchmove', throttle(onTouchMove, 10), { passive: false }); // Reduced throttle for smoother drawing

// Prevent scrolling when touching the canvas
canvas.addEventListener('touchstart', function(e) {
    if (e.target == canvas) {
        e.preventDefault();
    }
}, { passive: false });

function onTouchStart(evt) {
    evt.preventDefault(); // Prevent default touch actions
    
    if (evt.touches.length == 1) {
        // Single touch - drawing mode
        panning = false;
        drawing = true;
        
        // Get position relative to the canvas element
        const rect = canvas.getBoundingClientRect();
        const touch = evt.touches[0];
        const touchX = touch.clientX - rect.left;
        const touchY = touch.clientY - rect.top;
        
        // Start the stroke
        addToStroke((touchX / scale) - offsetX, (touchY / scale) - offsetY, penColour);
        
        lastTouches[0] = evt.touches[0];
    } else if (evt.touches.length >= 2) {
        // Multi-touch - panning/zooming mode
        panning = true;
        drawing = false;
        removeAllDots();

        lastTouches[0] = evt.touches[0];
        lastTouches[1] = evt.touches[1];
    }
}

function onTouchEnd(e) {
    if (drawing) {
        emitStroke();
        strokeHistory.push({ vectors: currentStroke, colour: penColour })
        actionHistory.push(currentStroke)
        currentStroke = [];
        redraw();
    }
    panning = false;
    drawing = false;

}
function onTouchMove(evt) {
    // Update coordinate display for touch
    if (evt.touches.length > 0) {
        updateCoordinateDisplayTouch(evt.touches[0]);
    }

    const touch1X = evt.touches[0].pageX;
    const touch1Y = evt.touches[0].pageY;
    const touch1Xprev = lastTouches[0].pageX;
    const touch1Yprev = lastTouches[0].pageY;

    if (panning) {
        // if panning there is more than 1 touch.
        // get the mid point of the first 2 touches

        const touch2X = evt.touches[1].pageX;
        const touch2Y = evt.touches[1].pageY;
        const midX = (touch1X + touch2X) / 2;
        const midY = (touch1Y + touch2Y) / 2;
        const hypot = Math.sqrt(Math.pow((touch1X - touch2X), 2) + Math.pow((touch1Y - touch2Y), 2));

        const touch2Xprev = lastTouches[1].pageX;
        const touch2Yprev = lastTouches[1].pageY;
        const midXprev = (touch1Xprev + touch2Xprev) / 2;
        const midYprev = (touch1Yprev + touch2Yprev) / 2;
        const hypotPrev = Math.sqrt(Math.pow((touch1Xprev - touch2Xprev), 2) + Math.pow((touch1Yprev - touch2Yprev), 2));

        var zoomAmount = hypot / hypotPrev;
        scale = scale * zoomAmount;
        const scaleAmount = 1 - zoomAmount;

        // calc how many pixels the touches have moved in the x and y direction
        const panX = midX - midXprev;
        const panY = midY - midYprev;
        // scale this movement based on the zoom level
        offsetX += (panX / scale);
        offsetY += (panY / scale);

        // Get the relative position of the middle of the zoom.
        // 0, 0 would be top left. 
        // 0, 1 would be top right etc.
        var zoomRatioX = midX / canvasWidth;
        var zoomRatioY = midY / canvasHeight;

        const unitsZoomedX = xUnitsScaled() * scaleAmount;
        const unitsZoomedY = yUnitsScaled() * scaleAmount;

        const unitsAddLeft = unitsZoomedX * zoomRatioX;
        const unitsAddTop = unitsZoomedY * zoomRatioY;

        offsetX += unitsAddLeft;
        offsetY += unitsAddTop;

        redraw();
        updateZoomDisplay();
    } else if (drawing) {
        if (currentStroke.length == 0) {
            // need to add the first touch
            addToStroke(toTrueX(touch1Xprev), toTrueY(touch1Yprev), penColour);
        }
        drawLine(touch1Xprev, touch1Yprev, touch1X, touch1Y, penColour);
        addToStroke(toTrueX(touch1X), toTrueY(touch1Y), penColour);
    }

    lastTouches[0] = evt.touches[0];
    lastTouches[1] = evt.touches[1];
}

function onMouseDown(evt) {

    if (evt.button == 2) {
        rightMouseDown = true;
    } else {
        rightMouseDown = false;
    }

    // Get position relative to the canvas element, not the document
    const rect = canvas.getBoundingClientRect();
    cursorX = evt.clientX - rect.left;
    cursorY = evt.clientY - rect.top;
    cursorXprev = cursorX;
    cursorYprev = cursorY;

    if (shiftDown || rightMouseDown) {
        canvas.style.cursor = 'grabbing';
        drawing = false;
        panning = true;
        removeAllDots();
    } else {
        panning = false;
        drawing = true;
        addToStroke((cursorX / scale) - offsetX, (cursorY / scale) - offsetY, penColour);
    }
}

function removeAllDots() {
    const dots = document.getElementsByClassName('dot');
    for (let i = 0; i < dots.length; i++) {
        const dot = dots[i];
        dot.remove();
    }
}

function toggleDark() {
    if (backgroundColour == darkBackgroundColour) {
        backgroundColour = lightBackgroundColour;
    } else {
        backgroundColour = darkBackgroundColour;
    }
    setDocumentTitle(backgroundColour);
    connection.invoke('SendBackgroundColour', { backgroundColour });
    redraw();
}

var cursorX = null;
var cursorY = null;
var cursorXprev = null;
var cursorYprev = null;
const lastTouches = [null, null];
var offsetX = 0;
var offsetY = 0;
var currentStroke = [];
function onMouseMove(evt) {
    cursorX = evt.pageX;
    cursorY = evt.pageY;

    // Update coordinate display
    updateCoordinateDisplay(evt);

    if (panning) {
        offsetX += (cursorX - cursorXprev) / scale;
        offsetY += (cursorY - cursorYprev) / scale;
        redraw()
    } else if (drawing) {
        addToStroke(toTrueX(cursorX), toTrueY(cursorY), penColour);
        drawLine(cursorXprev, cursorYprev, cursorX, cursorY, penColour);
    }
    const trueX = (cursorX / scale) - offsetX;
    const trueY = (cursorY / scale) - offsetY;
    onOwnCursorMove(trueX, trueY);

    cursorXprev = cursorX;
    cursorYprev = cursorY;
}

function onMouseUp(e) {
    if (drawing) {
        emitStroke();
        strokeHistory.push({ vectors: currentStroke, colour: penColour })
        actionHistory.push(currentStroke)
        currentStroke = [];
        redraw();
    }
    canvas.style.cursor = 'crosshair';
    rightMouseDown = false;
    panning = false;
    drawing = false;
}
function undoLast() {
    if (actionHistory.length == 0) return;
    const toUndo = actionHistory.pop();
    removeFromHistory(toUndo);
    connection.invoke('SendDelete', { data: toUndo });
}
function removeFromHistory(stroke) {
    for (let i = strokeHistory.length - 1; i >= 0; i--) {
        const historyElement = strokeHistory[i];
        if (strokesEqual(historyElement.vectors, stroke)) {
            strokeHistory.splice(i, 1);
            redraw();
            return;
        }
    }
}

function strokesEqual(strokeAVectors, strokeBVectors) {
    if (strokeAVectors.length != strokeBVectors.length) return false;
    for (let i = 0; i < strokeAVectors.length; i++) {
        const strokeAVector = strokeAVectors[i];
        const strokeBVector = strokeBVectors[i];
        if (!vectorsEqual(strokeAVector, strokeBVector)) return false;
    }
    return true;
}

function vectorsEqual(vectorA, vectorB) {
    if (vectorA.length != vectorB.length) return;
    for (let i = 0; i < vectorA.length; i++) {
        const elementA = vectorA[i];
        const elementB = vectorB[i];
        if (elementA != elementB) return false
    }
    return true;
}

function copyUrl() {
    var dummy = document.createElement('input'),
        text = window.location.href;
    document.body.appendChild(dummy);
    dummy.value = text;
    dummy.select();
    document.execCommand('copy');
    document.body.removeChild(dummy);
}

function setColour(newColour) {
    penColour = newColour;
}

function drawLine(x0, y0, x1, y1, colour) {
    context.beginPath();
    context.moveTo(x0, y0);
    context.lineTo(x1, y1);
    context.strokeStyle = colour;
    context.lineWidth = 2;
    context.stroke();
}

function addToStroke(x0, y0, colour) {
    currentStroke.push([x0, y0]);
}
function drawStroke({ vectors, colour }) {
    context.beginPath();
    context.lineJoin = "round";
    context.lineCap = "round";
    if (!vectors[0]) return;
    context.moveTo(toScreenX(vectors[0][0]), toScreenY(vectors[0][1]));
    for (let i = 0; i < vectors.length; i++) {
        let x0 = toScreenX(vectors[i][0])
        let y0 = toScreenY(vectors[i][1])
        context.lineTo(x0, y0);
    }
    context.strokeStyle = colour;
    context.lineWidth = 2;
    context.stroke();

}
function emitStroke() {
    connection.invoke('SendStroke', { data: { vectors: currentStroke, colour: penColour } });
}
function emitStrokes(strokes) {
    connection.invoke('SendStrokes', { data: strokes });
}

function toScreenX(xTrue) {
    return (xTrue + offsetX) * scale
}
function toScreenY(yTrue) {
    return (yTrue + offsetY) * scale
}
function toTrueX(xScreen) {
    return (xScreen / scale) - offsetX
}
function toTrueY(yScreen) {
    return (yScreen / scale) - offsetY
}

// socket.on('drawing', onDrawingEvent);
connection.on('stroke', onStrokeEvent);
connection.on('strokes', onStrokesEvent);
connection.on('board', function(board) {
    // Expecting board to be an array of strokes
    if (Array.isArray(board)) {
        strokeHistory = board.map(x => x.data ? x.data : x); // handle both {data:...} and direct
        redraw();
    }
});
connection.on('delete', onUndoStrokeEvent);
connection.on('cursormove', onOtherCursorMove);
connection.on('backgroundcolour', onBackgroundColourChange);

function onUndoStrokeEvent(data) {
    removeFromHistory(data.data);
}
function onStrokeEvent(data) {
    strokeHistory.push(data.data);
    drawStroke(data.data);
}
function onStrokesEvent(data) {
    strokeHistory = [...data.data, ...strokeHistory];
    redraw();
}

// another user drawing
function onDrawingEvent(data) {
    canvasHistory.push({ x0: data.x0, y0: data.y0, x1: data.x1, y1: data.y1, colour: data.colour });
    drawLine((data.x0 + offsetX) * scale, (data.y0 + offsetY) * scale, (data.x1 + offsetX) * scale, (data.y1 + offsetY) * scale, data.colour);
}
var connectedUsers = 0;
function updateUserCount(count) {
    const userCountElement = document.getElementById('userCount');
    if (userCountElement) {
        userCountElement.innerHTML = `${count} 👥`;
    }
}

connection.on('updateUserCount', function (count) {
    updateUserCount(count);
});

// Replace the existing onUsersChanged function
function onUsersChanged(data) {
    connectedUsers = data;
    updateUserCount(data);
}

function onDisconnect(data) {
    const dot = document.getElementById(data);
    if (!dot) return;
    dot.remove();
}
function onOtherCursorMove(data) {
    let dot = document.getElementById(data.socket);
    if (!dot) {
        dot = createDot(data.socket);
    }
    // dot.style.left = `${data.x-7}px`
    // dot.style.top = `${data.y-7}px`
    dot.style.left = `${(data.x - 5 + offsetX) * scale}px`;
    dot.style.top = `${(data.y - 5 + offsetY) * scale}px`;
    dot.style.backgroundColor = data.colour;
}
function onBackgroundColourChange(colour) {
    setDocumentTitle(colour);
    backgroundColour = colour;
    redraw();
}
function setDocumentTitle(colour) {
    if (colour == lightBackgroundColour) {
        document.title = 'Omni Canvas';
    } else {
        document.title = 'Omni Canvas';
    }
}
function onOwnCursorMove(x, y) {
    connection.invoke('SendCursorMove', { x, y, colour: penColour });
}
function createDot(socketId) {
    const dot = document.createElement('div')
    dot.className = "dot";
    dot.id = socketId;
    dot.style.position = 'fixed';
    document.body.appendChild(dot);
    return dot;
}
function onHistoryEvent(drawHistory) {
    strokeHistory = drawHistory.history;
    backgroundColour = drawHistory.backgroundColour;
    redraw();
}
function redraw() {
    // Check if we're in a mobile context
    const isMobile = window.innerWidth <= 768;
    
    // Get the current dimensions of the canvas container
    canvasWidth = canvas.width = canvas.offsetWidth;
    canvasHeight = canvas.height = canvas.offsetHeight;
    
    // Fill background
    context.fillStyle = backgroundColour;
    context.fillRect(0, 0, canvas.width, canvas.height);
    
    // Set line properties - thicker lines on mobile for better touch experience
    context.lineJoin = "round";
    context.lineCap = "round";
    context.lineWidth = isMobile ? 5 : 3;

    // Redraw all strokes
    strokeHistory.forEach(stroke => {
        drawStroke(stroke);
    });
    
    // Adjust cursor style based on device
    if (isMobile) {
        canvas.style.cursor = 'default'; // Default cursor on mobile as it's touch-based
    } else {
        canvas.style.cursor = shiftDown ? 'grab' : 'crosshair';
    }
}

// limit the number of events per second
function throttle(callback, delay) {
    var previousCall = new Date().getTime();
    return function () {
        var time = new Date().getTime();

        if ((time - previousCall) >= delay) {
            previousCall = time;
            callback.apply(null, arguments);
        }
    };
}

// https://stackoverflow.com/a/30832210/10159640
function download() {
    let data = JSON.stringify(strokeHistory);
    let filename = 'omnicanvas.json';
    let type = 'json'
    var file = new Blob([data], { type: type });
    if (window.navigator.msSaveOrOpenBlob) // IE10+
        window.navigator.msSaveOrOpenBlob(file, filename);
    else { // Others
        var a = document.createElement("a"),
            url = URL.createObjectURL(file);
        a.href = url;
        a.download = filename;
        document.body.appendChild(a);
        a.click();
        fileSavedModalAlert();
        setTimeout(function () {
            document.body.removeChild(a);
            window.URL.revokeObjectURL(url);
        }, 0);
    }
}

function fileSavedModalAlert() {
    $('#file-saved-modal').modal({ show: true });
}

function load(file) {
    const reader = new FileReader();
    reader.addEventListener('load', (event) => {
        let loadedData = JSON.parse(event.target.result);
        onStrokesEvent({ data: loadedData });
        emitStrokes(loadedData);
    });
    reader.readAsText(file);
}

const dropArea = document.getElementById('whiteboard');

dropArea.addEventListener('dragover', (event) => {
    event.stopPropagation();
    event.preventDefault();
    event.dataTransfer.dropEffect = 'copy';
});

dropArea.addEventListener('drop', (event) => {
    event.stopPropagation();
    event.preventDefault();
    const fileList = event.dataTransfer.files;
    for (let i = 0; i < fileList.length; i++) {
        const file = fileList[i];
        load(file);
    }
});

$('#help-modal').modal({ show: true });

let userName = null;

// In the section where you handle the login modal
$(document).ready(function () {
    $('#loginModal').modal('show');
    $('#userName').focus();
    $('#joinRoom').on('click', function () {
        const name = $('#userName').val().trim();
        if (name) {
            userName = name;
            localStorage.setItem('userName', userName);
            connection.invoke('JoinRoom', userName);
            $('#loginModal').modal('hide');
            
            // Notify chat.js
            window.dispatchEvent(new CustomEvent('userNameSet', { detail: userName }));
        } else {
            $('#userName').addClass('is-invalid');
        }
    });
    $('#userName').on('keypress', function (e) {
        if (e.which === 13) $('#joinRoom').click();
    });
});

// Prevent drawing until user has joined
function canDraw() {
    return !!userName;
}

// Example: block drawing if not joined
canvas.addEventListener('mousedown', function (e) {
    if (!canDraw()) {
        e.preventDefault();
        return false;
    }
}, true);

connection.on('updateUserCount', function (count) {
    document.getElementById('userCount').innerText = count + ' user(s)';
});

// Add this function to get the current view state
function getCurrentViewState() {
    // Calculate the center point of the current view
    const centerX = -offsetX + (canvasWidth / 2) / scale;
    const centerY = -offsetY + (canvasHeight / 2) / scale;
    
    return {
        x: centerX.toFixed(2),
        y: centerY.toFixed(2),
        scale: scale.toFixed(2)
    };
}

// Add this function to animate to a specific position
function animateToPosition(targetX, targetY, targetScale) {
    // Convert string parameters to numbers if needed
    targetX = parseFloat(targetX);
    targetY = parseFloat(targetY);
    targetScale = parseFloat(targetScale);
    
    // Get current position (center of the view)
    const startX = -offsetX + (canvasWidth / 2) / scale;
    const startY = -offsetY + (canvasHeight / 2) / scale;
    const startScale = scale;
    
    // Animation parameters
    const duration = 1000; // ms
    const startTime = Date.now();
    
    function animate() {
        const elapsed = Date.now() - startTime;
        const progress = Math.min(elapsed / duration, 1);
        
        // Ease function (cubic ease in-out)
        const easeProgress = progress < 0.5 
            ? 4 * progress * progress * progress 
            : 1 - Math.pow(-2 * progress + 2, 3) / 2;
        
        // Interpolate values
        const currentScale = startScale + (targetScale - startScale) * easeProgress;
        const currentX = startX + (targetX - startX) * easeProgress;
        const currentY = startY + (targetY - startY) * easeProgress;
        
        // Update the view
        scale = currentScale;
        offsetX = -(currentX - (canvasWidth / 2) / currentScale);
        offsetY = -(currentY - (canvasHeight / 2) / currentScale);
        
        redraw();
        
        if (progress < 1) {
            requestAnimationFrame(animate);
        }
    }
    
    animate();
}

// Make these functions available globally
window.getCurrentViewState = getCurrentViewState;
window.animateToPosition = animateToPosition;
connection.on('cursormove', onOtherCursorMove);
connection.on('backgroundcolour', onBackgroundColourChange);

function onUndoStrokeEvent(data) {
    removeFromHistory(data.data);
}
function onStrokeEvent(data) {
    strokeHistory.push(data.data);
    drawStroke(data.data);
}
function onStrokesEvent(data) {
    strokeHistory = [...data.data, ...strokeHistory];
    redraw();
}

// another user drawing
function onDrawingEvent(data) {
    canvasHistory.push({ x0: data.x0, y0: data.y0, x1: data.x1, y1: data.y1, colour: data.colour });
    drawLine((data.x0 + offsetX) * scale, (data.y0 + offsetY) * scale, (data.x1 + offsetX) * scale, (data.y1 + offsetY) * scale, data.colour);
}
var connectedUsers = 0;
function updateUserCount(count) {
    const userCountElement = document.getElementById('userCount');
    if (userCountElement) {
        userCountElement.innerHTML = `${count} 👥`;
    }
}

connection.on('updateUserCount', function (count) {
    updateUserCount(count);
});

// Coordinate display functions
function updateCoordinateDisplay(evt) {
    const rect = canvas.getBoundingClientRect();
    const mouseX = Math.round(evt.clientX - rect.left);
    const mouseY = Math.round(evt.clientY - rect.top);
    
    const canvasX = Math.round(toTrueX(evt.pageX));
    const canvasY = Math.round(toTrueY(evt.pageY));
    
    const mouseCoords = document.getElementById('mouseCoords');
    const canvasCoords = document.getElementById('canvasCoords');
    
    if (mouseCoords) {
        mouseCoords.textContent = `${mouseX}, ${mouseY}`;
    }
    if (canvasCoords) {
        canvasCoords.textContent = `${canvasX}, ${canvasY}`;
    }
}

function updateCoordinateDisplayTouch(touch) {
    const rect = canvas.getBoundingClientRect();
    const touchX = Math.round(touch.clientX - rect.left);
    const touchY = Math.round(touch.clientY - rect.top);
    
    const canvasX = Math.round(toTrueX(touch.pageX));
    const canvasY = Math.round(toTrueY(touch.pageY));
    
    const mouseCoords = document.getElementById('mouseCoords');
    const canvasCoords = document.getElementById('canvasCoords');
    
    if (mouseCoords) {
        mouseCoords.textContent = `${touchX}, ${touchY}`;
    }
    if (canvasCoords) {
        canvasCoords.textContent = `${canvasX}, ${canvasY}`;
    }
}

function updateZoomDisplay() {
    const zoomLevel = document.getElementById('zoomLevel');
    if (zoomLevel) {
        const zoomPercent = Math.round(scale * 100);
        zoomLevel.textContent = `${zoomPercent}%`;
    }
}

// Initialize zoom display on page load
window.addEventListener('load', function() {
    updateZoomDisplay();
});
