// Track game state locally
let gameState = {
    attemptsLeft: 3,
    score: 0,
    isPoweredOn: false,
    difficulty: 1 // Default to Moderate
};

// Initialize UI
document.addEventListener('DOMContentLoaded', function() {
    updateUIFromState();
    
    // Set the default difficulty radio button
    document.getElementById('difficultyModerate').checked = true;
});

function updateUIFromState() {
    document.getElementById('attempts').textContent = gameState.attemptsLeft;
    document.getElementById('score').textContent = gameState.score;
}

async function sendCommand(command) {
    const statusDiv = document.getElementById('status');
    statusDiv.textContent = 'Sending command...';
    statusDiv.className = 'status';
    
    let endpoint = '';
    if(command === 'on') endpoint = '/on';
    else if(command === 'off') endpoint = '/off';
    else if(command === 'lock') endpoint = '/lock';
    
    try {
        const response = await fetch(endpoint, { method: 'POST' });
        if(response.ok) {
            const result = await response.text();
            statusDiv.textContent = result;
            statusDiv.className = 'status success';
            
            // Update local game state based on command
            if(command === 'on') {
                gameState.isPoweredOn = true;
                gameState.attemptsLeft = 3;
                gameState.score = 0;
                document.getElementById('powerButton').textContent = 'POWER OFF';
                document.getElementById('powerButton').onclick = function() { sendCommand('off'); };
            } else if(command === 'off') {
                gameState.isPoweredOn = false;
                document.getElementById('powerButton').textContent = 'POWER ON';
                document.getElementById('powerButton').onclick = function() { sendCommand('on'); };
            } else if(command === 'lock' && gameState.isPoweredOn && gameState.attemptsLeft > 0) {
                gameState.attemptsLeft--;
            }
            
            updateUIFromState();
            refreshDebug();
        } else {
            statusDiv.textContent = 'Error: ' + response.statusText;
            statusDiv.className = 'status error';
        }
    } catch (error) {
        statusDiv.textContent = 'Error: ' + error.message;
        statusDiv.className = 'status error';
    }
}

function clearDebug() {
    fetch('/clear-debug', { method: 'POST' })
        .then(() => refreshDebug());
}

function refreshDebug() {
    fetch('/debug')
        .then(response => response.text())
        .then(data => {
            document.getElementById('debugOutput').innerHTML = data;
            document.getElementById('debugOutput').scrollTop = document.getElementById('debugOutput').scrollHeight;

            // Parse debug data to update game info
            updateGameInfo(data);
        });
}

function updateGameInfo(debugData) {
    // Only look for score in the debug output, attempts are tracked client-side
    
    // Process line by line to find the latest score value
    const lines = debugData.split('\n');
    for (let i = lines.length - 1; i >= 0; i--) {
        const line = lines[i];
        
        // Check for SCORE pattern (both regular and final score)
        if (line.includes('SCORE:') || line.includes('FINAL_SCORE:')) {
            const match = line.match(/(SCORE|FINAL_SCORE):\s*(\d+)/);
            if (match && match[2]) {
                // Update game state
                gameState.score = parseInt(match[2]);
                updateUIFromState();
                console.log('Found score:', match[2]);
                break; // Exit after finding the most recent score
            }
        }
        
        // Check for game reset or completed messages
        if (line.includes('GAME RESET') || line.includes('GAME STARTED')) {
            gameState.attemptsLeft = 3;
            gameState.score = 0;
            updateUIFromState();
            console.log('Game reset detected');
            break;
        }
    }
}

// Auto-refresh debug output every 2 seconds
setInterval(refreshDebug, 2000);

// Function to set the difficulty level
async function setDifficulty(level) {
    const statusDiv = document.getElementById('status');
    statusDiv.textContent = 'Setting difficulty...';
    statusDiv.className = 'status';
    
    try {
        const response = await fetch('/difficulty', { 
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ level: level })
        });
        
        if(response.ok) {
            const result = await response.text();
            statusDiv.textContent = result;
            statusDiv.className = 'status success';
            
            // Update local game state
            gameState.difficulty = level;
            
            // Log to console
            console.log('Difficulty set to:', level);
            
            // Refresh debug output
            refreshDebug();
        } else {
            statusDiv.textContent = 'Error: ' + response.statusText;
            statusDiv.className = 'status error';
        }
    } catch (error) {
        statusDiv.textContent = 'Error: ' + error.message;
        statusDiv.className = 'status error';
    }
}