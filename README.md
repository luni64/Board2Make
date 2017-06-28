

# Board2Make

If you want to use a modern Windows IDE to write sketches for the Teensy ARM boards (T3.0 up to T3.6) one possibiltiy is to use so called Makefile-Projects in the free community edition of [Microsoft Visual Studio](https://www.visualstudio.com/vs/community). It is not very difficult to write a generic [makefile](https://github.com/luni64/Board2Make/blob/master/src/makefile_test/makefile) which can be used to build your sketches. As long as you don't need to change settings for e.g., USB-Type, CPU-Speed, Optimizer Settings or the type of the board you don't need to touch the makefile. If you need to change those settings you usually need to adjust the makefile wich is somehow inconvenient. 

Board2Make is a small windows utility which helps updating your makefile with those changable settings. It parses the information it finds in a standard Teensyduino boards.txt file and uses it to display a simular menu as you get in the "Tools" menu of the  Arduio IDE. You can choose all the settings you need and Board2Make will generate a small makefile ([example](https://github.com/luni64/Board2Make/blob/master/src/makefile_test/teensy.mk)) which can be included from your main makefile. (The generated makefile does not assume any build environment and can be used for any makefile based compilation setup)

![GUI](/media/gui.png "GUI")

## Usage
- Start Board2Make
- Select a boards.txt file (you usually find it here ``` Arduino\hardware\teensy\avr ```)
- Chose the settings you need
- Click on Save Makefile to export the makefile 


## Build System and Binaries
Board2Make was built with the community edition of VisualStudio 2015. A precompiled .exe can be found [here](https://github.com/luni64/Board2Make/releases) (no installation required)



## Example Teensy Makefile Project
The VS solution also includes a simple example makefile project for compiling a blink sketch. 

Quick instructions (detailed instructions tbd)
- The makefile in the project assumes gcc, the teensy tools, make.exe and sed.exe in '''c:\toolchain''. Make sure to adapt the paths in the  makefile to fit your needs.
- You can get the required binaries here: 
  - GCC 4.8 [https://launchpad.net/gcc-arm-embedded/4.8/4.8-2014-q3-update](https://launchpad.net/gcc-arm-embedded/4.8/4.8-2014-q3-update) for Teensyduino < 1.37
  - Make: [http://gnuwin32.sourceforge.net/packages/make.htm](http://gnuwin32.sourceforge.net/packages/make.htm)
  - Sed:  [http://gnuwin32.sourceforge.net/packages/sed.htm](http://gnuwin32.sourceforge.net/packages/sed.htm)
- Make sure to adapt the paths in the properties of the makefile project (right click on the makefile_test project, and select properities)


