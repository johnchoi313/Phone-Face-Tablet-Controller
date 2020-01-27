#include <SpheroRVR.h>
#include <ArduinoJson.h>

//Speed settings
float accel = 0.5;
float leftSpeed = 0;
float rightSpeed = 0;
int leftTargetSpeed = 0;
int rightTargetSpeed = 0;

//LED Settings
static uint32_t ledGroup;
uint8_t red, green, blue;

//Parsing variables
String serialString = "";
int timer = 800;

void setup() {
  //Setup Sphero
  rvr.configUART(&Serial1);
}

void loop() {
  //---Read Serial Message---//
  char c = ' ';
  if(Serial.available() > 0) {
    //Get the JSON string via serial
    c = Serial.read();
    serialString += c;
    timer = 800;
  }
  if(c == '}'){
    //JSON input format: { "L":255, "R":-255 } |  { "L":255, "R":-255, "RE", "GR", "BL" }
    StaticJsonBuffer<100> jsonBufferIn;
    JsonObject& jsonIn = jsonBufferIn.parseObject(serialString);

    Serial.println(serialString);
    serialString = "";

    //Set the servos target speed
    leftTargetSpeed = constrain((jsonIn.containsKey("L") ? jsonIn["L"] : 0),-255,255);
    rightTargetSpeed = constrain((jsonIn.containsKey("R") ? jsonIn["R"] : 0),-255,255);    

    //Set LED Color to target color
    red = constrain((jsonIn.containsKey("RE") ? jsonIn["RE"] : 0),0,255);
    green = constrain((jsonIn.containsKey("GR") ? jsonIn["GR"] : 0),0,255);    
    blue = constrain((jsonIn.containsKey("BL") ? jsonIn["BL"] : 0),0,255);    
  }

  //---Set headlights to target color---//
  uint8_t colorArray[] = {red, green, blue, red, green, blue};
  rvr.setAllLeds(ledGroup, colorArray, sizeof(colorArray) / sizeof(colorArray[0]));
      
  //---Accelerate servos to target speed---//
  if(timer > 0) { timer--; }
  if(timer == 0) { leftTargetSpeed = 0; rightTargetSpeed = 0; }

  //Accelerate or decelerate motors to target speed
  if(leftSpeed > leftTargetSpeed) {leftSpeed -= accel;}
  if(leftSpeed < leftTargetSpeed) {leftSpeed += accel;} 
  if(rightSpeed > rightTargetSpeed) {rightSpeed -= accel;}
  if(rightSpeed < rightTargetSpeed) {rightSpeed += accel;} 
  //Drive with calculated speeds
  rvr.rawMotors((leftSpeed < 0) ? (RawMotorModes::forward) : (RawMotorModes::reverse), abs(leftSpeed), 
                (rightSpeed < 0) ? (RawMotorModes::forward) : (RawMotorModes::reverse), abs(rightSpeed));
                   
}
