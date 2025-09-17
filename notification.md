# Desktop Notification Flag System

A physical desktop notification system using Arduino that raises/lowers a 3D-printed flag and displays notification types through LEDs. Perfect for staying aware of important notifications without constant screen checking!

## 🚩 System Overview

**Concept:** Create a physical notification system that sits on your desk and communicates with your computer to display real-time notifications through a movable flag and colored LEDs.

**Key Features:**
- **Physical Flag:** Servo-controlled 3D-printed flag that raises for new notifications
- **LED Notification Types:** Different colors for email, meetings, break reminders, etc.
- **Switch Dismissal:** Simple switch press to acknowledge and dismiss notifications
- **Computer Integration:** Receives real notifications from your computer via USB serial
- **Minimal Interaction:** One switch press to dismiss all notifications

## 🧩 Required Parts

**Hardware Components:**
- 1x Elegoo UNO R3 (Arduino)
- 5x LEDs (Red, Blue, Green, Yellow, White recommended)
- 5x 1K resistors (for LED current limiting)
- 1x 1M resistor (for switch pullup)
- 1x Deegoo FPV MG 996R servo motor
- 1x UBEC DC/DC Converter 5V (for servo power)
- 1x Small tactile switch
- 2x Capacitors (for power filtering)
- Breadboard and jumper wires
- 3D printer access

**Software Requirements:**
- Arduino IDE or PlatformIO
- Python 3.x (for computer integration)
- PySerial library

## 🎨 Notification Types

### LED Color Coding
- 🔴 **Red LED:** Urgent/Critical notifications
- 🔵 **Blue LED:** Email notifications  
- 🟢 **Green LED:** Calendar/Meeting reminders
- 🟡 **Yellow LED:** Break time/Health reminders
- ⚪ **White LED:** General system notifications

### Flag States
- **Flag DOWN (0°):** No active notifications
- **Flag UP (90°):** Active notifications present
- **Flag WAVING:** New notification just arrived

## 🔌 Wiring Diagram

### Power Distribution
```
UBEC 5V Output → Servo Red Wire (VCC)
UBEC Ground    → Servo Brown Wire (GND) + Arduino GND
Arduino USB    → Computer (Serial + Power)
```

### LED Array Connections
```
Arduino Pin 3  → 1K Resistor → Red LED → GND     (Urgent)
Arduino Pin 4  → 1K Resistor → Blue LED → GND    (Email)
Arduino Pin 5  → 1K Resistor → Green LED → GND   (Calendar)
Arduino Pin 6  → 1K Resistor → Yellow LED → GND  (Break)
Arduino Pin 7  → 1K Resistor → White LED → GND   (System)
```

### Servo Flag Mechanism
```
Arduino Pin 9  → Servo Orange Wire (Signal)
UBEC 5V       → Servo Red Wire (Power)
Arduino GND   → Servo Brown Wire (Ground)
```

### Switch for Dismissal
```
Arduino Pin 2  → Switch Terminal 1
Switch Terminal 2 → 1M Resistor → Arduino GND
Arduino Pin 2     → 10K Resistor → Arduino 5V (pullup)
```

### Power Filtering
```
Capacitor 1: Arduino VIN → GND (100µF recommended)
Capacitor 2: UBEC Output → GND (100µF recommended)
```

## 📋 Step-by-Step Assembly

### Step 1: Power Setup
1. Connect UBEC ground to Arduino GND rail on breadboard
2. Connect UBEC 5V output to servo power (red wire)
3. Place filtering capacitors near power connections
4. Test power connections with multimeter

### Step 2: LED Notification Array
1. Insert 5 LEDs into breadboard in a row
2. Connect all LED cathodes (short legs) to GND rail
3. Connect each LED anode through 1K resistor to Arduino pins 3-7
4. Test each LED individually with simple blink code

### Step 3: Servo Flag Assembly
1. Attach servo horn to 3D-printed flag mount
2. Connect servo signal wire (orange) to Arduino pin 9
3. Connect servo power (red) to UBEC 5V output
4. Connect servo ground (brown) to GND rail
5. Test servo movement through full range

### Step 4: Switch Integration
1. Mount switch in accessible location on enclosure
2. Connect one terminal to Arduino pin 2
3. Connect other terminal through 1M resistor to GND
4. Add pullup resistor from pin 2 to 5V
5. Test switch with simple input reading code

### Step 5: Final Assembly
1. Mount all components in 3D-printed enclosure
2. Route cables cleanly to avoid interference
3. Secure Arduino with mounting posts
4. Test complete system before final assembly

## 💻 Arduino Code

```cpp
#include <Arduino.h>
#include <Servo.h>

// Pin definitions
const int ledPins[] = {3, 4, 5, 6, 7};  // Red, Blue, Green, Yellow, White
const int switchPin = 2;
const int servoPin = 9;
const int numLeds = 5;

// Notification types
enum NotificationType {
  URGENT = 0,    // Red LED
  EMAIL = 1,     // Blue LED  
  CALENDAR = 2,  // Green LED
  BREAK = 3,     // Yellow LED
  SYSTEM = 4     // White LED
};

// System components
Servo flagServo;
bool notificationActive[numLeds] = {false, false, false, false, false};
bool anyNotificationActive = false;
bool flagRaised = false;

// Switch handling
bool lastSwitchState = false;
unsigned long lastDebounceTime = 0;
const unsigned long debounceDelay = 50;

// Animation variables
unsigned long lastAnimationTime = 0;
int animationStep = 0;
bool isWaving = false;
unsigned long waveStartTime = 0;
const unsigned long waveDuration = 3000; // 3 seconds

void setup() {
  Serial.begin(9600);
  
  // Initialize LED pins
  for(int i = 0; i < numLeds; i++) {
    pinMode(ledPins[i], OUTPUT);
    digitalWrite(ledPins[i], LOW);
  }
  
  // Initialize switch
  pinMode(switchPin, INPUT_PULLUP);
  
  // Initialize servo
  flagServo.attach(servoPin);
  flagServo.write(0); // Flag down initially
  flagRaised = false;
  
  // Startup sequence
  Serial.println("=== DESKTOP NOTIFICATION FLAG SYSTEM ===");
  Serial.println("Notification types:");
  Serial.println("URGENT:0, EMAIL:1, CALENDAR:2, BREAK:3, SYSTEM:4");
  Serial.println("Commands: ADD:type, CLEAR:type, CLEAR:ALL, STATUS");
  Serial.println("Switch to dismiss all notifications");
  
  // Test sequence
  startupAnimation();
  
  Serial.println("System ready for notifications!");
}

void loop() {
  // Handle serial commands from computer
  handleSerialCommands();
  
  // Check switch for dismissal
  checkDismissalSwitch();
  
  // Update LED display
  updateLEDDisplay();
  
  // Handle flag animations
  updateFlagAnimation();
  
  delay(10);
}

void handleSerialCommands() {
  if(Serial.available()) {
    String command = Serial.readStringUntil('\n');
    command.trim();
    
    if(command.startsWith("ADD:")) {
      int type = command.substring(4).toInt();
      addNotification(type);
    }
    else if(command.startsWith("CLEAR:")) {
      String param = command.substring(6);
      if(param == "ALL") {
        clearAllNotifications();
      } else {
        int type = param.toInt();
        clearNotification(type);
      }
    }
    else if(command == "STATUS") {
      sendStatus();
    }
    else if(command == "TEST") {
      testAllSystems();
    }
    else {
      Serial.println("ERROR: Unknown command");
    }
  }
}

void addNotification(int type) {
  if(type >= 0 && type < numLeds) {
    if(!notificationActive[type]) {
      notificationActive[type] = true;
      updateNotificationState();
      triggerWaveAnimation();
      
      Serial.print("NOTIFICATION_ADDED:");
      Serial.println(type);
    }
  } else {
    Serial.println("ERROR: Invalid notification type");
  }
}

void clearNotification(int type) {
  if(type >= 0 && type < numLeds) {
    notificationActive[type] = false;
    updateNotificationState();
    
    Serial.print("NOTIFICATION_CLEARED:");
    Serial.println(type);
  }
}

void clearAllNotifications() {
  for(int i = 0; i < numLeds; i++) {
    notificationActive[i] = false;
  }
  updateNotificationState();
  Serial.println("ALL_NOTIFICATIONS_CLEARED");
}

void updateNotificationState() {
  // Check if any notifications are active
  anyNotificationActive = false;
  for(int i = 0; i < numLeds; i++) {
    if(notificationActive[i]) {
      anyNotificationActive = true;
      break;
    }
  }
  
  // Update flag position
  if(anyNotificationActive && !flagRaised) {
    raiseFlag();
  } else if(!anyNotificationActive && flagRaised) {
    lowerFlag();
  }
}

void raiseFlag() {
  flagServo.write(90); // Flag up position
  flagRaised = true;
  Serial.println("FLAG_RAISED");
}

void lowerFlag() {
  flagServo.write(0); // Flag down position
  flagRaised = false;
  Serial.println("FLAG_LOWERED");
}

void triggerWaveAnimation() {
  isWaving = true;
  waveStartTime = millis();
  animationStep = 0;
}

void updateFlagAnimation() {
  if(isWaving) {
    unsigned long currentTime = millis();
    
    if(currentTime - waveStartTime > waveDuration) {
      // End waving animation
      isWaving = false;
      if(anyNotificationActive) {
        flagServo.write(90); // Return to raised position
      } else {
        flagServo.write(0); // Return to lowered position
      }
      return;
    }
    
    // Create waving motion
    if(currentTime - lastAnimationTime > 200) { // Wave every 200ms
      lastAnimationTime = currentTime;
      
      int wavePositions[] = {90, 110, 90, 70, 90}; // Wave pattern
      int posIndex = animationStep % 5;
      flagServo.write(wavePositions[posIndex]);
      animationStep++;
    }
  }
}

void updateLEDDisplay() {
  static unsigned long lastBlink = 0;
  static bool blinkState = false;
  
  // Blink active notification LEDs
  if(millis() - lastBlink > 1000) { // Blink every second
    lastBlink = millis();
    blinkState = !blinkState;
    
    for(int i = 0; i < numLeds; i++) {
      if(notificationActive[i]) {
        digitalWrite(ledPins[i], blinkState ? HIGH : LOW);
      } else {
        digitalWrite(ledPins[i], LOW);
      }
    }
  }
}

void checkDismissalSwitch() {
  bool currentSwitchState = !digitalRead(switchPin); // Inverted due to pullup
  
  // Debounce switch
  if(currentSwitchState != lastSwitchState) {
    lastDebounceTime = millis();
  }
  
  if((millis() - lastDebounceTime) > debounceDelay) {
    if(currentSwitchState && !lastSwitchState) {
      // Switch pressed - dismiss all notifications
      if(anyNotificationActive) {
        dismissAllNotifications();
      }
    }
  }
  
  lastSwitchState = currentSwitchState;
}

void dismissAllNotifications() {
  clearAllNotifications();
  
  // Acknowledgment animation
  for(int i = 0; i < 3; i++) {
    for(int j = 0; j < numLeds; j++) {
      digitalWrite(ledPins[j], HIGH);
    }
    delay(100);
    for(int j = 0; j < numLeds; j++) {
      digitalWrite(ledPins[j], LOW);
    }
    delay(100);
  }
  
  Serial.println("ALL_NOTIFICATIONS_DISMISSED");
}

void sendStatus() {
  Serial.print("STATUS:");
  for(int i = 0; i < numLeds; i++) {
    Serial.print(notificationActive[i] ? "1" : "0");
    if(i < numLeds - 1) Serial.print(",");
  }
  Serial.print("|FLAG:");
  Serial.print(flagRaised ? "UP" : "DOWN");
  Serial.println();
}

void startupAnimation() {
  // LED sweep
  for(int i = 0; i < numLeds; i++) {
    digitalWrite(ledPins[i], HIGH);
    delay(200);
    digitalWrite(ledPins[i], LOW);
  }
  
  // Flag test
  flagServo.write(90);
  delay(1000);
  flagServo.write(0);
  delay(500);
  
  Serial.println("STARTUP_COMPLETE");
}

void testAllSystems() {
  Serial.println("TESTING_ALL_SYSTEMS");
  
  // Test each notification type
  for(int i = 0; i < numLeds; i++) {
    addNotification(i);
    delay(1000);
    clearNotification(i);
    delay(500);
  }
  
  Serial.println("SYSTEM_TEST_COMPLETE");
}
```

## 🖥️ Computer Integration Script

### Python Notification Monitor

```python
#!/usr/bin/env python3
"""
Desktop Notification Flag - Computer Integration
Monitors system notifications and sends them to Arduino via serial
"""

import serial
import time
import threading
import json
from datetime import datetime
import platform

# For different operating systems
if platform.system() == "Darwin":  # macOS
    import pync
elif platform.system() == "Windows":
    import win10toast
elif platform.system() == "Linux":
    import notify2

class NotificationFlag:
    def __init__(self, port='/dev/cu.usbmodem*', baudrate=9600):
        self.arduino = None
        self.port = port
        self.baudrate = baudrate
        self.connected = False
        self.notification_counts = {
            'urgent': 0,
            'email': 0, 
            'calendar': 0,
            'break': 0,
            'system': 0
        }
        
    def connect(self):
        """Connect to Arduino"""
        try:
            # Auto-detect Arduino port
            import serial.tools.list_ports
            ports = serial.tools.list_ports.comports()
            arduino_port = None
            
            for port in ports:
                if 'Arduino' in port.description or 'usbmodem' in port.device:
                    arduino_port = port.device
                    break
            
            if arduino_port:
                self.arduino = serial.Serial(arduino_port, self.baudrate, timeout=1)
                time.sleep(2)  # Wait for Arduino to initialize
                self.connected = True
                print(f"Connected to Arduino on {arduino_port}")
                return True
            else:
                print("Arduino not found")
                return False
                
        except Exception as e:
            print(f"Connection error: {e}")
            return False
    
    def send_command(self, command):
        """Send command to Arduino"""
        if self.connected and self.arduino:
            try:
                self.arduino.write(f"{command}\n".encode())
                time.sleep(0.1)
                
                # Read response
                if self.arduino.in_waiting:
                    response = self.arduino.readline().decode().strip()
                    print(f"Arduino: {response}")
                    return response
            except Exception as e:
                print(f"Send error: {e}")
        return None
    
    def add_notification(self, notification_type, message=""):
        """Add new notification"""
        type_map = {
            'urgent': 0,
            'email': 1,
            'calendar': 2, 
            'break': 3,
            'system': 4
        }
        
        if notification_type in type_map:
            self.notification_counts[notification_type] += 1
            type_id = type_map[notification_type]
            self.send_command(f"ADD:{type_id}")
            print(f"Added {notification_type} notification: {message}")
    
    def clear_notification(self, notification_type):
        """Clear specific notification type"""
        type_map = {
            'urgent': 0,
            'email': 1,
            'calendar': 2,
            'break': 3, 
            'system': 4
        }
        
        if notification_type in type_map:
            type_id = type_map[notification_type]
            self.send_command(f"CLEAR:{type_id}")
            self.notification_counts[notification_type] = 0
    
    def clear_all(self):
        """Clear all notifications"""
        self.send_command("CLEAR:ALL")
        for key in self.notification_counts:
            self.notification_counts[key] = 0
    
    def get_status(self):
        """Get current status from Arduino"""
        return self.send_command("STATUS")
    
    def monitor_email(self):
        """Monitor email notifications (placeholder)"""
        # This would integrate with your email client
        # Examples: IMAP checking, Outlook integration, etc.
        pass
    
    def monitor_calendar(self):
        """Monitor calendar events (placeholder)"""
        # This would integrate with your calendar system
        # Examples: Google Calendar API, Outlook Calendar, etc.
        pass
    
    def break_reminder(self):
        """Send periodic break reminders"""
        while True:
            time.sleep(3600)  # Every hour
            current_time = datetime.now()
            if 9 <= current_time.hour <= 17:  # Work hours
                self.add_notification('break', 'Time for a break!')
    
    def test_notifications(self):
        """Test all notification types"""
        print("Testing all notification types...")
        
        notifications = [
            ('urgent', 'Critical system alert!'),
            ('email', 'New email received'),
            ('calendar', 'Meeting in 15 minutes'),
            ('break', 'Time for a stretch break'),
            ('system', 'System update available')
        ]
        
        for notif_type, message in notifications:
            self.add_notification(notif_type, message)
            time.sleep(2)
        
        time.sleep(5)
        print("Clearing all test notifications...")
        self.clear_all()

def main():
    # Initialize notification flag
    flag = NotificationFlag()
    
    if not flag.connect():
        print("Failed to connect to Arduino")
        return
    
    # Test the system
    flag.test_notifications()
    
    # Start break reminder thread
    break_thread = threading.Thread(target=flag.break_reminder, daemon=True)
    break_thread.start()
    
    # Main command loop
    print("\nNotification Flag Control")
    print("Commands: urgent, email, calendar, break, system, clear, status, quit")
    
    while True:
        try:
            command = input("> ").strip().lower()
            
            if command == "quit":
                break
            elif command in ['urgent', 'email', 'calendar', 'break', 'system']:
                message = input(f"Enter {command} message: ")
                flag.add_notification(command, message)
            elif command == "clear":
                notif_type = input("Clear which type (or 'all'): ").strip().lower()
                if notif_type == "all":
                    flag.clear_all()
                else:
                    flag.clear_notification(notif_type)
            elif command == "status":
                flag.get_status()
            elif command == "test":
                flag.test_notifications()
            else:
                print("Unknown command")
                
        except KeyboardInterrupt:
            break
    
    print("Shutting down...")
    flag.clear_all()

if __name__ == "__main__":
    main()
```

## 🖨️ 3D Printing Guide

### Flag Design
**File:** `notification_flag.stl`

**Specifications:**
- Flag dimensions: 60mm x 40mm x 2mm
- Servo horn mounting hole (25-tooth spline)
- Optional text/logo area
- Lightweight design for smooth movement

**Print Settings:**
- Layer height: 0.2mm
- Infill: 10% (lightweight)
- Supports: No
- Print time: ~20 minutes

### Mounting Bracket
**File:** `servo_bracket.stl`

**Features:**
- Servo mounting holes (MG996R compatible)
- Flag clearance space
- Desktop mounting base
- Cable management slot

**Print Settings:**
- Layer height: 0.3mm
- Infill: 20%
- Supports: Yes
- Print time: ~45 minutes

### Main Enclosure
**File:** `notification_box.stl`

**Features:**
- Arduino UNO mounting posts
- LED array slots (5x 5mm holes)
- Switch mounting hole
- Servo bracket mount
- USB cable access
- Ventilation slots

**Print Settings:**
- Layer height: 0.3mm  
- Infill: 15%
- Supports: Yes (for overhangs)
- Print time: ~2.5 hours

## 🔧 Troubleshooting

### Hardware Issues

**LEDs not responding:**
- Check resistor values and connections
- Verify LED polarity (long leg = positive)
- Test with multimeter

**Servo jerky movement:**
- Check power supply (UBEC) connections
- Verify capacitor filtering
- Ensure proper grounding

**Switch not registering:**
- Check pullup resistor value (should be 10K-1M)
- Verify switch mechanical contact
- Test with multimeter

**Serial communication problems:**
- Check USB cable (data + power capable)
- Verify baud rate (9600)
- Try different USB port

### Software Issues

**Arduino not responding:**
- Press reset button
- Check serial port in Arduino IDE
- Verify code upload success

**Python script errors:**
- Install required libraries: `pip install pyserial`
- Check port permissions (Linux/macOS)
- Verify Arduino port detection

### Integration Issues

**Notifications not triggering:**
- Check serial communication
- Verify command format
- Test with manual commands first

**Flag timing issues:**
- Adjust animation delays in code
- Check servo power supply stability
- Verify mechanical mounting

## 📈 Advanced Features

### Email Integration
```python
# Gmail IMAP example
import imaplib
import email

def check_gmail(username, password):
    mail = imaplib.IMAP4_SSL('imap.gmail.com')
    mail.login(username, password)
    mail.select('inbox')
    
    status, messages = mail.search(None, 'UNSEEN')
    unread_count = len(messages[0].split())
    
    if unread_count > 0:
        flag.add_notification('email', f'{unread_count} new emails')
```

### Calendar Integration
```python
# Google Calendar API example
from googleapiclient.discovery import build

def check_calendar_events():
    service = build('calendar', 'v3', credentials=creds)
    
    # Get upcoming events
    now = datetime.utcnow().isoformat() + 'Z'
    events_result = service.events().list(
        calendarId='primary', timeMin=now,
        maxResults=10, singleEvents=True,
        orderBy='startTime').execute()
    
    events = events_result.get('items', [])
    
    for event in events:
        start = event['start'].get('dateTime', event['start'].get('date'))
        # Check if event is within next 15 minutes
        # Send calendar notification
```

### Slack Integration
```python
# Slack webhook example
import requests

def send_slack_notification(message):
    webhook_url = "YOUR_SLACK_WEBHOOK_URL"
    payload = {"text": message}
    requests.post(webhook_url, json=payload)
```

## 📊 Usage Examples

### Daily Workflow
1. **Morning:** System starts, shows any overnight notifications
2. **Work Hours:** Receives email, calendar, and break notifications
3. **Meetings:** Calendar reminders 15 minutes before events
4. **Breaks:** Hourly break reminders during work hours
5. **Urgent:** Critical system alerts immediately raise flag

### Command Examples
```bash
# Add notifications
python notification_flag.py
> email
Enter email message: New message from boss

> calendar  
Enter calendar message: Team meeting in 15 minutes

> urgent
Enter urgent message: Server down!

# Check status
> status
STATUS:1,1,1,0,0|FLAG:UP

# Clear specific type
> clear
Clear which type: email

# Clear everything
> clear
Clear which type: all
```

## 🏆 Project Extensions

### Easy Additions
- Sound alerts using piezo buzzer
- WiFi connectivity for wireless notifications
- Mobile app integration
- Multiple flag colors
- Custom notification sounds

### Advanced Features
- Machine learning for notification priority
- Smart home integration
- Video call status indicator
- Ambient lighting effects
- Voice command integration

## 📚 Learning Outcomes

**Hardware Skills:**
- Servo motor control
- LED array management
- Switch debouncing
- Power supply design
- 3D mechanical integration

**Software Skills:**
- Serial communication protocols
- Multi-threaded programming
- System integration
- API development
- Cross-platform compatibility

**System Design:**
- Human-computer interaction
- Notification systems
- Physical computing
- Desktop automation
- User experience design

---

**Build Time:** 2-3 days
**Difficulty:** Intermediate
**Perfect For:** Productivity enhancement, office automation, maker showcases

**Stay notified without being distracted!** 🚩✨