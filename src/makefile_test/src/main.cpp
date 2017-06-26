#include <arduino.h>
#include "myToggler\Toggler.h"


Toggler t(LED_BUILTIN);

void setup()
{    
} 

void loop()
{  
    t.toggle();
    delay(250);
}

