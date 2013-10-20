#include <FastSPI_LED.h>

#define NUM_LEDS 12
#define LED_PIN  4

struct CRGB { 
  unsigned char g; 
  unsigned char r; 
  unsigned char b; 
};
struct CRGB *leds;

int leds_idx;
int leds_color;


void setup() {
  
  // Setup serial communication
  Serial.begin(57600);
  
  // Setup FastSPI_LED library: number of leds, chipset and Arduino PIN
  FastSPI_LED.setLeds(NUM_LEDS);
  FastSPI_LED.setChipset(CFastSPI_LED::SPI_WS2811);
  FastSPI_LED.setPin(LED_PIN);
  
  // Init FastSPI_LED library
  FastSPI_LED.init();
  FastSPI_LED.start();

  // Get data array and turn all the LEDs off
  leds = (struct CRGB*)FastSPI_LED.getRGBData();  
  memset(leds, 0, NUM_LEDS * 3);
  FastSPI_LED.show();
  
  // Init array index and color index
  leds_idx = 0;
  leds_color = 0;
}

void loop() {
  
  if (Serial.available() > 0) {

    char input_byte = Serial.read();

    //If LF was sent, the transmission is completed
    if(input_byte == 0x0A) {
      FastSPI_LED.show();
      leds_idx = 0;
      leds_color = 0;
    }

    else {
      
      // Save the received byte to the corresponding LED and color
      switch(leds_color) { 
        
        case 0:
          leds[leds_idx].r = input_byte;
          leds_color = 1;
          break;
          
        case 1:
          leds[leds_idx].g = input_byte;
          leds_color = 2;
          break;
          
        case 2:
          leds[leds_idx].b = input_byte;
          leds_color = 0;
          leds_idx++;
          break;  
      }      
    }
  }  
}
