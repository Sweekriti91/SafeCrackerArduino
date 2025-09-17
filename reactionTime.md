# Reaction Time Challenge Game

A reaction time testing game using Arduino, LEDs, servo motor, and a tiny switch. Perfect for a 3-day hackathon project!

## 🎮 Game Description

**Objective:** Test your reaction time by watching for specific LED patterns and hitting the switch at the right moment.

**How to Play:**
1. Game displays random sequences of LED flashes
2. Watch for the target pattern (3 consecutive flashes of the same LED)
3. Hit the switch as soon as you see the target pattern
4. Servo dial shows your reaction time
5. Try to beat your best time!

**Features:**
- 5 LEDs create random flash patterns
- Servo motor displays reaction time on a physical dial
- Tiny switch requires minimal interaction (perfect for small switches)
- Difficulty levels with faster patterns
- Audio feedback through LED timing patterns

## 🧩 Required Parts

**Exact Components Needed:**
- 1x Elegoo UNO R3 (Arduino)
- 5x LEDs (any colors)
- 5x 1K resistors (for LED current limiting)
- 1x 1M resistor (for switch pullup)
- 1x Deegoo FPV MG 996R servo motor
- 1x UBEC DC/DC Converter 5V (for servo power)
- 1x Small tactile switch
- 2x Capacitors (for power filtering)
- Breadboard
- Jumper wires
- 3D printer access

## 🔌 Wiring Diagram

### Power Connections
```
UBEC 5V Output → Servo Red Wire (VCC)
UBEC Ground    → Servo Brown Wire (GND)
UBEC Ground    → Arduino GND
Arduino 5V     → UBEC Input (if needed)
```

### LED Connections
```
Arduino Pin 3  → 1K Resistor → LED 1 → GND
Arduino Pin 4  → 1K Resistor → LED 2 → GND  
Arduino Pin 5  → 1K Resistor → LED 3 → GND
Arduino Pin 6  → 1K Resistor → LED 4 → GND
Arduino Pin 7  → 1K Resistor → LED 5 → GND
```

### Servo Connection
```
Arduino Pin 9  → Servo Orange Wire (Signal)
UBEC 5V       → Servo Red Wire (Power)
Arduino GND   → Servo Brown Wire (Ground)
```

### Switch Connection
```
Arduino Pin 2  → Switch Terminal 1
Switch Terminal 2 → 1M Resistor → Arduino GND
Arduino Pin 2     → 1M Resistor → Arduino 5V (pullup)
```

### Capacitor Connections (Power Filtering)
```
Capacitor 1: Arduino 5V → GND (near Arduino)
Capacitor 2: UBEC Output → GND (near servo)
```

## 📋 Step-by-Step Wiring Instructions

### Step 1: Power Setup
1. Connect UBEC ground to Arduino GND
2. Connect UBEC 5V output to servo red wire
3. Place capacitors for power filtering

### Step 2: LED Array
1. Insert 5 LEDs into breadboard
2. Connect each LED cathode (shorter leg) to GND rail
3. Connect each LED anode (longer leg) through 1K resistor to Arduino pins 3-7

### Step 3: Servo Connection  
1. Connect servo orange wire to Arduino pin 9
2. Connect servo red wire to UBEC 5V output
3. Connect servo brown wire to GND rail

### Step 4: Switch Setup
1. Connect one switch terminal to Arduino pin 2
2. Connect other switch terminal through 1M resistor to GND
3. Add pullup: Pin 2 through 1M resistor to 5V

### Step 5: Final Connections
1. Double-check all power connections
2. Verify LED resistors are properly connected
3. Test continuity with multimeter if available

## 💻 Arduino Code

```cpp
#include <Arduino.h>
#include <Servo.h>

// Pin definitions
const int ledPins[] = {3, 4, 5, 6, 7};
const int switchPin = 2;
const int servoPin = 9;
const int numLeds = 5;

// Game variables
Servo reactionServo;
unsigned long patternStartTime = 0;
unsigned long reactionTime = 0;
bool gameActive = false;
bool patternDetected = false;
int targetLed = 0;
int consecutiveCount = 0;
int bestTime = 9999;
int currentLevel = 1;

// Switch debouncing
bool lastSwitchState = false;
unsigned long lastDebounceTime = 0;
const unsigned long debounceDelay = 50;

void setup() {
  Serial.begin(9600);
  
  // Initialize LED pins
  for(int i = 0; i < numLeds; i++) {
    pinMode(ledPins[i], OUTPUT);
  }
  
  // Initialize switch
  pinMode(switchPin, INPUT_PULLUP);
  
  // Initialize servo
  reactionServo.attach(servoPin);
  reactionServo.write(0); // Start at 0 degrees
  
  // Welcome sequence
  Serial.println("=== REACTION TIME CHALLENGE ===");
  Serial.println("Watch for 3 consecutive flashes of the same LED!");
  Serial.println("Hit the switch as fast as you can!");
  
  // Demo LED sequence
  for(int i = 0; i < 3; i++) {
    allLedsOn();
    delay(200);
    allLedsOff();
    delay(200);
  }
  
  Serial.println("Game starting in 3 seconds...");
  delay(3000);
  
  startNewRound();
}

void loop() {
  // Check switch state
  checkSwitch();
  
  // Run game logic
  if(gameActive) {
    runGamePattern();
  }
  
  delay(10);
}

void checkSwitch() {
  bool currentSwitchState = !digitalRead(switchPin); // Inverted due to pullup
  
  // Debounce switch
  if(currentSwitchState != lastSwitchState) {
    lastDebounceTime = millis();
  }
  
  if((millis() - lastDebounceTime) > debounceDelay) {
    if(currentSwitchState && !lastSwitchState) {
      // Switch pressed!
      if(gameActive && patternDetected) {
        // Calculate reaction time
        reactionTime = millis() - patternStartTime;
        endRound(true);
      } else if(gameActive) {
        // False start
        endRound(false);
      }
    }
  }
  
  lastSwitchState = currentSwitchState;
}

void runGamePattern() {
  static unsigned long lastFlash = 0;
  static int currentLed = 0;
  static int flashCount = 0;
  static bool ledState = false;
  
  unsigned long currentTime = millis();
  unsigned long flashInterval = 500 - (currentLevel * 50); // Faster each level
  
  if(currentTime - lastFlash >= flashInterval) {
    if(!ledState) {
      // Turn on LED
      digitalWrite(ledPins[currentLed], HIGH);
      ledState = true;
      lastFlash = currentTime;
      
      // Check for pattern
      checkPattern(currentLed);
      
    } else {
      // Turn off LED
      digitalWrite(ledPins[currentLed], LOW);
      ledState = false;
      lastFlash = currentTime;
      flashCount++;
      
      // Move to next LED
      currentLed = random(numLeds);
      
      // End round after 20 flashes if no pattern detected
      if(flashCount >= 20) {
        endRound(false);
      }
    }
  }
}

void checkPattern(int ledIndex) {
  static int lastLed = -1;
  static int count = 0;
  
  if(ledIndex == lastLed) {
    count++;
    if(count >= 3 && !patternDetected) {
      // Pattern detected!
      patternDetected = true;
      patternStartTime = millis();
      Serial.println("PATTERN DETECTED! Hit the switch NOW!");
      
      // Flash all LEDs to indicate pattern
      for(int i = 0; i < 3; i++) {
        allLedsOn();
        delay(50);
        allLedsOff();
        delay(50);
      }
    }
  } else {
    count = 1; // Reset count for new LED
  }
  
  lastLed = ledIndex;
}

void startNewRound() {
  gameActive = true;
  patternDetected = false;
  consecutiveCount = 0;
  
  // Reset servo to start position
  reactionServo.write(0);
  
  Serial.print("Level ");
  Serial.print(currentLevel);
  Serial.println(" - Get ready!");
  
  delay(1000);
}

void endRound(bool success) {
  gameActive = false;
  allLedsOff();
  
  if(success) {
    Serial.print("SUCCESS! Reaction time: ");
    Serial.print(reactionTime);
    Serial.println(" ms");
    
    // Update best time
    if(reactionTime < bestTime) {
      bestTime = reactionTime;
      Serial.print("NEW BEST TIME: ");
      Serial.print(bestTime);
      Serial.println(" ms!");
    }
    
    // Show reaction time on servo (0-180 degrees for 0-1000ms)
    int servoAngle = map(reactionTime, 0, 1000, 0, 180);
    servoAngle = constrain(servoAngle, 0, 180);
    reactionServo.write(servoAngle);
    
    // Success LED pattern
    for(int i = 0; i < 5; i++) {
      allLedsOn();
      delay(100);
      allLedsOff();
      delay(100);
    }
    
    currentLevel++;
    if(currentLevel > 5) currentLevel = 5; // Max level
    
  } else {
    Serial.println("MISSED! Too slow or false start.");
    
    // Failure pattern
    for(int i = 0; i < numLeds; i++) {
      digitalWrite(ledPins[i], HIGH);
      delay(200);
      digitalWrite(ledPins[i], LOW);
    }
    
    // Reset level on failure
    currentLevel = 1;
  }
  
  Serial.print("Best time: ");
  Serial.print(bestTime);
  Serial.println(" ms");
  Serial.println("Starting new round in 3 seconds...");
  
  delay(3000);
  startNewRound();
}

void allLedsOn() {
  for(int i = 0; i < numLeds; i++) {
    digitalWrite(ledPins[i], HIGH);
  }
}

void allLedsOff() {
  for(int i = 0; i < numLeds; i++) {
    digitalWrite(ledPins[i], LOW);
  }
}
```

## 🖨️ 3D Printing Guide

### Servo Dial Design
**File to create:** `reaction_dial.stl`

**Specifications:**
- Diameter: 60mm
- Thickness: 3mm
- Servo horn mounting holes
- Scale markings: 0-1000ms
- Pointer arrow

**Print Settings:**
- Layer height: 0.2mm
- Infill: 20%
- Supports: No
- Print time: ~45 minutes

### Game Enclosure
**File to create:** `game_box.stl`

**Features:**
- Arduino mounting posts
- LED holes (5mm diameter)
- Switch access hole
- Servo mounting bracket
- Cable management slots

**Print Settings:**
- Layer height: 0.3mm
- Infill: 15%
- Supports: Yes (for overhangs)
- Print time: ~2 hours

## 🎯 Game Features

### Difficulty Levels
1. **Level 1:** 500ms between flashes
2. **Level 2:** 450ms between flashes  
3. **Level 3:** 400ms between flashes
4. **Level 4:** 350ms between flashes
5. **Level 5:** 300ms between flashes

### Scoring System
- **Excellent:** < 200ms
- **Good:** 200-400ms
- **Average:** 400-600ms
- **Slow:** 600-800ms
- **Too Slow:** > 800ms

### Visual Feedback
- **Pattern Detection:** All LEDs flash rapidly
- **Success:** LEDs chase pattern
- **Failure:** LEDs light sequentially
- **Best Time:** Special celebration pattern

## 🔧 Troubleshooting

### Common Issues

**LEDs not lighting:**
- Check resistor connections
- Verify LED orientation (longer leg = positive)
- Test with multimeter

**Servo not moving:**
- Check UBEC power connections
- Verify servo signal wire on pin 9
- Test servo separately

**Switch not responding:**
- Check pullup resistor connections
- Verify switch is making contact
- Test with multimeter

**Inconsistent timing:**
- Check for loose connections
- Verify power supply stability
- Add capacitors for filtering

### Testing Checklist
1. ✅ All LEDs light individually
2. ✅ Servo moves to different positions
3. ✅ Switch registers press/release
4. ✅ Serial monitor shows game messages
5. ✅ Pattern detection works correctly

## 📈 Possible Enhancements

### Easy Additions
- Sound effects using PWM on unused pins
- Different pattern types (alternating, specific sequences)
- High score memory using EEPROM
- Multiplayer support with multiple switches

### Advanced Features
- LCD display for scores
- Wireless connectivity for online leaderboards
- Custom pattern programming
- Variable difficulty algorithms

## 🏆 Competition Ideas

### Single Player Challenges
- Beat your best time
- Survive 10 rounds without missing
- Speed run through all difficulty levels

### Multiplayer Competitions  
- Head-to-head reaction battles
- Team relay challenges
- Tournament brackets

## 📚 Learning Outcomes

**Programming Concepts:**
- Interrupt handling
- State machines
- Timing and delays
- Random number generation
- Serial communication

**Hardware Skills:**
- Circuit building
- Power distribution
- Signal conditioning
- Mechanical integration

**Game Design:**
- User feedback systems
- Difficulty progression
- Score balancing
- Player engagement

---

**Total Build Time:** 2-3 days
**Skill Level:** Intermediate
**Perfect for:** Hackathons, maker competitions, learning projects

**Have fun building and testing your reaction time!** 🚀