/* 
 * NavAnalyst Demo Application
 * 
 * This demo application shows how many of the AGI.Foundation.Navigation methods can be used.
 * It shows a navigation analyst a snapshot of the Dilution of Precision (DOP) for a site, 
 * which satellites are in view of that site, and their azimuth and elevation. 
 * Additionally, we've incorporated an open-source charting package called
 * ZedGraph to draw the graphs in the application. Info on ZedGraph and can be found 
 * here: http://sourceforge.net/projects/zedgraph and here: http://zedgraph.sourceforge.net/documentation/default.html
 * We've added some interesting tooltips in the application as well. We'll explain those as we get further down.
 * 
 * Program Flow
 * Once NavAnalyst starts, the first thing you need to do is supply an almanac. Almanacs provide the 
 * information needed to propagate the GPS satellites' orbits. These orbits can then be used to determine 
 * visibility, azimuth, elevation, etc. An almanac provides reliable data for calculations related to dates 
 * 2 weeks on either side of the almanac's issue date (the information degrades slightly each day, but is 
 * still reliable for 2 weeks). One almanac is delivered with this demo, almanac_sem_week0917_319488.al3. When you start the application, 
 * click "..." next to "SEM Almanac."  Then, find almanac_sem_week0917_319488.al3.
 * Additional almanacs can be found on the AGI FTP site, located here: ftp://ftp.agi.com/pub/Catalog/Almanacs/SEM.
 * Once NavAnalyst starts, you will be able to select an almanac, by clicking the browse button (...) and picking a SEM formatted almanac. 
 * 
 * The demo start time/date is automatically set to March 20, 2017, 00:00 - the day the SampleAlmanac, and other files 
 * which we'll dicuss below, was issued. The stop date/time is set to March 21, 2017, 00:00.
 * 
 * Once the almanac is loaded, you can either output the almanac ephemeris into STK .e ephemeris files or go
 * on to add a receiver and further analysis. 
 * 
 * To export the almanac orbits to .e format, simply click the 'Export Ephemeris to .e files' button. A separate file 
 * will be created for each GPS satellite contained in the almanac. The files will be placed in the directory from which 
 * you selected the almanac. The orbits will be propagated for the time and timestep you specify on the Almanac tab.
 * 
 * To continue the analysis, click the 'Receiver' tab. A receiver model is required to specify the analysis conditions.
 * A receiver has the following properties:
 *      Fixed Mask Angle (the elevation angle below which GPS satellites will not be considered for tracking)
 *      Number of channels (each channel can track one GPS satellite)
 *      Solution type (a method of reducing the number of satellites to track if the number of satellites in view is greater 
 *          than the number of channels on the receiver)
 *      Location (latitude, longitude, and height)
 * The default settings represent a typical civilian GPS receiver. Change the location to the desired analysis location. 
 * Now click the 'Add Receiver' Button and NavAnalyst will calculate all the data necessary for the plots and add
 * the information to the tree in the upper-left of the tool. To change the receiver's characteristics, press the
 * 'Delete Receiver' button and repeat the above procedure.
 * 
 * Continuing on, you can now add either a Performance Assessment File (.paf), a Prediction Support File (.psf), or both, 
 * and then see navigation accuracy results. Sample files are provided for both types of accuracy calculation, found in Data/GPS. 
 * The information in the PSF is reliable for calculations related to dates 2 weeks on either side of the PSF's 
 * issue date (like the information in an almanac). However, the information in the PAF is ONLY reliable for calculations 
 * related to the exact day it was issued. If you want to calculate Assessed Accuracy over several days, you'll need to upload 
 * a PAF file for each day. See the documentation for an overview of navigation accuracy calculations.
 */
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.IO;

// Notice that only three references are needed in the reference section of the Solution Explorer (Core, Models, Platforms)
// You will need several namespaces from these three references however
using AGI.Foundation;
using AGI.Foundation.Navigation;
using AGI.Foundation.Navigation.DataReaders;
using AGI.Foundation.Time;
using AGI.Foundation.Geometry;
using AGI.Foundation.Coordinates;
using AGI.Foundation.Stk;
using AGI.Foundation.Celestial;
using AGI.Foundation.Platforms;
using AGI.Foundation.Propagators;
using AGI.Foundation.Access.Constraints;

// The reference for the ZedGraph charting package
using ZedGraph;

#region namespaces for localization exercise
using System.Threading;
using System.Globalization;
using AGI.Foundation.Navigation.Models;
#endregion

namespace NavAnalyst
{
    public partial class NavAnalyst : Form
   {           
        /// <summary>
        /// NavAnalyst Constructor
        /// </summary>
        public NavAnalyst()
        {
            #region code for localization exercise
            // Uncomment the appropriate code line to set the UI culture 
            // to French (France), German (Germany), Italian (Italy) or 
            // Japanese (Japan), or leave them all commented to use the 
            // default culture:
            // Thread.CurrentThread.CurrentUICulture = new CultureInfo("fr-FR");
            // Thread.CurrentThread.CurrentUICulture = new CultureInfo("de-DE");          
            // Thread.CurrentThread.CurrentUICulture = new CultureInfo("it-IT");
            // Thread.CurrentThread.CurrentUICulture = new CultureInfo("ja-JP");
            #endregion

            InitializeComponent();

            // Set the start and stop times to the selected dates, which are 24 hours apart.
            StartTime.Value = new DateTime(StartTime.Value.Year, StartTime.Value.Month, StartTime.Value.Day);
            StopTime.Value = StartTime.Value.AddDays(1.0);

            // Set dialogs to open in the application's execution directory.
            string dir = AppDomain.CurrentDomain.BaseDirectory + "/Data/GPS";
            openAlmanacDialog.InitialDirectory = dir;
            openPSFDialog.InitialDirectory = dir;
            openPAFDialog.InitialDirectory = dir;

            // Initialize the tree control
            rootNode = ObjectView.Nodes["NavAnalyst"];

            #region Strongly typed resources
            // Here and elsewhere we use strongly typed resources to support
            // localization of the application. String values for each
            // resource are defined in a resource file (in this case, 
            // Localization.resx).

            // Initialize the graphs with their respective axis labels and titles
            DOPGraph.GraphPane.Title.Text = Localization.DOPAnalysis;
            DOPGraph.GraphPane.XAxis.Title.Text = Localization.TimeUTC;
            DOPGraph.GraphPane.YAxis.Title.Text = Localization.DOPValue;

            AzElGraph.GraphPane.Title.Text = Localization.ElevationAngles;
            AzElGraph.GraphPane.XAxis.Title.Text = Localization.TimeUTC;
            AzElGraph.GraphPane.YAxis.Title.Text = Localization.ElevationDegrees;

            NavAccGraph.GraphPane.Title.Text = Localization.NavigationAccuracy;
            NavAccGraph.GraphPane.XAxis.Title.Text = Localization.TimeUTC;
            NavAccGraph.GraphPane.YAxis.Title.Text = Localization.PositionError;
            
            #endregion

            // Setup the line style definitions for the Azimuth/Elevation graph
            InitLineStyles();

            openAlmanacDialog.InitialDirectory = DataPath;
            openPAFDialog.InitialDirectory = DataPath;
            openPSFDialog.InitialDirectory = DataPath;
        }

        public static string DataPath
        {
            get { return Path.Combine(Application.StartupPath, "Data"); }
        }

        /// <summary>
        /// Method to open and initialize the almanac
        /// </summary>
        private void OnSelectAlmanacClick(object sender, EventArgs e)
        {
            // open a dialog to select the almanac file
            if (openAlmanacDialog.ShowDialog() == DialogResult.OK)
            {
                // enable the necessary buttons
                AddReceiver.Enabled = true;
                exportEphemeris.Enabled = true;

                // get the selected almanac information
                string fullpath = openAlmanacDialog.FileName;
                string filename = fullpath.Substring(fullpath.LastIndexOf("\\")+1);

                // set the almanac text field with just the almanac name
                almanacName.Text = filename;
                // set the tooltip for the almanac text box to the full path for the almanac
                toolTip1.SetToolTip(almanacName, openAlmanacDialog.FileName); 

                // update the treenode with the almanac information and expand the node
                TreeNode newNode = new TreeNode(filename);
                rootNode.Nodes.Add(newNode);
                rootNode.Expand();
            }
        }

        /// <summary>
        /// Method to export ephemeris to STK .e file
        /// </summary>
        #region ExportEphemeris
        private void OnExportEphemerisClick(object sender, EventArgs e)
        {
            // Open the almanac file
            SemAlmanac almanac;
            using (Stream stream = openAlmanacDialog.OpenFile())
            using (StreamReader reader = new StreamReader(stream))
            {
                almanac = SemAlmanac.ReadFrom(reader, 1);
            }

            // setup the progressbar
            // GPSElement contains a list of PRNs (PRNList) available in the almanac.
            progressBar1.Maximum = almanac.Count;
            progressBar1.Step = 1;

            // get just the almanac path here. This will allow us to place the new .e files in the same place the almanac was opened from.
            string outputPath = Path.GetDirectoryName(openAlmanacDialog.FileName);

            // Create a .e file for every PRN in the element set...
            foreach (SemAlmanacRecord record in almanac)
            {
                // provide a name and path for the new .e file.
                string filename = String.Format("PRN_{0:00}.e", record.PseudoRandomNumber);
                string path = outputPath + "\\" + filename;

                // check that the path exists
                if (File.Exists(path))
                {
                    // ask if it's OK to overwrite existing files. If it's not, advance the progress bar (passed this SV) and continue to the next SV
                    if (MessageBox.Show(string.Format("{0} " + Localization.existsOverwrite, filename), Localization.FileExists, MessageBoxButtons.YesNo) == DialogResult.No)
                    {
                        progressBar1.PerformStep();
                        continue;
                    }
                }
                // ok to overwrite existing files (or no file existed already)
                // the using block here will use the sw variable to write the .e file stream to.
                using (StreamWriter writer = File.CreateText(path))
                {
                    JulianDate startjd;
                    JulianDate stopjd;
                    // We need to know which time standard to use here. if the user has specified that GPS time is to be used
                    // we need to create the JulianDates with the GlobalPositioningSystemTime standard.
                    if (GPSTimeRadio.Checked)
                    {
                        startjd = new JulianDate(StartTime.Value, TimeStandard.GlobalPositioningSystemTime);
                        stopjd = new JulianDate(StopTime.Value, TimeStandard.GlobalPositioningSystemTime);
                    }
                    else
                    {
                        // otherwise, the default time standard is UTC
                        startjd = new JulianDate(StartTime.Value);
                        stopjd = new JulianDate(StopTime.Value);
                    }

                    // Now that we know the start and stop times for the .e file, we need to propagate the orbits
                    // the NavstarISGps200DPropagator take a Duration type for the timestep - let's create it
                    // using the user-specified timestep value
                    double timestep = double.Parse(TimeStep.Text);
                    Duration timestepDuration = Duration.FromSeconds(timestep);

                    // declare a NavstarISGps200DPropagator - this is used to propagate the satellite positions
                    // assign an instance of the propagator. The propagator constructor takes an almanac element set (for a single satellite)
                    NavstarISGps200DPropagator propagator = new NavstarISGps200DPropagator(record);

                    // now create an StkEphemerisFile object and assign its Ephemeris property the output of the propagator
                    StkEphemerisFile file = new StkEphemerisFile();

                    DateMotionCollection<Cartesian> ephemeris = propagator.Propagate(startjd, stopjd, timestepDuration, 0, propagator.ReferenceFrame);
                    StkEphemerisFile.EphemerisTimePos ephemerisFormat = new StkEphemerisFile.EphemerisTimePos
                    {
                        CoordinateSystem = propagator.ReferenceFrame, 
                        EphemerisData = ephemeris
                    };

                    file.Data = ephemerisFormat;

                    // write the .e ephemeris to the stream opened in this using block
                    file.WriteTo(writer);
                } // end of using block

                // update the progress bar
                progressBar1.PerformStep();
            } // end of PRN List

            // we're done!  Reset the progress bar
            progressBar1.Value = 0;
        }

        #endregion

        /// <summary>
        /// Method to calculate all the data required for the graphs.
        /// </summary>
        private void OnAddReceiverClick(object sender, EventArgs e)
        {
            #region CreateAndConfigureReceiver
            // Let's create the GPSReceiver. The receiver stores many properties and has a defined location. This location
            // is the point of reference for visibility calculations.
            receiver = new GpsReceiver();

            // add receiver to the tree
            TreeNode newNode = new TreeNode(Localization.Receiver);
            rootNode.Nodes.Add(newNode);

            // Easy reference to Earth Central body used to initialize the ElevationAngleAccessConstraint and
            // to calculate the Az/El/Range Data.
            EarthCentralBody earth = CentralBodiesFacet.GetFromContext().Earth;

            // set the receiver properties based on user selections
            // The receiver has a receiver FrontEnd that contains the visibility and tracking constraints
            // Be sure to convert your angles to Radians!
            double minimumAngle = Trig.DegreesToRadians(Double.Parse(MaskAngle.Text));
            receiver.ReceiverConstraints.Clear();
            receiver.ReceiverConstraints.Add(new ElevationAngleConstraint(earth, minimumAngle));
            receiver.NumberOfChannels = (int)NumberOfChannels.Value;
            receiver.NoiseModel = new ConstantGpsReceiverNoiseModel(0.8);

            // The receiver's methods of reducing the number of visible satellites to the limit imposed by the number of channels
            if (BestNSolType.Checked)
                receiver.ReceiverSolutionType = GpsReceiverSolutionType.BestN;
            else
                receiver.ReceiverSolutionType = GpsReceiverSolutionType.AllInView;

            // create a new location for the receiver by using the Cartographic type from AGI.Foundation.Coordinates
            // again, remember to convert from degrees to Radians! (except the height of course)
            Cartographic position = new Cartographic(Trig.DegreesToRadians(double.Parse(Longitude.Text)),
                                                     Trig.DegreesToRadians(double.Parse(Latitude.Text)),
                                                     double.Parse(ReceiverHeight.Text));

            // Now create an antenna for the GPS receiver. We specify the location of the antenna by assigning a
            // PointCartographic instance to the LocationPoint property. We specify that the antenna should be oriented
            // according to the typically-used East-North-Up axes by assigning an instance of AxesEastNorthUp to
            // the OrientationAxes property. While the orientation of the antenna won't affect which satellites are visible
            // or tracked in this case, it will affect the DOP values. For example, the EDOP value can be found in
            // DilutionOfPrecision.X, but only if we've configured the antenna to use East-North-Up axes.
            PointCartographic antennaLocationPoint = new PointCartographic(earth, position);
            Platform antenna = new Platform
            {
                LocationPoint = antennaLocationPoint, 
                OrientationAxes = new AxesEastNorthUp(earth, antennaLocationPoint)
            };
            receiver.Antenna = antenna;

            #endregion

            // update the tree to reflect the correct receiver info            
            newNode.Nodes.Add(new TreeNode(string.Format(Localization.FixedMaskAngle + "= {0:0.00} " + Localization.degrees, MaskAngle.Text)));
            newNode.Nodes.Add(new TreeNode(string.Format(Localization.NumberOfChannels + "= {0}", NumberOfChannels.Value)));
            newNode.Nodes.Add(new TreeNode(string.Format(Localization.SolutionType + "= {0}", receiver.ReceiverSolutionType == GpsReceiverSolutionType.AllInView ? Localization.AllInView : Localization.BestN)));
            newNode.Nodes.Add(new TreeNode(string.Format(Localization.Latitude + "= {0:0.000000} " + Localization.degrees, Latitude.Text)));
            newNode.Nodes.Add(new TreeNode(string.Format(Localization.Longitude + "= {0:0.000000} " + Localization.degrees, Longitude.Text)));
            newNode.Nodes.Add(new TreeNode(string.Format(Localization.Height + "= {0:0.000} " + Localization.meters, ReceiverHeight.Text)));

            rootNode.Expand();
            newNode.Expand();

            #region CalculateDataForGraphs

            // Now, we'll open the almanac
            SemAlmanac almanac;
            using (Stream stream = openAlmanacDialog.OpenFile())
            using (StreamReader reader = new StreamReader(stream))
            {
                // Read the SEM almanac from an almanac stream reader.
                almanac = SemAlmanac.ReadFrom(reader, 1);
            }

            // Now create a PlatformCollection to hold GpsSatellite object instances. The SemAlmanac method CreateSatellitesFromRecords returns 
            // just such a collection. We'll use this set of satellites as the set from which we'll try and track. There is a 
            // GpsSatellite object for each satellite specified in the almanac.
            PlatformCollection gpsSatellites = almanac.CreateSatelliteCollection();

            // We provide the receiver with the complete set of gpsSatellites to consider for visibility calculations.
            // This is usually all SVs defined in the almanac - however you may want to remove SVs that aren't healthy. This can
            // be done by creating the gpsSatellites collection above using another version of the CreateSatellitesFromRecords method that
            // takes a SatelliteOutageFileReader.
            receiver.NavigationSatellites = gpsSatellites;

            // Optimization opportunity: Add the following code in a thread. This will help for long duration analyses.

            // Now that we have the receiver and location setup, we need to evaluate all the pertinent data.
            // using a SatelliteTrackingEvaluator, we can track satellites and using a DOP Evaluator, 
            // we can calculate DOP at a specified time.
            // The receiver's GetSatelliteTrackingEvaluator method will provide a SatelliteTrackingEvaluator for you.
            // Similarly, the GetDilutionOfPrecisionEvaluator provides the DOP evaluator.
            // We create all evaluators in the same EvaluatorGroup for the best performance.

            EvaluatorGroup group = new EvaluatorGroup();
            Evaluator<int[]> satTrackingEvaluator = receiver.GetSatelliteTrackingIndexEvaluator(group);
            Evaluator<DilutionOfPrecision> dopEvaluator = receiver.GetDilutionOfPrecisionEvaluator(group);

            // We also need to create an evaluator to compute Azimuth/Elevation for each of the SVs
            MotionEvaluator<AzimuthElevationRange>[] aerEvaluators = new MotionEvaluator<AzimuthElevationRange>[gpsSatellites.Count];
            for (int i = 0; i < gpsSatellites.Count; ++i)
            {
                Platform satellite = receiver.NavigationSatellites[i];
                VectorTrueDisplacement vector = new VectorTrueDisplacement(antenna.LocationPoint, satellite.LocationPoint);
                aerEvaluators[i] = earth.GetAzimuthElevationRangeEvaluator(vector, group);
            }
 
            // First we'll initialize the data structures used to plot the data
            for (int i = 0; i < DOPData.Length; i++)
            {
                // PointPairList is defined in the ZedGraph reference
                DOPData[i] = new PointPairList();
            }

            // We need to know which time standard to use here. If the user has specified that GPS time is to be used
            // we need to create the JulianDates with the GlobalPositioningSystemTime standard.
            if (GPSTimeRadio.Checked)
            {
                startjd = new JulianDate(StartTime.Value, TimeStandard.GlobalPositioningSystemTime);
                stopjd = new JulianDate(StopTime.Value, TimeStandard.GlobalPositioningSystemTime);
            }
            else
            {
                // otherwise, the default time standard is UTC
                startjd = new JulianDate(StartTime.Value);
                stopjd = new JulianDate(StopTime.Value);
            }
            
            // Now we''ll create the variables we'll need for iterating through time.
            // The propagator requires a Duration type be used to specify the timestep.
            Duration dur = stopjd - startjd;
            double timestep = Double.Parse(TimeStep.Text);

            // Initialize the progressbar with appropriate values
            progressBar1.Maximum = (int)dur.TotalSeconds;
            progressBar1.Step = (int)timestep;

            // now we'll iterate through time by adding seconds to the start time JulianDate  
            // creating a new JulianDate 'evaluateTime' each time step.
            for (double t = 0; t <= dur.TotalSeconds; t += timestep)
            {
                JulianDate evaluateTime = startjd.AddSeconds(t);

                // The string 'trackedSVs' is the start of a string we'll continue to build through this time
                // iteration. It will contain the info we'll need to put in the DOP graph tooltips for the different
                // DOP series (VDOP, HDOP, etc.)
                String trackedSVs = Localization.Tracked + ": ";
                
                // The evaluator method GetTrackedSatellites will take the current time and the initial list of satellites and
                // determine which satellites can be tracked based on the receiver constraints setup earlier. This method 
                // returns a PlatformCollection object as well (though we'll cast each member of the Collection to a GPSSatellite type) 
                int[] trackedSatellites = satTrackingEvaluator.Evaluate(evaluateTime);
 
                foreach (int satelliteIndex in trackedSatellites)
                {
                    Platform satellite = receiver.NavigationSatellites[satelliteIndex];

                    // Now we have access to a Platform object representing a GPS satellite and calculate the azimuth and elevation
                    // of each.  Note that we're just calculating the azimuth and elevation, but could just as easily get the
                    // range as well.
                    AzimuthElevationRange aer = aerEvaluators[satelliteIndex].Evaluate(evaluateTime);

                    // Get the GpsSatelliteExtension attached to the platform. The extension extends a
                    // platform with GPS-specific information. In this case, we need the
                    // satellites PRN.
                    GpsSatelliteExtension extension = satellite.Extensions.GetByType<GpsSatelliteExtension>();

                    // Create two separate PointPairLists to hold the data stored by Time and Azimuth
                    PointPairList thisTimePointList, thisAzPointList;

                    // Before we can arbitrarily create new PointPair Lists, we have to see if the Data Storage structures already contain a list
                    // for this PRN.
                    // The variables AzElData_TimeBased and AzElData_AzimuthBased are dictionaries that hold the PointPairLists using the PRN
                    // as a key. We use this structure to store a large amount of data for every satellite in a single, easy to access, variable.
                    // if the satellite we're currently looking at already has a list defined in the dictionary, we'll use that one, otherwise
                    // we'll create a new list
                    if (AzElData_TimeBased.ContainsKey(extension.PseudoRandomNumber))
                    {
                        thisTimePointList = AzElData_TimeBased[extension.PseudoRandomNumber];
                        AzElData_TimeBased.Remove(extension.PseudoRandomNumber);
                    }
                    else
                    {
                        thisTimePointList = new PointPairList();
                    }

                    if (AzElData_AzimuthBased.ContainsKey(extension.PseudoRandomNumber))
                    {
                        thisAzPointList = AzElData_AzimuthBased[extension.PseudoRandomNumber];
                        AzElData_AzimuthBased.Remove(extension.PseudoRandomNumber);
                    }
                    else
                    {
                        thisAzPointList = new PointPairList();
                    }

                    // Now to get the actual Azimuth and elevation data

                    // Converting your Radians to degrees here makes the data appear in a more readable format. We also constrain the azimuth
                    // to be within the interval [0, 2*pi]
                    double azimuth = Trig.RadiansToDegrees(Trig.ZeroToTwoPi(aer.Azimuth));
                    double elevation = Trig.RadiansToDegrees(aer.Elevation);
                    #endregion
                    
                    // now create the point for the Azimuth based data
                    PointPair thisAzPoint = new PointPair(azimuth, elevation);                    
                    // and add the tooltip (ZedGraph uses the Tag property on a PointPair for the tooltip on that datapoint )
                    thisAzPoint.Tag = String.Format("PRN {0}, {1}, " + Localization.Az + ": {2:0.000}, " + Localization.El + ": {3:0.000}", extension.PseudoRandomNumber, evaluateTime.ToDateTime().ToString("M/d/yyyy, h:mm tt"), azimuth, elevation);
                    // and finally add this point to the list
                    thisAzPointList.Add(thisAzPoint);

                    // now we'll do the same for the time-based data, instead of adding the Az and El, we'll add the time and El.
                    // Create a new XDate object to store the time for this point
                    double txd = (double) new XDate(evaluateTime.ToDateTime());
                    // add the time and elevation data to this point
                    PointPair thisTimePoint = new PointPair(txd,Trig.RadiansToDegrees(aer.Elevation));            
                    // Create the tooltip tag
                    thisTimePoint.Tag = String.Format("PRN {0}, {1}, " + Localization.Az + ": {2:0.000}, " + Localization.El + ": {3:0.000}", extension.PseudoRandomNumber, evaluateTime.ToDateTime().ToString("M/d/yyyy, h:mm tt"), azimuth, elevation);
                    // finally add this point to the list
                    thisTimePointList.Add(thisTimePoint);
                    
                    //Now that this data is all calculated, we'll add the point lists to the correct data structures for the GpsSatellite we're working with
                    AzElData_TimeBased.Add(extension.PseudoRandomNumber, thisTimePointList);
                    AzElData_AzimuthBased.Add(extension.PseudoRandomNumber, thisAzPointList);

                    // now update the 'trackedSVs' string to be used for the DOP data tooltip
                    // wee need to do this inside the GpsSatellite loop because we need to get the entire list of tracked SVs for this time step.
                    // we won't use this string until we're out of the loop however.
                    trackedSVs += extension.PseudoRandomNumber.ToString() + ", ";
                }

                // now we're out of the GpsSatellite loop, we'll do some string manipulation to get the tooltip for the DOP data. 
                // (gets rid of the last inserted comma)
                string svs = trackedSVs.Substring(0, trackedSVs.LastIndexOf(' ') - 1);
                try
                {
                    // Now we use the evaluator to calculate the DilutionOfPrecision for us for this timestep
                    DilutionOfPrecision dop = dopEvaluator.Evaluate(evaluateTime);
  
                    // if the dop object throws an exception, there aren't enough tracked satellites to form a navigation solution (typically < 4 tracked)
                    // in that case we leave the data for this time unfilled. The graph will then have empty spots for this time.

                    // Here we create a new PointPair and a new XDate to add to the X-Axis
                    PointPair pp;
                    double txd = (double)new XDate(evaluateTime.ToDateTime());
                    // add the East DOP value and the time to the PointPair and set the tooltip tag property for this series.
                    pp = new PointPair(txd, dop.X);
                    pp.Tag = String.Format("{0}\n{1} " + Localization.EDOP + ": {2:0.000}", svs, evaluateTime.ToDateTime().ToString("M/d/yyyy, h:mm tt"), dop.X);
                    // add the point to the 0th element of the DOPData structure
                    DOPData[0].Add(pp);
                    // repeat for North DOP
                    pp = new PointPair(txd, dop.Y);
                    pp.Tag = String.Format("{0}\n{1} " + Localization.NDOP + ": {2:0.000}", svs, evaluateTime.ToDateTime().ToString("M/d/yyyy, h:mm tt"), dop.Y);
                    DOPData[1].Add(pp);
                    // repeat for the Vertical DOP
                    pp = new PointPair(txd, dop.Z);
                    pp.Tag = String.Format("{0}\n{1} " + Localization.VDOP + ": {2:0.000}", svs, evaluateTime.ToDateTime().ToString("M/d/yyyy, h:mm tt"), dop.Z);
                    DOPData[2].Add(pp);
                    // repeat for the Horizontal DOP
                    pp = new PointPair(txd, dop.XY);
                    pp.Tag = String.Format("{0}\n{1} " + Localization.HDOP + ": {2:0.000}", svs, evaluateTime.ToDateTime().ToString("M/d/yyyy, h:mm tt"), dop.XY);
                    DOPData[3].Add(pp);
                    // repeat for the Position DOP
                    pp = new PointPair(txd, dop.Position);
                    pp.Tag = String.Format("{0}\n{1} " + Localization.PDOP + ": {2:0.000}", svs, evaluateTime.ToDateTime().ToString("M/d/yyyy, h:mm tt"), dop.Position);
                    DOPData[4].Add(pp);
                    // repeat for the Time DOP
                    pp = new PointPair(txd, dop.Time);
                    pp.Tag = String.Format("{0}\n{1} " + Localization.TDOP + ": {2:0.000}", svs, evaluateTime.ToDateTime().ToString("M/d/yyyy, h:mm tt"), dop.Time);
                    DOPData[5].Add(pp);
                    // repeat for the Geometric DOP
                    pp = new PointPair(txd, dop.Geometric);
                    pp.Tag = String.Format("{0}\n{1} " + Localization.GDOP + ": {2:0.000}", svs, evaluateTime.ToDateTime().ToString("M/d/yyyy, h:mm tt"), dop.Geometric);
                    DOPData[6].Add(pp);

                    // Notice here that the different DOP values (East, North, etc) were denoted by the dop.X, dop.Y etc. This is because the 
                    // DOP values could be in any coordinate system. In our case, we're in the ENU coordinate system an X represents East, Y
                    // represents North, Z represents Vertical, XY represents horizontal. You can change the reference frame the DOP is reported in 
                    // but you will then have to understand that the dop.X value corresponds to your X-defined axis and so on.
                }
                catch
                {
                    // Do Nothing here - we just won't add the data to the data list
                }
                // update the progress bar - we're done with this time step!
                progressBar1.PerformStep();
            }
            // finally update the graphs
            UpdateDopGraph();
            updateAzElGraph();

            // reset the progress bar
            progressBar1.Value = 0;

            // and set the appropriate button states
            SetControlStates(true);
        }
        
        /// <summary>
        /// Removes the receiver definition and clears the graphs
        /// </summary>
        private void OnDeleteReceiverClick(object sender, EventArgs e)
        {
            // when a receiver is deleted, we need to clear the graphs and delete the information in the tree
            // to delete info from a graph, just clear the graphs's curvelist
            DOPGraph.GraphPane.CurveList.Clear();
            AzElGraph.GraphPane.CurveList.Clear();
            NavAccGraph.GraphPane.CurveList.Clear();

            // also, we need to clear all the data from the DOPData structure
            foreach (PointPairList ppl in DOPData)
            {
                ppl.Clear();
            }

            // clear all the Azimuth / elevation data from their respective structures.
            AzElData_TimeBased.Clear();
            AzElData_AzimuthBased.Clear();

            // Clear all the Nav Accuracy Data from the zedGraph data structure that contain them.
            AsAccData.Clear();
            PredAccData.Clear();

            // always do these two steps, these make the graphs update themselves
            DOPGraph.AxisChange();
            DOPGraph.Invalidate();

            AzElGraph.AxisChange();
            AzElGraph.Invalidate();

            NavAccGraph.AxisChange();
            NavAccGraph.Invalidate();

            // Now clear the tree of the receiver, PSF and PAF  information
            // Except for the first node (Almanac), remove all other nodes.
            string firstNodeName = rootNode.FirstNode.Text;
            rootNode.Nodes.Clear();
            TreeNode newNode = new TreeNode(firstNodeName);
            rootNode.Nodes.Add(newNode);
            rootNode.Expand();

            //Clear the textboxes that contain the PAF and PSF names.
            PAFName.Clear();
            PSFName.Clear();

            //enable and disable the appropriate buttons
            SetControlStates(false);
        }

        // These methods provide the menu bar functionality

        #region Menu Item select methods

        private void OnContentsToolStripMenuItemClick(object sender, EventArgs e)
        {
            // display the help form here
            HelpForm hf = new HelpForm();
            hf.Show();
        }

        private void OnAboutToolStripMenuItemClick(object sender, EventArgs e)
        {
            //Display About here
            AboutBox ab = new AboutBox();
            ab.ShowDialog();
        }

        private void OnExitToolStripMenuItemClick(object sender, EventArgs e)
        {
            // Exit here
            Application.Exit();
        }

        #endregion

        /// <summary>
        /// Method to set all changable controls enabled states
        /// </summary>
        /// <param name="isSet">state to which controls will be set</param>
        private void SetControlStates(bool isSet)
        {
            // set Add/Delete Receiver and PSF/PAF file selection button states
            AddReceiver.Enabled = !isSet;
            DeleteReceiver.Enabled = isSet;
            SelectPAF.Enabled = isSet;
            SelectPSF.Enabled = isSet;

            // set axis and series controls
            dateTimeXAxis.Enabled = isSet;
            azimuthAxis.Enabled = isSet;
            DisplayEDOP.Enabled = isSet;
            DisplayGDOP.Enabled = isSet;
            DisplayHDOP.Enabled = isSet;
            DisplayNDOP.Enabled = isSet;
            DisplayPDOP.Enabled = isSet;
            DisplayTDOP.Enabled = isSet;
            DisplayVDOP.Enabled = isSet;

            PredAccCheckBox.Enabled = isSet;
            AsAccCheckBox.Enabled = isSet;
            UseExtrapolationCheckBox.Enabled = isSet;
            ConfIntvlUpDown.Enabled = isSet;
        }

        /// <summary>
        /// Structure to hold the color and symbol type for the line styles
        /// </summary>
        private struct LineStylesForPRN
        {
            /// <summary>
            /// Color of the line and symbol
            /// </summary>
            public Color color;

            /// <summary>
            /// Symbol type (square, circle, etc.)
            /// </summary>
            public SymbolType symbol;
        }

        #region Navigation Accuracy (Assessed and Predicted) Computation

        /// <summary>
        /// Method to allow the user the select the PAF file and compute Assessed Navigation Accuracy.
        /// </summary>
        private void OnSelectPAFClick(object sender, EventArgs e)
        {
            // open a dialog to select the PAF file
            if (openPAFDialog.ShowDialog() == DialogResult.OK)
            {
                // get the selected PAF information
                string fullpath = openPAFDialog.FileName;
                string filename = Path.GetFileName(fullpath);

                // set the PAF text field with just the PAF name
                PAFName.Text = filename;
                // set the tooltip for the PAF text box to the full path for the PAF
                PAFtoolTip.SetToolTip(PAFName, openPAFDialog.FileName);

                // update the treenode with the PAF information and expand the node
                TreeNode newNode = new TreeNode(filename);
                rootNode.Nodes.Add(newNode);
                rootNode.Expand();

                //Invoke the method to calculate Assessed Navigation Accuracy.
                ComputeAssessedAccuracy(fullpath);
            }
        }

        /// <summary>
        /// Method to allow the user the select the PSF file and compute Predicted Navigation Accuracy.
        /// </summary>
        private void OnSelectPSFClick(object sender, EventArgs e)
        {
            // open a dialog to select the PSF file
            if (openPSFDialog.ShowDialog() == DialogResult.OK)
            {
                // get the selected PSF information
                string fullpath = openPSFDialog.FileName;
                string filename = Path.GetFileName(fullpath);

                // set the PSF text field with just the PSF name.
                PSFName.Text = filename;
                // set the tooltip for the PSF text box to the full path for the PSF.
                PSFtoolTip.SetToolTip(PSFName, openPSFDialog.FileName);

                // update the treenode with the PSF information and expand the node
                TreeNode newNode = new TreeNode(filename);
                rootNode.Nodes.Add(newNode);
                rootNode.Expand();

                //Now kick off the method to calculate Predicted Navigation Accuracy.
                ComputePredictedAccuracy(fullpath);
            }
        }

        #endregion

        private void NavAccGraph_Load(object sender, EventArgs e)
        {
        }

        private GpsReceiver receiver;
        private readonly TreeNode rootNode;
        private readonly PointPairList[] DOPData = new PointPairList[7];
        private readonly PointPairList AsAccData = new PointPairList();
        private readonly PointPairList PredAccData = new PointPairList();
        private Dictionary<int, PointPairList> AzElData_TimeBased = new Dictionary<int, PointPairList>();
        private Dictionary<int, PointPairList> AzElData_AzimuthBased = new Dictionary<int, PointPairList>();
        private LineStylesForPRN[] prnStyles = new LineStylesForPRN[36];
        private bool plotXAxisAsTime = true;
        private JulianDate startjd;
        private JulianDate stopjd;
   }
}
