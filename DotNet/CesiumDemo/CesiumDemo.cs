using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Web;
using AGI.Examples;
using AGI.Foundation;
using AGI.Foundation.Access;
using AGI.Foundation.Access.Constraints;
using AGI.Foundation.Celestial;
using AGI.Foundation.Cesium;
using AGI.Foundation.Coordinates;
using AGI.Foundation.Geometry;
using AGI.Foundation.Geometry.Shapes;
using AGI.Foundation.Infrastructure;
using AGI.Foundation.Platforms;
using AGI.Foundation.Propagators;
using AGI.Foundation.Terrain;
using AGI.Foundation.Time;

namespace CesiumDemo
{
    /// <summary>
    /// This class builds the set of objects which will be
    /// written out to a CZML document and sent to the client.
    /// </summary>
    public class CesiumDemo
    {
        /// <summary>
        /// Creates all objects in the demonstration.
        /// </summary>
        /// <param name="satelliteIdentifier">The NORAD identifier for the satellite to propagate.</param>
        public CesiumDemo(string satelliteIdentifier)
        {
            m_satelliteIdentifier = satelliteIdentifier;
            m_earth = CentralBodiesFacet.GetFromContext().Earth;

            // Create all objects for our scenario.

            // Each of these helper methods create some of the objects
            // which are stored in fields.
            // The work is divided up across methods to make the code easier to follow.
            CreateSatellite();
            CreateFacility();
            CreateSatelliteAccessLink();
            CreateFacilitySensor();
            CreateSensorDome();
            CreateAzimuthElevationMask();
            CreateAircraft();
        }

        /// <summary>
        /// Create a Platform for the requested satellite using a TLE for position.
        /// The satellite will be visually represented by a labeled glTF model,
        /// the satellite's orbit will be shown, and vectors will be drawn for
        /// the body axes of the satellite, plus a vector indicating the direction
        /// of the sun.
        /// </summary>
        private void CreateSatellite()
        {
            // Get the current TLE for the given satellite identifier.
            var tleList = TwoLineElementSetHelper.GetTles(m_satelliteIdentifier, JulianDate.Now);

            // Use the epoch of the first TLE, since the TLE may have been loaded from offline data.
            m_epoch = tleList[0].Epoch;

            // Propagate the TLE and use that as the satellite's location point.
            var locationPoint = new Sgp4Propagator(tleList).CreatePoint();
            m_satellite = new Platform
            {
                Name = "Satellite " + m_satelliteIdentifier,
                LocationPoint = locationPoint,
                // Orient the satellite using Vehicle Velocity Local Horizontal (VVLH) axes.
                OrientationAxes = new AxesVehicleVelocityLocalHorizontal(m_earth.FixedFrame, locationPoint),
            };

            // Set the identifier for the satellite in the CZML document.
            m_satellite.Extensions.Add(new IdentifierExtension(m_satelliteIdentifier));

            // Configure a glTF model for the satellite.
            m_satellite.Extensions.Add(new ModelGraphicsExtension(new ModelGraphics
            {
                // Link to a binary glTF file.
                Model = new CesiumResource(GetModelUri("satellite.glb"), CesiumResourceBehavior.LinkTo),
                // By default, Cesium plays all animations in the model simultaneously, which is not desirable.
                RunAnimations = false,
            }));

            // Configure a label for the satellite.
            m_satellite.Extensions.Add(new LabelGraphicsExtension(new LabelGraphics
            {
                // Use the name of the satellite as the text of the label.
                Text = m_satellite.Name,
                // Change the color of the label after 12 hours. This demonstrates specifying that 
                // a value varies over time using intervals.
                FillColor = new TimeIntervalCollection<Color>
                {
                    // Green for the first half day...
                    new TimeInterval<Color>(JulianDate.MinValue, m_epoch.AddDays(0.5), Color.Green, true, false),
                    // Red thereafter.
                    new TimeInterval<Color>(m_epoch.AddDays(0.5), JulianDate.MaxValue, Color.Red, false, true),
                },
                // Only show label when camera is far enough from the satellite,
                // to avoid visually clashing with the model.
                DistanceDisplayCondition = new Bounds(1000.0, double.MaxValue),
            }));

            // Configure graphical display of the orbital path of the satellite.
            m_satellite.Extensions.Add(new PathGraphicsExtension(new PathGraphics
            {
                // Configure the visual appearance of the line.
                Material = new PolylineOutlineMaterialGraphics
                {
                    Color = Color.White,
                    OutlineWidth = 1.0,
                    OutlineColor = Color.Black,
                },
                Width = 2.0,
                // Lead and Trail time indicate how much of the path to render.
                LeadTime = Duration.FromMinutes(44.0).TotalSeconds,
                TrailTime = Duration.FromMinutes(44.0).TotalSeconds,
            }));

            // Create vectors for the X, Y, and Z axes of the satellite.
            m_satelliteXAxis = CreateAxesVector(m_satellite, CartesianElement.X, Color.Green, "SatelliteX");
            m_satelliteYAxis = CreateAxesVector(m_satellite, CartesianElement.Y, Color.Red, "SatelliteY");
            m_satelliteZAxis = CreateAxesVector(m_satellite, CartesianElement.Z, Color.Blue, "SatelliteZ");

            // Create a vector from the satellite to the Sun.

            // Compute the vector from the satellite's location to the Sun's center of mass.
            var sunCenterOfMassPoint = CentralBodiesFacet.GetFromContext().Sun.CenterOfMassPoint;
            var vectorSatelliteToSun = new VectorTrueDisplacement(m_satellite.LocationPoint, sunCenterOfMassPoint);

            // Create the visual vector.
            m_satelliteSunVector = new GraphicalVector
            {
                LocationPoint = m_satellite.LocationPoint,
                Vector = vectorSatelliteToSun,
                VectorGraphics = new VectorGraphics
                {
                    Length = 5.0,
                    Color = Color.Yellow,
                },
            };

            // Set the identifier for the vector in the CZML document. 
            m_satelliteSunVector.Extensions.Add(new IdentifierExtension("SunVector"));

            // Orient the solar panels on the satellite model to point at the sun.
            var satelliteYVector = m_satellite.OrientationAxes.GetVectorElement(CartesianElement.Y);

            // allow only Z axis to rotate to follow sun vector. Constrain sun vector to Y, and satellite Y vector to X.
            var constrainedAxes = new AxesAlignedConstrained(satelliteYVector, AxisIndicator.First, vectorSatelliteToSun, AxisIndicator.Second);

            // Satellite axes are Vehicle Velocity Local Horizontal (VVLH) axes, where X is forward and Z is down,
            // but Cesium model axes are Z forward, Y up. So, create an axes rotates to the Cesium model axes.
            var offset = new UnitQuaternion(new ElementaryRotation(AxisIndicator.First, -Math.PI / 2)) *
                         new UnitQuaternion(new ElementaryRotation(AxisIndicator.Third, Math.PI / 2));
            var cesiumModelAxes = new AxesFixedOffset(m_satellite.OrientationAxes, offset);

            // The rotation will be from the Cesium model axes to the constrained axes.
            var solarPanelRotationAxes = new AxesInAxes(constrainedAxes, cesiumModelAxes);

            // Add a node transformation to rotate the SolarPanels node of the model.
            m_satellite.Extensions.GetByType<ModelGraphicsExtension>().ModelGraphics.NodeTransformations = new Dictionary<string, NodeTransformationGraphics>
            {
                {
                    "SolarPanels", new NodeTransformationGraphics
                    {
                        Rotation = new AxesCesiumProperty(solarPanelRotationAxes)
                    }
                }
            };
        }

        /// <summary>
        /// Create a vector that will draw one axis of the satellite's body axes.
        /// </summary>
        private static GraphicalVector CreateAxesVector(Platform platform, CartesianElement cartesianElement, Color color, string identifier)
        {
            var graphicalVector = new GraphicalVector
            {
                // Compute the vector for the given element in the satellite's body axes.
                Vector = platform.OrientationAxes.GetVectorElement(cartesianElement),
                // Specify the visual appearance of the vector.
                VectorGraphics = new VectorGraphics
                {
                    Length = 10.0,
                    Color = color,
                },
                LocationPoint = platform.LocationPoint,
            };

            // Set the identifier for the vector in the CZML document. 
            graphicalVector.Extensions.Add(new IdentifierExtension(identifier));

            return graphicalVector;
        }

        /// <summary>
        /// Create a facility at the location of AGI headquarters.
        /// The facility will be visually represented by a labeled glTF model.
        /// </summary>
        private void CreateFacility()
        {
            // Define the location of the facility using cartographic coordinates.
            var location = new Cartographic(Trig.DegreesToRadians(-75.596766667), Trig.DegreesToRadians(40.0388333333), 0.0);
            var locationPoint = new PointCartographic(m_earth, location);
            m_facility = new Platform
            {
                Name = "AGI HQ",
                LocationPoint = locationPoint,
                // Orient the facility using East-North-Up (ENU) axes.
                OrientationAxes = new AxesEastNorthUp(m_earth, locationPoint),
            };

            // Set the identifier for the facility in the CZML document. 
            m_facility.Extensions.Add(new IdentifierExtension("AGI"));

            // Configure a glTF model for the facility.
            m_facility.Extensions.Add(new ModelGraphicsExtension(new ModelGraphics
            {
                // Link to a binary glTF file.
                Model = new CesiumResource(GetModelUri("facility.glb"), CesiumResourceBehavior.LinkTo),
                RunAnimations = false,
                HeightReference = CesiumHeightReference.ClampToGround,
            }));

            // Configure label for AGI HQ.
            m_facility.Extensions.Add(new LabelGraphicsExtension(new LabelGraphics
            {
                Text = m_facility.Name,
                FillColor = Color.White,
                // Only show label when camera is far enough from the satellite,
                // to avoid visually clashing with the model.
                DistanceDisplayCondition = new Bounds(1000.0, double.MaxValue),
                HeightReference = CesiumHeightReference.ClampToGround,
            }));
        }

        /// <summary>
        /// Create an access link between AGI HQ and the satellite.
        /// </summary>
        private void CreateSatelliteAccessLink()
        {
            m_satelliteFacilityLink = new LinkInstantaneous(m_facility, m_satellite);

            // Set the identifier for the link in the CZML document. 
            m_satelliteFacilityLink.Extensions.Add(new IdentifierExtension("SatelliteFacilityAccess"));

            // Specify how access should be constrained.  In this case, 
            // access will only exist when no part of the earth is between AGI HQ and the satellite.
            m_accessQuery = new CentralBodyObstructionConstraint(m_satelliteFacilityLink, m_earth);

            // Configure graphical display of the access link.
            m_satelliteFacilityLink.Extensions.Add(new LinkGraphicsExtension(new LinkGraphics
            {
                // Show the access link only when access is satisfied.
                Show = new AccessQueryCesiumProperty<bool>(m_accessQuery, true, false, false),
                Material = new SolidColorMaterialGraphics(Color.Yellow),
            }));
        }

        /// <summary>
        /// Create a sensor, attached to the facility, oriented to target the satellite.
        /// </summary>
        private void CreateFacilitySensor()
        {
            m_facilitySensor = new Platform
            {
                // Use the same location point as the facility.
                LocationPoint = m_facility.LocationPoint,
                // Orient the sensor to target the link with the satellite.
                OrientationAxes = new AxesTargetingLink(m_satelliteFacilityLink, LinkRole.Transmitter, new VectorFixed(m_satellite.OrientationAxes, UnitCartesian.UnitZ))
            };

            // Set the identifier for the facility in the CZML document. 
            m_facilitySensor.Extensions.Add(new IdentifierExtension("Sensor"));

            // Define the sensor geometry.
            m_facilitySensor.Extensions.Add(new FieldOfViewExtension
            {
                FieldOfViewVolume = new RectangularPyramid
                {
                    XHalfAngle = Trig.DegreesToRadians(8.0),
                    YHalfAngle = Trig.DegreesToRadians(4.5),
                    Radius = 500000.0,
                }
            });

            // Configure graphical display of the sensor.
            m_facilitySensor.Extensions.Add(new FieldOfViewGraphicsExtension(new SensorFieldOfViewGraphics
            {
                // Show the sensor only when access is satisfied.
                Show = new AccessQueryCesiumProperty<bool>(m_accessQuery, true, false, false),
                DomeSurfaceMaterial = new GridMaterialGraphics
                {
                    Color = Color.White,
                    CellAlpha = 0.0,
                },
                LateralSurfaceMaterial = new SolidColorMaterialGraphics(Color.FromArgb(128, Color.Green)),
            }));
        }

        /// <summary>
        /// Create another ground facility with a sensor dome, and a rotating sensor inside the dome.
        /// </summary>
        private void CreateSensorDome()
        {
            // Define the location of the facility using cartographic coordinates.
            var locationPoint = new PointCartographic(m_earth, new Cartographic(Trig.DegreesToRadians(-122.3), Trig.DegreesToRadians(46), 456.359));

            m_sensorDome = new Platform
            {
                Name = "Sensor Dome",
                LocationPoint = locationPoint,
                OrientationAxes = new AxesEastNorthUp(m_earth, locationPoint),
            };

            // Set the identifier for the facility in the CZML document. 
            m_sensorDome.Extensions.Add(new IdentifierExtension("SensorDome"));

            // Define the sensor geometry.
            var dome = new ComplexConic();
            dome.SetHalfAngles(0.0, Math.PI);
            dome.SetClockAngles(0.0, Math.PI * 2);
            dome.Radius = 10000.0;
            m_sensorDome.Extensions.Add(new FieldOfViewExtension(dome));

            // Configure graphical display of the sensor dome.
            m_sensorDome.Extensions.Add(new FieldOfViewGraphicsExtension(new SensorFieldOfViewGraphics
            {
                DomeSurfaceMaterial = new GridMaterialGraphics
                {
                    Color = Color.White,
                    CellAlpha = 0.1,
                },
            }));

            // Define a rotating axes.
            var rotatingAxes = new AxesLinearRate
            {
                ReferenceAxes = new AxesEastNorthUp(m_earth, locationPoint),
                ReferenceEpoch = m_epoch,
                InitialRotation = UnitQuaternion.Identity,
                SpinAxis = UnitCartesian.UnitZ,
                InitialRotationalVelocity = Trig.DegreesToRadians(5.0), // 5 degrees per second
                RotationalAcceleration = 0.0,
            };

            // Define a rotation around X.
            UnitQuaternion quaternion = new UnitQuaternion(new AngleAxisRotation(Math.PI / 3.0, UnitCartesian.UnitX));
            // Define an angular offset for the rotating axes.
            var rotatedOffsetAxes = new AxesFixedOffset(rotatingAxes, quaternion);

            m_rotatingSensor = new Platform
            {
                Name = "Rotating Sensor",
                LocationPoint = locationPoint,
                OrientationAxes = rotatedOffsetAxes
            };

            // Set the identifier for the sensor in the CZML document. 
            m_rotatingSensor.Extensions.Add(new IdentifierExtension("RotatingSensor"));

            // Define the sensor geometry.
            m_rotatingSensor.Extensions.Add(new FieldOfViewExtension(new RectangularPyramid
            {
                XHalfAngle = Trig.DegreesToRadians(30),
                YHalfAngle = Trig.DegreesToRadians(30),
                Radius = 10000.0,
            }));

            // Configure graphical display of the sensor.
            m_rotatingSensor.Extensions.Add(new FieldOfViewGraphicsExtension(new SensorFieldOfViewGraphics
            {
                DomeSurfaceMaterial = new GridMaterialGraphics
                {
                    Color = Color.Green,
                    CellAlpha = 0.5,
                },
                LateralSurfaceMaterial = new GridMaterialGraphics
                {
                    Color = Color.Pink,
                    CellAlpha = 0.5,
                },
                IntersectionColor = Color.White,
                ShowIntersection = true,
                ShowEllipsoidHorizonSurfaces = true,
            }));
        }

        /// <summary>
        /// Create a fan to visualize an azimuth-elevation mask.
        /// </summary>
        private void CreateAzimuthElevationMask()
        {
            // load terrain data for Mount St. Helens.
            string dataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, AppDomain.CurrentDomain.RelativeSearchPath ?? "", "Data");
            var terrain = new AGIProcessedDataTerrain(Path.Combine(dataPath, @"Terrain\StHelens.pdtt"));

            // Calculate the surface position at the center of the terrain
            double longitude = (terrain.BoundingExtent.WestLongitude + terrain.BoundingExtent.EastLongitude) / 2.0;
            double latitude = (terrain.BoundingExtent.NorthLatitude + terrain.BoundingExtent.SouthLatitude) / 2.0;
            Cartographic observerPosition = new Cartographic(longitude, latitude, terrain.GetHeight(longitude, latitude));

            // Sample using 360 azimuth rays at 0.000275 degrees
            const int numberOfAzimuthSteps = 360;
            double stepSize = Trig.DegreesToRadians(0.000275);
            const double maxSearchAngle = 0.025;

            // Compute the mask.
            var mask = TerrainAzimuthElevationMask.Compute(terrain, observerPosition, numberOfAzimuthSteps, stepSize, maxSearchAngle);

            var locationPoint = new PointCartographic(m_earth, observerPosition);
            m_maskPlatform = new Platform
            {
                Name = "Azimuth Elevation Mask",
                LocationPoint = locationPoint,
                OrientationAxes = new AxesNorthEastDown(m_earth, locationPoint),
            };

            // Set the identifier for the mask in the CZML document. 
            m_maskPlatform.Extensions.Add(new IdentifierExtension("Mask"));

            // Attach the computed mask.
            m_maskPlatform.Extensions.Add(new AzimuthElevationMaskExtension(mask));

            // Define the graphics of the mask.
            m_maskPlatform.Extensions.Add(new AzimuthElevationMaskGraphicsExtension(new AzimuthElevationMaskGraphics
            {
                ProjectionRange = 5000.0,
                Material = new StripeMaterialGraphics
                {
                    EvenColor = Color.Blue,
                    OddColor = Color.White,
                    Repeat = 16.0,
                },
                Projection = AzimuthElevationMaskGraphicsProjection.ProjectToRange,
                NumberOfRings = 8,
                Outline = true,
                OutlineColor = Color.Black,
                OutlineWidth = 2.0,
            }));
        }

        /// <summary>
        /// Create an aircraft with two sensors.
        /// The aircraft will be visually represented by a labeled glTF model.
        /// </summary>
        private void CreateAircraft()
        {
            // Define waypoints for the aircraft's path and use the propagated point as the location point.
            Cartographic point1 = new Cartographic(Trig.DegreesToRadians(-122.0), Trig.DegreesToRadians(46.3), 4000.0);
            Cartographic point2 = new Cartographic(Trig.DegreesToRadians(-122.28), Trig.DegreesToRadians(46.25), 4100.0);
            Cartographic point3 = new Cartographic(Trig.DegreesToRadians(-122.2), Trig.DegreesToRadians(46.1), 6000.0);
            Cartographic point4 = new Cartographic(Trig.DegreesToRadians(-121.5), Trig.DegreesToRadians(46.0), 7000.0);
            Waypoint waypoint1 = new Waypoint(m_epoch, point1, 20.0, 0.0);
            Waypoint waypoint2 = new Waypoint(waypoint1, m_earth.Shape, point2, 20.0);
            Waypoint waypoint3 = new Waypoint(waypoint2, m_earth.Shape, point3, 20.0);
            Waypoint waypoint4 = new Waypoint(waypoint3, m_earth.Shape, point4, 20.0);
            var waypointPropagator = new WaypointPropagator(m_earth, waypoint1, waypoint2, waypoint3, waypoint4);
            var locationPoint = waypointPropagator.CreatePoint();

            m_aircraft = new Platform
            {
                Name = "Aircraft",
                LocationPoint = locationPoint,
                OrientationAxes = new AxesVehicleVelocityLocalHorizontal(m_earth.FixedFrame, locationPoint),
            };

            // Set the identifier for the aircraft in the CZML document. 
            m_aircraft.Extensions.Add(new IdentifierExtension("Aircraft"));

            // Hermite interpolation works better for aircraft-like vehicles.
            m_aircraft.Extensions.Add(new CesiumPositionExtension
            {
                InterpolationAlgorithm = CesiumInterpolationAlgorithm.Hermite
            });

            // Configure a glTF model for the aircraft.
            m_aircraft.Extensions.Add(new ModelGraphicsExtension(new ModelGraphics
            {
                // Link to a binary glTF file.
                Model = new CesiumResource(GetModelUri("aircraft.glb"), CesiumResourceBehavior.LinkTo),
                // Flip the model visually to point Z in the correct direction.
                NodeTransformations = new Dictionary<string, NodeTransformationGraphics>
                {
                    {
                        "Aircraft", new NodeTransformationGraphics
                        {
                            Rotation = new UnitQuaternion(new ElementaryRotation(AxisIndicator.Third, Math.PI))
                        }
                    }
                },
                RunAnimations = false,
            }));

            // Show the path of the aircraft.
            m_aircraft.Extensions.Add(new PathGraphicsExtension(new PathGraphics
            {
                Width = 2.0,
                LeadTime = Duration.FromHours(1.0).TotalSeconds,
                TrailTime = Duration.FromHours(1.0).TotalSeconds,
                Material = new PolylineOutlineMaterialGraphics
                {
                    Color = Color.White,
                    OutlineColor = Color.Black,
                    OutlineWidth = 1.0,
                },
            }));

            // Configure label for the aircraft.
            m_aircraft.Extensions.Add(new LabelGraphicsExtension(new LabelGraphics
            {
                Text = m_aircraft.Name,
                // Change label color over time.
                FillColor = new TimeIntervalCollection<Color>
                {
                    // Green by default...
                    TimeInterval.Infinite.AddData(Color.Green),
                    // Red between first and second waypoints.
                    new TimeInterval<Color>(waypoint1.Date, waypoint2.Date, Color.Red),
                },
                // Only show label when camera is far enough from the aircraft,
                // to avoid visually clashing with the model.
                DistanceDisplayCondition = new Bounds(1000.0, double.MaxValue),
            }));

            // Define a description for the aircraft which will be shown when selected in Cesium.
            m_aircraft.Extensions.Add(new DescriptionExtension(new Description("Aircraft with two offset sensors")));

            // Create 30 degree simple conic sensor definition
            var sensorCone = new ComplexConic();
            sensorCone.SetHalfAngles(0.0, Trig.DegreesToRadians(15));
            sensorCone.SetClockAngles(Trig.DegreesToRadians(20), Trig.DegreesToRadians(50));
            sensorCone.Radius = double.PositiveInfinity;

            // Create a sensor pointing "forward".
            // Position sensor underneath the wing.
            var sensorOneLocationPoint = new PointFixedOffset(m_aircraft.ReferenceFrame, new Cartesian(-3.0, 8.0, 0.0));
            var sensorAxesOne = new AxesAlignedConstrained(m_aircraft.OrientationAxes.GetVectorElement(CartesianElement.Z), AxisIndicator.Third,
                                                           m_aircraft.OrientationAxes.GetVectorElement(CartesianElement.X), AxisIndicator.First);
            // This rotation points the z-axis of the volume back along the x-axis of the ellipsoid.
            var rotationOne = new UnitQuaternion(new ElementaryRotation(AxisIndicator.Second, Constants.HalfPi / 4));

            m_aircraftSensorOne = new Platform
            {
                LocationPoint = sensorOneLocationPoint,
                OrientationAxes = new AxesFixedOffset(sensorAxesOne, rotationOne),
            };

            // Set the identifier for the sensor in the CZML document. 
            m_aircraftSensorOne.Extensions.Add(new IdentifierExtension("AircraftSensorOne"));

            m_aircraftSensorOne.Extensions.Add(new CesiumPositionExtension
            {
                InterpolationAlgorithm = CesiumInterpolationAlgorithm.Hermite
            });

            // Define the sensor geometry.
            m_aircraftSensorOne.Extensions.Add(new FieldOfViewExtension(sensorCone));

            // Configure graphical display of the sensor.
            m_aircraftSensorOne.Extensions.Add(new FieldOfViewGraphicsExtension(new SensorFieldOfViewGraphics
            {
                // Configure the outline of the projection onto the earth.
                EllipsoidSurfaceMaterial = new SolidColorMaterialGraphics(Color.White),
                IntersectionWidth = 2.0,
                LateralSurfaceMaterial = new GridMaterialGraphics
                {
                    Color = Color.FromArgb(171, Color.Blue),
                },
            }));

            // Create sensor pointing to the "side".
            // Position sensor underneath the wing.
            var sensorTwoLocationPoint = new PointFixedOffset(m_aircraft.ReferenceFrame, new Cartesian(-3.0, -8.0, 0.0));
            var sensorAxesTwo = new AxesAlignedConstrained(m_aircraft.OrientationAxes.GetVectorElement(CartesianElement.Z), AxisIndicator.Third,
                                                           m_aircraft.OrientationAxes.GetVectorElement(CartesianElement.Y), AxisIndicator.Second);

            // This rotation points the z-axis of the volume back along the x-axis of the ellipsoid.
            var rotationTwo = new UnitQuaternion(new ElementaryRotation(AxisIndicator.First, Constants.HalfPi / 2));

            m_aircraftSensorTwo = new Platform
            {
                LocationPoint = sensorTwoLocationPoint,
                OrientationAxes = new AxesFixedOffset(sensorAxesTwo, rotationTwo),
            };

            // Set the identifier for the sensor in the CZML document. 
            m_aircraftSensorTwo.Extensions.Add(new IdentifierExtension("AircraftSensorTwo"));

            m_aircraftSensorTwo.Extensions.Add(new CesiumPositionExtension
            {
                InterpolationAlgorithm = CesiumInterpolationAlgorithm.Hermite
            });

            // Define the sensor geometry.
            m_aircraftSensorTwo.Extensions.Add(new FieldOfViewExtension(sensorCone));

            // Configure graphical display of the sensor.
            m_aircraftSensorTwo.Extensions.Add(new FieldOfViewGraphicsExtension(new SensorFieldOfViewGraphics
            {
                // Configure the outline of the projection onto the earth.
                EllipsoidSurfaceMaterial = new SolidColorMaterialGraphics(Color.White),
                IntersectionWidth = 2.0,
                LateralSurfaceMaterial = new GridMaterialGraphics
                {
                    Color = Color.FromArgb(171, Color.Red),
                },
            }));

            // Create an access link between the aircraft and the observer position
            // on Mount St. Helens, using the same azimuth elevation mask to constrain access.

            m_aircraftAzimuthElevationMaskLink = new LinkInstantaneous(m_maskPlatform, m_aircraft);

            // Set the identifier for the link in the CZML document. 
            m_aircraftAzimuthElevationMaskLink.Extensions.Add(new IdentifierExtension("AircraftMountStHelensAccess"));

            // Constrain access using the azimuth-elevation mask.
            var query = new AzimuthElevationMaskConstraint(m_aircraftAzimuthElevationMaskLink, LinkRole.Transmitter);

            // Configure graphical display of the access link.
            m_aircraftAzimuthElevationMaskLink.Extensions.Add(new LinkGraphicsExtension(new LinkGraphics
            {
                // Show the access link only when access is satisfied.
                Show = new AccessQueryCesiumProperty<bool>(query, true, false, false),
                Material = new SolidColorMaterialGraphics(Color.Yellow),
            }));
        }

        /// <summary>
        /// This method will be called after all the objects above are created.
        /// </summary>
        public void WriteDocument(TextWriter writer)
        {
            // Configure the interval over which to generate data.
            // In this case, compute 1 day of data.
            var dataInterval = new TimeInterval(m_epoch, m_epoch.AddDays(1));

            // Create and configure the CZML document.
            var czmlDocument = new CzmlDocument
            {
                Name = "CesiumDemo",
                Description = "Demonstrates CZML generation using STK Components",
                RequestedInterval = dataInterval,
                // For this demonstration, include whitespace in the CZML
                // to enable easy inspection of the contents. In a real application,
                // this would usually be false to reduce file size.
                PrettyFormatting = true,
                // Configure the clock on the client to reflect the time for which the data is computed.
                Clock = new Clock
                {
                    Interval = dataInterval,
                    CurrentTime = dataInterval.Start,
                    Multiplier = 15.0,
                },
            };

            // Add all of our objects with graphical extensions.
            czmlDocument.ObjectsToWrite.Add(m_satellite);
            czmlDocument.ObjectsToWrite.Add(m_satelliteXAxis);
            czmlDocument.ObjectsToWrite.Add(m_satelliteYAxis);
            czmlDocument.ObjectsToWrite.Add(m_satelliteZAxis);
            czmlDocument.ObjectsToWrite.Add(m_satelliteSunVector);
            czmlDocument.ObjectsToWrite.Add(m_facility);
            czmlDocument.ObjectsToWrite.Add(m_satelliteFacilityLink);
            czmlDocument.ObjectsToWrite.Add(m_facilitySensor);
            czmlDocument.ObjectsToWrite.Add(m_sensorDome);
            czmlDocument.ObjectsToWrite.Add(m_rotatingSensor);
            czmlDocument.ObjectsToWrite.Add(m_maskPlatform);
            czmlDocument.ObjectsToWrite.Add(m_aircraft);
            czmlDocument.ObjectsToWrite.Add(m_aircraftSensorOne);
            czmlDocument.ObjectsToWrite.Add(m_aircraftSensorTwo);
            czmlDocument.ObjectsToWrite.Add(m_aircraftAzimuthElevationMaskLink);

            // Write the CZML.
            czmlDocument.WriteDocument(writer);
        }

        private static Uri GetModelUri(string modelName)
        {
            return new Uri(HttpContext.Current.Request.Url, "models/" + modelName);
        }

        private readonly string m_satelliteIdentifier;
        private readonly EarthCentralBody m_earth;
        private JulianDate m_epoch;
        private Platform m_satellite;
        private GraphicalVector m_satelliteXAxis;
        private GraphicalVector m_satelliteYAxis;
        private GraphicalVector m_satelliteZAxis;
        private GraphicalVector m_satelliteSunVector;
        private Platform m_facility;
        private LinkInstantaneous m_satelliteFacilityLink;
        private AccessQuery m_accessQuery;
        private Platform m_facilitySensor;
        private Platform m_sensorDome;
        private Platform m_rotatingSensor;
        private Platform m_maskPlatform;
        private Platform m_aircraft;
        private Platform m_aircraftSensorOne;
        private Platform m_aircraftSensorTwo;
        private LinkInstantaneous m_aircraftAzimuthElevationMaskLink;
    }
}
