namespace Communications
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
            this.m_linkComboBox = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.m_hangUpButton = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.transmitPowerLabel = new System.Windows.Forms.Label();
            this.m_transmitPowerTrackBar = new System.Windows.Forms.TrackBar();
            this.m_placeCallButton = new System.Windows.Forms.Button();
            this.displayedLinkBudgetLabel = new System.Windows.Forms.Label();
            this.m_insight3DPanel = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)(this.m_transmitPowerTrackBar)).BeginInit();
            this.SuspendLayout();
            // 
            // m_linkComboBox
            // 
            this.m_linkComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.m_linkComboBox.FormattingEnabled = true;
            this.m_linkComboBox.Location = new System.Drawing.Point(15, 251);
            this.m_linkComboBox.Name = "m_linkComboBox";
            this.m_linkComboBox.Size = new System.Drawing.Size(142, 21);
            this.m_linkComboBox.TabIndex = 25;
            this.m_linkComboBox.SelectedIndexChanged += new System.EventHandler(this.OnLinkComboBoxSelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(52, 93);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(40, 13);
            this.label2.TabIndex = 24;
            this.label2.Text = "5 dBW";
            // 
            // m_hangUpButton
            // 
            this.m_hangUpButton.Location = new System.Drawing.Point(15, 209);
            this.m_hangUpButton.Name = "m_hangUpButton";
            this.m_hangUpButton.Size = new System.Drawing.Size(75, 23);
            this.m_hangUpButton.TabIndex = 23;
            this.m_hangUpButton.Text = "Hang Up";
            this.m_hangUpButton.UseVisualStyleBackColor = true;
            this.m_hangUpButton.Click += new System.EventHandler(this.OnHangUpClick);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(52, 154);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(40, 13);
            this.label3.TabIndex = 22;
            this.label3.Text = "0 dBW";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(52, 32);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(46, 13);
            this.label1.TabIndex = 21;
            this.label1.Text = "10 dBW";
            // 
            // transmitPowerLabel
            // 
            this.transmitPowerLabel.AutoSize = true;
            this.transmitPowerLabel.Location = new System.Drawing.Point(12, 9);
            this.transmitPowerLabel.Name = "transmitPowerLabel";
            this.transmitPowerLabel.Size = new System.Drawing.Size(80, 13);
            this.transmitPowerLabel.TabIndex = 20;
            this.transmitPowerLabel.Text = "Transmit Power";
            // 
            // m_transmitPowerTrackBar
            // 
            this.m_transmitPowerTrackBar.LargeChange = 1;
            this.m_transmitPowerTrackBar.Location = new System.Drawing.Point(23, 25);
            this.m_transmitPowerTrackBar.Name = "m_transmitPowerTrackBar";
            this.m_transmitPowerTrackBar.Orientation = System.Windows.Forms.Orientation.Vertical;
            this.m_transmitPowerTrackBar.Size = new System.Drawing.Size(45, 149);
            this.m_transmitPowerTrackBar.TabIndex = 19;
            this.m_transmitPowerTrackBar.Value = 5;
            this.m_transmitPowerTrackBar.ValueChanged += new System.EventHandler(this.OnTransmitPowerValueChanged);
            // 
            // m_placeCallButton
            // 
            this.m_placeCallButton.Location = new System.Drawing.Point(15, 180);
            this.m_placeCallButton.Name = "m_placeCallButton";
            this.m_placeCallButton.Size = new System.Drawing.Size(75, 23);
            this.m_placeCallButton.TabIndex = 18;
            this.m_placeCallButton.Text = "Place Call";
            this.m_placeCallButton.UseVisualStyleBackColor = true;
            this.m_placeCallButton.Click += new System.EventHandler(this.OnPlaceCallClick);
            // 
            // displayedLinkBudgetLabel
            // 
            this.displayedLinkBudgetLabel.AutoSize = true;
            this.displayedLinkBudgetLabel.Location = new System.Drawing.Point(12, 235);
            this.displayedLinkBudgetLabel.Name = "displayedLinkBudgetLabel";
            this.displayedLinkBudgetLabel.Size = new System.Drawing.Size(113, 13);
            this.displayedLinkBudgetLabel.TabIndex = 27;
            this.displayedLinkBudgetLabel.Text = "Displayed Link Budget";
            // 
            // m_insight3DPanel
            // 
            this.m_insight3DPanel.Location = new System.Drawing.Point(163, 12);
            this.m_insight3DPanel.Name = "m_insight3DPanel";
            this.m_insight3DPanel.Size = new System.Drawing.Size(833, 706);
            this.m_insight3DPanel.TabIndex = 26;
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1008, 730);
            this.Controls.Add(this.m_insight3DPanel);
            this.Controls.Add(this.displayedLinkBudgetLabel);
            this.Controls.Add(this.m_linkComboBox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.m_hangUpButton);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.transmitPowerLabel);
            this.Controls.Add(this.m_transmitPowerTrackBar);
            this.Controls.Add(this.m_placeCallButton);
            this.Name = "Main";
            this.Text = "Communications Example";
            ((System.ComponentModel.ISupportInitialize)(this.m_transmitPowerTrackBar)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox m_linkComboBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button m_hangUpButton;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label transmitPowerLabel;
        private System.Windows.Forms.TrackBar m_transmitPowerTrackBar;
        private System.Windows.Forms.Button m_placeCallButton;
        private System.Windows.Forms.Label displayedLinkBudgetLabel;
        private System.Windows.Forms.Panel m_insight3DPanel;

    }
}

