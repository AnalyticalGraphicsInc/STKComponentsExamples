namespace GPSCommunications
{
    partial class Main
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
            this.btnCalculate = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.cbReceiverType = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.cbForTrackedSVsOnly = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.cbL5Jammer = new System.Windows.Forms.CheckBox();
            this.cbL2Jammer = new System.Windows.Forms.CheckBox();
            this.cbL1Jammer = new System.Windows.Forms.CheckBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.cbOutputRcvrNoise = new System.Windows.Forms.CheckBox();
            this.cbOutputNI = new System.Windows.Forms.CheckBox();
            this.cbOutputJS = new System.Windows.Forms.CheckBox();
            this.cbOutputCNI = new System.Windows.Forms.CheckBox();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnCalculate
            // 
            this.btnCalculate.Location = new System.Drawing.Point(187, 161);
            this.btnCalculate.Name = "btnCalculate";
            this.btnCalculate.Size = new System.Drawing.Size(89, 23);
            this.btnCalculate.TabIndex = 0;
            this.btnCalculate.Text = "Calculate";
            this.btnCalculate.UseVisualStyleBackColor = true;
            this.btnCalculate.Click += new System.EventHandler(this.btnCalculate_Click);
            // 
            // textBox1
            // 
            this.textBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox1.Location = new System.Drawing.Point(12, 195);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBox1.Size = new System.Drawing.Size(275, 304);
            this.textBox1.TabIndex = 1;
            // 
            // cbReceiverType
            // 
            this.cbReceiverType.FormattingEnabled = true;
            this.cbReceiverType.Items.AddRange(new object[] {
            "SingleFrequencyL1CA",
            "SingleFrequencyL1M",
            "SingleFrequencyL1PY",
            "DualFrequencyL1CAL2C",
            "DualFrequencyL1CAL5IQ",
            "DualFrequencyL2CL5IQ",
            "DualFrequencyL1PYL2PY",
            "DualFrequencyHandoverL1CAL1PYL2PY",
            "DualFrequencyMCodeL1ML2M"});
            this.cbReceiverType.Location = new System.Drawing.Point(16, 30);
            this.cbReceiverType.Name = "cbReceiverType";
            this.cbReceiverType.Size = new System.Drawing.Size(256, 21);
            this.cbReceiverType.TabIndex = 6;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(19, 14);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(102, 13);
            this.label2.TabIndex = 7;
            this.label2.Text = "GPS Receiver Type";
            // 
            // cbForTrackedSVsOnly
            // 
            this.cbForTrackedSVsOnly.AutoSize = true;
            this.cbForTrackedSVsOnly.Location = new System.Drawing.Point(6, 105);
            this.cbForTrackedSVsOnly.Name = "cbForTrackedSVsOnly";
            this.cbForTrackedSVsOnly.Size = new System.Drawing.Size(124, 17);
            this.cbForTrackedSVsOnly.TabIndex = 12;
            this.cbForTrackedSVsOnly.Text = "For tracked SVs only";
            this.cbForTrackedSVsOnly.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.cbL5Jammer);
            this.groupBox1.Controls.Add(this.cbL2Jammer);
            this.groupBox1.Controls.Add(this.cbL1Jammer);
            this.groupBox1.Location = new System.Drawing.Point(187, 60);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(89, 87);
            this.groupBox1.TabIndex = 16;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Add Jammers";
            // 
            // cbL5Jammer
            // 
            this.cbL5Jammer.AutoSize = true;
            this.cbL5Jammer.Location = new System.Drawing.Point(15, 62);
            this.cbL5Jammer.Name = "cbL5Jammer";
            this.cbL5Jammer.Size = new System.Drawing.Size(38, 17);
            this.cbL5Jammer.TabIndex = 2;
            this.cbL5Jammer.Text = "L5";
            this.cbL5Jammer.UseVisualStyleBackColor = true;
            // 
            // cbL2Jammer
            // 
            this.cbL2Jammer.AutoSize = true;
            this.cbL2Jammer.Location = new System.Drawing.Point(15, 39);
            this.cbL2Jammer.Name = "cbL2Jammer";
            this.cbL2Jammer.Size = new System.Drawing.Size(38, 17);
            this.cbL2Jammer.TabIndex = 1;
            this.cbL2Jammer.Text = "L2";
            this.cbL2Jammer.UseVisualStyleBackColor = true;
            // 
            // cbL1Jammer
            // 
            this.cbL1Jammer.AutoSize = true;
            this.cbL1Jammer.Location = new System.Drawing.Point(15, 18);
            this.cbL1Jammer.Name = "cbL1Jammer";
            this.cbL1Jammer.Size = new System.Drawing.Size(38, 17);
            this.cbL1Jammer.TabIndex = 0;
            this.cbL1Jammer.Text = "L1";
            this.cbL1Jammer.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.cbOutputRcvrNoise);
            this.groupBox2.Controls.Add(this.cbOutputNI);
            this.groupBox2.Controls.Add(this.cbOutputJS);
            this.groupBox2.Controls.Add(this.cbOutputCNI);
            this.groupBox2.Controls.Add(this.cbForTrackedSVsOnly);
            this.groupBox2.Location = new System.Drawing.Point(22, 60);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(159, 129);
            this.groupBox2.TabIndex = 17;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Output";
            // 
            // cbOutputRcvrNoise
            // 
            this.cbOutputRcvrNoise.AutoSize = true;
            this.cbOutputRcvrNoise.Location = new System.Drawing.Point(15, 78);
            this.cbOutputRcvrNoise.Name = "cbOutputRcvrNoise";
            this.cbOutputRcvrNoise.Size = new System.Drawing.Size(99, 17);
            this.cbOutputRcvrNoise.TabIndex = 21;
            this.cbOutputRcvrNoise.Text = "Receiver Noise";
            this.cbOutputRcvrNoise.UseVisualStyleBackColor = true;
            // 
            // cbOutputNI
            // 
            this.cbOutputNI.AutoSize = true;
            this.cbOutputNI.Location = new System.Drawing.Point(15, 60);
            this.cbOutputNI.Name = "cbOutputNI";
            this.cbOutputNI.Size = new System.Drawing.Size(43, 17);
            this.cbOutputNI.TabIndex = 20;
            this.cbOutputNI.Text = "N+I";
            this.cbOutputNI.UseVisualStyleBackColor = true;
            // 
            // cbOutputJS
            // 
            this.cbOutputJS.AutoSize = true;
            this.cbOutputJS.Location = new System.Drawing.Point(15, 39);
            this.cbOutputJS.Name = "cbOutputJS";
            this.cbOutputJS.Size = new System.Drawing.Size(43, 17);
            this.cbOutputJS.TabIndex = 19;
            this.cbOutputJS.Text = "J/S";
            this.cbOutputJS.UseVisualStyleBackColor = true;
            // 
            // cbOutputCNI
            // 
            this.cbOutputCNI.AutoSize = true;
            this.cbOutputCNI.Location = new System.Drawing.Point(15, 19);
            this.cbOutputCNI.Name = "cbOutputCNI";
            this.cbOutputCNI.Size = new System.Drawing.Size(67, 17);
            this.cbOutputCNI.TabIndex = 18;
            this.cbOutputCNI.Text = "C/(N0+I)";
            this.cbOutputCNI.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(299, 511);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.cbReceiverType);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.btnCalculate);
            this.Name = "Form1";
            this.Text = "GPS Communications";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnCalculate;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.ComboBox cbReceiverType;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox cbForTrackedSVsOnly;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.CheckBox cbL5Jammer;
        private System.Windows.Forms.CheckBox cbL2Jammer;
        private System.Windows.Forms.CheckBox cbL1Jammer;
        private System.Windows.Forms.CheckBox cbOutputRcvrNoise;
        private System.Windows.Forms.CheckBox cbOutputNI;
        private System.Windows.Forms.CheckBox cbOutputJS;
        private System.Windows.Forms.CheckBox cbOutputCNI;
    }
}

