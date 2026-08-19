namespace AGI.Examples
{
    partial class TrajectoryOptimizationDemo
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
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.screenshotButton = new System.Windows.Forms.Button();
            this.OutputTable = new System.Windows.Forms.ListView();
            this.Parameter = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.OutputResults = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.runIterationButton = new System.Windows.Forms.Button();
            this.resetButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.screenshotButton);
            this.splitContainer1.Panel1.Controls.Add(this.OutputTable);
            this.splitContainer1.Panel1.Controls.Add(this.runIterationButton);
            this.splitContainer1.Panel1.Controls.Add(this.resetButton);
            this.splitContainer1.Size = new System.Drawing.Size(1169, 482);
            this.splitContainer1.SplitterDistance = 314;
            this.splitContainer1.TabIndex = 0;
            // 
            // screenshotButton
            // 
            this.screenshotButton.Location = new System.Drawing.Point(205, 12);
            this.screenshotButton.Name = "screenshotButton";
            this.screenshotButton.Size = new System.Drawing.Size(101, 23);
            this.screenshotButton.TabIndex = 2;
            this.screenshotButton.Text = "Take Screenshot";
            this.screenshotButton.UseVisualStyleBackColor = true;
            this.screenshotButton.Click += new System.EventHandler(this.screenshotButton_Click);
            // 
            // OutputTable
            // 
            this.OutputTable.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.Parameter,
            this.OutputResults});
            this.OutputTable.GridLines = true;
            this.OutputTable.Location = new System.Drawing.Point(3, 76);
            this.OutputTable.Name = "OutputTable";
            this.OutputTable.Size = new System.Drawing.Size(307, 368);
            this.OutputTable.TabIndex = 2;
            this.OutputTable.UseCompatibleStateImageBehavior = false;
            this.OutputTable.View = System.Windows.Forms.View.Details;
            // 
            // Parameter
            // 
            this.Parameter.Text = "Parameters";
            this.Parameter.Width = 119;
            // 
            // OutputResults
            // 
            this.OutputResults.Text = "Output Results";
            this.OutputResults.Width = 184;
            // 
            // runIterationButton
            // 
            this.runIterationButton.Location = new System.Drawing.Point(3, 12);
            this.runIterationButton.Name = "runIterationButton";
            this.runIterationButton.Size = new System.Drawing.Size(101, 23);
            this.runIterationButton.TabIndex = 0;
            this.runIterationButton.Text = "Run One Iteration";
            this.runIterationButton.UseVisualStyleBackColor = true;
            this.runIterationButton.Click += new System.EventHandler(this.runIterationButton_Click);
            // 
            // resetButton
            // 
            this.resetButton.Location = new System.Drawing.Point(110, 12);
            this.resetButton.Name = "resetButton";
            this.resetButton.Size = new System.Drawing.Size(89, 23);
            this.resetButton.TabIndex = 1;
            this.resetButton.Text = "Reset Optimizer";
            this.resetButton.UseVisualStyleBackColor = true;
            this.resetButton.Click += new System.EventHandler(this.resetButton_Click);
            // 
            // TrajectoryOptimizationDemo
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(1169, 482);
            this.Controls.Add(this.splitContainer1);
            this.Name = "TrajectoryOptimizationDemo";
            this.Text = "Trajectory Optimization Demo";
            this.splitContainer1.Panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Button resetButton;
        private System.Windows.Forms.Button runIterationButton;
        private System.Windows.Forms.ListView OutputTable;
        private System.Windows.Forms.ColumnHeader Parameter;
        private System.Windows.Forms.ColumnHeader OutputResults;
        private System.Windows.Forms.Button screenshotButton;
    }
}

