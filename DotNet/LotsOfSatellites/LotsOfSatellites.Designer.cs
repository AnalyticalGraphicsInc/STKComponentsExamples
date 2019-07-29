using AGI.Foundation.Graphics;
namespace AGI.Examples.LotsOfSatellites
{
    partial class LotsOfSatellites
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LotsOfSatellites));
            this.m_insight3DPanel = new System.Windows.Forms.Panel();
            this.SuspendLayout();
            // 
            // m_insight3DPanel
            // 
            this.m_insight3DPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.m_insight3DPanel.Location = new System.Drawing.Point(0, 0);
            this.m_insight3DPanel.Name = "m_insight3DPanel";
            this.m_insight3DPanel.Size = new System.Drawing.Size(792, 566);
            this.m_insight3DPanel.TabIndex = 0;
            // 
            // LotsOfSatellites
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(792, 566);
            this.Controls.Add(this.m_insight3DPanel);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "LotsOfSatellites";
            this.Text = "Lots of Satellites";
            this.Load += new System.EventHandler(this.OnLoad);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel m_insight3DPanel;


    }
}