using System.IO.Ports;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Serial port configuration
SerialPort? serialPort = null;
string portName = "/dev/tty.usbmodem101"; 
int baudRate = 9600;
StringBuilder debugOutput = new StringBuilder();

// Game debug data
string gameStatus = "STANDBY";
string connectionStatus = "Disconnected";
int targetAngle = 0;
int currentAngle = 0;
int attemptsRemaining = 3;
int score = 0;
int gameTime = 0;
int currentDifficulty = 1; // 0=Easy, 1=Normal, 2=Expert

// Initialize serial port
try
{
    serialPort = new SerialPort(portName, baudRate);
    serialPort.DtrEnable = true;
    serialPort.RtsEnable = true;
    serialPort.ReadTimeout = 1000;
    serialPort.WriteTimeout = 1000;
    
    serialPort.DataReceived += (sender, e) =>
    {/* Lines 34-73 omitted */};
    
    serialPort.Open();
    connectionStatus = "Connected";
    Console.WriteLine($"Connected to Arduino on {portName} at {baudRate} baud");
}
catch (Exception ex)
{
    connectionStatus = $"Failed: {ex.Message}";
    Console.WriteLine($"Failed to connect to Arduino: {ex.Message}");
}

// Serve the main page
app.MapGet("/", () =>
{
    // Create HTML with traditional string concatenation to avoid interpolation issues
    string html = "<!DOCTYPE html>\n<html>\n<head>\n";
    html += "    <title>Safe-Cracker Control Panel</title>\n";
    html += "    <style>\n";
    html += "        body {\n";
    html += "            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;\n";
    html += "            background-color: #1a1a1a;\n";
    html += "            color: #fff;\n";
    html += "            margin: 0;\n";
    html += "            padding: 20px;\n";
    html += "        }\n";
    html += "        .container {\n";
    html += "            max-width: 800px;\n";
    html += "            margin: 0 auto;\n";
    html += "        }\n";
    html += "        .header {\n";
    html += "            background-color: #2c3e50;\n";
    html += "            padding: 20px;\n";
    html += "            border-radius: 10px;\n";
    html += "            text-align: center;\n";
    html += "            margin-bottom: 20px;\n";
    html += "        }\n";
    html += "        h1 {\n";
    html += "            margin: 0;\n";
    html += "            font-size: 2em;\n";
    html += "        }\n";
    html += "        .status-bar {\n";
    html += "            background-color: #2c2c2c;\n";
    html += "            padding: 15px;\n";
    html += "            border-radius: 5px;\n";
    html += "            margin-bottom: 20px;\n";
    html += "            display: flex;\n";
    html += "            justify-content: space-around;\n";
    html += "        }\n";
    html += "        .status-item {\n";
    html += "            padding: 10px;\n";
    html += "            text-align: center;\n";
    html += "        }\n";
    html += "        .status-label {\n";
    html += "            color: #888;\n";
    html += "            font-size: 0.9em;\n";
    html += "            margin-bottom: 5px;\n";
    html += "        }\n";
    html += "        .status-value {\n";
    html += "            font-size: 1.5em;\n";
    html += "            font-weight: bold;\n";
    html += "            color: #4ecdc4;\n";
    html += "        }\n";
    html += "        .panel {\n";
    html += "            background-color: #2c2c2c;\n";
    html += "            padding: 20px;\n";
    html += "            border-radius: 10px;\n";
    html += "            margin-bottom: 20px;\n";
    html += "        }\n";
    html += "        .instructions {\n";
    html += "            background-color: #34495e;\n";
    html += "            padding: 20px;\n";
    html += "            border-radius: 10px;\n";
    html += "            margin-bottom: 20px;\n";
    html += "            text-align: center;\n";
    html += "        }\n";
    html += "        .controls {\n";
    html += "            display: flex;\n";
    html += "            flex-direction: column;\n";
    html += "            gap: 15px;\n";
    html += "        }\n";
    html += "        .control-group {\n";
    html += "            background-color: #1a1a1a;\n";
    html += "            padding: 15px;\n";
    html += "            border-radius: 5px;\n";
    html += "            margin-bottom: 10px;\n";
    html += "        }\n";
    html += "        button {\n";
    html += "            padding: 15px 30px;\n";
    html += "            font-size: 16px;\n";
    html += "            border: none;\n";
    html += "            border-radius: 5px;\n";
    html += "            cursor: pointer;\n";
    html += "            font-weight: bold;\n";
    html += "            width: 100%;\n";
    html += "            margin: 5px 0;\n";
    html += "        }\n";
    html += "        .power-on {\n";
    html += "            background-color: #27ae60;\n";
    html += "            color: white;\n";
    html += "        }\n";
    html += "        .power-off {\n";
    html += "            background-color: #7f8c8d;\n";
    html += "            color: white;\n";
    html += "        }\n";
    html += "        .lock-in {\n";
    html += "            background-color: #e74c3c;\n";
    html += "            color: white;\n";
    html += "            font-size: 24px;\n";
    html += "            padding: 30px;\n";
    html += "        }\n";
    html += "        .radio-group {\n";
    html += "            display: flex;\n";
    html += "            gap: 10px;\n";
    html += "            margin-bottom: 10px;\n";
    html += "        }\n";
    html += "        .radio-label {\n";
    html += "            margin-right: 10px;\n";
    html += "            cursor: pointer;\n";
    html += "        }\n";
    html += "        #status {\n";
    html += "            margin-top: 10px;\n";
    html += "            padding: 10px;\n";
    html += "            border-radius: 5px;\n";
    html += "            text-align: center;\n";
    html += "            font-weight: bold;\n";
    html += "            min-height: 30px;\n";
    html += "        }\n";
    html += "        .success {\n";
    html += "            background-color: #27ae60;\n";
    html += "            color: white;\n";
    html += "        }\n";
    html += "        .error {\n";
    html += "            background-color: #e74c3c;\n";
    html += "            color: white;\n";
    html += "        }\n";
    html += "    </style>\n";
    html += "</head>\n";
    html += "<body>\n";
    html += "    <div class=\"container\">\n";
    html += "        <div class=\"header\">\n";
    html += "            <h1>SAFE-CRACKER CONTROL PANEL</h1>\n";
    html += "            <p>Arduino Connection: <span style=\"color: " + (connectionStatus == "Connected" ? "#27ae60" : "#e74c3c") + "\">" + connectionStatus + "</span></p>\n";
    html += "        </div>\n";
    html += "\n";
    html += "        <div class=\"instructions\">\n";
    html += "            <h3>How to Play:</h3>\n";
    html += "            <p><strong>Lock In:</strong> Press the red LOCK-IN button when you see GREEN</p>\n";
    html += "        </div>\n";
    html += "\n";
    html += "        <div class=\"status-bar\">\n";
    html += "            <div class=\"status-item\">\n";
    html += "                <div class=\"status-label\">Attempts Left</div>\n";
    html += "                <div class=\"status-value\" style=\"color: " + (attemptsRemaining <= 1 ? "#e74c3c" : "#4ecdc4") + "\">" + attemptsRemaining + "</div>\n";
    html += "            </div>\n";
    html += "            <div class=\"status-item\">\n";
    html += "                <div class=\"status-label\">Score</div>\n";
    html += "                <div class=\"status-value\" style=\"color: #f39c12\">" + score + "</div>\n";
    html += "            </div>\n";
    html += "        </div>\n";
    html += "\n";
    html += "        <div class=\"panel\">\n";
    html += "            <h3>Game Controls</h3>\n";
    html += "            <div class=\"controls\">\n";
    html += "                <div class=\"control-group\">\n";
    html += "                    <h4>Difficulty Level</h4>\n";
    html += "                    <div class=\"radio-group\">\n";
    html += "                        <label class=\"radio-label\">\n";
    html += "                            <input type=\"radio\" name=\"difficulty\" value=\"0\" " + (currentDifficulty == 0 ? "checked" : "") + " onclick=\"setDifficulty(0)\"> Easy\n";
    html += "                        </label>\n";
    html += "                        <label class=\"radio-label\">\n";
    html += "                            <input type=\"radio\" name=\"difficulty\" value=\"1\" " + (currentDifficulty == 1 ? "checked" : "") + " onclick=\"setDifficulty(1)\"> Normal\n";
    html += "                        </label>\n";
    html += "                        <label class=\"radio-label\">\n";
    html += "                            <input type=\"radio\" name=\"difficulty\" value=\"2\" " + (currentDifficulty == 2 ? "checked" : "") + " onclick=\"setDifficulty(2)\"> Expert\n";
    html += "                        </label>\n";
    html += "                    </div>\n";
    html += "                </div>\n";
    html += "\n";
    html += "                <div class=\"control-group\">\n";
    html += "                    <h4>Power Control</h4>\n";
    html += "                    <button id=\"powerButton\" class=\"power-on\" onclick=\"togglePower()\">POWER " + (gameStatus == "STANDBY" ? "ON" : "OFF") + "</button>\n";
    html += "                </div>\n";
    html += "\n";
    html += "                <div class=\"control-group\">\n";
    html += "                    <button class=\"lock-in\" onclick=\"sendCommand('lock')\">LOCK-IN POSITION</button>\n";
    html += "                </div>\n";
    html += "            </div>\n";
    html += "            <div id=\"status\"></div>\n";
    html += "        </div>\n";
    html += "    </div>\n";
    html += "\n";
    html += "    <script>\n";
    html += "        async function sendCommand(command) {\n";
    html += "            const statusDiv = document.getElementById('status');\n";
    html += "            statusDiv.textContent = 'Sending command...';\n";
    html += "            statusDiv.className = '';\n";
    html += "            \n";
    html += "            let endpoint = '';\n";
    html += "            if(command === 'on') endpoint = '/on';\n";
    html += "            else if(command === 'off') endpoint = '/off';\n";
    html += "            else if(command === 'lock') endpoint = '/lock';\n";
    html += "            \n";
    html += "            try {\n";
    html += "                const response = await fetch(endpoint, { method: 'POST' });\n";
    html += "                if(response.ok) {\n";
    html += "                    const result = await response.text();\n";
    html += "                    statusDiv.textContent = result;\n";
    html += "                    statusDiv.className = 'success';\n";
    html += "                    updateStatus();\n";
    html += "                } else {\n";
    html += "                    statusDiv.textContent = 'Error: ' + response.statusText;\n";
    html += "                    statusDiv.className = 'error';\n";
    html += "                }\n";
    html += "            } catch (error) {\n";
    html += "                statusDiv.textContent = 'Error: ' + error.message;\n";
    html += "                statusDiv.className = 'error';\n";
    html += "            }\n";
    html += "        }\n";
    html += "\n";
    html += "        function setDifficulty(level) {\n";
    html += "            fetch('/difficulty', {\n";
    html += "                method: 'POST',\n";
    html += "                headers: { 'Content-Type': 'application/json' },\n";
    html += "                body: JSON.stringify({ difficulty: level })\n";
    html += "            })\n";
    html += "            .then(response => response.text())\n";
    html += "            .then(result => {\n";
    html += "                const statusDiv = document.getElementById('status');\n";
    html += "                statusDiv.textContent = result;\n";
    html += "                statusDiv.className = 'success';\n";
    html += "            });\n";
    html += "        }\n";
    html += "\n";
    html += "        function togglePower() {\n";
    html += "            const powerButton = document.getElementById('powerButton');\n";
    html += "            const isOn = powerButton.textContent.includes('OFF');\n";
    html += "            sendCommand(isOn ? 'off' : 'on');\n";
    html += "            powerButton.textContent = isOn ? 'POWER ON' : 'POWER OFF';\n";
    html += "            powerButton.className = isOn ? 'power-on' : 'power-off';\n";
    html += "        }\n";
    html += "\n";
    html += "        function updateStatus() {\n";
    html += "            fetch('/status')\n";
    html += "            .then(response => response.json())\n";
    html += "            .then(data => {\n";
    html += "                document.querySelector('.status-value:nth-child(2)').textContent = data.attemptsRemaining;\n";
    html += "                document.querySelector('.status-value:nth-child(2)').style.color = data.attemptsRemaining <= 1 ? '#e74c3c' : '#4ecdc4';\n";
    html += "                document.querySelector('.status-value:nth-child(4)').textContent = data.score;\n";
    html += "                \n";
    html += "                const powerButton = document.getElementById('powerButton');\n";
    html += "                powerButton.textContent = data.gameStatus === 'STANDBY' ? 'POWER ON' : 'POWER OFF';\n";
    html += "                powerButton.className = data.gameStatus === 'STANDBY' ? 'power-on' : 'power-off';\n";
    html += "            });\n";
    html += "        }\n";
    html += "\n";
    html += "        setInterval(updateStatus, 1000);\n";
    html += "    </script>\n";
    html += "</body>\n";
    html += "</html>\n";
    
    return html;
});

// Power ON endpoint
app.MapPost("/on", () =>
{/* Lines 667-682 omitted */});

// Power OFF endpoint
app.MapPost("/off", () =>
{/* Lines 687-702 omitted */});

// Lock-in endpoint
app.MapPost("/lock", () =>
{/* Lines 707-722 omitted */});

// Difficulty endpoint
app.MapPost("/difficulty", (HttpContext context) =>
{/* Lines 727-753 omitted */});

// Test LEDs endpoint
app.MapPost("/test-leds", () =>
{/* Lines 758-773 omitted */});

// Status endpoint
app.MapGet("/status", () =>
{/* Lines 778-790 omitted */});

// Cleanup on shutdown
app.Lifetime.ApplicationStopping.Register(() =>
{
    if (serialPort != null && serialPort.IsOpen)
    {/* Lines 797-800 omitted */}
});

Console.WriteLine("Starting Safe-Cracker Control Panel at http://localhost:5001");
app.Run("http://localhost:5001");

// Helper class for difficulty request
public class DifficultyRequest
{
    public int difficulty { get; set; }
}