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

// Initialize serial port
try
{
    serialPort = new SerialPort(portName, baudRate);
    serialPort.DtrEnable = true;
    serialPort.RtsEnable = true;
    serialPort.ReadTimeout = 1000;
    serialPort.WriteTimeout = 1000;
    
    serialPort.DataReceived += (sender, e) =>
    {
        try
        {
            string data = serialPort.ReadLine();
            
            // Add timestamp to debug log
            debugOutput.AppendLine($"[{DateTime.Now:HH:mm:ss.fff}] {data}");
            
            // Update connection status if a status message is received
            if (data.StartsWith("STATUS:"))
            {
                gameStatus = data.Substring(7);
            }
        }
        catch (Exception ex)
        {
            debugOutput.AppendLine($"[{DateTime.Now:HH:mm:ss.fff}] Error: {ex.Message}");
        }
    };
    
    serialPort.Open();
    connectionStatus = "Connected";
    debugOutput.AppendLine($"[{DateTime.Now:HH:mm:ss.fff}] Connected to Arduino on {portName} at {baudRate} baud");
    Console.WriteLine($"Connected to Arduino on {portName} at {baudRate} baud");
}
catch (Exception ex)
{
    connectionStatus = $"Failed: {ex.Message}";
    debugOutput.AppendLine($"[{DateTime.Now:HH:mm:ss.fff}] Connection failed: {ex.Message}");
    Console.WriteLine($"Failed to connect to Arduino: {ex.Message}");
}

// Serve the main page
app.MapGet("/", () =>
{
    // Create HTML with traditional string concatenation
    string html = "<!DOCTYPE html>\n<html>\n<head>\n";
    html += "    <title>Arduino Debug Control Panel</title>\n";
    html += "    <style>\n";
    html += "        body {\n";
    html += "            font-family: monospace;\n";
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
    html += "            border-radius: 5px;\n";
    html += "            text-align: center;\n";
    html += "            margin-bottom: 20px;\n";
    html += "        }\n";
    html += "        h1 {\n";
    html += "            margin: 0;\n";
    html += "            font-size: 1.5em;\n";
    html += "        }\n";
    html += "        .panel {\n";
    html += "            background-color: #2c2c2c;\n";
    html += "            padding: 15px;\n";
    html += "            border-radius: 5px;\n";
    html += "            margin-bottom: 15px;\n";
    html += "        }\n";
    html += "        .controls {\n";
    html += "            display: flex;\n";
    html += "            gap: 10px;\n";
    html += "            margin-bottom: 15px;\n";
    html += "        }\n";
    html += "        button {\n";
    html += "            padding: 15px 30px;\n";
    html += "            font-size: 16px;\n";
    html += "            border: none;\n";
    html += "            border-radius: 5px;\n";
    html += "            cursor: pointer;\n";
    html += "            font-weight: bold;\n";
    html += "            flex: 1;\n";
    html += "        }\n";
    html += "        .power-button {\n";
    html += "            background-color: #27ae60;\n";
    html += "            color: white;\n";
    html += "        }\n";
    html += "        .lock-button {\n";
    html += "            background-color: #e74c3c;\n";
    html += "            color: white;\n";
    html += "        }\n";
    html += "        .debug {\n";
    html += "            background-color: #1a1a1a;\n";
    html += "            padding: 15px;\n";
    html += "            border-radius: 5px;\n";
    html += "            font-family: monospace;\n";
    html += "            font-size: 12px;\n";
    html += "            height: 300px;\n";
    html += "            overflow-y: auto;\n";
    html += "            white-space: pre-wrap;\n";
    html += "            color: #4ecdc4;\n";
    html += "        }\n";
    html += "        .status {\n";
    html += "            margin-top: 10px;\n";
    html += "            padding: 10px;\n";
    html += "            border-radius: 5px;\n";
    html += "            text-align: center;\n";
    html += "            font-weight: bold;\n";
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
    html += "            <h1>ARDUINO DEBUG CONTROL PANEL</h1>\n";
    html += "            <p>Arduino Connection: <span style=\"color: " + (connectionStatus == "Connected" ? "#27ae60" : "#e74c3c") + "\">" + connectionStatus + "</span></p>\n";
    html += "        </div>\n";
    html += "\n";
    html += "        <div class=\"panel\">\n";
    html += "            <h3>Controls</h3>\n";
    html += "            <div class=\"controls\">\n";
    html += "                <button id=\"powerButton\" class=\"power-button\" onclick=\"sendCommand('on')\">POWER ON</button>\n";
    html += "                <button class=\"lock-button\" onclick=\"sendCommand('lock')\">LOCK-IN</button>\n";
    html += "            </div>\n";
    html += "            <div id=\"status\" class=\"status\"></div>\n";
    html += "        </div>\n";
    html += "\n";
    html += "        <div class=\"panel\">\n";
    html += "            <h3>Serial Debug Output</h3>\n";
    html += "            <div id=\"debugOutput\" class=\"debug\">" + debugOutput.ToString() + "</div>\n";
    html += "            <button onclick=\"clearDebug()\" style=\"margin-top: 10px; background-color: #7f8c8d; color: white;\">Clear Debug</button>\n";
    html += "        </div>\n";
    html += "    </div>\n";
    html += "\n";
    html += "    <script>\n";
    html += "        async function sendCommand(command) {\n";
    html += "            const statusDiv = document.getElementById('status');\n";
    html += "            statusDiv.textContent = 'Sending command...';\n";
    html += "            statusDiv.className = 'status';\n";
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
    html += "                    statusDiv.className = 'status success';\n";
    html += "                    \n";
    html += "                    // Update power button text based on command\n";
    html += "                    if(command === 'on') {\n";
    html += "                        document.getElementById('powerButton').textContent = 'POWER OFF';\n";
    html += "                        document.getElementById('powerButton').onclick = function() { sendCommand('off'); };\n";
    html += "                    } else if(command === 'off') {\n";
    html += "                        document.getElementById('powerButton').textContent = 'POWER ON';\n";
    html += "                        document.getElementById('powerButton').onclick = function() { sendCommand('on'); };\n";
    html += "                    }\n";
    html += "                    \n";
    html += "                    refreshDebug();\n";
    html += "                } else {\n";
    html += "                    statusDiv.textContent = 'Error: ' + response.statusText;\n";
    html += "                    statusDiv.className = 'status error';\n";
    html += "                }\n";
    html += "            } catch (error) {\n";
    html += "                statusDiv.textContent = 'Error: ' + error.message;\n";
    html += "                statusDiv.className = 'status error';\n";
    html += "            }\n";
    html += "        }\n";
    html += "\n";
    html += "        function clearDebug() {\n";
    html += "            fetch('/clear-debug', { method: 'POST' })\n";
    html += "                .then(() => refreshDebug());\n";
    html += "        }\n";
    html += "\n";
    html += "        function refreshDebug() {\n";
    html += "            fetch('/debug')\n";
    html += "                .then(response => response.text())\n";
    html += "                .then(data => {\n";
    html += "                    document.getElementById('debugOutput').innerHTML = data;\n";
    html += "                    document.getElementById('debugOutput').scrollTop = document.getElementById('debugOutput').scrollHeight;\n";
    html += "                });\n";
    html += "        }\n";
    html += "\n";
    html += "        // Auto-refresh debug output every 2 seconds\n";
    html += "        setInterval(refreshDebug, 2000);\n";
    html += "    </script>\n";
    html += "</body>\n";
    html += "</html>\n";
    
    return Results.Content(html, "text/html");
});

// Debug output endpoint
app.MapGet("/debug", () => 
{
    return Results.Content(debugOutput.ToString(), "text/plain");
});

// Clear debug endpoint
app.MapPost("/clear-debug", () => 
{
    debugOutput.Clear();
    return Results.Ok("Debug output cleared");
});

// Power ON endpoint
app.MapPost("/on", () =>
{
    if (serialPort != null && serialPort.IsOpen)
    {
        debugOutput.AppendLine($"[{DateTime.Now:HH:mm:ss.fff}] Sending: P");
        serialPort.WriteLine("P");
        gameStatus = "RUNNING";
        return Results.Ok("Power ON command sent!");
    }
    else
    {
        return Results.BadRequest("Arduino not connected");
    }
});

// Power OFF endpoint
app.MapPost("/off", () =>
{
    if (serialPort != null && serialPort.IsOpen)
    {
        debugOutput.AppendLine($"[{DateTime.Now:HH:mm:ss.fff}] Sending: O");
        serialPort.WriteLine("O");
        gameStatus = "STANDBY";
        return Results.Ok("Power OFF command sent!");
    }
    else
    {
        return Results.BadRequest("Arduino not connected");
    }
});

// Lock-in endpoint
app.MapPost("/lock", () =>
{
    if (serialPort != null && serialPort.IsOpen)
    {
        debugOutput.AppendLine($"[{DateTime.Now:HH:mm:ss.fff}] Sending: L");
        serialPort.WriteLine("L");
        return Results.Ok("Lock-in command sent!");
    }
    else
    {
        return Results.BadRequest("Arduino not connected");
    }
});

// Cleanup on shutdown
app.Lifetime.ApplicationStopping.Register(() =>
{
    if (serialPort != null && serialPort.IsOpen)
    {
        serialPort.Close();
        serialPort.Dispose();
    }
});

Console.WriteLine("Starting Arduino Debug Control Panel at http://localhost:5001");
app.Run("http://localhost:5001");