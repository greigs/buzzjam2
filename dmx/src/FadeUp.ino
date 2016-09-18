/* Welcome to DmxMaster. This library allows you to control DMX stage and
** architectural lighting and visual effects easily from Arduino. DmxMaster
** is compatible with the Tinker.it! DMX shield and all known DIY Arduino
** DMX control circuits.
**
** DmxMaster is available from: http://code.google.com/p/tinkerit/
** Help and support: http://groups.google.com/group/DmxMaster */

/* To use DmxMaster, you will need the following line. Arduino will
** auto-insert it if you select Sketch > Import Library > DmxMaster. */

#include <DmxMaster.h>

int channel = 1;
void setup() {
  /* The most common pin for DMX output is pin 3, which DmxMaster
** uses by default. If you need to change that, do it here. */
  DmxMaster.usePin(3);

  /* DMX devices typically need to receive a complete set of channels
** even if you only need to adjust the first channel. You can
** easily change the number of channels sent here. If you don't
** do this, DmxMaster will set the maximum channel number to the
** highest channel you DmxMaster.write() to. */
  DmxMaster.maxChannel(4);
}

bool toggle = false;

void loop() {

  int r = random(0,255);
  int g = random(0,255);
  int b = random(0,255);

  if (toggle){
      writeRGB(0,0,0);
  }
  else{
    writeRGB(r,g,b);
  }
  toggle = !toggle;

  delay(100);
}

void writeRGB(int r, int g, int b){
    DmxMaster.write(1, r);
    DmxMaster.write(2, g);
    DmxMaster.write(3, b);
}
