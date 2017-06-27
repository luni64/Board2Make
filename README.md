

# Board2Make

If you want to use a modern Windows IDE to write sketches for the Teensy ARM boards (T3.0 up to T3.6) one possibiltiy is to use so called Makefile-Projects in the free Community Edition of [Microsoft Visual Studio](https://www.visualstudio.com/vs/community). It is not very difficult to write a generic [makefile](https://github.com/luni64/Board2Make/blob/master/src/makefile_test/makefile) which can be used to build your sketches. As long as you don't need to change settings for e.g., USB-Type, CPU-Speed, Optimizer Settings or the type of the board you don't need to touch the makefile. If you need to change those settings you usually need to adjust the makefile wich is somehow inconvenient. 

Board2Make is a small windows utility which helps updating your makefile with those changable settings. It parses the information it finds in a standard Teensyduino boards.txt file and uses it to display a simular menu as you get in the "Tools" menu of the  Arduio IDE. You can choose all the settings you need and Board2Make will generate a small makefile ([example](https://github.com/luni64/Board2Make/blob/master/src/makefile_test/teensy.mk)) which can be included from your main makefile. 

![GUI](/media/gui.png "GUI")

## Usage
- Start Board2Make
- Select a boards.txt file (you usually find it here ``` Arduino\hardware\teensy\avr ```)
- Chose the settings you need
- Click on Save Makefile to export the makefile 


## Build System and Binaries
Board2Make was built with the community edition of VisualStudio 2015. A precompiled .exe can be found [here](https://github.com/luni64/Board2Make/releases) (no installation required)



## Example Teensy Project
The VS solution also includes a simple example makefile project for compiling a blink sketch. 

Quick instructions (detailes instructions tbd)
- Make sure to adapt the paths in the project makefile to your system 
- Make sure to adapt the paths in the properties of the makefile project 


