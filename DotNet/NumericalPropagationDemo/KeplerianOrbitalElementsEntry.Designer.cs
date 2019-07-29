using System;
using AGI.Foundation;
using System.Windows.Forms;
namespace AGI.Examples
{
    partial class KeplerianOrbitalElementsEntry
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.m_labelSemiMajorAxis = new System.Windows.Forms.Label();
            this.m_labelEccentricity = new System.Windows.Forms.Label();
            this.m_labelInclination = new System.Windows.Forms.Label();
            this.m_labelArgumentOfPeriapsis = new System.Windows.Forms.Label();
            this.m_labelRightAscensionOfTheAcscendingNode = new System.Windows.Forms.Label();
            this.m_labelTrueAnomaly = new System.Windows.Forms.Label();
            this.m_trueAnomaly = new System.Windows.Forms.TextBox();
            this.m_RAAN = new System.Windows.Forms.TextBox();
            this.m_argumentOfPeriapsis = new System.Windows.Forms.TextBox();
            this.m_inclination = new System.Windows.Forms.TextBox();
            this.m_eccentricity = new System.Windows.Forms.TextBox();
            this.m_semiMajorAxis = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // m_labelSemiMajorAxis
            // 
            this.m_labelSemiMajorAxis.AutoSize = true;
            this.m_labelSemiMajorAxis.Location = new System.Drawing.Point(12, 10);
            this.m_labelSemiMajorAxis.Name = "m_labelSemiMajorAxis";
            this.m_labelSemiMajorAxis.Size = new System.Drawing.Size(101, 13);
            this.m_labelSemiMajorAxis.TabIndex = 0;
            this.m_labelSemiMajorAxis.Text = "Semi-Major Axis (m):";
            this.m_labelSemiMajorAxis.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // m_labelEccentricity
            // 
            this.m_labelEccentricity.AutoSize = true;
            this.m_labelEccentricity.Location = new System.Drawing.Point(48, 39);
            this.m_labelEccentricity.Name = "m_labelEccentricity";
            this.m_labelEccentricity.Size = new System.Drawing.Size(65, 13);
            this.m_labelEccentricity.TabIndex = 1;
            this.m_labelEccentricity.Text = "Eccentricity:";
            this.m_labelEccentricity.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // m_labelInclination
            // 
            this.m_labelInclination.AutoSize = true;
            this.m_labelInclination.Location = new System.Drawing.Point(31, 65);
            this.m_labelInclination.Name = "m_labelInclination";
            this.m_labelInclination.Size = new System.Drawing.Size(82, 13);
            this.m_labelInclination.TabIndex = 2;
            this.m_labelInclination.Text = "Inclination (rad):";
            this.m_labelInclination.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // m_labelArgumentOfPeriapsis
            // 
            this.m_labelArgumentOfPeriapsis.AutoSize = true;
            this.m_labelArgumentOfPeriapsis.Location = new System.Drawing.Point(3, 91);
            this.m_labelArgumentOfPeriapsis.Name = "m_labelArgumentOfPeriapsis";
            this.m_labelArgumentOfPeriapsis.Size = new System.Drawing.Size(110, 13);
            this.m_labelArgumentOfPeriapsis.TabIndex = 3;
            this.m_labelArgumentOfPeriapsis.Text = "Arg. of Periapsis (rad):";
            this.m_labelArgumentOfPeriapsis.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // m_labelRightAscensionOfTheAcscendingNode
            // 
            this.m_labelRightAscensionOfTheAcscendingNode.Location = new System.Drawing.Point(5, 112);
            this.m_labelRightAscensionOfTheAcscendingNode.Name = "m_labelRightAscensionOfTheAcscendingNode";
            this.m_labelRightAscensionOfTheAcscendingNode.Size = new System.Drawing.Size(108, 26);
            this.m_labelRightAscensionOfTheAcscendingNode.TabIndex = 4;
            this.m_labelRightAscensionOfTheAcscendingNode.Text = "Right Ascension of the Ascending Node (rad):";
            this.m_labelRightAscensionOfTheAcscendingNode.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // m_labelTrueAnomaly
            // 
            this.m_labelTrueAnomaly.AutoSize = true;
            this.m_labelTrueAnomaly.Location = new System.Drawing.Point(17, 146);
            this.m_labelTrueAnomaly.Name = "m_labelTrueAnomaly";
            this.m_labelTrueAnomaly.Size = new System.Drawing.Size(96, 13);
            this.m_labelTrueAnomaly.TabIndex = 5;
            this.m_labelTrueAnomaly.Text = "True Anomaly(rad):";
            this.m_labelTrueAnomaly.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // m_trueAnomaly
            // 
            this.m_trueAnomaly.Location = new System.Drawing.Point(129, 142);
            this.m_trueAnomaly.Name = "m_trueAnomaly";
            this.m_trueAnomaly.Size = new System.Drawing.Size(111, 20);
            this.m_trueAnomaly.TabIndex = 11;
            // 
            // m_RAAN
            // 
            this.m_RAAN.Location = new System.Drawing.Point(129, 116);
            this.m_RAAN.Name = "m_RAAN";
            this.m_RAAN.Size = new System.Drawing.Size(111, 20);
            this.m_RAAN.TabIndex = 10;
            // 
            // m_argumentOfPeriapsis
            // 
            this.m_argumentOfPeriapsis.Location = new System.Drawing.Point(129, 87);
            this.m_argumentOfPeriapsis.Name = "m_argumentOfPeriapsis";
            this.m_argumentOfPeriapsis.Size = new System.Drawing.Size(111, 20);
            this.m_argumentOfPeriapsis.TabIndex = 9;
            // 
            // m_inclination
            // 
            this.m_inclination.Location = new System.Drawing.Point(129, 61);
            this.m_inclination.Name = "m_inclination";
            this.m_inclination.Size = new System.Drawing.Size(111, 20);
            this.m_inclination.TabIndex = 8;
            // 
            // m_eccentricity
            // 
            this.m_eccentricity.Location = new System.Drawing.Point(129, 35);
            this.m_eccentricity.Name = "m_eccentricity";
            this.m_eccentricity.Size = new System.Drawing.Size(111, 20);
            this.m_eccentricity.TabIndex = 7;
            // 
            // m_semiMajorAxis
            // 
            this.m_semiMajorAxis.Location = new System.Drawing.Point(129, 6);
            this.m_semiMajorAxis.Name = "m_semiMajorAxis";
            this.m_semiMajorAxis.Size = new System.Drawing.Size(111, 20);
            this.m_semiMajorAxis.TabIndex = 6;
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(5, 113);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(108, 26);
            this.label1.TabIndex = 4;
            this.label1.Text = "Right Ascension of the Ascending Node (rad):";
            this.label1.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // KeplerianOrbitalElementsEntry
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.m_trueAnomaly);
            this.Controls.Add(this.m_RAAN);
            this.Controls.Add(this.m_argumentOfPeriapsis);
            this.Controls.Add(this.m_inclination);
            this.Controls.Add(this.m_eccentricity);
            this.Controls.Add(this.m_semiMajorAxis);
            this.Controls.Add(this.m_labelTrueAnomaly);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.m_labelRightAscensionOfTheAcscendingNode);
            this.Controls.Add(this.m_labelArgumentOfPeriapsis);
            this.Controls.Add(this.m_labelInclination);
            this.Controls.Add(this.m_labelEccentricity);
            this.Controls.Add(this.m_labelSemiMajorAxis);
            this.Name = "KeplerianOrbitalElementsEntry";
            this.Size = new System.Drawing.Size(253, 167);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label m_labelSemiMajorAxis;
        private System.Windows.Forms.Label m_labelEccentricity;
        private System.Windows.Forms.Label m_labelInclination;
        private System.Windows.Forms.Label m_labelArgumentOfPeriapsis;
        private System.Windows.Forms.Label m_labelRightAscensionOfTheAcscendingNode;
        private System.Windows.Forms.Label m_labelTrueAnomaly;
        private System.Windows.Forms.TextBox m_semiMajorAxis;
        private System.Windows.Forms.TextBox m_eccentricity;
        private System.Windows.Forms.TextBox m_inclination;
        private System.Windows.Forms.TextBox m_argumentOfPeriapsis;
        private System.Windows.Forms.TextBox m_RAAN;
        private System.Windows.Forms.TextBox m_trueAnomaly;
        private Label label1;
    }
}
