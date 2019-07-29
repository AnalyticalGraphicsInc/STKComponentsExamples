namespace AGI.Examples
{
    partial class ForceModelSettings
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.m_gravityField = new System.Windows.Forms.RadioButton();
            this.m_twoBody = new System.Windows.Forms.RadioButton();
            this.label9 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.m_primaryCB = new System.Windows.Forms.ComboBox();
            this.m_gravFile = new System.Windows.Forms.TextBox();
            this.m_findGravityFile = new System.Windows.Forms.Button();
            this.m_degree = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.m_order = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.m_tides = new System.Windows.Forms.ComboBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.m_thirdBodies = new System.Windows.Forms.CheckedListBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.label12 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.m_reflectivity = new System.Windows.Forms.TextBox();
            this.m_eclipsingBodies = new System.Windows.Forms.CheckedListBox();
            this.m_shadowModel = new System.Windows.Forms.ComboBox();
            this.m_useSRP = new System.Windows.Forms.CheckBox();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.label11 = new System.Windows.Forms.Label();
            this.m_kp = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.m_solarFlux = new System.Windows.Forms.TextBox();
            this.m_averageSolarFlux = new System.Windows.Forms.TextBox();
            this.m_dragCoeff = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.m_densityModel = new System.Windows.Forms.ComboBox();
            this.m_useDrag = new System.Windows.Forms.CheckBox();
            this.m_done = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.m_gravityField);
            this.groupBox1.Controls.Add(this.m_twoBody);
            this.groupBox1.Controls.Add(this.label9);
            this.groupBox1.Controls.Add(this.label8);
            this.groupBox1.Controls.Add(this.m_primaryCB);
            this.groupBox1.Controls.Add(this.m_gravFile);
            this.groupBox1.Controls.Add(this.m_findGravityFile);
            this.groupBox1.Controls.Add(this.m_degree);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.m_order);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.m_tides);
            this.groupBox1.Location = new System.Drawing.Point(9, 10);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(2);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(2);
            this.groupBox1.Size = new System.Drawing.Size(201, 219);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Central Body Gravity";
            // 
            // m_gravityField
            // 
            this.m_gravityField.AutoSize = true;
            this.m_gravityField.Checked = true;
            this.m_gravityField.Location = new System.Drawing.Point(8, 65);
            this.m_gravityField.Name = "m_gravityField";
            this.m_gravityField.Size = new System.Drawing.Size(83, 17);
            this.m_gravityField.TabIndex = 11;
            this.m_gravityField.TabStop = true;
            this.m_gravityField.Text = "Gravity Field";
            this.m_gravityField.UseVisualStyleBackColor = true;
            // 
            // m_twoBody
            // 
            this.m_twoBody.AutoSize = true;
            this.m_twoBody.Location = new System.Drawing.Point(9, 45);
            this.m_twoBody.Name = "m_twoBody";
            this.m_twoBody.Size = new System.Drawing.Size(109, 17);
            this.m_twoBody.TabIndex = 10;
            this.m_twoBody.Text = "Two Body Gravity";
            this.m_twoBody.UseVisualStyleBackColor = true;
            this.m_twoBody.CheckedChanged += new System.EventHandler(this.OnTwoBodyCheckedChanged);
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(65, 191);
            this.label9.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(36, 13);
            this.label9.TabIndex = 9;
            this.label9.Text = "Tides:";
            this.label9.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(5, 21);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(70, 13);
            this.label8.TabIndex = 8;
            this.label8.Text = "Central Body:";
            // 
            // m_primaryCB
            // 
            this.m_primaryCB.FormattingEnabled = true;
            this.m_primaryCB.Location = new System.Drawing.Point(78, 17);
            this.m_primaryCB.Name = "m_primaryCB";
            this.m_primaryCB.Size = new System.Drawing.Size(98, 21);
            this.m_primaryCB.TabIndex = 7;
            this.m_primaryCB.SelectedIndexChanged += new System.EventHandler(this.OnPrimaryCBSelectedIndexChanged);
            // 
            // m_gravFile
            // 
            this.m_gravFile.Location = new System.Drawing.Point(7, 88);
            this.m_gravFile.Margin = new System.Windows.Forms.Padding(2);
            this.m_gravFile.Name = "m_gravFile";
            this.m_gravFile.ReadOnly = true;
            this.m_gravFile.Size = new System.Drawing.Size(190, 20);
            this.m_gravFile.TabIndex = 6;
            // 
            // m_findGravityFile
            // 
            this.m_findGravityFile.Location = new System.Drawing.Point(122, 112);
            this.m_findGravityFile.Margin = new System.Windows.Forms.Padding(2);
            this.m_findGravityFile.Name = "m_findGravityFile";
            this.m_findGravityFile.Size = new System.Drawing.Size(75, 23);
            this.m_findGravityFile.TabIndex = 5;
            this.m_findGravityFile.Text = "Browse";
            this.m_findGravityFile.UseVisualStyleBackColor = true;
            this.m_findGravityFile.Click += new System.EventHandler(this.OnFindGravityFileClick);
            // 
            // m_degree
            // 
            this.m_degree.Location = new System.Drawing.Point(105, 164);
            this.m_degree.Margin = new System.Windows.Forms.Padding(2);
            this.m_degree.Name = "m_degree";
            this.m_degree.Size = new System.Drawing.Size(92, 20);
            this.m_degree.TabIndex = 4;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(59, 167);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(45, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Degree:";
            this.label2.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // m_order
            // 
            this.m_order.Location = new System.Drawing.Point(105, 140);
            this.m_order.Margin = new System.Windows.Forms.Padding(2);
            this.m_order.Name = "m_order";
            this.m_order.Size = new System.Drawing.Size(92, 20);
            this.m_order.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(65, 143);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(36, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Order:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // m_tides
            // 
            this.m_tides.FormattingEnabled = true;
            this.m_tides.Location = new System.Drawing.Point(105, 188);
            this.m_tides.Margin = new System.Windows.Forms.Padding(2);
            this.m_tides.Name = "m_tides";
            this.m_tides.Size = new System.Drawing.Size(92, 21);
            this.m_tides.TabIndex = 0;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.m_thirdBodies);
            this.groupBox2.Location = new System.Drawing.Point(9, 233);
            this.groupBox2.Margin = new System.Windows.Forms.Padding(2);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Padding = new System.Windows.Forms.Padding(2);
            this.groupBox2.Size = new System.Drawing.Size(201, 190);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Third Body Gravity";
            // 
            // m_thirdBodies
            // 
            this.m_thirdBodies.FormattingEnabled = true;
            this.m_thirdBodies.Location = new System.Drawing.Point(4, 18);
            this.m_thirdBodies.Margin = new System.Windows.Forms.Padding(2);
            this.m_thirdBodies.Name = "m_thirdBodies";
            this.m_thirdBodies.Size = new System.Drawing.Size(193, 169);
            this.m_thirdBodies.TabIndex = 0;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.label12);
            this.groupBox3.Controls.Add(this.label10);
            this.groupBox3.Controls.Add(this.label3);
            this.groupBox3.Controls.Add(this.m_reflectivity);
            this.groupBox3.Controls.Add(this.m_eclipsingBodies);
            this.groupBox3.Controls.Add(this.m_shadowModel);
            this.groupBox3.Controls.Add(this.m_useSRP);
            this.groupBox3.Location = new System.Drawing.Point(214, 11);
            this.groupBox3.Margin = new System.Windows.Forms.Padding(2);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Padding = new System.Windows.Forms.Padding(2);
            this.groupBox3.Size = new System.Drawing.Size(212, 219);
            this.groupBox3.TabIndex = 2;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Solar Radiation Force";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(14, 41);
            this.label12.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(76, 13);
            this.label12.TabIndex = 5;
            this.label12.Text = "Shadow Type:";
            this.label12.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(4, 91);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(93, 13);
            this.label10.TabIndex = 4;
            this.label10.Text = "Occluding Bodies:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(27, 67);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(62, 13);
            this.label3.TabIndex = 3;
            this.label3.Text = "Reflectivity:";
            this.label3.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // m_reflectivity
            // 
            this.m_reflectivity.Location = new System.Drawing.Point(94, 64);
            this.m_reflectivity.Name = "m_reflectivity";
            this.m_reflectivity.Size = new System.Drawing.Size(109, 20);
            this.m_reflectivity.TabIndex = 2;
            // 
            // m_eclipsingBodies
            // 
            this.m_eclipsingBodies.FormattingEnabled = true;
            this.m_eclipsingBodies.Location = new System.Drawing.Point(4, 109);
            this.m_eclipsingBodies.Margin = new System.Windows.Forms.Padding(2);
            this.m_eclipsingBodies.Name = "m_eclipsingBodies";
            this.m_eclipsingBodies.Size = new System.Drawing.Size(199, 109);
            this.m_eclipsingBodies.TabIndex = 1;
            // 
            // m_shadowModel
            // 
            this.m_shadowModel.FormattingEnabled = true;
            this.m_shadowModel.Location = new System.Drawing.Point(94, 38);
            this.m_shadowModel.Margin = new System.Windows.Forms.Padding(2);
            this.m_shadowModel.Name = "m_shadowModel";
            this.m_shadowModel.Size = new System.Drawing.Size(109, 21);
            this.m_shadowModel.TabIndex = 1;
            // 
            // m_useSRP
            // 
            this.m_useSRP.AutoSize = true;
            this.m_useSRP.Location = new System.Drawing.Point(7, 21);
            this.m_useSRP.Margin = new System.Windows.Forms.Padding(2);
            this.m_useSRP.Name = "m_useSRP";
            this.m_useSRP.Size = new System.Drawing.Size(45, 17);
            this.m_useSRP.TabIndex = 0;
            this.m_useSRP.Text = "Use";
            this.m_useSRP.UseVisualStyleBackColor = true;
            this.m_useSRP.CheckedChanged += new System.EventHandler(this.OnUseSRPCheckedChanged);
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.label11);
            this.groupBox4.Controls.Add(this.m_kp);
            this.groupBox4.Controls.Add(this.label7);
            this.groupBox4.Controls.Add(this.label6);
            this.groupBox4.Controls.Add(this.label5);
            this.groupBox4.Controls.Add(this.m_solarFlux);
            this.groupBox4.Controls.Add(this.m_averageSolarFlux);
            this.groupBox4.Controls.Add(this.m_dragCoeff);
            this.groupBox4.Controls.Add(this.label4);
            this.groupBox4.Controls.Add(this.m_densityModel);
            this.groupBox4.Controls.Add(this.m_useDrag);
            this.groupBox4.Location = new System.Drawing.Point(214, 233);
            this.groupBox4.Margin = new System.Windows.Forms.Padding(2);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Padding = new System.Windows.Forms.Padding(2);
            this.groupBox4.Size = new System.Drawing.Size(212, 190);
            this.groupBox4.TabIndex = 3;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Drag";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(9, 41);
            this.label11.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(77, 13);
            this.label11.TabIndex = 11;
            this.label11.Text = "Density Model:";
            this.label11.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // m_kp
            // 
            this.m_kp.Location = new System.Drawing.Point(90, 142);
            this.m_kp.Name = "m_kp";
            this.m_kp.Size = new System.Drawing.Size(109, 20);
            this.m_kp.TabIndex = 10;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(15, 145);
            this.label7.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(70, 13);
            this.label7.TabIndex = 9;
            this.label7.Text = "Geomag. KP:";
            this.label7.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(29, 119);
            this.label6.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(56, 13);
            this.label6.TabIndex = 8;
            this.label6.Text = "Solar Flux:";
            this.label6.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(6, 93);
            this.label5.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(78, 13);
            this.label5.TabIndex = 7;
            this.label5.Text = "Avg Solar Flux:";
            this.label5.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // m_solarFlux
            // 
            this.m_solarFlux.Location = new System.Drawing.Point(90, 116);
            this.m_solarFlux.Name = "m_solarFlux";
            this.m_solarFlux.Size = new System.Drawing.Size(109, 20);
            this.m_solarFlux.TabIndex = 6;
            // 
            // m_averageSolarFlux
            // 
            this.m_averageSolarFlux.Location = new System.Drawing.Point(90, 90);
            this.m_averageSolarFlux.Name = "m_averageSolarFlux";
            this.m_averageSolarFlux.Size = new System.Drawing.Size(109, 20);
            this.m_averageSolarFlux.TabIndex = 5;
            // 
            // m_dragCoeff
            // 
            this.m_dragCoeff.Location = new System.Drawing.Point(90, 64);
            this.m_dragCoeff.Name = "m_dragCoeff";
            this.m_dragCoeff.Size = new System.Drawing.Size(109, 20);
            this.m_dragCoeff.TabIndex = 4;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(24, 67);
            this.label4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(61, 13);
            this.label4.TabIndex = 3;
            this.label4.Text = "Drag Coeff:";
            this.label4.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // m_densityModel
            // 
            this.m_densityModel.FormattingEnabled = true;
            this.m_densityModel.Location = new System.Drawing.Point(90, 38);
            this.m_densityModel.Margin = new System.Windows.Forms.Padding(2);
            this.m_densityModel.Name = "m_densityModel";
            this.m_densityModel.Size = new System.Drawing.Size(109, 21);
            this.m_densityModel.TabIndex = 2;
            // 
            // m_useDrag
            // 
            this.m_useDrag.AutoSize = true;
            this.m_useDrag.Location = new System.Drawing.Point(15, 17);
            this.m_useDrag.Margin = new System.Windows.Forms.Padding(2);
            this.m_useDrag.Name = "m_useDrag";
            this.m_useDrag.Size = new System.Drawing.Size(45, 17);
            this.m_useDrag.TabIndex = 1;
            this.m_useDrag.Text = "Use";
            this.m_useDrag.UseVisualStyleBackColor = true;
            this.m_useDrag.CheckedChanged += new System.EventHandler(this.OnUseDragCheckedChanged);
            // 
            // m_done
            // 
            this.m_done.Location = new System.Drawing.Point(351, 427);
            this.m_done.Margin = new System.Windows.Forms.Padding(2);
            this.m_done.Name = "m_done";
            this.m_done.Size = new System.Drawing.Size(75, 23);
            this.m_done.TabIndex = 4;
            this.m_done.Text = "OK";
            this.m_done.UseVisualStyleBackColor = true;
            this.m_done.Click += new System.EventHandler(this.OnDoneClick);
            // 
            // ForceModelSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(429, 454);
            this.ControlBox = false;
            this.Controls.Add(this.m_done);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(445, 492);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(445, 492);
            this.Name = "ForceModelSettings";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "Force Model Settings";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.CheckBox m_useSRP;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.CheckBox m_useDrag;
        private System.Windows.Forms.Button m_done;
        private System.Windows.Forms.TextBox m_degree;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox m_order;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox m_tides;
        private System.Windows.Forms.CheckedListBox m_thirdBodies;
        private System.Windows.Forms.ComboBox m_shadowModel;
        private System.Windows.Forms.ComboBox m_densityModel;
        private System.Windows.Forms.TextBox m_gravFile;
        private System.Windows.Forms.Button m_findGravityFile;
        private System.Windows.Forms.CheckedListBox m_eclipsingBodies;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox m_reflectivity;
        private System.Windows.Forms.TextBox m_dragCoeff;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox m_solarFlux;
        private System.Windows.Forms.TextBox m_averageSolarFlux;
        private System.Windows.Forms.TextBox m_kp;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.ComboBox m_primaryCB;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.RadioButton m_gravityField;
        private System.Windows.Forms.RadioButton m_twoBody;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label12;
    }
}