# Smart Focus Timer (Pomodoro) - Implementation Guide

A physical Pomodoro timer that helps you stay focused without digital distractions. Features a servo-driven analog dial showing time remaining, color-coded LEDs for work/break phases, and simple switch control.

## 🍅 Project Overview

**Concept:** Create a tangible productivity tool that uses the Pomodoro Technique with physical feedback instead of screen-based timers that can be distracting.

**Key Features:**
- **Physical Time Display:** Servo rotates pointer on 3D-printed dial (0° to 180°)
- **Color-Coded Phases:** Red LED (work), Green LED (break), Blue LED (long break)
- **Simple Control:** One switch to start/pause/reset sessions
- **Ambient Feedback:** Visual cues without screen distractions
- **Standard Timing:** 25min work, 5min break, 15min long break every 4th cycle

## 🧩 Required Parts

**From Your Available Components:**
- 1x Elegoo UNO R3 (Arduino)
- 1x Deegoo FPV MG 996R servo motor
- 3x LEDs (Red, Green, Blue recommended)
- 4x 1K resistors (3 for LEDs, 1 for switch)
- 1x Small tactile switch
- 1x UBEC DC/DC Converter 5V (for servo power)
- 1x Capacitor (for power filtering)
- Breadboard and jumper wires
- 3D printer access

## 🔌 Wiring Diagram

### Power Distribution
```
UBEC 5V Output → Servo Red Wire (VCC)
UBEC Ground    → Servo Brown Wire (GND) + Arduino GND
Arduino USB    → Computer (Serial + Power)
```

### LED Phase Indicators
```
Arduino Pin 6  → 1K Resistor → Red LED → GND     (Work Phase)
Arduino Pin 7  → 1K Resistor → Green LED → GND   (Short Break)
Arduino Pin 8  → 1K Resistor → Blue LED → GND    (Long Break)
```

### Servo Timer Dial
```
Arduino Pin 9  → Servo Orange Wire (Signal)
UBEC 5V       → Servo Red Wire (Power)
Arduino GND   → Servo Brown Wire (Ground)
```

### Switch Control
```
Arduino Pin 2  → Switch Terminal 1
Switch Terminal 2 → 1K Resistor → Arduino GND
Arduino Pin 2     → 10K Resistor → Arduino 5V (pullup)
```

### Power Filtering
```
Capacitor: UBEC Output → GND (near servo for noise reduction)
```

## 📋 Step-by-Step Wiring

### Step 1: Power Setup
1. Connect UBEC ground to Arduino GND rail on breadboard
2. Connect UBEC 5V output to servo red wire
3. Place capacitor between UBEC output and ground for filtering
4. Test power connections with multimeter

### Step 2: LED Array Setup
1. Insert Red, Green, Blue LEDs into breadboard
2. Connect all LED cathodes (short legs) to GND rail
3. Connect LED anodes through 1K resistors to pins 6, 7, 8
4. Test each LED with simple blink code

### Step 3: Servo Integration
1. Connect servo orange wire to Arduino pin 9
2. Connect servo red wire to UBEC 5V output
3. Connect servo brown wire to GND rail
4. Test servo sweep (0° to 180°)

### Step 4: Switch Installation
1. Mount switch in accessible location
2. Connect one terminal to Arduino pin 2
3. Connect other terminal through 1K resistor to GND
4. Add pullup: pin 2 through 10K resistor to 5V
5. Test switch detection

## 💻 Complete Arduino Code

```cpp
#include <Arduino.h>
#include <Servo.h>

// Pin definitions
const int servoPin = 9;
const int redLED = 6;      // Work phase indicator
const int greenLED = 7;    // Short break indicator
const int blueLED = 8;     // Long break indicator
const int switchPin = 2;   // Control switch

// Servo setup
Servo timerDial;

// Timer state variables
unsigned long sessionStart = 0;
unsigned long sessionDuration = 0;
bool timerRunning = false;
int currentPhase = 0;      // 0=idle, 1=work, 2=short break, 3=long break
int completedSessions = 0;
bool sessionPaused = false;
unsigned long pausedTime = 0;

// Switch handling
bool lastSwitchState = HIGH;
unsigned long lastDebounce = 0;
const unsigned long debounceDelay = 50;

// Timing constants (in milliseconds)
const unsigned long WORK_DURATION = 25UL * 60 * 1000;      // 25 minutes
const unsigned long SHORT_BREAK = 5UL * 60 * 1000;         // 5 minutes
const unsigned long LONG_BREAK = 15UL * 60 * 1000;         // 15 minutes

// For testing, use shorter durations (uncomment these lines)
// const unsigned long WORK_DURATION = 10 * 1000;          // 10 seconds
// const unsigned long SHORT_BREAK = 5 * 1000;             // 5 seconds
// const unsigned long LONG_BREAK = 8 * 1000;              // 8 seconds

void setup() {
  Serial.begin(9600);
  
  // Initialize LED pins
  pinMode(redLED, OUTPUT);
  pinMode(greenLED, OUTPUT);
  pinMode(blueLED, OUTPUT);
  
  // Initialize switch pin
  pinMode(switchPin, INPUT_PULLUP);
  
  // Initialize servo
  timerDial.attach(servoPin);
  timerDial.write(0);  // Start at 0 degrees (session start position)
  
  // Welcome sequence
  performStartupAnimation();
  
  Serial.println("=== POMODORO FOCUS TIMER ===");
  Serial.println("Press switch to start work session");
  Serial.println("Red=Work | Green=Short Break | Blue=Long Break");
  
  resetToIdle();
}

void loop() {
  handleSwitchInput();
  updateTimer();
  updateDisplay();
  delay(100);  // Small delay for stability
}

void handleSwitchInput() {
  bool switchState = digitalRead(switchPin);
  
  // Debounce switch
  if (switchState != lastSwitchState) {
    lastDebounce = millis();
  }
  
  if ((millis() - lastDebounce) > debounceDelay) {
    if (switchState == LOW && lastSwitchState == HIGH) {
      // Switch pressed (falling edge)
      onSwitchPressed();
    }
  }
  
  lastSwitchState = switchState;
}

void onSwitchPressed() {
  if (currentPhase == 0) {
    // Start new work session
    startWorkSession();
  } else if (timerRunning) {
    // Pause current session
    pauseSession();
  } else if (sessionPaused) {
    // Resume paused session
    resumeSession();
  } else {
    // Session completed, start next phase
    startNextPhase();
  }
}

void startWorkSession() {
  currentPhase = 1;
  sessionStart = millis();
  sessionDuration = WORK_DURATION;
  timerRunning = true;
  sessionPaused = false;
  completedSessions++;
  
  Serial.print("Work Session #");
  Serial.print(completedSessions);
  Serial.println(" Started!");
  
  // Visual feedback
  flashCurrentPhaseLED(3);
}

void startShortBreak() {
  currentPhase = 2;
  sessionStart = millis();
  sessionDuration = SHORT_BREAK;
  timerRunning = true;
  sessionPaused = false;
  
  Serial.println("Short Break Started - Time to relax!");
  flashCurrentPhaseLED(2);
}

void startLongBreak() {
  currentPhase = 3;
  sessionStart = millis();
  sessionDuration = LONG_BREAK;
  timerRunning = true;
  sessionPaused = false;
  
  Serial.println("Long Break Started - Great job completing 4 sessions!");
  flashCurrentPhaseLED(4);
}

void pauseSession() {
  if (timerRunning) {
    pausedTime = millis() - sessionStart;
    timerRunning = false;
    sessionPaused = true;
    
    Serial.println("Session Paused");
    
    // Dim the current phase LED to indicate pause
    analogWrite(getCurrentLEDPin(), 50);
  }
}

void resumeSession() {
  if (sessionPaused) {
    sessionStart = millis() - pausedTime;
    timerRunning = true;
    sessionPaused = false;
    
    Serial.println("Session Resumed");
  }
}

void startNextPhase() {
  if (currentPhase == 1) {
    // Work session completed
    if (completedSessions % 4 == 0) {
      // Every 4th session gets a long break
      startLongBreak();
    } else {
      // Regular short break
      startShortBreak();
    }
  } else {
    // Break completed, ready for next work session
    resetToIdle();
  }
}

void resetToIdle() {
  currentPhase = 0;
  timerRunning = false;
  sessionPaused = false;
  sessionDuration = 0;
  
  // Reset display
  allLEDsOff();
  timerDial.write(0);
  
  Serial.println("Ready for next work session - Press switch to start");
}

void updateTimer() {
  if (!timerRunning || sessionDuration == 0) return;
  
  unsigned long elapsed = millis() - sessionStart;
  
  if (elapsed >= sessionDuration) {
    // Session completed
    timerRunning = false;
    sessionPaused = false;
    
    Serial.println("*** SESSION COMPLETE! ***");
    
    // Completion celebration
    performCompletionAnimation();
    
    // Wait for user to start next phase
    Serial.println("Press switch to continue...");
  }
}

void updateDisplay() {
  // Update LED indicators
  allLEDsOff();
  
  if (currentPhase > 0 && (timerRunning || sessionPaused)) {
    int ledPin = getCurrentLEDPin();
    if (sessionPaused) {
      // Breathing effect when paused
      int brightness = (sin(millis() / 1000.0) + 1) * 127;
      analogWrite(ledPin, brightness);
    } else {
      digitalWrite(ledPin, HIGH);
    }
  }
  
  // Update servo position based on progress
  if (timerRunning && sessionDuration > 0) {
    unsigned long elapsed = millis() - sessionStart;
    float progress = (float)elapsed / sessionDuration;
    
    // Map progress to servo angle (0° = start, 180° = complete)
    int servoAngle = (int)(progress * 180);
    servoAngle = constrain(servoAngle, 0, 180);
    
    timerDial.write(servoAngle);
    
    // Debug output every 10 seconds
    if (elapsed % 10000 < 100) {
      Serial.print("Progress: ");
      Serial.print((int)(progress * 100));
      Serial.print("% | Time left: ");
      Serial.print((sessionDuration - elapsed) / 1000);
      Serial.println(" seconds");
    }
  }
}

int getCurrentLEDPin() {
  switch(currentPhase) {
    case 1: return redLED;     // Work
    case 2: return greenLED;   // Short break
    case 3: return blueLED;    // Long break
    default: return -1;
  }
}

void allLEDsOff() {
  digitalWrite(redLED, LOW);
  digitalWrite(greenLED, LOW);
  digitalWrite(blueLED, LOW);
}

void flashCurrentPhaseLED(int times) {
  int ledPin = getCurrentLEDPin();
  if (ledPin == -1) return;
  
  for(int i = 0; i < times; i++) {
    digitalWrite(ledPin, HIGH);
    delay(200);
    digitalWrite(ledPin, LOW);
    delay(200);
  }
}

void performStartupAnimation() {
  Serial.println("Initializing Pomodoro Timer...");
  
  // Servo sweep with LED chase
  for(int angle = 0; angle <= 180; angle += 10) {
    timerDial.write(angle);
    
    // LED chase effect
    allLEDsOff();
    if (angle < 60) digitalWrite(redLED, HIGH);
    else if (angle < 120) digitalWrite(greenLED, HIGH);
    else digitalWrite(blueLED, HIGH);
    
    delay(100);
  }
  
  // Return to start
  allLEDsOff();
  timerDial.write(0);
  delay(500);
  
  Serial.println("Timer ready!");
}

void performCompletionAnimation() {
  // Flash all LEDs
  for(int i = 0; i < 5; i++) {
    digitalWrite(redLED, HIGH);
    digitalWrite(greenLED, HIGH);
    digitalWrite(blueLED, HIGH);
    delay(200);
    allLEDsOff();
    delay(200);
  }
  
  // Servo celebration wiggle
  for(int i = 0; i < 3; i++) {
    timerDial.write(160);
    delay(200);
    timerDial.write(180);
    delay(200);
  }
}
```

## 🖨️ 3D Printing Designs

### Timer Dial Face
**File:** `pomodoro_dial.stl`

**Specifications:**
- Diameter: 120mm
- Thickness: 3mm
- Center servo horn mounting hole
- Time markings: 0, 5, 10, 15, 20, 25 minutes
- Phase labels: "WORK" (red), "BREAK" (green)

**Print Settings:**
- Layer height: 0.2mm
- Infill: 15%
- Supports: No
- Print time: ~1 hour

### Servo Pointer/Hand
**File:** `timer_pointer.stl`

**Specifications:**
- Length: 50mm
- Width: 8mm at base, tapering to 2mm tip
- Thickness: 2mm
- Servo horn attachment point
- Arrow tip design

**Print Settings:**
- Layer height: 0.2mm
- Infill: 100% (for strength)
- Supports: No
- Print time: ~15 minutes

### Project Enclosure
**File:** `pomodoro_case.stl`

**Features:**
- Dimensions: 140mm x 100mm x 80mm
- Arduino mounting posts
- Servo mounting bracket with dial clearance
- LED indicator windows (3x 5mm holes)
- Switch access hole (6mm)
- USB cable slot
- Ventilation slots

**Print Settings:**
- Layer height: 0.3mm
- Infill: 20%
- Supports: Yes (for overhangs)
- Print time: ~3 hours

## 🔧 Assembly Instructions

### Phase 1: Electronics Testing (Day 1)
1. **Breadboard Prototype:**
   - Wire all components according to diagram
   - Upload test code to verify each component
   - Test servo movement (0° to 180°)
   - Test LED indicators
   - Test switch input

2. **Code Testing:**
   - Use shortened durations for quick testing
   - Verify timer logic and phase transitions
   - Test pause/resume functionality
   - Confirm servo positioning accuracy

### Phase 2: 3D Printing (Day 1-2)
1. **Print Schedule:**
   - Start dial face and pointer (Day 1 evening)
   - Print enclosure overnight
   - Print any custom modifications

2. **Post-Processing:**
   - Remove supports carefully
   - Sand contact surfaces
   - Test fit all components

### Phase 3: Final Assembly (Day 2-3)
1. **Mechanical Assembly:**
   - Mount servo in enclosure bracket
   - Attach pointer to servo horn
   - Install dial face with proper clearance
   - Mount Arduino on standoffs

2. **Electronics Integration:**
   - Transfer breadboard circuit to enclosure
   - Install LEDs in indicator windows
   - Mount switch in access hole
   - Route cables cleanly

3. **Final Testing:**
   - Complete system functionality test
   - Calibrate servo positioning
   - Verify all timer phases
   - Test switch responsiveness

## 📖 Usage Guide

### Basic Operation
1. **Start Work Session:** Press switch once - Red LED lights, servo begins moving
2. **Pause Session:** Press switch during active session - LED dims, servo stops
3. **Resume Session:** Press switch while paused - LED brightens, servo continues
4. **Complete Session:** When servo reaches 180°, celebration animation plays
5. **Continue Cycle:** Press switch to start break or next work session

### Visual Indicators
- **Red LED Solid:** Work session active
- **Green LED Solid:** Short break active
- **Blue LED Solid:** Long break active
- **LED Breathing:** Session paused
- **All LEDs Flash:** Session completed
- **Servo Position:** Time progress (0° = start, 180° = complete)

### Session Patterns
**Standard Pomodoro Cycle:**
1. Work (25 min) → Short Break (5 min)
2. Work (25 min) → Short Break (5 min)
3. Work (25 min) → Short Break (5 min)
4. Work (25 min) → **Long Break (15 min)**
5. Repeat from step 1

## ⚙️ Customization Options

### Timing Adjustments
```cpp
// Modify these constants for different durations:
const unsigned long WORK_DURATION = 25UL * 60 * 1000;   // Work time
const unsigned long SHORT_BREAK = 5UL * 60 * 1000;      // Short break
const unsigned long LONG_BREAK = 15UL * 60 * 1000;      // Long break
```

### Alternative Techniques
- **52/17 Method:** 52 minutes work, 17 minutes break
- **90-Minute Cycles:** Based on ultradian rhythms
- **Custom Intervals:** Set your own preferred timing

### Enhanced Features
```cpp
// Add sound alerts
void playAlert() {
  // Connect buzzer to pin 10
  tone(10, 1000, 500);  // 1kHz for 500ms
}

// Add ambient lighting
void setAmbientColor(int r, int g, int b) {
  // Use RGB LED for mood lighting
  analogWrite(redLED, r);
  analogWrite(greenLED, g);
  analogWrite(blueLED, b);
}
```

## 🔍 Troubleshooting

### Servo Issues
**Problem:** Servo jittery or not moving smoothly
- **Solution:** Add capacitor (100µF) between servo power and ground
- **Check:** Verify UBEC can supply enough current (2A minimum)

**Problem:** Servo not reaching full 180° range
- **Solution:** Adjust servo limits in code, check mechanical clearance

### LED Problems
**Problem:** LEDs too dim or not lighting
- **Solution:** Check resistor values (1K for 5V), verify LED polarity

**Problem:** LEDs flickering
- **Solution:** Check breadboard connections, ensure stable power

### Switch Issues
**Problem:** Switch not responding or false triggers
- **Solution:** Increase debounce delay, check pullup resistor value

**Problem:** Multiple triggers from single press
- **Solution:** Improve switch mounting, add better debouncing

### Timer Accuracy
**Problem:** Timer drift over long periods
- **Solution:** Arduino timing is approximate, consider RTC module for precision

**Problem:** Session doesn't pause correctly
- **Solution:** Check switch debouncing, verify pause logic in code

## 📈 Enhancement Ideas

### Level 1 Additions
- **Sound alerts** using piezo buzzer
- **Session counter** display on 7-segment LED
- **Battery power** for portable use
- **Desk mounting** bracket

### Level 2 Features
- **WiFi connectivity** for session logging
- **Mobile app** integration via Bluetooth
- **Multiple timers** for different projects
- **Environmental sensors** (temperature, light)

### Level 3 Integrations
- **Computer integration** - pause when you're away
- **Calendar sync** - automatic work blocks
- **Productivity tracking** - long-term statistics
- **Smart home** integration

## 📊 Expected Results

### Productivity Benefits
- **Improved Focus:** Physical timer reduces digital distractions
- **Better Time Awareness:** Analog display shows progress intuitively
- **Habit Formation:** Tactile interaction reinforces routine
- **Stress Reduction:** Forced breaks prevent burnout

### Technical Learning
- **Servo Control:** PWM signals and positioning
- **State Machines:** Timer logic and phase management
- **User Interface:** Physical interaction design
- **3D Integration:** Mechanical and electronic system combination

## 📅 Project Timeline

**Day 1 (5-6 hours):**
- Electronics prototyping and testing ✓
- Code development and debugging ✓
- Start 3D printing (dial and pointer) ✓

**Day 2 (4-5 hours):**
- Complete 3D printing (enclosure) ✓
- Mechanical assembly and testing ✓
- Electronics integration ✓

**Day 3 (2-3 hours):**
- Final calibration and testing ✓
- Enclosure completion ✓
- Usage optimization ✓

**Total Time: 11-14 hours over 3 days**

This Smart Focus Timer combines proven productivity techniques with hands-on engineering, creating a useful tool that eliminates digital distractions while helping maintain focus and work-life balance!

---

**Perfect for:** Students, remote workers, makers, anyone wanting to improve focus
**Skill Level:** Intermediate
**Useful Life:** Daily productivity tool beyond the hackathon

**Build it once, use it every day! 🍅⏰**