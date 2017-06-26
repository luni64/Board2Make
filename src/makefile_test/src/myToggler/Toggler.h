#pragma once

class Toggler
{
public:
    Toggler(const int pin);
    void toggle() const;   

protected:
    const int pin;
}; 

