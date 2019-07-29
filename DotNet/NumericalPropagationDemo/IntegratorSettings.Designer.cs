namespace AGI.Examples
{
    partial class IntegratorSettings
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
            this.m_integrator = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.m_finish = new System.Windows.Forms.Button();
            this.m_fixedOrRelative = new System.Windows.Forms.ComboBox();
            this.label7 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.m_maxError = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.m_maxStep = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.m_minStep = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.m_stepSize = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // m_integrator
            // 
            this.m_integrator.FormattingEnabled = true;
            this.m_integrator.Location = new System.Drawing.Point(70, 11);
            this.m_integrator.Margin = new System.Windows.Forms.Padding(2);
            this.m_integrator.Name = "m_integrator";
            this.m_integrator.Size = new System.Drawing.Size(177, 21);
            this.m_integrator.TabIndex = 0;
            this.m_integrator.SelectedIndexChanged += new System.EventHandler(this.OnIntegratorSelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(11, 14);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(55, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Integrator:";
            // 
            // m_finish
            // 
            this.m_finish.Location = new System.Drawing.Point(171, 179);
            this.m_finish.Margin = new System.Windows.Forms.Padding(2);
            this.m_finish.Name = "m_finish";
            this.m_finish.Size = new System.Drawing.Size(75, 23);
            this.m_finish.TabIndex = 12;
            this.m_finish.Text = "OK";
            this.m_finish.UseVisualStyleBackColor = true;
            this.m_finish.Click += new System.EventHandler(this.OnFinishClick);
            // 
            // m_fixedOrRelative
            // 
            this.m_fixedOrRelative.FormattingEnabled = true;
            this.m_fixedOrRelative.Location = new System.Drawing.Point(126, 154);
            this.m_fixedOrRelative.Margin = new System.Windows.Forms.Padding(2);
            this.m_fixedOrRelative.Name = "m_fixedOrRelative";
            this.m_fixedOrRelative.Size = new System.Drawing.Size(121, 21);
            this.m_fixedOrRelative.TabIndex = 22;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(20, 157);
            this.label7.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(87, 13);
            this.label7.TabIndex = 21;
            this.label7.Text = "Step Adjustment:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(52, 129);
            this.label5.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(55, 13);
            this.label5.TabIndex = 20;
            this.label5.Text = "Max Error:";
            // 
            // m_maxError
            // 
            this.m_maxError.Location = new System.Drawing.Point(126, 126);
            this.m_maxError.Name = "m_maxError";
            this.m_maxError.Size = new System.Drawing.Size(121, 20);
            this.m_maxError.TabIndex = 19;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(26, 101);
            this.label4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(81, 13);
            this.label4.TabIndex = 18;
            this.label4.Text = "Max Step (sec):";
            // 
            // m_maxStep
            // 
            this.m_maxStep.Location = new System.Drawing.Point(126, 98);
            this.m_maxStep.Name = "m_maxStep";
            this.m_maxStep.Size = new System.Drawing.Size(121, 20);
            this.m_maxStep.TabIndex = 17;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(29, 72);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(78, 13);
            this.label3.TabIndex = 16;
            this.label3.Text = "Min Step (sec):";
            // 
            // m_minStep
            // 
            this.m_minStep.Location = new System.Drawing.Point(126, 69);
            this.m_minStep.Name = "m_minStep";
            this.m_minStep.Size = new System.Drawing.Size(121, 20);
            this.m_minStep.TabIndex = 15;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(26, 44);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(81, 13);
            this.label2.TabIndex = 14;
            this.label2.Text = "Step Size (sec):";
            // 
            // m_stepSize
            // 
            this.m_stepSize.Location = new System.Drawing.Point(126, 41);
            this.m_stepSize.Name = "m_stepSize";
            this.m_stepSize.Size = new System.Drawing.Size(121, 20);
            this.m_stepSize.TabIndex = 13;
            // 
            // IntegratorSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(249, 208);
            this.ControlBox = false;
            this.Controls.Add(this.m_fixedOrRelative);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.m_maxError);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.m_maxStep);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.m_minStep);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.m_stepSize);
            this.Controls.Add(this.m_finish);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.m_integrator);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(265, 246);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(265, 246);
            this.Name = "IntegratorSettings";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "Integrator Settings";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox m_integrator;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button m_finish;
        private System.Windows.Forms.ComboBox m_fixedOrRelative;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox m_maxError;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox m_maxStep;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox m_minStep;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox m_stepSize;
    }
}