using System.IO.Ports;
using System.Text;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Configure static files middleware
app.UseStaticFiles();

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
    // Read the HTML template from file
    string htmlTemplate = File.ReadAllText("Views/Index.html");
    
    // Replace placeholders with dynamic content
    string connectionColor = connectionStatus == "Connected" ? "#27ae60" : "#e74c3c";
    htmlTemplate = htmlTemplate.Replace("{{CONNECTION_COLOR}}", connectionColor);
    htmlTemplate = htmlTemplate.Replace("{{CONNECTION_STATUS}}", connectionStatus);
    htmlTemplate = htmlTemplate.Replace("{{DEBUG_OUTPUT}}", debugOutput.ToString());
    
    return Results.Content(htmlTemplate, "text/html");
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