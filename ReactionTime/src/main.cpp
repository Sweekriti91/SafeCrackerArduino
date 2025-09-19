#include <Arduino.h>
#include <Servo.h>

// Pin definitions
const int servoPin = 10;  // Working servo pin
const int ledPins[] = {3, 4, 5, 6};  // Orange, Red, Green, Blue
const int numLeds = 4;

// LED indices for easy reference
const int ORANGE = 0, RED = 1, GREEN = 2, BLUE = 3;

// Cam Lock Positions for 3D Printed Sliding Lid
const int LOCKED_ANGLE = 0;      // Cam flat edge blocks lid
const int UNLOCKED_ANGLE = 120;  // Cam round edge allows sliding
const int FULL_OPEN_ANGLE = 180; // Full celebration position

// Function declarations
void generateNewCombination();
void displayGameInfo();
void startNewRound();
void handleSerialInput();
void attemptLockIn();
void updateSweeping();
void updateLEDFeedback();
void safeUnlocked();
void gameOver();
void allLEDsOff();
void allLEDsOn();
void performWelcomeAnimation();
void successAnimation();
void failureAnimation();
void unlockAnimation();
void alarmAnimation();
void gameOffMode();
void startGame();
void endGame();
void sendGameStatus();
void testAllLEDs();
void displayLockStatus();

// Game components
Servo safeDial;
int targetAngle = 0;          // The target angle to find
int currentAngle = 0;         // Current servo position
bool gameActive = false;
bool sweeping = false;
bool gameOn = false;          // Game power state (controlled by web)
int sweepDirection = 1;       // 1 for forward, -1 for backward

// Game settings
int difficulty = 1;           // 0=Easy, 1=Normal, 2=Expert
const int TOLERANCE[] = {12, 8, 5};  // Degrees tolerance by difficulty
const int SWEEP_SPEED[] = {1, 2, 3}; // Degrees per update by difficulty
const unsigned long SWEEP_DELAY = 50; // Milliseconds between servo movements

// Scoring
int score = 0;
int bonusMultiplier[] = {1, 2, 3};  // Score multiplier by difficulty
const int BASE_SCORE = 100;
const int TIME_BONUS = 50;
const int ACCURACY_BONUS = 30;

// Timing variables
unsigned long lastSweepTime = 0;
unsigned long gameStartTime = 0;
unsigned long lockInTime = 0;
int attemptsRemaining = 3;
int attemptsUsed = 0;

void setup() {
  Serial.begin(9600);
  
  // Initialize LED pins
  for(int i = 0; i < numLeds; i++) {
    pinMode(ledPins[i], OUTPUT);
    digitalWrite(ledPins[i], LOW);
  }
  
  // Initialize servo and lock safe
  safeDial.attach(servoPin);
  safeDial.write(LOCKED_ANGLE);  // Start locked
  
  Serial.println("=== SAFE-CRACKER WITH CAM-LOCK LID ===");
  Serial.println("STATUS:STANDBY");
  displayLockStatus();
  
  // Show power-off state
  gameOffMode();
}

void loop() {
  handleSerialInput();   // Web commands
  
  if(gameOn && gameActive) {
    updateSweeping();
    updateLEDFeedback();
    sendGameStatus();
  }
  
  delay(10);
}

void handleSerialInput() {
  if (Serial.available() > 0) {
    String command = Serial.readStringUntil('\n');
    command.trim();
    
    Serial.print("RECEIVED COMMAND: ");
    Serial.println(command);
    
    if (command == "P") {
      // Power ON command from web
      startGame();
    }
    else if (command == "O") {
      // Power OFF command from web
      endGame();
    }
    else if (command == "L" && gameOn && gameActive && sweeping) {
      // Web lock-in command
      attemptLockIn();
    }
    else if (command == "T") {
      // Test all LEDs
      testAllLEDs();
    }
    else if (command == "U") {
      // Debug: Manual unlock
      Serial.println("DEBUG: Manual unlock");
      safeDial.write(UNLOCKED_ANGLE);
      delay(500);
      displayLockStatus();
    }
    else if (command == "K") {
      // Debug: Manual lock
      Serial.println("DEBUG: Manual lock");
      safeDial.write(LOCKED_ANGLE);
      delay(500);
      displayLockStatus();
    }
    else if (command.startsWith("D:")) {
      // Set difficulty: D:0 (Easy), D:1 (Normal), D:2 (Expert)
      difficulty = command.substring(2).toInt();
      difficulty = constrain(difficulty, 0, 2);
      Serial.print("DIFFICULTY:");
      Serial.println(difficulty);
    }
  }
}

void displayLockStatus() {
  int currentPos = safeDial.read();
  if(currentPos >= UNLOCKED_ANGLE - 10) {
    Serial.println("LOCK_STATUS:UNLOCKED - Lid can slide open!");
  } else {
    Serial.println("LOCK_STATUS:LOCKED - Cam blocking lid");
  }
}

void testAllLEDs() {
  Serial.println("STATUS:LED_TEST");
  
  // Test each LED individually
  for(int i = 0; i < numLeds; i++) {
    allLEDsOff();
    digitalWrite(ledPins[i], HIGH);
    Serial.print("LED_TEST:");
    switch(i) {
      case ORANGE: Serial.println("ORANGE"); break;
      case RED: Serial.println("RED"); break;
      case GREEN: Serial.println("GREEN"); break;
      case BLUE: Serial.println("BLUE"); break;
    }
    delay(500);
  }
  
  // All LEDs on
  allLEDsOn();
  Serial.println("LED_TEST:ALL_ON");
  delay(1000);
  
  // All LEDs off
  allLEDsOff();
  Serial.println("LED_TEST:COMPLETE");
}

void startGame() {
  if(!gameOn) {
    gameOn = true;
    score = 0;
    attemptsUsed = 0;
    gameStartTime = millis();
    
    Serial.println("STATUS:POWER_ON");
    Serial.print("DIFFICULTY:");
    Serial.println(difficulty);
    
    // Lock the safe at start
    safeDial.write(LOCKED_ANGLE);
    delay(500);
    displayLockStatus();
    
    // Welcome sequence
    performWelcomeAnimation();
    
    // Generate combination and start
    generateNewCombination();
    displayGameInfo();
    startNewRound();
  }
}

void endGame() {
  if(gameOn) {
    gameOn = false;
    gameActive = false;
    sweeping = false;
    
    Serial.println("STATUS:POWER_OFF");
    Serial.print("FINAL_SCORE:");
    Serial.println(score);
    
    // Lock safe and turn off all LEDs
    safeDial.write(LOCKED_ANGLE);
    allLEDsOff();
    delay(500);
    displayLockStatus();
    
    // Show standby mode
    gameOffMode();
  }
}

void generateNewCombination() {
  Serial.println("STATUS:GENERATING_COMBINATION");
  
  // Generate single target angle between 20 and 90 degrees (keep in sweep range)
  targetAngle = random(20, 90);
  
  attemptsRemaining = 3;
}

void displayGameInfo() {
  Serial.print("TARGET:");
  Serial.println(targetAngle);
  
  Serial.print("TOLERANCE:");
  Serial.println(TOLERANCE[difficulty]);
  
  Serial.print("ATTEMPTS:");
  Serial.println(attemptsRemaining);
  
  Serial.print("SCORE:");
  Serial.println(score);
}

void startNewRound() {
  gameActive = true;
  sweeping = true;
  currentAngle = LOCKED_ANGLE;  // Start from locked position
  sweepDirection = 1;
  
  safeDial.write(currentAngle);
  allLEDsOff();
  
  Serial.println("STATUS:SWEEPING");
}

void attemptLockIn() {
  sweeping = false;
  attemptsUsed++;
  lockInTime = millis();
  
  int distance = abs(currentAngle - targetAngle);
  int tolerance = TOLERANCE[difficulty];
  
  Serial.print("LOCK_ATTEMPT:");
  Serial.print(currentAngle);
  Serial.print(",");
  Serial.print(targetAngle);
  Serial.print(",");
  Serial.print(distance);
  Serial.print(",");
  Serial.println(attemptsRemaining);
  
  if(distance <= tolerance) {
    // Success! Calculate score
    int accuracyScore = map(distance, 0, tolerance, ACCURACY_BONUS, 0);
    int timeBonus = 0;
    unsigned long timeTaken = (lockInTime - gameStartTime) / 1000; // seconds
    
    if(timeTaken < 30) {
      timeBonus = TIME_BONUS * (30 - timeTaken) / 30;
    }
    
    int roundScore = (BASE_SCORE + accuracyScore + timeBonus) * bonusMultiplier[difficulty];
    score += roundScore;
    
    Serial.println("RESULT:CORRECT");
    Serial.print("ROUND_SCORE:");
    Serial.println(roundScore);
    Serial.print("TOTAL_SCORE:");
    Serial.println(score);
    
    // Success animation then unlock
    successAnimation();
    safeUnlocked();
    
  } else {
    // Wrong guess
    attemptsRemaining--;
    Serial.print("RESULT:WRONG,");
    Serial.println(attemptsRemaining);
    
    // Failure feedback
    failureAnimation();
    
    if(attemptsRemaining <= 0) {
      // Game over
      gameOver();
    } else {
      // Try again - return to sweeping
      delay(1500);
      sweeping = true;
      Serial.println("STATUS:SWEEPING");
    }
  }
}

void updateSweeping() {
  if(!sweeping || !gameActive || !gameOn) return;
  
  if(millis() - lastSweepTime >= SWEEP_DELAY) {
    lastSweepTime = millis();
    
    // Move servo based on difficulty speed
    currentAngle += sweepDirection * SWEEP_SPEED[difficulty];
    
    // Don't go past 90° during gameplay (keep it locked)
    if(currentAngle >= 90) {
      currentAngle = 90;
      sweepDirection = -1;
    } else if(currentAngle <= 0) {
      currentAngle = 0;
      sweepDirection = 1;
    }
    
    safeDial.write(currentAngle);
  }
}

void updateLEDFeedback() {
  if(!sweeping || !gameActive || !gameOn) return;
  
  int distance = abs(currentAngle - targetAngle);
  
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

void sendGameStatus() {
  static unsigned long lastStatusTime = 0;
  
  // Send status every 500ms
  if(millis() - lastStatusTime >= 500) {
    lastStatusTime = millis();
    
    Serial.print("GAME_STATUS:");
    Serial.print(currentAngle);
    Serial.print(",");
    Serial.print(targetAngle);
    Serial.print(",");
    Serial.print(attemptsRemaining);
    Serial.print(",");
    Serial.print(score);
    Serial.print(",");
    Serial.print((millis() - gameStartTime) / 1000); // seconds
    Serial.print(",");
    Serial.println(difficulty);
  }
}

void safeUnlocked() {
  gameActive = false;
  
  Serial.println("");
  Serial.println("🎉 *** SAFE CRACKED! *** 🎉");
  Serial.println("STATUS:SAFE_UNLOCKED");
  Serial.print("FINAL_SCORE:");
  Serial.println(score);
  Serial.print("ATTEMPTS_USED:");
  Serial.println(attemptsUsed);
  
  // Move to unlock position first
  Serial.println("Unlocking safe...");
  safeDial.write(UNLOCKED_ANGLE);
  delay(1000);  // Give time to see the unlock
  displayLockStatus();
  
  // Epic unlock animation
  unlockAnimation();
  
  // Keep in unlocked position
  safeDial.write(UNLOCKED_ANGLE);
  Serial.println("🔓 Slide the lid open to claim your prize!");
  Serial.println("STATUS:GAME_COMPLETE");
}

void gameOver() {
  gameActive = false;
  
  Serial.println("STATUS:GAME_OVER");
  Serial.print("FINAL_TARGET:");
  Serial.println(targetAngle);
  Serial.print("FINAL_SCORE:");
  Serial.println(score);
  
  // Lock the safe
  safeDial.write(LOCKED_ANGLE);
  delay(500);
  displayLockStatus();
  
  // Alarm animation
  alarmAnimation();
  
  Serial.println("Safe is locked. Press switch to try again!");
  Serial.println("STATUS:RESTART_AVAILABLE");
}

void gameOffMode() {
  // Slow breathing LED effect to show system is off but ready
  for(int i = 0; i < 3; i++) {
    digitalWrite(ledPins[RED], HIGH);
    delay(200);
    digitalWrite(ledPins[RED], LOW);
    delay(200);
  }
  
  Serial.println("STATUS:STANDBY");
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
  Serial.println("STATUS:WELCOME_ANIMATION");
  
  // Servo sweep with LED chase (limited range to stay locked)
  for(int angle = 0; angle <= 90; angle += 10) {
    safeDial.write(angle);
    
    // LED chase effect
    allLEDsOff();
    int ledIndex = map(angle, 0, 90, 0, numLeds - 1);
    digitalWrite(ledPins[ledIndex], HIGH);
    
    delay(100);
  }
  
  // Return to locked position
  safeDial.write(LOCKED_ANGLE);
  allLEDsOff();
  delay(500);
}

void successAnimation() {
  // Flash green LED and tease unlock progress
  for(int i = 0; i < 5; i++) {
    digitalWrite(ledPins[GREEN], HIGH);
    delay(200);
    digitalWrite(ledPins[GREEN], LOW);
    delay(200);
  }
  
  // Tease unlock by moving slightly toward unlock angle
  int teaseAngle = 60;  // Partway to unlock
  safeDial.write(teaseAngle);
  delay(500);
  safeDial.write(LOCKED_ANGLE);
  delay(300);
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
  
  // Final celebration - show unlock motion
  for(int i = 0; i < 3; i++) {
    safeDial.write(FULL_OPEN_ANGLE);
    allLEDsOn();
    delay(300);
    allLEDsOff();
    safeDial.write(UNLOCKED_ANGLE);
    delay(300);
  }
}

void alarmAnimation() {
  // Rapid red flashing alarm
  for(int i = 0; i < 10; i++) {
    allLEDsOn();
    delay(100);
    allLEDsOff();
    delay(100);
  }
  
  // Servo shaking motion (but keep it locked)
  for(int i = 0; i < 5; i++) {
    safeDial.write(30);
    delay(200);
    safeDial.write(60);
    delay(200);
  }
  
  safeDial.write(LOCKED_ANGLE); // Return to locked
}
