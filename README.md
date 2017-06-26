# Board2Make

(documentation under construction)

If you want to use a modern Windows IDE to write sketches for the Teensy ARM boards (T3.0 up to T3.6) one possibiltiy is to use so called Makefile-Projects in the free Community Edition of [Microsoft Visual Studio](https://www.visualstudio.com/vs/community). It is not very difficult to write a generic makefile which can be used for all sketches without the need for changing. This is, as long as you stay with the same board and the same settings for USB-Type, CPU-Speed, Optimizer Settings etc...

Board2Make is a small utility to extract the information in the boards.txt file of Teensyduino. It uses this information to generate the same settings menus as you get in the Arduio IDE and generates a simple include file for your makefile with all the required flags. 
![GUI](/media/gui.png "GUI")

## Build System
VisualStudio 2015 Communitiy Edition (and higher)

## Example Teensy Project
The VS solution also includes a simple example makefile project for compiling a blink sketch. 

Quick instructions (detailes instructions tbd)
- Make sure to adapt the paths in the project makefile to your system 
- Make sure to adapt the paths in the properties of the makefile project 


