#include <ArduinoJson.h>

#define AIN1 3
#define AIN2 11
#define APWM 5

#define BIN1 4
#define BIN2 2
#define BPWM 6

#define STBY 7

float accel = 0.1;
float leftSpeed = 0;
float rightSpeed = 0;
int leftTargetSpeed = 0;
int rightTargetSpeed = 0;

//Parsing variables
String serialString = "";

int timer = 10000;

void setup() {
  Serial.begin(9600);

  pinMode(AIN1, OUTPUT);
  pinMode(AIN2, OUTPUT);
  pinMode(APWM, OUTPUT);
  
  pinMode(BIN1, OUTPUT);
  pinMode(BIN2, OUTPUT);
  pinMode(BPWM, OUTPUT);

  pinMode(STBY, OUTPUT);
  digitalWrite(STBY, 1);
}

void loop() {
  
  char c = ' ';
  if(Serial.available() > 0) {
    //Get the JSON string via serial
    c = Serial.read();
    serialString += c;
    timer = 10000;
  }

  if(c == '}'){
    //JSON input format: { "L":255, "R":-255 }
    StaticJsonBuffer<100> jsonBufferIn;
    JsonObject& jsonIn = jsonBufferIn.parseObject(serialString);

    Serial.println(serialString);
    serialString = "";

    //Set the servos target speed
    leftTargetSpeed = constrain((jsonIn.containsKey("L") ? jsonIn["L"] : 0),-255,255);
    rightTargetSpeed = constrain((jsonIn.containsKey("R") ? jsonIn["R"] : 0),-255,255);    
  }

  if(timer > 0) { timer--; }
  if(timer == 0) {
    leftTargetSpeed = 0;
    rightTargetSpeed = 0;
  }

  //Accelerate servos to target speed
  if(leftSpeed > leftTargetSpeed) {leftSpeed -= accel;}
  if(leftSpeed < leftTargetSpeed) {leftSpeed += accel;} 
  digitalWrite(AIN1, (leftSpeed < 0) ? HIGH : LOW);
  digitalWrite(AIN2, (leftSpeed < 0) ? LOW : HIGH);
  analogWrite(APWM, abs(leftSpeed));
  
  if(rightSpeed > rightTargetSpeed) {rightSpeed -= accel;}
  if(rightSpeed < rightTargetSpeed) {rightSpeed += accel;} 
  digitalWrite(BIN1, (rightSpeed < 0) ? HIGH : LOW);
  digitalWrite(BIN2, (rightSpeed < 0) ? LOW : HIGH);  
  analogWrite(BPWM, abs(rightSpeed));
  
}
