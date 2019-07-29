using System;
using System.Drawing;
using System.Windows.Forms;
using AGI.Foundation;
using AGI.Foundation.Navigation;
using AGI.Foundation.Navigation.DataReaders;
using AGI.Foundation.Time;
using ZedGraph;

namespace NavAnalyst
{
    public partial class NavAnalyst : Form
    {
        #region NavAccuracy

        /// <summary>
        /// Method to calculate the Assessed Navigation Accuracy.
        /// </summary>
        /// <param name="pafFile">Fully qualified PAF file.</param>
        private void ComputeAssessedAccuracy(string pafFile)
        {
            // See the documentation for an overview of calculating navigation accuracy.

            // Populate the satellites with PAF data, extrapolating if the user requests.
            PerformanceAssessmentFile paf = PerformanceAssessmentFile.ReadFrom(pafFile);
            paf.DefaultAllowExtrapolation = UseExtrapolationCheckBox.Checked;

            try
            {
                //Obtain the evaluator.
                using (Evaluator<NavigationAccuracyAssessed> accuracyAssessedEvaluator = receiver.GetNavigationAccuracyAssessedEvaluator(paf))
                {
                    //Now, let's set up the data structures for the graph to display.
                    ComputeValuesForAssessedAccGraph(accuracyAssessedEvaluator);
                }
                //And now, display the graph.
                DisplayNavAccGraph(AsAccData, Color.Blue, AsAccCheckBox.Checked, Localization.Assessed);
            }
            catch (SystemException e)
            {
                MessageBox.Show(e.Message);
            }
        }

        /// <summary>
        /// Method to obtain the Assessed Nav Accuracy at each timestep and populate the 
        /// data structures that will be used to draw the graph.
        /// </summary>
        /// <param name="accuracyAssessedEvaluator">Evaluator for Assesssed Nav Accuracy.</param>
        private void ComputeValuesForAssessedAccGraph(Evaluator<NavigationAccuracyAssessed> accuracyAssessedEvaluator)
        {
            Duration dur = stopjd - startjd;
            double timestep = Double.Parse(TimeStep.Text);
            Duration ts = Duration.FromSeconds(timestep);

            // Initialize the progressbar with appropriate values
            progressBar1.Maximum = (int)dur.TotalSeconds;
            progressBar1.Step = (int)timestep;

            // now we'll iterate through time by adding seconds to the start time JulianDate object - 
            // creating a new JulianDate each time step.
            for (JulianDate jd = startjd; jd <= stopjd; jd += ts)
            {
                try
                {
                    //Evaluate at this particular time.
                    NavigationAccuracyAssessed accuracyAssessed = accuracyAssessedEvaluator.Evaluate(jd);
                    double txd = new XDate(jd.ToDateTime());
                    if (accuracyAssessed != null)
                    {
                        // Store it away in the PointPairList.
                        AsAccData.Add(txd, accuracyAssessed.PositionSignalInSpace);
                    }
                }
                catch
                {
                }
                // update the progress bar - we're done with this time step!
                progressBar1.PerformStep();
            }
            // reset the progress bar
            progressBar1.Value = 0;
        }

        /// <summary>
        /// Method to calculate the Predicted Navigation Accuracy.
        /// </summary>
        private void ComputePredictedAccuracy(string psfFile)
        {
            // Populate the satellites with PSF data
            PredictionSupportFile psf = PredictionSupportFile.ReadFrom(psfFile);

            try
            {
                // Obtain the Predicted Accuracy Evaluator.
                using (Evaluator<NavigationAccuracyPredicted> accuracyPredictedEvaluator = receiver.GetNavigationAccuracyPredictedEvaluator(psf))
                {
                    // Now, let's set up the data structures for the graph to display.
                    ComputeValuesForPredictedAccGraph(accuracyPredictedEvaluator);
                }
                // And now, display the graph.
                DisplayNavAccGraph(PredAccData, Color.Red, PredAccCheckBox.Checked, Localization.Predicted);
            }
            catch (SystemException e)
            {
                MessageBox.Show(e.Message);
            }
        }

        /// <summary>
        /// Method to obtain the Predicted Nav Accuracy at each timestep and populate the 
        /// data structures that will be used to draw the graph.
        /// </summary>
        /// <param name="accuracyPredictedEvaluator">Evaluator for Predicted Nav Accuracy.</param>
        private void ComputeValuesForPredictedAccGraph(Evaluator<NavigationAccuracyPredicted> accuracyPredictedEvaluator)
        {
            Duration dur = stopjd - startjd;
            double timestep = Double.Parse(TimeStep.Text);
            Duration ts = Duration.FromSeconds(timestep);
            PredAccData.Clear();

            // create a new Confidence Interval
            ConfidenceInterval ci = new ConfidenceInterval();

            // Initialize the progressbar with appropriate values
            progressBar1.Maximum = (int)dur.TotalSeconds;
            progressBar1.Step = (int)timestep;

            // now we'll iterate through time by adding seconds to the start time JulianDate object - 
            // creating a new JulianDate each time step.
            for (JulianDate jd = startjd; jd <= stopjd; jd += ts)
            {
                try
                {
                    NavigationAccuracyPredicted accuracyPredicted = accuracyPredictedEvaluator.Evaluate(jd);
                    double txd = new XDate(jd.ToDateTime());
                    // Lets use the specified confidence interval for our Accuracy Predictions.
                    if (accuracyPredicted != null)
                    {
                        // we're using a ConfidenceInterval instance here to convert the predicted nav accuracy to a standard 
                        // confidence percentile.
                        PredAccData.Add(txd,
                                        ci.ConvertToGlobalPositioningSystemConfidence(accuracyPredicted.PositionSignalInSpace,
                                                                                      (int)ConfIntvlUpDown.Value,
                                                                                      ConfidenceIntervalVariableDimension.Three));
                    }
                }
                catch
                {
                }
                // update the progress bar - we're done with this time step!
                progressBar1.PerformStep();
            }
            // reset the progress bar
            progressBar1.Value = 0;
        }

        #endregion
    }
}