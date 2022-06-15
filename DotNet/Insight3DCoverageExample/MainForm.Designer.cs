namespace Spatial_Library_Exercise
{
    partial class MainForm
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.btnRun = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.gbInput = new System.Windows.Forms.GroupBox();
            this.nudResolution = new System.Windows.Forms.NumericUpDown();
            this.lblResolution = new System.Windows.Forms.Label();
            this.cbWorldView3 = new System.Windows.Forms.CheckBox();
            this.cbWorldView2 = new System.Windows.Forms.CheckBox();
            this.cbWorldView1 = new System.Windows.Forms.CheckBox();
            this.lblSatellite = new System.Windows.Forms.Label();
            this.cmbRegion = new System.Windows.Forms.ComboBox();
            this.lblRegion = new System.Windows.Forms.Label();
            this.gbOutput = new System.Windows.Forms.GroupBox();
            this.txtResult = new System.Windows.Forms.TextBox();
            this.m_insight3DPanel = new System.Windows.Forms.Panel();
            this.gbInput.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudResolution)).BeginInit();
            this.gbOutput.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnRun
            // 
            this.btnRun.Location = new System.Drawing.Point(896, 994);
            this.btnRun.Margin = new System.Windows.Forms.Padding(56, 41, 56, 41);
            this.btnRun.Name = "btnRun";
            this.btnRun.Size = new System.Drawing.Size(240, 86);
            this.btnRun.TabIndex = 0;
            this.btnRun.Text = "Run";
            this.btnRun.UseVisualStyleBackColor = true;
            this.btnRun.Click += new System.EventHandler(this.btnRun_Click);
            // 
            // btnClose
            // 
            this.btnClose.Location = new System.Drawing.Point(1152, 994);
            this.btnClose.Margin = new System.Windows.Forms.Padding(56, 41, 56, 41);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(240, 86);
            this.btnClose.TabIndex = 1;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // gbInput
            // 
            this.gbInput.Controls.Add(this.nudResolution);
            this.gbInput.Controls.Add(this.lblResolution);
            this.gbInput.Controls.Add(this.cbWorldView3);
            this.gbInput.Controls.Add(this.cbWorldView2);
            this.gbInput.Controls.Add(this.cbWorldView1);
            this.gbInput.Controls.Add(this.lblSatellite);
            this.gbInput.Controls.Add(this.cmbRegion);
            this.gbInput.Controls.Add(this.lblRegion);
            this.gbInput.Location = new System.Drawing.Point(27, 21);
            this.gbInput.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.gbInput.Name = "gbInput";
            this.gbInput.Padding = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.gbInput.Size = new System.Drawing.Size(1363, 365);
            this.gbInput.TabIndex = 2;
            this.gbInput.TabStop = false;
            this.gbInput.Text = "Input";
            this.gbInput.Enter += new System.EventHandler(this.gbInput_Enter);
            // 
            // nudResolution
            // 
            this.nudResolution.DecimalPlaces = 2;
            this.nudResolution.Location = new System.Drawing.Point(283, 291);
            this.nudResolution.Margin = new System.Windows.Forms.Padding(53555, 18049, 53555, 18049);
            this.nudResolution.Name = "nudResolution";
            this.nudResolution.Size = new System.Drawing.Size(373, 38);
            this.nudResolution.TabIndex = 7;
            this.nudResolution.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // lblResolution
            // 
            this.lblResolution.AutoSize = true;
            this.lblResolution.Location = new System.Drawing.Point(29, 296);
            this.lblResolution.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);
            this.lblResolution.Name = "lblResolution";
            this.lblResolution.Size = new System.Drawing.Size(246, 32);
            this.lblResolution.TabIndex = 6;
            this.lblResolution.Text = "Select Resolution:";
            // 
            // cbWorldView3
            // 
            this.cbWorldView3.AutoSize = true;
            this.cbWorldView3.Location = new System.Drawing.Point(283, 215);
            this.cbWorldView3.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.cbWorldView3.Name = "cbWorldView3";
            this.cbWorldView3.Size = new System.Drawing.Size(205, 36);
            this.cbWorldView3.TabIndex = 5;
            this.cbWorldView3.Text = "WorldView3";
            this.cbWorldView3.UseVisualStyleBackColor = true;
            // 
            // cbWorldView2
            // 
            this.cbWorldView2.AutoSize = true;
            this.cbWorldView2.Location = new System.Drawing.Point(283, 160);
            this.cbWorldView2.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.cbWorldView2.Name = "cbWorldView2";
            this.cbWorldView2.Size = new System.Drawing.Size(205, 36);
            this.cbWorldView2.TabIndex = 4;
            this.cbWorldView2.Text = "WorldView2";
            this.cbWorldView2.UseVisualStyleBackColor = true;
            // 
            // cbWorldView1
            // 
            this.cbWorldView1.AutoSize = true;
            this.cbWorldView1.Location = new System.Drawing.Point(283, 105);
            this.cbWorldView1.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.cbWorldView1.Name = "cbWorldView1";
            this.cbWorldView1.Size = new System.Drawing.Size(205, 36);
            this.cbWorldView1.TabIndex = 3;
            this.cbWorldView1.Text = "WorldView1";
            this.cbWorldView1.UseVisualStyleBackColor = true;
            // 
            // lblSatellite
            // 
            this.lblSatellite.AutoSize = true;
            this.lblSatellite.Location = new System.Drawing.Point(29, 162);
            this.lblSatellite.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);
            this.lblSatellite.Name = "lblSatellite";
            this.lblSatellite.Size = new System.Drawing.Size(214, 32);
            this.lblSatellite.TabIndex = 2;
            this.lblSatellite.Text = "Select Satellite:";
            // 
            // cmbRegion
            // 
            this.cmbRegion.FormattingEnabled = true;
            this.cmbRegion.Items.AddRange(new object[] {
            "Pennsylvania",
            "Virginia",
            "West Virginia"});
            this.cmbRegion.Location = new System.Drawing.Point(283, 38);
            this.cmbRegion.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.cmbRegion.Name = "cmbRegion";
            this.cmbRegion.Size = new System.Drawing.Size(367, 39);
            this.cmbRegion.TabIndex = 1;
            // 
            // lblRegion
            // 
            this.lblRegion.AutoSize = true;
            this.lblRegion.Location = new System.Drawing.Point(29, 45);
            this.lblRegion.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);
            this.lblRegion.Name = "lblRegion";
            this.lblRegion.Size = new System.Drawing.Size(201, 32);
            this.lblRegion.TabIndex = 0;
            this.lblRegion.Text = "Select Region:";
            // 
            // gbOutput
            // 
            this.gbOutput.Controls.Add(this.txtResult);
            this.gbOutput.Location = new System.Drawing.Point(27, 401);
            this.gbOutput.Margin = new System.Windows.Forms.Padding(56, 41, 56, 41);
            this.gbOutput.Name = "gbOutput";
            this.gbOutput.Padding = new System.Windows.Forms.Padding(56, 41, 56, 41);
            this.gbOutput.Size = new System.Drawing.Size(1363, 579);
            this.gbOutput.TabIndex = 3;
            this.gbOutput.TabStop = false;
            this.gbOutput.Text = "Output";
            // 
            // txtResult
            // 
            this.txtResult.Location = new System.Drawing.Point(37, 55);
            this.txtResult.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.txtResult.Multiline = true;
            this.txtResult.Name = "txtResult";
            this.txtResult.Size = new System.Drawing.Size(1289, 505);
            this.txtResult.TabIndex = 0;
            // 
            // m_insight3DPanel
            // 
            this.m_insight3DPanel.Location = new System.Drawing.Point(1417, 39);
            this.m_insight3DPanel.Margin = new System.Windows.Forms.Padding(56, 41, 56, 41);
            this.m_insight3DPanel.Name = "m_insight3DPanel";
            this.m_insight3DPanel.Size = new System.Drawing.Size(1821, 1352);
            this.m_insight3DPanel.TabIndex = 0;
            this.m_insight3DPanel.Paint += new System.Windows.Forms.PaintEventHandler(this.m_insight3DPanel_Paint_1);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(16F, 31F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(3251, 1410);
            this.Controls.Add(this.gbOutput);
            this.Controls.Add(this.gbInput);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.btnRun);
            this.Controls.Add(this.m_insight3DPanel);
            this.Name = "MainForm";
            this.Text = "Main Form";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.gbInput.ResumeLayout(false);
            this.gbInput.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudResolution)).EndInit();
            this.gbOutput.ResumeLayout(false);
            this.gbOutput.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnRun;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.GroupBox gbInput;
        private System.Windows.Forms.NumericUpDown nudResolution;
        private System.Windows.Forms.Label lblResolution;
        private System.Windows.Forms.CheckBox cbWorldView3;
        private System.Windows.Forms.CheckBox cbWorldView2;
        private System.Windows.Forms.CheckBox cbWorldView1;
        private System.Windows.Forms.Label lblSatellite;
        private System.Windows.Forms.ComboBox cmbRegion;
        private System.Windows.Forms.Label lblRegion;
        private System.Windows.Forms.GroupBox gbOutput;
        private System.Windows.Forms.TextBox txtResult;
        private System.Windows.Forms.Panel m_insight3DPanel;
    }
}

