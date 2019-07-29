namespace AGI.Examples
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
            this.m_forceModels = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.m_propagate = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.m_integrator = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.m_mass = new System.Windows.Forms.TextBox();
            this.m_area = new System.Windows.Forms.TextBox();
            this.m_start = new System.Windows.Forms.DateTimePicker();
            this.m_end = new System.Windows.Forms.DateTimePicker();
            this.m_propagationProgress = new System.Windows.Forms.ProgressBar();
            this.m_keplerianOrbitalElementsEntry = new AGI.Examples.KeplerianOrbitalElementsEntry();
            this.m_insight3DPanel = new System.Windows.Forms.Panel();
            this.SuspendLayout();
            // 
            // m_forceModels
            // 
            this.m_forceModels.Location = new System.Drawing.Point(11, 317);
            this.m_forceModels.Name = "m_forceModels";
            this.m_forceModels.Size = new System.Drawing.Size(92, 23);
            this.m_forceModels.TabIndex = 2;
            this.m_forceModels.Text = "Force Models...";
            this.m_forceModels.UseVisualStyleBackColor = true;
            this.m_forceModels.Click += new System.EventHandler(this.OnForceModelsClick);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(35, 32);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(68, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Start/Epoch:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(71, 60);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(29, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "End:";
            // 
            // m_propagate
            // 
            this.m_propagate.Location = new System.Drawing.Point(11, 389);
            this.m_propagate.Name = "m_propagate";
            this.m_propagate.Size = new System.Drawing.Size(92, 23);
            this.m_propagate.TabIndex = 7;
            this.m_propagate.Text = "Propagate";
            this.m_propagate.UseVisualStyleBackColor = true;
            this.m_propagate.Click += new System.EventHandler(this.OnPropagateClick);
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(24, 244);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(100, 33);
            this.label3.TabIndex = 8;
            this.label3.Text = "Cross sectional area (m*m):";
            this.label3.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // m_integrator
            // 
            this.m_integrator.Location = new System.Drawing.Point(11, 346);
            this.m_integrator.Name = "m_integrator";
            this.m_integrator.Size = new System.Drawing.Size(92, 23);
            this.m_integrator.TabIndex = 10;
            this.m_integrator.Text = "Integrator...";
            this.m_integrator.UseVisualStyleBackColor = true;
            this.m_integrator.Click += new System.EventHandler(this.OnIntegratorClick);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(71, 282);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(56, 13);
            this.label4.TabIndex = 12;
            this.label4.Text = "Mass (kg):";
            this.label4.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // m_mass
            // 
            this.m_mass.Location = new System.Drawing.Point(139, 279);
            this.m_mass.Name = "m_mass";
            this.m_mass.Size = new System.Drawing.Size(112, 20);
            this.m_mass.TabIndex = 11;
            // 
            // m_area
            // 
            this.m_area.Location = new System.Drawing.Point(139, 249);
            this.m_area.Name = "m_area";
            this.m_area.Size = new System.Drawing.Size(112, 20);
            this.m_area.TabIndex = 9;
            // 
            // m_start
            // 
            this.m_start.CustomFormat = "";
            this.m_start.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.m_start.Location = new System.Drawing.Point(106, 28);
            this.m_start.Name = "m_start";
            this.m_start.Size = new System.Drawing.Size(147, 20);
            this.m_start.TabIndex = 13;
            // 
            // m_end
            // 
            this.m_end.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.m_end.Location = new System.Drawing.Point(106, 55);
            this.m_end.Name = "m_end";
            this.m_end.Size = new System.Drawing.Size(147, 20);
            this.m_end.TabIndex = 14;
            // 
            // m_propagationProgress
            // 
            this.m_propagationProgress.Location = new System.Drawing.Point(5, 431);
            this.m_propagationProgress.Name = "m_propagationProgress";
            this.m_propagationProgress.Size = new System.Drawing.Size(248, 23);
            this.m_propagationProgress.TabIndex = 15;
            // 
            // m_keplerianOrbitalElementsEntry
            // 
            this.m_keplerianOrbitalElementsEntry.Location = new System.Drawing.Point(11, 76);
            this.m_keplerianOrbitalElementsEntry.Name = "m_keplerianOrbitalElementsEntry";
            this.m_keplerianOrbitalElementsEntry.Size = new System.Drawing.Size(240, 167);
            this.m_keplerianOrbitalElementsEntry.TabIndex = 1;
            // 
            // m_insight3DPanel
            // 
            this.m_insight3DPanel.Location = new System.Drawing.Point(259, 12);
            this.m_insight3DPanel.Name = "m_insight3DPanel";
            this.m_insight3DPanel.Size = new System.Drawing.Size(541, 442);
            this.m_insight3DPanel.TabIndex = 16;
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(812, 466);
            this.Controls.Add(this.m_insight3DPanel);
            this.Controls.Add(this.m_propagationProgress);
            this.Controls.Add(this.m_end);
            this.Controls.Add(this.m_start);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.m_mass);
            this.Controls.Add(this.m_integrator);
            this.Controls.Add(this.m_area);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.m_propagate);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.m_forceModels);
            this.Controls.Add(this.m_keplerianOrbitalElementsEntry);
            this.MinimumSize = new System.Drawing.Size(750, 500);
            this.Name = "Main";
            this.Text = "Numerical Propagation Demo";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private KeplerianOrbitalElementsEntry m_keplerianOrbitalElementsEntry;
        private System.Windows.Forms.Button m_forceModels;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button m_propagate;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox m_area;
        private System.Windows.Forms.Button m_integrator;
        private System.Windows.Forms.TextBox m_mass;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.DateTimePicker m_start;
        private System.Windows.Forms.DateTimePicker m_end;
        private System.Windows.Forms.ProgressBar m_propagationProgress;
        private System.Windows.Forms.Panel m_insight3DPanel;
    }
}

