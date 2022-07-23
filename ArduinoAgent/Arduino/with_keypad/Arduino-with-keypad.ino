#include <Joystick.h>
#include <NewEncoder.h>
#include <Keypad.h>

// Rotary Encoder Inputs
#define CLK1 19
#define DT1 18
#define SW1 17

#define CLK2 2
#define DT2 3
#define SW2 4

#define VRx A0
#define VRy A1

const byte ROWS = 4; 
const byte COLS = 4; 

char hexaKeys[ROWS][COLS] = {
  {'1', '2', '3', 'A'},
  {'4', '5', '6', 'B'},
  {'7', '8', '9', 'C'},
  {'*', '0', '#', 'D'}
};

byte rowPins[ROWS] = {31, 33, 35, 37}; 
byte colPins[COLS] = {39, 41, 43, 45}; 
Keypad customKeypad = Keypad(makeKeymap(hexaKeys), rowPins, colPins, ROWS, COLS); 

// Rotatry encoder variables
int currentStateCLK1;
int lastStateCLK1;
int currentStateCLK2;
int lastStateCLK2;

String currentEvent = "";
String currentDir1 = "";
String currentDir2 = "";
boolean rotaryEncoderRotating1 = false;
boolean rotaryEncoderRotating2 = false;

unsigned long lastButton1Press = 0;
unsigned long lastButton2Press = 0;

Joystick joystick(VRx, VRy, 8);

NewEncoder encoderLower(DT1, CLK1, -32768, 32767, 0, FULL_PULSE);
NewEncoder encoderUpper(DT2, CLK2, -32768, 32767, 0, FULL_PULSE);
int16_t prevEncoderValueLower;
int16_t prevEncoderValueUpper;

void setup() {
	// Set encoder pins as inputs
	pinMode(SW1, INPUT_PULLUP);
  pinMode(SW2, INPUT_PULLUP);

	// Setup joystick
	joystick.initialize();  
	joystick.calibrate();
	joystick.setSensivity(3);
  
	// Setup Serial Monitor
	Serial.begin(9600);

  NewEncoder::EncoderState encoderState1;
  NewEncoder::EncoderState encoderState2;
  
  encoderLower.begin();
  encoderLower.getState(encoderState1);
  prevEncoderValueLower = encoderState1.currentValue;

  encoderUpper.begin();
  encoderUpper.getState(encoderState2);
  prevEncoderValueUpper = encoderState2.currentValue;
}

void loop() {
  int16_t currentEncoderValueLower;
  int16_t currentEncoderValueUpper;
  NewEncoder::EncoderState currentEncoderStateLower;
  NewEncoder::EncoderState currentEncoderStateUpper;

  // Read rotary encoder lower
  if (encoderLower.getState(currentEncoderStateLower)) {
    currentEncoderValueLower = currentEncoderStateLower.currentValue;
    if (currentEncoderValueLower != prevEncoderValueLower) {
      if(currentEncoderValueLower > prevEncoderValueLower){
        Serial.println("EncoderLower:CW:" + String(currentEncoderValueLower - prevEncoderValueLower));
      }
      else{
        Serial.println("EncoderLower:CCW:" + String(prevEncoderValueLower - currentEncoderValueLower));
      }
      prevEncoderValueLower = currentEncoderValueLower;
    }
  }

  // Read rotary encoder upper
  if (encoderUpper.getState(currentEncoderStateUpper)) {
    currentEncoderValueUpper = currentEncoderStateUpper.currentValue;
    if (currentEncoderValueUpper != prevEncoderValueUpper) {
      if(currentEncoderValueUpper > prevEncoderValueUpper){
        Serial.println("EncoderUpper:CW:" + String(currentEncoderValueUpper - prevEncoderValueUpper));
      }
      else{
        Serial.println("EncoderUpper:CCW:" + String(prevEncoderValueUpper - currentEncoderValueUpper));
      }
      prevEncoderValueUpper = currentEncoderValueUpper;
    }
  }

	// Read the rotary encoder button state
	int btnState1 = digitalRead(SW1);
	int btnState2 = digitalRead(SW2);

	//If we detect LOW signal, button is pressed
	if (btnState1 == LOW) {
		//if 500ms have passed since last LOW pulse, it means that the
		//button has been pressed, released and pressed again
		if (millis() - lastButton1Press > 500) {
			Serial.println("EncoderLower:SW");
		}

		// Remember last button press event
		lastButton1Press = millis();
	}

	if (btnState2 == LOW) {
		//if 500ms have passed since last LOW pulse, it means that the
		//button has been pressed, released and pressed again
		if (millis() - lastButton2Press > 500) {
			Serial.println("EncoderUpper:SW");
		}

		// Remember last button press event
		lastButton2Press = millis();
	}

  // Read joystick
  if(joystick.isPressed())
  {
    Serial.println("Joystick:SW");
  }

	// Read joystick (updated for joystick mounted at 90 degree)
	if(joystick.isReleased())
	{
		// left
		if(joystick.isLeft())
		{
      //Serial.println("Joystick:LEFT");
			Serial.println("Joystick:UP");
		}

		// right
		if(joystick.isRight())
		{
      //Serial.println("Joystick:RIGHT");
			Serial.println("Joystick:DOWN");
		}

		// up
		if(joystick.isUp())
		{
      //Serial.println("Joystick:UP");
			Serial.println("Joystick:RIGHT");
		}

		// down
		if(joystick.isDown())
		{
      //Serial.println("Joystick:DOWN");
			Serial.println("Joystick:LEFT");
		}
	}

  // Read keypad
  char customKey = customKeypad.getKey();
  
  if (customKey){
    if(customKey == '#')
    {
      Serial.println("Keypad:KeyPound");
    }
    else if(customKey == '*')
    {
      Serial.println("Keypad:KeyAsterisk");
    }
    else
    {
      Serial.println("Keypad:Key" + String(customKey));
    }
  }

	// slow down a bit
	delay(100);
}
