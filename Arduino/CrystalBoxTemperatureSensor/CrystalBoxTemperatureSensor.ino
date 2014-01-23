// Example testing sketch for various DHT humidity/temperature sensors
// Written by ladyada, public domain

#include "DHT.h"

#include <Wire.h>
#include "LCD.h"
#include "LiquidCrystal_I2C.h"

#define DHTPIN1 7
#define DHTPIN2 8
#define DHTTYPE DHT22   // DHT 22  (AM2302)
//#define DHTTYPE DHT21   // DHT 21 (AM2301)
DHT dht1(DHTPIN1, DHTTYPE);
DHT dht2(DHTPIN2, DHTTYPE);

#define I2C_ADDR    0x3F // <<----- Add your address here.  Find it from I2C Scanner
#define BACKLIGHT_PIN     3
#define En_pin  2
#define Rw_pin  1
#define Rs_pin  0
#define D4_pin  4
#define D5_pin  5
#define D6_pin  6
#define D7_pin  7

LiquidCrystal_I2C lcd((uint8_t)I2C_ADDR,(uint8_t)En_pin,(uint8_t)Rw_pin,(uint8_t)Rs_pin,(uint8_t)D4_pin,(uint8_t)D5_pin,(uint8_t)D6_pin,(uint8_t)D7_pin);

void setup() {
    Serial.begin(9600); 
    //Serial.println("DHTxx test!");
    lcd.begin(20,4);
    lcd.setBacklightPin(BACKLIGHT_PIN,POSITIVE);
    lcd.setBacklight(HIGH);
    dht1.begin();
    dht2.begin();
    lcd.setCursor(3,0);
    lcd.print("Humidity1: "); 
    
    lcd.setCursor(0,1);
    lcd.print("Temperature1: "); 
    
    lcd.setCursor(3,2);
    lcd.print("Humidity2: "); 
    
    lcd.setCursor(0,3);
    lcd.print("Temperature2: "); 
    
}

void csharpfriendlyfloat2str(float v, int width, int precision, char* out){
  if(!isnan(v)){
    dtostrf(v, width, precision, out);
  }else{
    strcpy(out, "NaN"); //C# needs capital N
  }
}

void loop() {
    float t1 = dht1.readTemperature();
    float h1 = dht1.readHumidity();
    float t2 = dht2.readTemperature();
    float h2 = dht2.readHumidity();
    
    while(Serial.available() > 0){
        char cmd = Serial.read();
        switch(cmd){
          case 'C': //check command
            Serial.write("Temp Sensor\n");
            break;
          case 'D': //ask for data
            //Return T1, H1, T2, H2
            //note that NaN is case sensitive "NaN"
            //C# needs N to be capitalized
            char temp1[30];
            char temp2[30];
            char hum1[30];
            char hum2[30];
            char msgbuffer[120];
            int width = 5;
            int precision = 2;
            csharpfriendlyfloat2str(t1, width, precision, temp1);
            csharpfriendlyfloat2str(t2, width, precision, temp2);
            csharpfriendlyfloat2str(h1, width, precision, hum1);
            csharpfriendlyfloat2str(h2, width, precision, hum2);
            sprintf(msgbuffer, "%s,%s,%s,%s\n",temp1,hum1,temp2,hum2);
            Serial.write(msgbuffer);
            break;
        }
    }
    
    lcd.setCursor(14,0);
    if(!isnan(h1)){
      lcd.print(h1,2);
      lcd.print('%');
    }else{
      lcd.print("      ");
    }
    lcd.setCursor(14,1);
    if(!isnan(t1)){
      lcd.print(t1,2);
      lcd.print('C');
    }else{
      lcd.print("      ");
    }
    lcd.setCursor(14,2);
    if(!isnan(h2)){
      lcd.print(h2,2);
      lcd.print('%');
    }else{
      lcd.print("      ");
    }
    lcd.setCursor(14,3);
    if(!isnan(t2)){
      lcd.print(t2,2);
      lcd.print('C');
    }else{
      lcd.print("      ");
    }
    
    //delay(200);
}

