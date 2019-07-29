using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;
using AGI.Foundation;
using AGI.Foundation.Access;
using AGI.Foundation.Access.Constraints;
using AGI.Foundation.Celestial;
using AGI.Foundation.Communications;
using AGI.Foundation.Communications.SignalProcessing;
using AGI.Foundation.Coordinates;
using AGI.Foundation.Geometry;
using AGI.Foundation.Infrastructure;
using AGI.Foundation.Navigation;
using AGI.Foundation.Navigation.Advanced;
using AGI.Foundation.Navigation.DataReaders;
using AGI.Foundation.Platforms;
using AGI.Foundation.Time;

namespace GPSCommunications
{
    public partial class Main : Form
    {
        public Main()
        {
            InitializeComponent();

            var earth = CentralBodiesFacet.GetFromContext().Earth;

            // Load the GPS satellites from the almanac, including transmitters for specific block types,
            // using the data from the GPSData.txt file.
            m_gpsConstellation = GetGpsCommunicationsConstellation();

            // create the receiver using a standard setup
            m_receiver = new GpsReceiver
            {
                NumberOfChannels = 12,
                ReceiverSolutionType = GpsReceiverSolutionType.AllInView, NavigationSatellites = m_gpsConstellation
            };
            m_receiver.ReceiverConstraints.Add(new ElevationAngleConstraint(Trig.DegreesToRadians(5.0)));

            m_location = new PointCartographic(earth, new Cartographic(Trig.DegreesToRadians(-107.494000), Trig.DegreesToRadians(30.228800), 0));
            m_orientation = new AxesEastNorthUp(earth, m_location);
        }

        private PlatformCollection GetGpsCommunicationsConstellation()
        {
            try
            {
                m_analysisTime = new JulianDate(DateTime.Today);
                return GpsCommunicationsConstellation.Download();
            }
            catch (DataUnavailableException)
            {
                // Read from local files if the machine does not have access to the internet.
                m_analysisTime = new JulianDate(new GregorianDate(2017, 3, 20));
                string dataPath = Path.Combine(Application.StartupPath, "Data");
                var almanac = SemAlmanac.ReadFrom(Path.Combine(dataPath, "almanac_sem_week0917_319488.al3"), 1);
                var gpsSatelliteInfo = GpsDataFile.ReadActive(Path.Combine(dataPath, "GPSData.txt"));
                return GpsCommunicationsConstellation.Create(almanac, gpsSatelliteInfo);
            }
        }

        private void btnCalculate_Click(object sender, EventArgs e)
        {
            textBox1.Clear();

            // determine the user's selected receiver type
            GpsSignalConfiguration selectedReceiver = GetSelectedReceiver();

            // create a GPS Comm front end
            var frontEnd = new GpsCommunicationsFrontEnd(m_gpsConstellation, selectedReceiver, m_location, m_orientation)
            {
                // set the front end's epoch - essentially when it starts receiving signals
                Epoch = m_analysisTime
            };

            // apply the front end to the receiver's Antenna property
            m_receiver.Antenna = frontEnd;

            // create a new communications-based noise model for the receiver
            m_receiver.NoiseModel = new GpsCommunicationsNoiseModel(frontEnd);

            // Jammer location - 10 km directly above the receiver
            var jammerLocationPoint = new PointFixedOffset(frontEnd.ReferenceFrame, new Cartesian(0, 0, 10000));
            if (cbL1Jammer.Checked)
            {
                // 1 watt, narrow band jammer centered at L1
                // increase to 10 watts to see a more dramatic effect
                // adds interferers to the front end's internal CommunicationSystem
                frontEnd.InterferingSources.Add(new SimpleAnalogTransmitter("L1Jammer", jammerLocationPoint, 1575.42e6, 1.0, 1.023e6));
            }

            if (cbL2Jammer.Checked)
            {
                // 1 watt, narrow band jammer centered at L2
                frontEnd.InterferingSources.Add(new SimpleAnalogTransmitter("L2Jammer", jammerLocationPoint, 1227.6e6, 1.0, 1.023e6));
            }

            if (cbL5Jammer.Checked)
            {
                // 1 watt, narrow band jammer centered at L5
                frontEnd.InterferingSources.Add(new SimpleAnalogTransmitter("L5Jammer", jammerLocationPoint, 1176.45e6, 1.0, 1.023e6));
            }

            var builder = new StringBuilder();

            // get an evaluator for the receiver that reports which satellites are tracked
            // we could get the standard DilutionOfPrecision, AssessedNavigationAccuracy or PredictedNavigationAccuracy
            // evaluators here as well.
            PlatformCollection trackedSatellites;
            using (var satelliteTrackingEvaluator = m_receiver.GetSatelliteTrackingEvaluator())
            {
                // evaluate the satellite tracking evaluator at a single time
                trackedSatellites = satelliteTrackingEvaluator.Evaluate(m_analysisTime);
            }

            builder.AppendFormat("{0} SVs tracked", trackedSatellites.Count)
                   .AppendLine();

            var prnsTracked = new List<int>();

            if (trackedSatellites.Count > 0)
            {
                // create the link budget scalars by hand for just the tracked SVs.
                // we'll use the link budgets we received from the front end later
                // this section shows how to create them from scratch if necessary

                // get the propagation graph from the front end
                var graph = frontEnd.GetFrontEndSignalPropagationGraph();

                foreach (var satellite in trackedSatellites)
                {
                    int prn = ServiceHelper.GetService<IGpsPrnService>(satellite).PseudoRandomNumber;
                    prnsTracked.Add(prn);

                    var satelliteInformation = ServiceHelper.GetService<IGpsSatelliteInformationService>(satellite).Information;

                    builder.AppendFormat("SVN {0}, PRN {1}, Block: {2}", satelliteInformation.SatelliteVehicleNumber, prn, satelliteInformation.Block)
                           .AppendLine();

                    var receiverChannels = frontEnd.GetReceiverChannels();
                    if (receiverChannels.ContainsSatelliteID(prn))
                    {
                        // gets the first channel in the receiver tracking the specified PRN.
                        // there should only be one.
                        var channel = receiverChannels.FindFirst(prn);

                        // get the link for the primary signal on that channel
                        var linkService = ServiceHelper.GetService<ILinkService>(channel.FindFirst(NavigationSignalPriority.Primary).ChannelLink);
                        var primarySignalReceiver = linkService.Receiver;

                        // use the transmitter on that link to identify the proper signal to analyze
                        var strategy = new IntendedSignalByTransmitter(linkService.Transmitter);

                        // create the desired scalars
                        if (cbOutputCNI.Checked)
                        {
                            var scalar = new ScalarCarrierToNoiseDensityPlusInterference(primarySignalReceiver, graph, strategy);
                            using (var evaluator = scalar.GetEvaluator())
                            {
                                builder.AppendFormat("C/(N0+I): {0:F5} dB-Hz", CommunicationAnalysis.ToDecibels(evaluator.Evaluate(m_analysisTime)))
                                       .AppendLine();
                            }
                        }

                        if (cbOutputJS.Checked)
                        {
                            var scalar = new ScalarJammingToSignal(primarySignalReceiver, graph, strategy);
                            using (var evaluator = scalar.GetEvaluator())
                            {
                                builder.AppendFormat("J/S: {0:F5} dB", Math.Abs(CommunicationAnalysis.ToDecibels(evaluator.Evaluate(m_analysisTime))))
                                       .AppendLine();
                            }
                        }

                        if (cbOutputNI.Checked)
                        {
                            var scalar = new ScalarNoisePlusInterference(linkService.Receiver, graph, strategy);
                            using (var evaluator = scalar.GetEvaluator())
                            {
                                builder.AppendFormat("N+I: {0:F5} dB", CommunicationAnalysis.ToDecibels(evaluator.Evaluate(m_analysisTime)))
                                       .AppendLine();
                            }
                        }

                        if (cbOutputRcvrNoise.Checked)
                        {
                            var scalar = new ScalarGpsCommunicationsReceiverChannelNoise(m_receiver, prn);
                            using (var evaluator = scalar.GetEvaluator())
                            {
                                builder.AppendFormat("Receiver Noise: {0:F5} meters", evaluator.Evaluate(m_analysisTime))
                                       .AppendLine();
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("No Channel tracking that PRN!");
                    }
                }
            }

            // Use the link budgets provided by the front-end.
            // Note that GpsReceiver noise is not a LinkBudget parameter.
            foreach (var linkBudgetScalars in frontEnd.GetAllLinkBudgets(m_receiver))
            {
                if (cbForTrackedSVsOnly.Checked)
                {
                    if (prnsTracked.Contains(linkBudgetScalars.SatelliteID))
                    {
                        if (cbOutputCNI.Checked)
                        {
                            using (var evaluator = linkBudgetScalars.CarrierToNoiseDensityPlusInterference.GetEvaluator())
                            {
                                builder.AppendFormat("C/(N0+I) for PRN {0}, on {1}: {2:F5} dB-Hz",
                                                     linkBudgetScalars.SatelliteID, linkBudgetScalars.SignalType, CommunicationAnalysis.ToDecibels(evaluator.Evaluate(m_analysisTime)))
                                       .AppendLine();
                            }
                        }

                        if (cbOutputJS.Checked)
                        {
                            using (var evaluator = linkBudgetScalars.JammingToSignal.GetEvaluator())
                            {
                                builder.AppendFormat("J/S for PRN {0}, on {1}: {2:F5} dB",
                                                     linkBudgetScalars.SatelliteID, linkBudgetScalars.SignalType, Math.Abs(CommunicationAnalysis.ToDecibels(evaluator.Evaluate(m_analysisTime))))
                                       .AppendLine();
                            }
                        }

                        if (cbOutputNI.Checked)
                        {
                            using (var evaluator = linkBudgetScalars.NoisePlusInterference.GetEvaluator())
                            {
                                builder.AppendFormat("N+I for PRN {0}, on {1}: {2:F5} dB",
                                                     linkBudgetScalars.SatelliteID, linkBudgetScalars.SignalType, CommunicationAnalysis.ToDecibels(evaluator.Evaluate(m_analysisTime)))
                                       .AppendLine();
                            }
                        }
                    }
                }
                else
                {
                    if (cbOutputCNI.Checked)
                    {
                        using (var evaluator = linkBudgetScalars.CarrierToNoiseDensityPlusInterference.GetEvaluator())
                        {
                            builder.AppendFormat("C/(N0+I) for PRN {0}, on {1}: {2:F5} dB-Hz",
                                                 linkBudgetScalars.SatelliteID, linkBudgetScalars.SignalType, CommunicationAnalysis.ToDecibels(evaluator.Evaluate(m_analysisTime)))
                                   .AppendLine();
                        }
                    }

                    if (cbOutputJS.Checked)
                    {
                        using (var evaluator = linkBudgetScalars.JammingToSignal.GetEvaluator())
                        {
                            builder.AppendFormat("J/S for PRN {0}, on {1}: {2:F5} dB",
                                                 linkBudgetScalars.SatelliteID, linkBudgetScalars.SignalType, Math.Abs(CommunicationAnalysis.ToDecibels(evaluator.Evaluate(m_analysisTime))))
                                   .AppendLine();
                        }
                    }

                    if (cbOutputNI.Checked)
                    {
                        using (var evaluator = linkBudgetScalars.NoisePlusInterference.GetEvaluator())
                        {
                            builder.AppendFormat("N+I for PRN {0}, on {1}: {2:F5} dB",
                                                 linkBudgetScalars.SatelliteID, linkBudgetScalars.SignalType, CommunicationAnalysis.ToDecibels(evaluator.Evaluate(m_analysisTime)))
                                   .AppendLine();
                        }
                    }
                }
            }

            textBox1.Text = builder.ToString();
        }

        private GpsSignalConfiguration GetSelectedReceiver()
        {
            switch ((string)cbReceiverType.SelectedItem)
            {
                case "SingleFrequencyL1CA":
                    return GpsSignalConfiguration.SingleFrequencyL1CA;
                case "SingleFrequencyL1M":
                    return GpsSignalConfiguration.SingleFrequencyL1M;
                case "SingleFrequencyL1PY":
                    return GpsSignalConfiguration.SingleFrequencyL1PY;
                case "DualFrequencyL1CAL2C":
                    return GpsSignalConfiguration.DualFrequencyL1CAL2C;
                case "DualFrequencyL1CAL5IQ":
                    return GpsSignalConfiguration.DualFrequencyL1CAL5IQ;
                case "DualFrequencyL2CL5IQ":
                    return GpsSignalConfiguration.DualFrequencyL2CL5IQ;
                case "DualFrequencyL1PYL2PY":
                    return GpsSignalConfiguration.DualFrequencyL1PYL2PY;
                case "DualFrequencyHandoverL1CAL1PYL2PY":
                    return GpsSignalConfiguration.DualFrequencyHandoverL1CAL1PYL2PY;
                case "DualFrequencyMCodeL1ML2M":
                    return GpsSignalConfiguration.DualFrequencyMCodeL1ML2M;
                default:
                    cbReceiverType.SelectedText = "SingleFrequencyL1CA";
                    return GpsSignalConfiguration.SingleFrequencyL1CA;
            }
        }

        private readonly PlatformCollection m_gpsConstellation;
        private readonly GpsReceiver m_receiver;
        private readonly PointCartographic m_location;
        private readonly AxesEastNorthUp m_orientation;
        private JulianDate m_analysisTime;
    }
}