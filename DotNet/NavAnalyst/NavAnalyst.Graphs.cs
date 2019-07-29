using System;
using System.Drawing;
using System.Windows.Forms;
using ZedGraph;

namespace NavAnalyst
{
    public partial class NavAnalyst : Form
    {
        /// <summary>
        /// Updates the DOP graph with the calculated data
        /// </summary>
        private void UpdateDopGraph()
        {
            // Now that the DOPData PointPairList array is filled in, we can update the DOP graph
            GraphPane myPane = DOPGraph.GraphPane;

            // if the checkbox is selected for this DOP type....
            if (DisplayEDOP.Checked)
            {
                // use the AddCurve method to add a new series of data to an existing graph
                // The text supplied in the first parameter will be used in the legend.
                LineItem myCurve = myPane.AddCurve(Localization.EDOP, DOPData[0], Color.Red, SymbolType.Circle);
                // Fill the symbols with the same color as the lines
                myCurve.Symbol.Fill = new Fill(Color.Red);
            }

            // continue for all DOP types supported
            if (DisplayNDOP.Checked)
            {
                LineItem myCurve = myPane.AddCurve(Localization.NDOP, DOPData[1], Color.Green, SymbolType.Circle);
                myCurve.Symbol.Fill = new Fill(Color.Green);
            }

            if (DisplayVDOP.Checked)
            {
                LineItem myCurve = myPane.AddCurve(Localization.VDOP, DOPData[2], Color.Gold, SymbolType.Circle);
                myCurve.Symbol.Fill = new Fill(Color.Gold);
            }

            if (DisplayHDOP.Checked)
            {
                LineItem myCurve = myPane.AddCurve(Localization.HDOP, DOPData[3], Color.Blue, SymbolType.Circle);
                myCurve.Symbol.Fill = new Fill(Color.Blue);
            }

            if (DisplayPDOP.Checked)
            {
                LineItem myCurve = myPane.AddCurve(Localization.PDOP, DOPData[4], Color.Magenta, SymbolType.Circle);
                myCurve.Symbol.Fill = new Fill(Color.Magenta);
            }

            if (DisplayTDOP.Checked)
            {
                LineItem myCurve = myPane.AddCurve(Localization.TDOP, DOPData[5], Color.Black, SymbolType.Circle);
                myCurve.Symbol.Fill = new Fill(Color.Black);
            }

            if (DisplayGDOP.Checked)
            {
                LineItem myCurve = myPane.AddCurve(Localization.GDOP, DOPData[6], Color.Cyan, SymbolType.Circle);
                myCurve.Symbol.Fill = new Fill(Color.Cyan);
            }

            // update the X-Axis Min and Max values. XDate is a ZedGraph type that takes a .Net DateTime structure.
            // Use the JulianDate method ToDateTime to retrieve this structure from a JulianDate
            DOPGraph.GraphPane.XAxis.Scale.Min = (double)new XDate(startjd.ToDateTime());
            DOPGraph.GraphPane.XAxis.Scale.Max = (double)new XDate(stopjd.ToDateTime());

            // Show the x axis grid
            myPane.XAxis.MajorGrid.IsVisible = true;

            // make x-axis a date type
            myPane.XAxis.Type = AxisType.Date;

            // Custom Axis string for display
            myPane.XAxis.Scale.Format = "dd-MMM\nhh:mm";

            // change the angle of the string if desired
            //myPane.XAxis.Scale.FontSpec.Angle = 40;

            // set the X-Axis Title
            if (GPSTimeRadio.Checked)
                myPane.XAxis.Title.Text = Localization.TimeGPST;
            else
                myPane.XAxis.Title.Text = Localization.TimeUTC;

            // Align the Y axis labels so they are flush to the axis
            myPane.YAxis.Scale.Align = AlignP.Inside;

            // Fill the axis background with a gradient
            myPane.Chart.Fill = new Fill(Color.White, Color.LightGray, 45.0f);

            // Enable scrollbars if needed
            DOPGraph.IsShowHScrollBar = true;
            DOPGraph.IsShowVScrollBar = true;
            DOPGraph.IsAutoScrollRange = true;
            DOPGraph.IsScrollY2 = true;

            // Make sure the Graph gets redrawn
            DOPGraph.AxisChange();
            DOPGraph.Invalidate();
        }

        /// <summary>
        /// Updates the Azimuth/Elevation graph with the calculated data
        /// </summary>
        private void updateAzElGraph()
        {
            // Plotting the Azimuth / Elevation data is a little trickier than plotting the DOP data
            // We have to check if we're plotting the data's X-Axis as Azimuth or time.
            GraphPane myPane = AzElGraph.GraphPane;

            // We'll create a sorted list of PRNs and add each PRN in turn. This will ensure the legend is sorted.
            int[] sortedPRNArray = new int[AzElData_TimeBased.Keys.Count];
            AzElData_TimeBased.Keys.CopyTo(sortedPRNArray, 0);
            Array.Sort(sortedPRNArray);

            // We'll now iterate over each satellite (PRN) and add either the time-based or Azimuth-based data to the graph
            foreach (int PRN in sortedPRNArray)
            {
                LineItem myCurve;
                if (plotXAxisAsTime)
                {
                    // we're using the appropriate line color and symbol here.
                    myCurve = myPane.AddCurve(String.Format("PRN {0}", PRN), AzElData_TimeBased[PRN], prnStyles[PRN].color, prnStyles[PRN].symbol);
                }
                else
                {
                    myCurve = myPane.AddCurve(String.Format("PRN {0}", PRN), AzElData_AzimuthBased[PRN], prnStyles[PRN].color, prnStyles[PRN].symbol);
                }
                myCurve.Symbol.Fill = new Fill(prnStyles[PRN].color);
                myCurve.Line.IsVisible = false;
            }

            // Now set the X-Axis characteristics based on the user preference
            if (plotXAxisAsTime)
            {
                AzElGraph.GraphPane.XAxis.Scale.Min = (double)new XDate(startjd.ToDateTime());
                AzElGraph.GraphPane.XAxis.Scale.Max = (double)new XDate(stopjd.ToDateTime());

                // make x-axis a date type
                myPane.XAxis.Type = AxisType.Date;

                myPane.XAxis.Scale.Format = "dd-MMM\nhh:mm";
                //myPane.XAxis.Scale.FontSpec.Angle = 40;

                if (GPSTimeRadio.Checked)
                    myPane.XAxis.Title.Text = Localization.TimeGPST;
                else
                    myPane.XAxis.Title.Text = Localization.TimeUTC;
            }
            else
            {
                AzElGraph.GraphPane.XAxis.Scale.Min = 0.0;
                AzElGraph.GraphPane.XAxis.Scale.Max = 360.0;
                // make x-axis a date type
                myPane.XAxis.Type = AxisType.Linear;

                myPane.XAxis.Scale.FormatAuto = true;

                myPane.XAxis.Title.Text = Localization.AzimuthDegrees;
            }

            // Show the x axis grid
            myPane.XAxis.MajorGrid.IsVisible = true;

            // Align the Y axis labels so they are flush to the axis
            myPane.YAxis.Scale.Align = AlignP.Inside;

            // Fill the axis background with a gradient
            myPane.Chart.Fill = new Fill(Color.White, Color.LightGray, 45.0f);

            // Enable scrollbars if needed
            AzElGraph.IsShowHScrollBar = true;
            AzElGraph.IsShowVScrollBar = true;
            AzElGraph.IsAutoScrollRange = true;
            AzElGraph.IsScrollY2 = true;

            // Make sure the Graph gets redrawn
            AzElGraph.AxisChange();
            AzElGraph.Invalidate();
        }

        // These methods turn on or off the specific Graph types being plotted
        // first by clearing the graphs and then rerunning the updateGraph code.
        // The updateGraph code checks each checkbox to see if it should plot that
        // type.
        #region Graph Check Changed methods

        private void DisplayEDOP_CheckedChanged(object sender, EventArgs e)
        {
            DOPGraph.GraphPane.CurveList.Clear();
            UpdateDopGraph();
        }

        private void DisplayNDOP_CheckedChanged(object sender, EventArgs e)
        {
            DOPGraph.GraphPane.CurveList.Clear();
            UpdateDopGraph();
        }

        private void DisplayVDOP_CheckedChanged(object sender, EventArgs e)
        {
            DOPGraph.GraphPane.CurveList.Clear();
            UpdateDopGraph();
        }

        private void DisplayHDOP_CheckedChanged(object sender, EventArgs e)
        {
            DOPGraph.GraphPane.CurveList.Clear();
            UpdateDopGraph();
        }

        private void DisplayPDOP_CheckedChanged(object sender, EventArgs e)
        {
            DOPGraph.GraphPane.CurveList.Clear();
            UpdateDopGraph();
        }

        private void DisplayTDOP_CheckedChanged(object sender, EventArgs e)
        {
            DOPGraph.GraphPane.CurveList.Clear();
            UpdateDopGraph();
        }

        private void DisplayGDOP_CheckedChanged(object sender, EventArgs e)
        {
            DOPGraph.GraphPane.CurveList.Clear();
            UpdateDopGraph();
        }

        private void AsAccCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            NavAccGraph.GraphPane.CurveList.Clear();
            DisplayNavAccGraph(PredAccData, System.Drawing.Color.Red, PredAccCheckBox.Checked, Localization.Predicted);
            DisplayNavAccGraph(AsAccData, System.Drawing.Color.Blue, AsAccCheckBox.Checked, Localization.Assessed);
        }

        private void PredAccCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            NavAccGraph.GraphPane.CurveList.Clear();
            DisplayNavAccGraph(PredAccData, System.Drawing.Color.Red, PredAccCheckBox.Checked, Localization.Predicted);
            DisplayNavAccGraph(AsAccData, System.Drawing.Color.Blue, AsAccCheckBox.Checked, Localization.Assessed);
        }

        #endregion

        // These methods switch the X-Axis on the Azimuth Elevation graph between time and azimuth.
        // This allows for further investigation into the nav properties at the receiver's location.
        // The update is done by setting the plotXAxisAsTime variable to the correct value, then 
        // clearing the graphs and finally rerunning the updateGraph code.
        // The updateGraph code checks the plotXAxisAsTime variable to see which mode it should plot in.
        #region Azimuth/Elevation X-Axis Change Methods

        private void dateTimeXAxis_CheckedChanged(object sender, EventArgs e)
        {
            plotXAxisAsTime = dateTimeXAxis.Checked;
            AzElGraph.GraphPane.CurveList.Clear();
            updateAzElGraph();
        }

        private void azimuthAxis_CheckedChanged(object sender, EventArgs e)
        {
            plotXAxisAsTime = dateTimeXAxis.Checked;
            AzElGraph.GraphPane.CurveList.Clear();
            updateAzElGraph();
        }
        #endregion

        /// <summary>
        /// Azimuth/Elevation Line Style initialization method
        /// </summary>
        private void InitLineStyles()
        {
            // These line styles will ensure the a given satellite (denoted by prn) will always have the same symbol and color.
            prnStyles[1].color = Color.Red;
            prnStyles[1].symbol = SymbolType.Circle;
            prnStyles[2].color = Color.Red;
            prnStyles[2].symbol = SymbolType.Diamond;
            prnStyles[3].color = Color.Red;
            prnStyles[3].symbol = SymbolType.Triangle;
            prnStyles[4].color = Color.Red;
            prnStyles[4].symbol = SymbolType.Square;
            prnStyles[5].color = Color.Red;
            prnStyles[5].symbol = SymbolType.TriangleDown;

            prnStyles[6].color = Color.Gold;
            prnStyles[6].symbol = SymbolType.Circle;
            prnStyles[7].color = Color.Gold;
            prnStyles[7].symbol = SymbolType.Diamond;
            prnStyles[8].color = Color.Gold;
            prnStyles[8].symbol = SymbolType.Triangle;
            prnStyles[9].color = Color.Gold;
            prnStyles[9].symbol = SymbolType.Square;
            prnStyles[10].color = Color.Gold;
            prnStyles[10].symbol = SymbolType.TriangleDown;

            prnStyles[11].color = Color.LimeGreen;
            prnStyles[11].symbol = SymbolType.Circle;
            prnStyles[12].color = Color.LimeGreen;
            prnStyles[12].symbol = SymbolType.Diamond;
            prnStyles[13].color = Color.LimeGreen;
            prnStyles[13].symbol = SymbolType.Triangle;
            prnStyles[14].color = Color.LimeGreen;
            prnStyles[14].symbol = SymbolType.Square;
            prnStyles[15].color = Color.LimeGreen;
            prnStyles[15].symbol = SymbolType.TriangleDown;

            prnStyles[16].color = Color.BlueViolet;
            prnStyles[16].symbol = SymbolType.Circle;
            prnStyles[17].color = Color.BlueViolet;
            prnStyles[17].symbol = SymbolType.Diamond;
            prnStyles[18].color = Color.BlueViolet;
            prnStyles[18].symbol = SymbolType.Triangle;
            prnStyles[19].color = Color.BlueViolet;
            prnStyles[19].symbol = SymbolType.Square;
            prnStyles[20].color = Color.BlueViolet;
            prnStyles[20].symbol = SymbolType.TriangleDown;

            prnStyles[21].color = Color.Black;
            prnStyles[21].symbol = SymbolType.Circle;
            prnStyles[22].color = Color.Black;
            prnStyles[22].symbol = SymbolType.Diamond;
            prnStyles[23].color = Color.Black;
            prnStyles[23].symbol = SymbolType.Triangle;
            prnStyles[24].color = Color.Black;
            prnStyles[24].symbol = SymbolType.Square;
            prnStyles[25].color = Color.Black;
            prnStyles[25].symbol = SymbolType.TriangleDown;

            prnStyles[26].color = Color.Magenta;
            prnStyles[26].symbol = SymbolType.Circle;
            prnStyles[27].color = Color.Magenta;
            prnStyles[27].symbol = SymbolType.Diamond;
            prnStyles[28].color = Color.Magenta;
            prnStyles[28].symbol = SymbolType.Triangle;
            prnStyles[29].color = Color.Magenta;
            prnStyles[29].symbol = SymbolType.Square;
            prnStyles[30].color = Color.Magenta;
            prnStyles[30].symbol = SymbolType.TriangleDown;

            prnStyles[31].color = Color.Cyan;
            prnStyles[31].symbol = SymbolType.Circle;
            prnStyles[32].color = Color.Cyan;
            prnStyles[32].symbol = SymbolType.Diamond;
            prnStyles[33].color = Color.Cyan;
            prnStyles[33].symbol = SymbolType.Triangle;
            prnStyles[34].color = Color.Cyan;
            prnStyles[34].symbol = SymbolType.Square;
            prnStyles[35].color = Color.Cyan;
            prnStyles[35].symbol = SymbolType.TriangleDown;
        }

        /// <summary>
        /// Method to draw the graph for both Predicted and Assessed Navigation Acc.
        /// </summary>
        /// <param name="navAccGraphData">The appropriate data structure (Assessed or Predicted)</param>
        /// <param name="color">The color which we want to draw the graph</param>
        /// <param name="selected">Do we want to draw this or not, based on user selection</param>
        /// <param name="legend">The string to be used to represent the legend</param>
        private void DisplayNavAccGraph(PointPairList navAccGraphData, Color color, bool selected, string legend)
        {
            // Update the Accuracy graph using the PointPairList array.
            GraphPane myPane = NavAccGraph.GraphPane;

            if (selected)
            {
                // use the AddCurve method to add a new series of data to an existing graph
                // The text supplied in the first parameter will be used in the legend.
                LineItem myCurve = myPane.AddCurve(legend, navAccGraphData, color, SymbolType.Circle);
                // Fill the symbols with the same color as the lines
                myCurve.Symbol.Fill = new Fill(color);
            }

            // update the X-Axis Min and Max values. XDate is a ZedGraph type that takes a .Net DateTime structure.
            // Use the JulianDate method ToDateTime to retrieve this structure from a JulianDate
            NavAccGraph.GraphPane.XAxis.Scale.Min = (double)new XDate(startjd.ToDateTime());
            NavAccGraph.GraphPane.XAxis.Scale.Max = (double)new XDate(stopjd.ToDateTime());

            // Show the x axis grid
            myPane.XAxis.MajorGrid.IsVisible = true;

            // make x-axis a date type
            myPane.XAxis.Type = AxisType.Date;

            // Custom Axis string for display
            myPane.XAxis.Scale.Format = "dd-MMM\nhh:mm";

            // set the X-Axis Title
            if (GPSTimeRadio.Checked)
                myPane.XAxis.Title.Text = Localization.TimeGPST;
            else
                myPane.XAxis.Title.Text = Localization.TimeUTC;

            // Align the Y axis labels so they are flush to the axis
            myPane.YAxis.Scale.Align = AlignP.Inside;

            // Fill the axis background with a gradient
            myPane.Chart.Fill = new Fill(Color.White, Color.LightGray, 45.0f);

            // Enable scrollbars if needed
            NavAccGraph.IsShowHScrollBar = true;
            NavAccGraph.IsShowVScrollBar = true;
            NavAccGraph.IsAutoScrollRange = true;
            NavAccGraph.IsScrollY2 = true;

            // Make sure the Graph gets redrawn
            NavAccGraph.AxisChange();
            NavAccGraph.Invalidate();
        }
    }
}
