# Communications Demo

This application uses the **Communications Library** to setup a call between two locations and visualize the scenario and link budget results. Using [SimpleDigitalTransmitter](https://help.agi.com/AGIComponentsJava/Javadoc/agi-foundation-communications-SimpleDigitalTransmitter.html), [Transceiver](https://help.agi.com/AGIComponentsJava/Javadoc/agi-foundation-communications-Transceiver.html) and [SimpleReceiver](https://help.agi.com/AGIComponentsJava/Javadoc/agi-foundation-communications-SimpleReceiver.html), the application looks at the link budget for the call, allowing the user to vary the transmission power of the calling phone, and on which links to report the link budget parameters. The application also shows how visualization can be incorporated into an analysis scenario, bringing in Insight3D and its various platforms, markers and models. For more information on the Communications Library, see the [Communications](https://help.agi.com/AGIComponentsJava/html/Communications.htm) topic.

![Communications Demo](Images/ExampleCommunicationsDemoJava.png)

## Compilation

To compile this sample application with Ant:
  * Copy your AGI.Foundation.lic file into the src directory.
  * Run "ant package".  

The application will be compiled, packaged into a jar, and placed in the dist 
directory.  You can then double-click the CommunicationsDemo.jar file to run the 
application, or, simply run "ant run".
