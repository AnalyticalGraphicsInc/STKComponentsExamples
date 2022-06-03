# Numerical Propagation Demo

This application uses a [NumericalPropagator](https://help.agi.com/AGIComponentsJava/Javadoc/agi-foundation-propagators-NumericalPropagator.html) to propagate a satellite, which is then visualized in Insight3D using a [MarkerGraphicsExtension](https://help.agi.com/AGIComponentsJava/Javadoc/agi-foundation-platforms-MarkerGraphicsExtension.html) and a [PathPrimitive](https://help.agi.com/AGIComponentsJava/Javadoc/agi-foundation-graphics-PathPrimitive.html). Force models can be configured to act on the satellite's orbit being propagated, specifically: [SphericalHarmonicGravity](https://help.agi.com/AGIComponentsJava/Javadoc/agi-foundation-celestial-SphericalHarmonicGravity.html), [TwoBodyGravity](https://help.agi.com/AGIComponentsJava/Javadoc/agi-foundation-celestial-TwoBodyGravity.html), [SimpleSolarRadiationForce](https://help.agi.com/AGIComponentsJava/Javadoc/agi-foundation-celestial-SimpleSolarRadiationForce.html), [ThirdBodyGravity](https://help.agi.com/AGIComponentsJava/Javadoc/agi-foundation-celestial-ThirdBodyGravity.html), and [AtmosphericDragForce](https://help.agi.com/AGIComponentsJava/Javadoc/agi-foundation-celestial-AtmosphericDragForce.html). A [RungeKutta4Integrator](https://help.agi.com/AGIComponentsJava/Javadoc/agi-foundation-numericalmethods-RungeKutta4Integrator.html) or [RungeKuttaFehlberg78Integrator](https://help.agi.com/AGIComponentsJava/Javadoc/agi-foundation-numericalmethods-RungeKuttaFehlberg78Integrator.html) can be configured for use by the propagator. The state described by the [NumericalPropagatorDefinition](https://help.agi.com/AGIComponentsJava/Javadoc/agi-foundation-propagators-NumericalPropagatorDefinition.html) in this demo contains just a position and velocity representing one satellite. More generally, the state can contain any number of elements to be propagated including multiple positions, mass, or other parameters with their own differential equations. For more information see the [Orbit Propagation](https://help.agi.com/AGIComponentsJava/html/OrbitPropagation.htm) topic.

![Numerical Propagation Demo](Images/ExampleNumericalPropagationDemoJava.png)

## Compilation

To compile this sample application with Ant:
  * Copy your AGI.Foundation.lic file into the src directory.
  * Run "ant package".  

The application will be compiled, packaged into a jar, and placed in the dist 
directory.  You can then double-click the NumericalPropagationDemo.jar file to run the 
application, or, simply run "ant run".
