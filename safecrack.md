# Safe-Cracker Game - Implementation Guide

A physical combination lock puzzle game that combines servo-controlled dial movement with LED feedback to create an engaging "crack the safe" experience. Perfect for demonstrating precision control and game logic!

## 🔐 Game Overview

**Concept:** Create a realistic safe-cracking experience using a servo-controlled dial that sweeps through combinations while LEDs provide "hot/cold" feedback. Players must flip the switch at exactly the right moment to lock in their guess.

**Gameplay Flow:**
1. **Setup Phase:** Arduino generates random combination (3 numbers)
2. **Power Control:** Web interface powers the safe on/off
3. **Scanning Phase:** Servo slowly sweeps through dial positions (0-180°)
4. **Feedback Phase:** LEDs show proximity to correct combination
5. **Lock-in Phase:** Player clicks web Lock-In button when they think servo is at correct position
6. **Success/Fail:** Servo either unlocks (full rotation) or resets for next attempt

**Visual Appeal:** Looks like a real safe dial with smooth servo movement and dramatic LED feedback patterns!

## 🧩 Required Parts

**From Your Available Components:**
- 1x Elegoo UNO R3 (Arduino)
- 1x Deegoo FPV MG 996R servo motor
- 4x Single-color LEDs (Orange, Red, Green, Blue)
- 4x 1K resistors (for LED current limiting)
- Breadboard (30×10) and jumper wires
- 3D printer access

**Unused components (save for future projects):**
- 2x 1M resistors (not needed for web control)
- 1x 3-prong tactile switch (replaced by web interface)

## 🎯 Game Mechanics

### Combination System
- **3-digit combination:** Each digit is a servo angle (0-180°)
- **Tolerance zones:** ±5° acceptance range for each number
- **Random generation:** New combination each game
- **Difficulty levels:** Narrow/wide tolerance zones

### LED Feedback System
```
🔴🔵 Red + Blue:     VERY COLD (>40° away)
🔵 Blue only:        COOL (25-40° away)
🟠 Orange only:      WARM (15-25° away)  
🟠🟢 Orange + Green:  HOT (5-15° away)
🟢 Green only:       VERY HOT! (<5° away)
```

### Web Control System
- **Power Control:** Web Power On/Off buttons control the entire safe system
- **Lock-in Timing:** Web Lock-In button must be clicked while servo passes correct angle
- **Precision Challenge:** Lock-In button must be clicked within tolerance window
- **Multiple Attempts:** Failed attempts provide feedback for next try
- **Real-time Debug:** Web interface shows game status and debug information

## 🔌 Wiring Diagram

### Power Distribution
```
12V DC Power Supply  → UBEC Input (red/black wires)
UBEC 5V Output       → Servo Red Wire (dedicated servo power)
UBEC GND Output      → Servo Brown Wire + Arduino GND (shared ground)
Arduino 5V           → Breadboard (+) Rail → LED power
Arduino GND          → Breadboard (-) Rail → LED/Switch ground
Arduino USB          → Computer (Serial + Arduino power)
```

### LED Feedback Array
```
Arduino Pin 3  → 1K Resistor → Orange LED → GND     (Warm indicator)
Arduino Pin 4  → 1K Resistor → Red LED → GND        (Cool indicator)
Arduino Pin 5  → 1K Resistor → Green LED → GND      (Hot indicator)
Arduino Pin 6  → 1K Resistor → Blue LED → GND       (Cold indicator)
```

### Servo Safe Dial
```
Arduino Pin 10       → Servo Orange Wire (Signal) [Pin 9 had issues]
UBEC 5V Output       → Servo Red Wire (Power) [External power required]
UBEC GND Output      → Servo Brown Wire + Arduino GND (CRITICAL!)
12V Power Supply     → UBEC Input (converts to 5V for servo)
```

### Web Control Interface
```
Arduino USB          → Computer (Serial communication at 9600 baud)
Web Interface        → Power On/Off Commands via Serial (P/O)
Web Interface        → Lock-In Command via Serial (L)
Note: No physical switches needed - complete web control
```



## 📋 Detailed Breadboard Wiring Guide

### 📐 Breadboard Specifications
**30-row × 10-column breadboard layout:**
```
Columns:  a b c d e | f g h i j
Rows:     1 2 3 4 5 | 6 7 8 9 10
          ...       | ...
          26 27 28 29 30 | 26 27 28 29 30
```

### Step 1: Power Rail Setup
1. **GND Rail:** Connect Arduino **GND** to breadboard **bottom (-) rail**
2. **5V Rail:** Connect Arduino **5V** to breadboard **top (+) rail**

### Step 2: LED Array Wiring

**Orange LED (Pin 3) - Row 5:**
1. Insert **1K resistor** from **5a** to **5e**
2. Connect **jumper wire** from Arduino **Pin 3** to **5a**
3. Insert **Orange LED**:
   - **Long leg** → **5f**
   - **Short leg** → **bottom (-) rail**

**Red LED (Pin 4) - Row 8:**
1. Insert **1K resistor** from **8a** to **8e**
2. Connect **jumper wire** from Arduino **Pin 4** to **8a**
3. Insert **Red LED**:
   - **Long leg** → **8f**
   - **Short leg** → **bottom (-) rail**

**Green LED (Pin 5) - Row 11:**
1. Insert **1K resistor** from **11a** to **11e**
2. Connect **jumper wire** from Arduino **Pin 5** to **11a**
3. Insert **Green LED**:
   - **Long leg** → **11f**
   - **Short leg** → **bottom (-) rail**

**Blue LED (Pin 6) - Row 14:**
1. Insert **1K resistor** from **14a** to **14e**
2. Connect **jumper wire** from Arduino **Pin 6** to **14a**
3. Insert **Blue LED**:
   - **Long leg** → **14f**
   - **Short leg** → **bottom (-) rail**

### Step 3: Web Control Setup (No Physical Switches)

**Serial Communication Only:**
- Arduino communicates via USB serial connection
- Web application sends commands: 'P' (Power On), 'O' (Power Off), 'L' (Lock-In)
- No additional wiring needed for control interface

### Step 4: Servo Connections (External UBEC Power)

**CRITICAL: Servo needs external power to prevent Arduino resets**

**UBEC Power Setup:**
1. **12V Power Supply** → **UBEC Input** (red/black wires)
2. **UBEC 5V Output** → **Servo Red Wire** (dedicated power)
3. **UBEC GND Output** → **Servo Brown Wire** (servo ground)
4. **UBEC GND Output** → **Arduino GND** (CRITICAL shared ground!)

**Signal Connection:**
1. **Arduino Pin 10** → **Servo Orange Wire** (signal)

**⚠️ Common Ground is Essential:**
Without connecting UBEC GND to Arduino GND, the servo won't respond to signals!

### 📊 Visual Breadboard Layout

```
30×10 Breadboard Layout:
    a b c d e | f g h i j
 1  + + + + + | + + + + +  ← Top (+) rail ← Arduino 5V ← Servo Red
 2  
 3  
 4  
 5  3→●─●─●─● | ●←🟠LED   ← Orange LED + 1K resistor
 6  
 7  
 8  4→●─●─●─● | ●←🔴LED   ← Red LED + 1K resistor
 9  
10  
11  5→●─●─●─● | ●←🟢LED   ← Green LED + 1K resistor
12  
13  
14  6→●─●─●─● | ●←🔵LED   ← Blue LED + 1K resistor
15  
16  
17  
18  
19  
20  2→●─────── | ─────●   ← Switch top prong (Pin 2)
21            |      ●   ← Switch middle prong (unused)
22            |      ●→GND ← Switch bottom prong (to ground)
23  
24  
25  
...
30  - - - - - | - - - - -  ← Bottom (-) rail ← Arduino GND ← Servo Brown
```

### 🔧 Component Placement Summary

| Row | Columns | Component | Arduino Connection |
|-----|---------|-----------|-------------------|
| 5 | a-e, f | Orange LED + 1K resistor | Pin 3 |
| 8 | a-e, f | Red LED + 1K resistor | Pin 4 |
| 11 | a-e, f | Green LED + 1K resistor | Pin 5 |
| 14 | a-e, f | Blue LED + 1K resistor | Pin 6 |
| External | - | UBEC 5V to servo red wire | External Power |
| External | - | UBEC GND to servo brown + Arduino GND | Shared Ground |
| Direct | - | Servo signal (orange wire) | Pin 10 |
| USB | - | Arduino USB to computer | Serial communication |

### ✅ Connection Verification Checklist

**Power connections:**
- [ ] Arduino 5V → Breadboard top (+) rail
- [ ] Arduino GND → Breadboard bottom (-) rail
- [ ] Servo red wire → Breadboard top (+) rail
- [ ] Servo brown wire → Breadboard bottom (-) rail

**LED connections (all follow same pattern):**
- [ ] Arduino Pin 3 → 1K resistor → Orange LED long leg
- [ ] Arduino Pin 4 → 1K resistor → Red LED long leg
- [ ] Arduino Pin 5 → 1K resistor → Green LED long leg
- [ ] Arduino Pin 6 → 1K resistor → Blue LED long leg
- [ ] All LED short legs → Breadboard bottom (-) rail

**Switch connections:**
- [ ] Arduino Pin 2 → Switch top prong (20h)
- [ ] Switch bottom prong (22h) → Breadboard bottom (-) rail
- [ ] Switch middle prong (21h) left unconnected

**Servo connections:**
- [ ] 12V Power Supply connected to UBEC input
- [ ] UBEC 5V output → Servo red wire (external power)
- [ ] UBEC GND output → Servo brown wire (servo ground)
- [ ] UBEC GND output → Arduino GND (CRITICAL shared ground)
- [ ] Servo orange wire → Arduino Pin 10 (signal)

### 🔌 Switch Logic Explanation

**With 3-prong switch + Arduino internal pullup:**
- **Switch not pressed:** Pin 2 reads HIGH (pulled up internally by Arduino)
- **Switch pressed:** Pin 2 reads LOW (top prong connects to bottom prong, connecting to ground)
- **Code setup:** `pinMode(switchPin, INPUT_PULLUP);`

### 🚀 Testing Each Component

**LED Test Code:**
```cpp
void testLEDs() {
  const int ledPins[] = {3, 4, 5, 6};
  const char* colors[] = {"Orange", "Red", "Green", "Blue"};
  
  for(int i = 0; i < 4; i++) {
    Serial.print("Testing ");
    Serial.print(colors[i]);
    Serial.println(" LED");
    digitalWrite(ledPins[i], HIGH);
    delay(1000);
    digitalWrite(ledPins[i], LOW);
    delay(200);
  }
}
```

**Switch Test Code:**
```cpp
void testSwitch() {
  if(digitalRead(2) == LOW) {
    Serial.println("Switch PRESSED");
  } else {
    Serial.println("Switch released");
  }
}
```

**Servo Test Code:**
```cpp
void testServo() {
  for(int angle = 0; angle <= 180; angle += 10) {
    safeDial.write(angle);
    Serial.print("Servo angle: ");
    Serial.println(angle);
    delay(200);
  }
}
```

**Power Verification:**
- Check 12V at power supply output
- Check 5V at UBEC output
- Verify common ground connection

## 💻 Complete Arduino Code

```cpp
#include <Arduino.h>
#include <Servo.h>

// Pin definitions
const int servoPin = 10;  // Pin 10 (Pin 9 didn't work reliably)
const int ledPins[] = {3, 4, 5, 6};  // Orange, Red, Green, Blue
const int switchPin = 2;
const int numLeds = 4;

// LED indices for easy reference
const int ORANGE = 0, RED = 1, GREEN = 2, BLUE = 3;

// Game components
Servo safeDial;
int combination[3];           // The secret combination (3 angles)
int currentNumber = 0;        // Which number we're trying to crack (0, 1, or 2)
int currentAngle = 0;         // Current servo position
bool gameActive = false;
bool sweeping = false;
int sweepDirection = 1;       // 1 for forward, -1 for backward

// Switch handling
bool lastSwitchState = HIGH;
unsigned long lastDebounce = 0;
const unsigned long debounceDelay = 50;

// Game settings
const int SWEEP_SPEED = 2;           // Degrees per update
const int TOLERANCE = 8;             // Degrees tolerance for correct guess
const unsigned long SWEEP_DELAY = 50; // Milliseconds between servo movements

// Timing variables
unsigned long lastSweepTime = 0;
int attemptsRemaining = 3;

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
  safeDial.attach(servoPin);
  safeDial.write(0);
  
  // Welcome sequence
  performWelcomeAnimation();
  
  // Generate new combination and start game
  generateNewCombination();
  displayGameInfo();
  
  Serial.println("=== SAFE-CRACKER GAME ===");
  Serial.println("Watch the LEDs for hot/cold feedback!");
  Serial.println("Flip switch when dial passes correct angle!");
  
  startNewRound();
}

void loop() {
  handleSwitchInput();
  updateSweeping();
  updateLEDFeedback();
  delay(10);
}

void generateNewCombination() {
  Serial.println("Generating new combination...");
  
  for(int i = 0; i < 3; i++) {
    combination[i] = random(20, 160);  // Avoid extreme ends for better gameplay
    
    // Ensure numbers aren't too close together
    if(i > 0) {
      while(abs(combination[i] - combination[i-1]) < 20) {
        combination[i] = random(20, 160);
      }
    }
  }
  
  currentNumber = 0;
  attemptsRemaining = 3;
}

void displayGameInfo() {
  Serial.println("--- NEW SAFE COMBINATION ---");
  Serial.print("Secret combination: ");
  for(int i = 0; i < 3; i++) {
    Serial.print(combination[i]);
    if(i < 2) Serial.print("-");
  }
  Serial.println(" (for debugging)");
  Serial.print("Now cracking number ");
  Serial.print(currentNumber + 1);
  Serial.print(" of 3");
  Serial.println();
}

void startNewRound() {
  gameActive = true;
  sweeping = true;
  currentAngle = 0;
  sweepDirection = 1;
  
  safeDial.write(currentAngle);
  allLEDsOff();
  
  Serial.print("=== ROUND ");
  Serial.print(currentNumber + 1);
  Serial.print(" of 3 ===");
  Serial.println();
  Serial.print("Target: ");
  Serial.print(combination[currentNumber]);
  Serial.print("° (±");
  Serial.print(TOLERANCE);
  Serial.println("°)");
  Serial.println("Dial sweeping... watch for GREEN LED!");
}

void handleSwitchInput() {
  bool switchState = digitalRead(switchPin);
  
  // Debounce switch
  if(switchState != lastSwitchState) {
    lastDebounce = millis();
  }
  
  if((millis() - lastDebounce) > debounceDelay) {
    if(switchState == LOW && lastSwitchState == HIGH) {
      // Switch pressed!
      if(gameActive && sweeping) {
        attemptLockIn();
      } else if(!gameActive) {
        // Game over, start new game
        generateNewCombination();
        displayGameInfo();
        startNewRound();
      }
    }
  }
  
  lastSwitchState = switchState;
}

void attemptLockIn() {
  sweeping = false;
  
  int target = combination[currentNumber];
  int distance = abs(currentAngle - target);
  
  Serial.print("Lock-in attempt at ");
  Serial.print(currentAngle);
  Serial.print("° (target: ");
  Serial.print(target);
  Serial.print("°, distance: ");
  Serial.print(distance);
  Serial.println("°)");
  
  if(distance <= TOLERANCE) {
    // Correct guess!
    Serial.println("*** CORRECT! ***");
    
    // Success animation
    successAnimation();
    
    currentNumber++;
    
    if(currentNumber >= 3) {
      // All numbers cracked - safe opens!
      safeUnlocked();
    } else {
      // Next number
      Serial.println("Moving to next number...");
      delay(2000);
      startNewRound();
    }
    
  } else {
    // Wrong guess
    attemptsRemaining--;
    Serial.print("Wrong! ");
    Serial.print(attemptsRemaining);
    Serial.println(" attempts remaining.");
    
    // Failure feedback
    failureAnimation();
    
    if(attemptsRemaining <= 0) {
      // Game over
      gameOver();
    } else {
      // Try again
      delay(1500);
      startNewRound();
    }
  }
}

void updateSweeping() {
  if(!sweeping || !gameActive) return;
  
  if(millis() - lastSweepTime >= SWEEP_DELAY) {
    lastSweepTime = millis();
    
    // Move servo
    currentAngle += sweepDirection * SWEEP_SPEED;
    
    // Reverse direction at limits
    if(currentAngle >= 180) {
      currentAngle = 180;
      sweepDirection = -1;
    } else if(currentAngle <= 0) {
      currentAngle = 0;
      sweepDirection = 1;
    }
    
    safeDial.write(currentAngle);
  }
}

void updateLEDFeedback() {
  if(!sweeping || !gameActive) return;
  
  int target = combination[currentNumber];
  int distance = abs(currentAngle - target);
  
  // Clear all LEDs first
  allLEDsOff();
  
  // Set LEDs based on distance to target
  if(distance <= 5) {
    // Very hot - GREEN only
    digitalWrite(ledPins[GREEN], HIGH);
  } else if(distance <= 15) {
    // Hot - ORANGE + GREEN
    digitalWrite(ledPins[ORANGE], HIGH);
    digitalWrite(ledPins[GREEN], HIGH);
  } else if(distance <= 25) {
    // Warm - ORANGE only
    digitalWrite(ledPins[ORANGE], HIGH);
  } else if(distance <= 40) {
    // Cool - BLUE only
    digitalWrite(ledPins[BLUE], HIGH);
  } else {
    // Very Cold - RED + BLUE
    digitalWrite(ledPins[RED], HIGH);
    digitalWrite(ledPins[BLUE], HIGH);
  }
}

void safeUnlocked() {
  gameActive = false;
  
  Serial.println("");
  Serial.println("🎉 *** SAFE CRACKED! *** 🎉");
  Serial.println("The safe is now unlocked!");
  
  // Epic unlock animation
  unlockAnimation();
  
  Serial.println("");
  Serial.println("Press switch to play again!");
}

void gameOver() {
  gameActive = false;
  
  Serial.println("");
  Serial.println("💥 GAME OVER 💥");
  Serial.println("Safe alarm triggered!");
  Serial.print("The combination was: ");
  for(int i = 0; i < 3; i++) {
    Serial.print(combination[i]);
    if(i < 2) Serial.print("-");
  }
  Serial.println();
  
  // Alarm animation
  alarmAnimation();
  
  Serial.println("Press switch to try again!");
}

void allLEDsOff() {
  for(int i = 0; i < numLeds; i++) {
    digitalWrite(ledPins[i], LOW);
  }
}

void allLEDsOn() {
  for(int i = 0; i < numLeds; i++) {
    digitalWrite(ledPins[i], HIGH);
  }
}

void performWelcomeAnimation() {
  Serial.println("Initializing Safe-Cracker...");
  
  // Servo sweep with LED chase
  for(int angle = 0; angle <= 180; angle += 10) {
    safeDial.write(angle);
    
    // LED chase effect
    allLEDsOff();
    int ledIndex = map(angle, 0, 180, 0, numLeds - 1);
    digitalWrite(ledPins[ledIndex], HIGH);
    
    delay(100);
  }
  
  // Return to start
  safeDial.write(0);
  allLEDsOff();
  delay(500);
}

void successAnimation() {
  // Flash green LED
  for(int i = 0; i < 5; i++) {
    digitalWrite(ledPins[GREEN], HIGH);
    delay(200);
    digitalWrite(ledPins[GREEN], LOW);
    delay(200);
  }
}

void failureAnimation() {
  // Flash red and blue LEDs (error indication)
  for(int i = 0; i < 3; i++) {
    digitalWrite(ledPins[RED], HIGH);
    digitalWrite(ledPins[BLUE], HIGH);
    delay(300);
    digitalWrite(ledPins[RED], LOW);
    digitalWrite(ledPins[BLUE], LOW);
    delay(300);
  }
}

void unlockAnimation() {
  // Celebratory LED sequence
  for(int round = 0; round < 3; round++) {
    // Chase through all colors
    digitalWrite(ledPins[BLUE], HIGH);
    delay(150);
    allLEDsOff();
    
    digitalWrite(ledPins[RED], HIGH);
    delay(150);
    allLEDsOff();
    
    digitalWrite(ledPins[ORANGE], HIGH);
    delay(150);
    allLEDsOff();
    
    digitalWrite(ledPins[GREEN], HIGH);
    delay(150);
    allLEDsOff();
    delay(150);
  }
  
  // Final celebration with servo
  for(int i = 0; i < 3; i++) {
    // Servo full rotation to simulate unlocking
    for(int angle = 0; angle <= 180; angle += 20) {
      safeDial.write(angle);
      allLEDsOn();
      delay(50);
      allLEDsOff();
      delay(50);
    }
  }
  
  safeDial.write(90); // Park in middle
}

void alarmAnimation() {
  // Rapid red flashing alarm
  for(int i = 0; i < 10; i++) {
    allLEDsOn();
    delay(100);
    allLEDsOff();
    delay(100);
  }
  
  // Servo shaking motion
  for(int i = 0; i < 5; i++) {
    safeDial.write(45);
    delay(200);
    safeDial.write(135);
    delay(200);
  }
  
  safeDial.write(0); // Return to start
}
```

## 🖨️ 3D Printing Designs

### Safe Dial Face
**File:** `safe_dial.stl`

**Specifications:**
- Diameter: 100mm
- Thickness: 5mm
- Center servo horn mounting hole
- Number markings: 0-100 around perimeter
- Safe-style design with tick marks

**Print Settings:**
- Layer height: 0.2mm
- Infill: 20%
- Supports: No
- Print time: ~1.5 hours

### Dial Pointer/Handle
**File:** `dial_pointer.stl`

**Specifications:**
- Classic safe dial handle design
- Length: 25mm
- Servo horn attachment point
- Ergonomic grip texture

**Print Settings:**
- Layer height: 0.2mm
- Infill: 100% (for durability)
- Supports: No
- Print time: ~20 minutes

### Safe Enclosure/Vault
**File:** `safe_enclosure.stl`

**Features:**
- Safe-like appearance with thick walls
- Dial window with clearance for movement
- LED indicator slots arranged like alarm lights
- Switch mounting hole (labeled "LOCK-IN")
- Arduino mounting compartment
- Bank vault aesthetic details

**Print Settings:**
- Layer height: 0.3mm
- Infill: 25%
- Supports: Yes (for overhangs)
- Print time: ~4 hours

## 🎮 Gameplay Mechanics

### Difficulty Levels

**Easy Mode:**
- Tolerance: ±10°
- Slow sweep speed
- Extended LED feedback zones
- 5 attempts per number

**Normal Mode:**
- Tolerance: ±8°
- Medium sweep speed
- Standard LED feedback
- 3 attempts per number

**Expert Mode:**
- Tolerance: ±5°
- Fast sweep speed
- Narrow LED feedback zones
- 2 attempts per number

### Scoring System
```cpp
int calculateScore() {
  int baseScore = 1000;
  int penaltyPerAttempt = 100;
  int timeBonus = max(0, 300 - (gameTimeSeconds / 10));
  
  return baseScore - (attemptsUsed * penaltyPerAttempt) + timeBonus;
}
```

### Advanced Features
- **Multi-stage combinations:** More than 3 numbers
- **Time pressure mode:** Must crack within time limit
- **Memory challenge:** Remember previous combinations
- **Progressive difficulty:** Each round gets harder

## 🔧 Assembly Instructions

### Phase 1: Electronics Prototyping (Day 1)
1. **Breadboard Setup:**
   - Wire all components per diagram
   - Test each LED individually
   - Verify servo smooth movement
   - Test switch debouncing

2. **Code Testing:**
   - Upload basic component tests
   - Verify LED feedback patterns
   - Test combination generation
   - Debug switch timing

### Phase 2: 3D Printing (Day 1-2)
1. **Print Queue:**
   - Dial face and pointer (Day 1 evening)
   - Safe enclosure (overnight/Day 2)
   - Test fit and modifications

2. **Post-Processing:**
   - Remove supports carefully
   - Sand contact surfaces smooth
   - Test servo horn fit
   - Check component clearances

### Phase 3: Integration (Day 2-3)
1. **Mechanical Assembly:**
   - Mount servo in enclosure
   - Attach dial face and pointer
   - Install LED array in indicator slots
   - Mount switch with clear labeling

2. **Final Testing:**
   - Complete gameplay testing
   - Calibrate servo positioning
   - Verify LED feedback accuracy
   - Test multiple game rounds

## 📖 Usage Guide

### How to Play
1. **Game Start:** Click "POWER ON" button in web interface
2. **Combination Generation:** Arduino generates random 3-number combination
3. **Round 1:** Servo sweeps dial while LEDs show hot/cold feedback
4. **Lock-in:** Click "LOCK-IN" button when dial passes correct angle for first number
5. **Success/Retry:** Correct guess advances to next number; wrong guess uses attempt
6. **Complete:** Crack all 3 numbers to unlock the safe!
7. **Power Off:** Click "POWER OFF" button when done

### LED Feedback Legend
- **🔴� Red + Blue:** Very Cold (far from target)
- **� Blue Only:** Cool (getting warmer)
- **� Orange Only:** Warm (close)
- **�🟢 Orange + Green:** Hot (very close)
- **🟢 Green Only:** Very Hot! (lock in now!)

### Visual Cues
- **Smooth dial sweep:** Game active, watch for feedback
- **Web interface status:** Shows connection and game state
- **LED patterns:** Hot/cold proximity feedback
- **Power button states:** Green for on, gray for off, red disabled lock-in
- **Rapid LED flash:** Success/failure feedback after lock-in
- **Full rotation:** Safe unlocked!

## 🌐 Web Controller Interface

### Features
- **Connection Status:** Shows Arduino connection and port
- **Game Status:** Real-time updates from Arduino
- **Lock-In Button:** Large, prominent button for critical timing
- **Debug Information:** Toggleable section showing:
  - Current combination (for debugging)
  - Round number and attempts remaining
  - Last lock-in attempt details
  - Full Arduino serial output log

### Usage
1. **Start Web Interface:** Use VS Code to run the web application (see deployment guide below)
2. **Open Browser:** Navigate to http://localhost:5001
3. **Power On Safe:** Click "POWER ON" button in web interface
4. **Watch LEDs:** Monitor hot/cold feedback on physical device
5. **Lock-In:** Click "LOCK-IN" button when GREEN LED appears
6. **Monitor Progress:** Use debug section to track game state
7. **Power Off:** Click "POWER OFF" button to end game

### Technical Implementation
- **ASP.NET Core 9.0:** Minimal API serving HTML interface
- **Serial Communication:** Real-time data exchange with Arduino
- **Command Protocol:** Sends 'L' character for lock-in commands
- **Data Parsing:** Extracts game state from Arduino serial output

### Timing Adjustments
```cpp
// Modify these for different gameplay feel:
const int SWEEP_SPEED = 2;           // Degrees per step (1=slow, 5=fast)
const int TOLERANCE = 8;             // Acceptance range (5=hard, 15=easy)
const unsigned long SWEEP_DELAY = 50; // Milliseconds between movements
```

### LED Feedback Zones
```cpp
// Adjust feedback sensitivity:
if(distance <= 5)       // GREEN only (Very Hot)
if(distance <= 15)      // ORANGE + GREEN (Hot)  
if(distance <= 25)      // ORANGE only (Warm)
if(distance <= 40)      // BLUE only (Cool)
else                    // RED + BLUE (Very Cold)
```

### Sound Enhancement
```cpp
// Add buzzer for audio feedback
void playProximityTone(int distance) {
  int frequency = map(distance, 0, 90, 2000, 200);
  tone(buzzerPin, frequency, 100);
}
```

## � Critical Build Issues & Solutions

### Issue 1: Arduino Keeps Resetting
**Problem:** Arduino resets continuously when servo tries to move
**Cause:** Servo draws too much current from Arduino 5V pin
**Solution:** Use external UBEC power supply for servo
```
12V Power Supply → UBEC → 5V Servo Power
Arduino 5V → LEDs only (low current)
```

### Issue 2: Servo Gets Power But Won't Move
**Problem:** 5V reaches servo but no movement occurs
**Cause:** Missing common ground between Arduino and UBEC
**Solution:** Connect UBEC GND to Arduino GND
```
UBEC GND Output → Arduino GND (shared reference)
```

### Issue 3: Pin 9 Servo Signal Issues
**Problem:** Servo doesn't respond reliably on Pin 9
**Cause:** Possible pin conflict or interference
**Solution:** Use Pin 10 for servo signal instead
```
const int servoPin = 10;  // More reliable than Pin 9
```

### Issue 4: Switch Always Reads LOW
**Problem:** SPDT switch stuck in one position
**Cause:** Wrong pin assignment for switch type
**Solution:** Use center pin to Pin 2, one outer pin to GND
```
Switch Center (20h) → Arduino Pin 2
Switch Outer (22h) → GND Rail
Switch Other (21h) → Not connected
```

## �🔍 Troubleshooting

### Servo Issues
**Problem:** Jerky or inconsistent movement
- **Solution:** Check power connections, verify Arduino 5V output
- **Check:** Monitor serial output for any power-related resets

**Problem:** Servo doesn't reach full range
- **Solution:** Adjust code limits, check mechanical binding

**Problem:** Arduino resets during servo movement
- **Solution:** If power issues occur, consider external 5V supply

### LED Problems
**Problem:** Inconsistent feedback patterns
- **Solution:** Verify wiring, check resistor values
- **Debug:** Use Serial monitor to track distance calculations

### Switch Timing
**Problem:** Switch not registering during sweep
- **Solution:** Increase debounce delay, check pullup resistor
- **Timing:** Ensure switch press duration matches sweep speed

### Game Logic
**Problem:** Combinations too easy/hard
- **Solution:** Adjust tolerance values and feedback zones
- **Balance:** Test with multiple players for difficulty tuning

## 📈 Enhancement Ideas

### Level 1 Additions
- **Sound effects** using piezo buzzer
- **Score display** on 7-segment LED
- **Time pressure** mode with countdown
- **Hint system** for struggling players

### Level 2 Features
- **Multiple safe types** with different mechanics
- **Network multiplayer** via WiFi module
- **Mobile app** companion for statistics
- **Physical lock** mechanism that actually opens

### Level 3 Advanced
- **Computer vision** to detect player gestures
- **Voice commands** for hands-free operation
- **Escape room integration** with other puzzles
- **Professional installation** in entertainment venues

## 📊 Expected Results

### Entertainment Value
- **Engaging gameplay:** Physical interaction beats screen-based games
- **Skill development:** Timing and pattern recognition
- **Replayability:** Random combinations keep it fresh
- **Social appeal:** Great for groups and demonstrations

### Technical Learning
- **Precision control:** Servo positioning and timing
- **Feedback systems:** LED patterns and user interaction
- **Game logic:** State machines and scoring
- **Physical computing:** Mechanical-electronic integration

## 📅 Project Timeline

**Day 1 (5-6 hours):**
- Electronics prototyping and testing ✓
- Game logic development ✓
- Start 3D printing (dial components) ✓

**Day 2 (4-5 hours):**
- Complete 3D printing (enclosure) ✓
- Mechanical assembly ✓
- System integration and testing ✓

**Day 3 (2-3 hours):**
- Final calibration and balancing ✓
- Aesthetic finishing ✓
- Gameplay testing and optimization ✓

**Total Time: 11-14 hours over 3 days**

This Safe-Cracker game combines the thrill of puzzle-solving with impressive physical mechanics, creating an engaging experience that showcases both programming skills and mechanical engineering!

---

## 🚀 How to Deploy - VS Code Guide

### Prerequisites
- VS Code installed with C# Dev Kit extension
- .NET 9.0 SDK installed
- Arduino IDE or PlatformIO extension
- Arduino connected via USB

### Step 1: Open Project in VS Code
1. **Open VS Code**
2. **File → Open Folder**
3. **Navigate to:** `/Users/sweeky/WorkStuff/hack2025`
4. **Select folder** and click "Open"
5. **Trust the workspace** when prompted

### Step 2: Upload Arduino Code
1. **Open Arduino file:** `ReactionTime/src/main.cpp`
2. **Install PlatformIO extension** if not already installed:
   - Go to Extensions (Ctrl+Shift+X)
   - Search "PlatformIO IDE"
   - Click Install
3. **Build and upload Arduino code:**
   - Open Command Palette (Ctrl+Shift+P)
   - Type "PlatformIO: Upload"
   - Select the upload command
   - Wait for upload to complete

### Step 3: Start Web Application
1. **Open terminal in VS Code:** View → Terminal (Ctrl+`)
2. **Navigate to web project:**
   ```
   cd ArduinoWebController
   ```
3. **Restore dependencies:**
   ```
   dotnet restore
   ```
4. **Run the application:**
   ```
   dotnet run
   ```
5. **Look for output:** "Starting Safe-Cracker web controller at http://localhost:5001"

### Step 4: Open Web Interface
1. **Open browser** (Chrome, Firefox, Safari, etc.)
2. **Navigate to:** http://localhost:5001
3. **Verify connection status** shows "Connected"
4. **Check Arduino serial output** in VS Code terminal for debug info

### Step 5: Play the Game!
1. **Click "POWER ON"** to start the game
2. **Watch the LEDs** on your breadboard for hot/cold feedback
3. **Click "LOCK-IN"** when you see the GREEN LED
4. **Complete all 3 rounds** to crack the safe
5. **Click "POWER OFF"** when finished

### Troubleshooting in VS Code

**Arduino Upload Issues:**
- Check USB connection
- Verify correct COM port in `platformio.ini`
- Try different USB cable
- Check Arduino is recognized in Device Manager

**Web App Won't Start:**
- Ensure .NET 9.0 SDK is installed
- Run `dotnet --version` in terminal
- Check for any error messages in terminal
- Try `dotnet clean` then `dotnet run`

**Serial Connection Problems:**
- Check Arduino is connected and programmed
- Verify COM port in Program.cs matches actual port
- Close Arduino IDE Serial Monitor if open
- Try unplugging/reconnecting Arduino

**LED/Servo Issues:**
- Verify all wiring connections
- Check power supply connections
- Ensure UBEC is providing 5V to servo
- Test individual components with simple code

### VS Code Extensions for Better Development
1. **C# Dev Kit** - Essential for .NET development
2. **PlatformIO IDE** - Arduino code compilation and upload
3. **Serial Monitor** - View Arduino output directly in VS Code
4. **Live Server** - For any HTML/CSS tweaks
5. **GitLens** - If you want to track changes

### Development Workflow
1. **Edit Arduino code** in `ReactionTime/src/main.cpp`
2. **Upload changes** using PlatformIO commands
3. **Modify web interface** in `ArduinoWebController/Program.cs`
4. **Restart web app** (Ctrl+C then `dotnet run`)
5. **Test changes** in browser
6. **Monitor serial output** in VS Code terminal

### Project Structure in VS Code
```
hack2025/
├── ArduinoWebController/          ← Web application
│   ├── Program.cs                 ← Main web server code
│   └── ArduinoWebController.csproj
├── ReactionTime/                  ← Arduino project
│   ├── src/main.cpp              ← Arduino game code
│   └── platformio.ini             ← Arduino configuration
└── safecrack.md                   ← This documentation
```

**Ready to crack some safes! 🔐⚡**

**Perfect for:** Game enthusiasts, puzzle lovers, demonstration projects
**Skill Level:** Intermediate
**Entertainment Value:** High replayability and social appeal