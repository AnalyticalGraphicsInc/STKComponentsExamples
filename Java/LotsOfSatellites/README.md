# Lots of Satellites

This application uses [Sgp4Propagator](https://help.agi.com/AGIComponentsJava/Javadoc/agi-foundation-propagators-Sgp4Propagator.html) to propagate up to 10,000 satellites, which are visualized in Insight3D using [MarkerBatchPrimitive](https://help.agi.com/AGIComponentsJava/Javadoc/agi-foundation-graphics-MarkerBatchPrimitive.html). Satellites shown in red have access to a ground station located in Australia.

![Lots of Satellites](Images/ExampleLotsOfSatellites.jpg)

## Compilation

To compile this sample application with Ant:
  * Copy your AGI.Foundation.lic file into the src directory.
  * Run "ant package".  

The application will be compiled, packaged into a jar, and placed in the dist 
directory.  You can then double-click the LotsOfSatellites.jar file to run the 
application, or, simply run "ant run".
