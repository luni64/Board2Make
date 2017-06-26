#include <arduino.h>
#include "Toggler.h"

Toggler::Toggler(const int _pin)
    :pin(_pin)
{
    pinMode(pin, OUTPUT);
}

void Toggler::toggle() const
{
    digitalWriteFast(pin, !digitalReadFast(pin));
}


