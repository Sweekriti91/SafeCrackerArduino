# Safe-Cracker Game Project

## Overview
Safe-Cracker is an interactive Arduino-based game that simulates cracking a safe by finding the correct angle position. Players rotate a dial and use LED feedback to determine how close they are to the target angle. The game features a web-based control panel for easy interaction and monitoring.

## Features
- **Arduino-based hardware**: Physical interaction through a rotation sensor
- **LED feedback system**: Visual cues indicate proximity to the target angle
- **Web-based control panel**: Easy game management through a browser
- **Multiple difficulty levels**: Easy, Normal, and Expert modes with varying precision requirements
- **Score tracking**: Points awarded based on speed and accuracy

## Project Structure
```
Project/
├── ArduinoWebController/     # C# web application for game control
│   ├── Program.cs            # Main web server and game logic
│   └── ArduinoWebController.csproj
│
└── ReactionTime/             # Arduino project (renamed from original)
    ├── platformio.ini        # PlatformIO configuration
    ├── src/
    │   └── main.cpp          # Arduino code for the hardware
    ├── include/              # Header files
    └── lib/                  # Arduino libraries
```

## Getting Started

### Prerequisites
- Arduino board (recommended: Arduino Uno or Nano)
- Rotary encoder or potentiometer
- LEDs (green, red, blue, orange)
- .NET SDK (version 9.0 or later)
- PlatformIO (optional, for Arduino development)

### Installation
1. Clone the repository:
   ```
   git clone https://github.com/yourusername/safe-cracker.git
   cd safe-cracker
   ```

2. Set up the Arduino hardware:
   - Connect the rotary encoder to specified pins
   - Connect LEDs to their respective pins
   - Upload the code using Arduino IDE or PlatformIO

3. Run the web controller:
   ```
   cd ArduinoWebController
   dotnet run
   ```

4. Open a browser and navigate to:
   ```
   http://localhost:5001
   ```

## How to Play
1. Select a difficulty level (Easy, Normal, or Expert)
2. Press the "POWER ON" button to start the game
3. Rotate the dial to find the target angle
4. Press the red "LOCK-IN" button when the GREEN LED lights up
5. Score points based on speed and accuracy

## Detailed Implementation
For a detailed explanation of the project implementation, design decisions, and technical specifications, please refer to the safecrack.md document.

## Hardware Requirements
- Arduino board
- Rotary encoder or potentiometer
- 4 LEDs (green, red, blue, orange)
- Resistors for LEDs
- Breadboard and connecting wires
- USB cable for Arduino connection

## Software Dependencies
- .NET 9.0 SDK
- System.IO.Ports NuGet package (for serial communication)

## Troubleshooting
- **Serial port not found**: Check the port name in Program.cs and update if necessary
- **LEDs not responding**: Verify the pin connections and hardware setup
- **Web interface not loading**: Ensure the web server is running and accessible

