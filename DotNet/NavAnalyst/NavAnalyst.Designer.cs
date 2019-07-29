namespace NavAnalyst
{
    partial class NavAnalyst
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NavAnalyst));
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.ObjectView = new System.Windows.Forms.TreeView();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.GPSTimeRadio = new System.Windows.Forms.RadioButton();
            this.UTCTimeRadio = new System.Windows.Forms.RadioButton();
            this.label11 = new System.Windows.Forms.Label();
            this.exportEphemeris = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.almanacName = new System.Windows.Forms.TextBox();
            this.SelectAlmanac = new System.Windows.Forms.Button();
            this.TimeStep = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.StopTime = new System.Windows.Forms.DateTimePicker();
            this.label1 = new System.Windows.Forms.Label();
            this.StartTime = new System.Windows.Forms.DateTimePicker();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.AllInViewSolType = new System.Windows.Forms.RadioButton();
            this.BestNSolType = new System.Windows.Forms.RadioButton();
            this.label10 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.NumberOfChannels = new System.Windows.Forms.NumericUpDown();
            this.label8 = new System.Windows.Forms.Label();
            this.ReceiverHeight = new System.Windows.Forms.TextBox();
            this.DeleteReceiver = new System.Windows.Forms.Button();
            this.label7 = new System.Windows.Forms.Label();
            this.Longitude = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.Latitude = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.MaskAngle = new System.Windows.Forms.TextBox();
            this.AddReceiver = new System.Windows.Forms.Button();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.label15 = new System.Windows.Forms.Label();
            this.ConfIntvlUpDown = new System.Windows.Forms.NumericUpDown();
            this.UseExtrapolationCheckBox = new System.Windows.Forms.CheckBox();
            this.label14 = new System.Windows.Forms.Label();
            this.PSFName = new System.Windows.Forms.TextBox();
            this.SelectPSF = new System.Windows.Forms.Button();
            this.label13 = new System.Windows.Forms.Label();
            this.PAFName = new System.Windows.Forms.TextBox();
            this.SelectPAF = new System.Windows.Forms.Button();
            this.graphtabs = new System.Windows.Forms.TabControl();
            this.DOPTab = new System.Windows.Forms.TabPage();
            this.panel1 = new System.Windows.Forms.Panel();
            this.DisplayGDOP = new System.Windows.Forms.CheckBox();
            this.DisplayTDOP = new System.Windows.Forms.CheckBox();
            this.DisplayPDOP = new System.Windows.Forms.CheckBox();
            this.DisplayHDOP = new System.Windows.Forms.CheckBox();
            this.DisplayVDOP = new System.Windows.Forms.CheckBox();
            this.DisplayNDOP = new System.Windows.Forms.CheckBox();
            this.DisplayEDOP = new System.Windows.Forms.CheckBox();
            this.DOPGraph = new ZedGraph.ZedGraphControl();
            this.AzElTab = new System.Windows.Forms.TabPage();
            this.panel2 = new System.Windows.Forms.Panel();
            this.label12 = new System.Windows.Forms.Label();
            this.azimuthAxis = new System.Windows.Forms.RadioButton();
            this.dateTimeXAxis = new System.Windows.Forms.RadioButton();
            this.AzElGraph = new ZedGraph.ZedGraphControl();
            this.NavAccTab = new System.Windows.Forms.TabPage();
            this.panel3 = new System.Windows.Forms.Panel();
            this.PredAccCheckBox = new System.Windows.Forms.CheckBox();
            this.AsAccCheckBox = new System.Windows.Forms.CheckBox();
            this.NavAccGraph = new ZedGraph.ZedGraphControl();
            this.openAlmanacDialog = new System.Windows.Forms.OpenFileDialog();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contentsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openPAFDialog = new System.Windows.Forms.OpenFileDialog();
            this.openPSFDialog = new System.Windows.Forms.OpenFileDialog();
            this.PAFtoolTip = new System.Windows.Forms.ToolTip(this.components);
            this.PSFtoolTip = new System.Windows.Forms.ToolTip(this.components);
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.NumberOfChannels)).BeginInit();
            this.tabPage3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ConfIntvlUpDown)).BeginInit();
            this.graphtabs.SuspendLayout();
            this.DOPTab.SuspendLayout();
            this.panel1.SuspendLayout();
            this.AzElTab.SuspendLayout();
            this.panel2.SuspendLayout();
            this.NavAccTab.SuspendLayout();
            this.panel3.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            resources.ApplyResources(this.splitContainer1, "splitContainer1");
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.splitContainer2);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.graphtabs);
            // 
            // splitContainer2
            // 
            this.splitContainer2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            resources.ApplyResources(this.splitContainer2, "splitContainer2");
            this.splitContainer2.Name = "splitContainer2";
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.progressBar1);
            this.splitContainer2.Panel1.Controls.Add(this.ObjectView);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.tabControl1);
            // 
            // progressBar1
            // 
            resources.ApplyResources(this.progressBar1, "progressBar1");
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Step = 1;
            // 
            // ObjectView
            // 
            resources.ApplyResources(this.ObjectView, "ObjectView");
            this.ObjectView.Name = "ObjectView";
            this.ObjectView.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            ((System.Windows.Forms.TreeNode)(resources.GetObject("ObjectView.Nodes")))});
            // 
            // tabControl1
            // 
            resources.ApplyResources(this.tabControl1, "tabControl1");
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.GPSTimeRadio);
            this.tabPage1.Controls.Add(this.UTCTimeRadio);
            this.tabPage1.Controls.Add(this.label11);
            this.tabPage1.Controls.Add(this.exportEphemeris);
            this.tabPage1.Controls.Add(this.label4);
            this.tabPage1.Controls.Add(this.almanacName);
            this.tabPage1.Controls.Add(this.SelectAlmanac);
            this.tabPage1.Controls.Add(this.TimeStep);
            this.tabPage1.Controls.Add(this.label3);
            this.tabPage1.Controls.Add(this.label2);
            this.tabPage1.Controls.Add(this.StopTime);
            this.tabPage1.Controls.Add(this.label1);
            this.tabPage1.Controls.Add(this.StartTime);
            resources.ApplyResources(this.tabPage1, "tabPage1");
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // GPSTimeRadio
            // 
            resources.ApplyResources(this.GPSTimeRadio, "GPSTimeRadio");
            this.GPSTimeRadio.Name = "GPSTimeRadio";
            this.GPSTimeRadio.UseVisualStyleBackColor = true;
            // 
            // UTCTimeRadio
            // 
            resources.ApplyResources(this.UTCTimeRadio, "UTCTimeRadio");
            this.UTCTimeRadio.Checked = true;
            this.UTCTimeRadio.Name = "UTCTimeRadio";
            this.UTCTimeRadio.TabStop = true;
            this.UTCTimeRadio.UseVisualStyleBackColor = true;
            // 
            // label11
            // 
            resources.ApplyResources(this.label11, "label11");
            this.label11.Name = "label11";
            // 
            // exportEphemeris
            // 
            resources.ApplyResources(this.exportEphemeris, "exportEphemeris");
            this.exportEphemeris.Name = "exportEphemeris";
            this.exportEphemeris.UseVisualStyleBackColor = true;
            this.exportEphemeris.Click += new System.EventHandler(this.OnExportEphemerisClick);
            // 
            // label4
            // 
            resources.ApplyResources(this.label4, "label4");
            this.label4.Name = "label4";
            // 
            // almanacName
            // 
            resources.ApplyResources(this.almanacName, "almanacName");
            this.almanacName.Name = "almanacName";
            this.toolTip1.SetToolTip(this.almanacName, resources.GetString("almanacName.ToolTip"));
            this.PAFtoolTip.SetToolTip(this.almanacName, resources.GetString("almanacName.ToolTip1"));
            // 
            // SelectAlmanac
            // 
            resources.ApplyResources(this.SelectAlmanac, "SelectAlmanac");
            this.SelectAlmanac.Name = "SelectAlmanac";
            this.SelectAlmanac.UseVisualStyleBackColor = true;
            this.SelectAlmanac.Click += new System.EventHandler(this.OnSelectAlmanacClick);
            // 
            // TimeStep
            // 
            this.TimeStep.FormattingEnabled = true;
            this.TimeStep.Items.AddRange(new object[] {
            resources.GetString("TimeStep.Items"),
            resources.GetString("TimeStep.Items1"),
            resources.GetString("TimeStep.Items2"),
            resources.GetString("TimeStep.Items3"),
            resources.GetString("TimeStep.Items4"),
            resources.GetString("TimeStep.Items5"),
            resources.GetString("TimeStep.Items6")});
            resources.ApplyResources(this.TimeStep, "TimeStep");
            this.TimeStep.Name = "TimeStep";
            // 
            // label3
            // 
            resources.ApplyResources(this.label3, "label3");
            this.label3.Name = "label3";
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.Name = "label2";
            // 
            // StopTime
            // 
            resources.ApplyResources(this.StopTime, "StopTime");
            this.StopTime.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.StopTime.Name = "StopTime";
            this.StopTime.ShowUpDown = true;
            this.StopTime.Value = new System.DateTime(2016, 10, 6, 0, 0, 0, 0);
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // StartTime
            // 
            resources.ApplyResources(this.StartTime, "StartTime");
            this.StartTime.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.StartTime.MinDate = new System.DateTime(1980, 1, 6, 0, 0, 0, 0);
            this.StartTime.Name = "StartTime";
            this.StartTime.ShowUpDown = true;
            this.StartTime.Value = new System.DateTime(2017, 3, 17, 0, 0, 0, 0);
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.AllInViewSolType);
            this.tabPage2.Controls.Add(this.BestNSolType);
            this.tabPage2.Controls.Add(this.label10);
            this.tabPage2.Controls.Add(this.label9);
            this.tabPage2.Controls.Add(this.NumberOfChannels);
            this.tabPage2.Controls.Add(this.label8);
            this.tabPage2.Controls.Add(this.ReceiverHeight);
            this.tabPage2.Controls.Add(this.DeleteReceiver);
            this.tabPage2.Controls.Add(this.label7);
            this.tabPage2.Controls.Add(this.Longitude);
            this.tabPage2.Controls.Add(this.label6);
            this.tabPage2.Controls.Add(this.Latitude);
            this.tabPage2.Controls.Add(this.label5);
            this.tabPage2.Controls.Add(this.MaskAngle);
            this.tabPage2.Controls.Add(this.AddReceiver);
            resources.ApplyResources(this.tabPage2, "tabPage2");
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // AllInViewSolType
            // 
            resources.ApplyResources(this.AllInViewSolType, "AllInViewSolType");
            this.AllInViewSolType.Checked = true;
            this.AllInViewSolType.Name = "AllInViewSolType";
            this.AllInViewSolType.TabStop = true;
            this.AllInViewSolType.UseVisualStyleBackColor = true;
            // 
            // BestNSolType
            // 
            resources.ApplyResources(this.BestNSolType, "BestNSolType");
            this.BestNSolType.Name = "BestNSolType";
            this.BestNSolType.UseVisualStyleBackColor = true;
            // 
            // label10
            // 
            resources.ApplyResources(this.label10, "label10");
            this.label10.Name = "label10";
            // 
            // label9
            // 
            resources.ApplyResources(this.label9, "label9");
            this.label9.Name = "label9";
            // 
            // NumberOfChannels
            // 
            resources.ApplyResources(this.NumberOfChannels, "NumberOfChannels");
            this.NumberOfChannels.Maximum = new decimal(new int[] {
            20,
            0,
            0,
            0});
            this.NumberOfChannels.Minimum = new decimal(new int[] {
            4,
            0,
            0,
            0});
            this.NumberOfChannels.Name = "NumberOfChannels";
            this.NumberOfChannels.Value = new decimal(new int[] {
            12,
            0,
            0,
            0});
            // 
            // label8
            // 
            resources.ApplyResources(this.label8, "label8");
            this.label8.Name = "label8";
            // 
            // ReceiverHeight
            // 
            resources.ApplyResources(this.ReceiverHeight, "ReceiverHeight");
            this.ReceiverHeight.Name = "ReceiverHeight";
            // 
            // DeleteReceiver
            // 
            resources.ApplyResources(this.DeleteReceiver, "DeleteReceiver");
            this.DeleteReceiver.Name = "DeleteReceiver";
            this.DeleteReceiver.UseVisualStyleBackColor = true;
            this.DeleteReceiver.Click += new System.EventHandler(this.OnDeleteReceiverClick);
            // 
            // label7
            // 
            resources.ApplyResources(this.label7, "label7");
            this.label7.Name = "label7";
            // 
            // Longitude
            // 
            resources.ApplyResources(this.Longitude, "Longitude");
            this.Longitude.Name = "Longitude";
            // 
            // label6
            // 
            resources.ApplyResources(this.label6, "label6");
            this.label6.Name = "label6";
            // 
            // Latitude
            // 
            resources.ApplyResources(this.Latitude, "Latitude");
            this.Latitude.Name = "Latitude";
            // 
            // label5
            // 
            resources.ApplyResources(this.label5, "label5");
            this.label5.Name = "label5";
            // 
            // MaskAngle
            // 
            resources.ApplyResources(this.MaskAngle, "MaskAngle");
            this.MaskAngle.Name = "MaskAngle";
            // 
            // AddReceiver
            // 
            resources.ApplyResources(this.AddReceiver, "AddReceiver");
            this.AddReceiver.Name = "AddReceiver";
            this.AddReceiver.UseVisualStyleBackColor = true;
            this.AddReceiver.Click += new System.EventHandler(this.OnAddReceiverClick);
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.label15);
            this.tabPage3.Controls.Add(this.ConfIntvlUpDown);
            this.tabPage3.Controls.Add(this.UseExtrapolationCheckBox);
            this.tabPage3.Controls.Add(this.label14);
            this.tabPage3.Controls.Add(this.PSFName);
            this.tabPage3.Controls.Add(this.SelectPSF);
            this.tabPage3.Controls.Add(this.label13);
            this.tabPage3.Controls.Add(this.PAFName);
            this.tabPage3.Controls.Add(this.SelectPAF);
            resources.ApplyResources(this.tabPage3, "tabPage3");
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // label15
            // 
            resources.ApplyResources(this.label15, "label15");
            this.label15.Name = "label15";
            // 
            // ConfIntvlUpDown
            // 
            resources.ApplyResources(this.ConfIntvlUpDown, "ConfIntvlUpDown");
            this.ConfIntvlUpDown.Name = "ConfIntvlUpDown";
            this.ConfIntvlUpDown.Value = new decimal(new int[] {
            50,
            0,
            0,
            0});
            // 
            // UseExtrapolationCheckBox
            // 
            resources.ApplyResources(this.UseExtrapolationCheckBox, "UseExtrapolationCheckBox");
            this.UseExtrapolationCheckBox.Name = "UseExtrapolationCheckBox";
            this.UseExtrapolationCheckBox.UseVisualStyleBackColor = true;
            // 
            // label14
            // 
            resources.ApplyResources(this.label14, "label14");
            this.label14.Name = "label14";
            // 
            // PSFName
            // 
            resources.ApplyResources(this.PSFName, "PSFName");
            this.PSFName.Name = "PSFName";
            this.toolTip1.SetToolTip(this.PSFName, resources.GetString("PSFName.ToolTip"));
            this.PAFtoolTip.SetToolTip(this.PSFName, resources.GetString("PSFName.ToolTip1"));
            // 
            // SelectPSF
            // 
            resources.ApplyResources(this.SelectPSF, "SelectPSF");
            this.SelectPSF.Name = "SelectPSF";
            this.SelectPSF.UseVisualStyleBackColor = true;
            this.SelectPSF.Click += new System.EventHandler(this.OnSelectPSFClick);
            // 
            // label13
            // 
            resources.ApplyResources(this.label13, "label13");
            this.label13.Name = "label13";
            // 
            // PAFName
            // 
            resources.ApplyResources(this.PAFName, "PAFName");
            this.PAFName.Name = "PAFName";
            this.toolTip1.SetToolTip(this.PAFName, resources.GetString("PAFName.ToolTip"));
            this.PAFtoolTip.SetToolTip(this.PAFName, resources.GetString("PAFName.ToolTip1"));
            // 
            // SelectPAF
            // 
            resources.ApplyResources(this.SelectPAF, "SelectPAF");
            this.SelectPAF.Name = "SelectPAF";
            this.SelectPAF.UseVisualStyleBackColor = true;
            this.SelectPAF.Click += new System.EventHandler(this.OnSelectPAFClick);
            // 
            // graphtabs
            // 
            this.graphtabs.Controls.Add(this.DOPTab);
            this.graphtabs.Controls.Add(this.AzElTab);
            this.graphtabs.Controls.Add(this.NavAccTab);
            resources.ApplyResources(this.graphtabs, "graphtabs");
            this.graphtabs.Name = "graphtabs";
            this.graphtabs.SelectedIndex = 0;
            // 
            // DOPTab
            // 
            this.DOPTab.Controls.Add(this.panel1);
            this.DOPTab.Controls.Add(this.DOPGraph);
            resources.ApplyResources(this.DOPTab, "DOPTab");
            this.DOPTab.Name = "DOPTab";
            this.DOPTab.UseVisualStyleBackColor = true;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.DisplayGDOP);
            this.panel1.Controls.Add(this.DisplayTDOP);
            this.panel1.Controls.Add(this.DisplayPDOP);
            this.panel1.Controls.Add(this.DisplayHDOP);
            this.panel1.Controls.Add(this.DisplayVDOP);
            this.panel1.Controls.Add(this.DisplayNDOP);
            this.panel1.Controls.Add(this.DisplayEDOP);
            resources.ApplyResources(this.panel1, "panel1");
            this.panel1.Name = "panel1";
            // 
            // DisplayGDOP
            // 
            resources.ApplyResources(this.DisplayGDOP, "DisplayGDOP");
            this.DisplayGDOP.Checked = true;
            this.DisplayGDOP.CheckState = System.Windows.Forms.CheckState.Checked;
            this.DisplayGDOP.Name = "DisplayGDOP";
            this.PSFtoolTip.SetToolTip(this.DisplayGDOP, resources.GetString("DisplayGDOP.ToolTip"));
            this.DisplayGDOP.UseVisualStyleBackColor = true;
            this.DisplayGDOP.CheckedChanged += new System.EventHandler(this.DisplayGDOP_CheckedChanged);
            // 
            // DisplayTDOP
            // 
            resources.ApplyResources(this.DisplayTDOP, "DisplayTDOP");
            this.DisplayTDOP.Checked = true;
            this.DisplayTDOP.CheckState = System.Windows.Forms.CheckState.Checked;
            this.DisplayTDOP.Name = "DisplayTDOP";
            this.PSFtoolTip.SetToolTip(this.DisplayTDOP, resources.GetString("DisplayTDOP.ToolTip"));
            this.DisplayTDOP.UseVisualStyleBackColor = true;
            this.DisplayTDOP.CheckedChanged += new System.EventHandler(this.DisplayTDOP_CheckedChanged);
            // 
            // DisplayPDOP
            // 
            resources.ApplyResources(this.DisplayPDOP, "DisplayPDOP");
            this.DisplayPDOP.Checked = true;
            this.DisplayPDOP.CheckState = System.Windows.Forms.CheckState.Checked;
            this.DisplayPDOP.Name = "DisplayPDOP";
            this.PSFtoolTip.SetToolTip(this.DisplayPDOP, resources.GetString("DisplayPDOP.ToolTip"));
            this.DisplayPDOP.UseVisualStyleBackColor = true;
            this.DisplayPDOP.CheckedChanged += new System.EventHandler(this.DisplayPDOP_CheckedChanged);
            // 
            // DisplayHDOP
            // 
            resources.ApplyResources(this.DisplayHDOP, "DisplayHDOP");
            this.DisplayHDOP.Checked = true;
            this.DisplayHDOP.CheckState = System.Windows.Forms.CheckState.Checked;
            this.DisplayHDOP.Name = "DisplayHDOP";
            this.PSFtoolTip.SetToolTip(this.DisplayHDOP, resources.GetString("DisplayHDOP.ToolTip"));
            this.DisplayHDOP.UseVisualStyleBackColor = true;
            this.DisplayHDOP.CheckedChanged += new System.EventHandler(this.DisplayHDOP_CheckedChanged);
            // 
            // DisplayVDOP
            // 
            resources.ApplyResources(this.DisplayVDOP, "DisplayVDOP");
            this.DisplayVDOP.Checked = true;
            this.DisplayVDOP.CheckState = System.Windows.Forms.CheckState.Checked;
            this.DisplayVDOP.Name = "DisplayVDOP";
            this.PSFtoolTip.SetToolTip(this.DisplayVDOP, resources.GetString("DisplayVDOP.ToolTip"));
            this.DisplayVDOP.UseVisualStyleBackColor = true;
            this.DisplayVDOP.CheckedChanged += new System.EventHandler(this.DisplayVDOP_CheckedChanged);
            // 
            // DisplayNDOP
            // 
            resources.ApplyResources(this.DisplayNDOP, "DisplayNDOP");
            this.DisplayNDOP.Checked = true;
            this.DisplayNDOP.CheckState = System.Windows.Forms.CheckState.Checked;
            this.DisplayNDOP.Name = "DisplayNDOP";
            this.PSFtoolTip.SetToolTip(this.DisplayNDOP, resources.GetString("DisplayNDOP.ToolTip"));
            this.DisplayNDOP.UseVisualStyleBackColor = true;
            this.DisplayNDOP.CheckedChanged += new System.EventHandler(this.DisplayNDOP_CheckedChanged);
            // 
            // DisplayEDOP
            // 
            resources.ApplyResources(this.DisplayEDOP, "DisplayEDOP");
            this.DisplayEDOP.Checked = true;
            this.DisplayEDOP.CheckState = System.Windows.Forms.CheckState.Checked;
            this.DisplayEDOP.Name = "DisplayEDOP";
            this.toolTip1.SetToolTip(this.DisplayEDOP, resources.GetString("DisplayEDOP.ToolTip"));
            this.PSFtoolTip.SetToolTip(this.DisplayEDOP, resources.GetString("DisplayEDOP.ToolTip1"));
            this.DisplayEDOP.UseVisualStyleBackColor = true;
            this.DisplayEDOP.CheckedChanged += new System.EventHandler(this.DisplayEDOP_CheckedChanged);
            // 
            // DOPGraph
            // 
            resources.ApplyResources(this.DOPGraph, "DOPGraph");
            this.DOPGraph.EditButtons = System.Windows.Forms.MouseButtons.Left;
            this.DOPGraph.EditModifierKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.None)));
            this.DOPGraph.IsAutoScrollRange = false;
            this.DOPGraph.IsEnableHEdit = false;
            this.DOPGraph.IsEnableHPan = true;
            this.DOPGraph.IsEnableHZoom = true;
            this.DOPGraph.IsEnableVEdit = false;
            this.DOPGraph.IsEnableVPan = true;
            this.DOPGraph.IsEnableVZoom = true;
            this.DOPGraph.IsPrintFillPage = true;
            this.DOPGraph.IsPrintKeepAspectRatio = true;
            this.DOPGraph.IsScrollY2 = false;
            this.DOPGraph.IsShowContextMenu = true;
            this.DOPGraph.IsShowCopyMessage = true;
            this.DOPGraph.IsShowCursorValues = false;
            this.DOPGraph.IsShowHScrollBar = false;
            this.DOPGraph.IsShowPointValues = true;
            this.DOPGraph.IsShowVScrollBar = false;
            this.DOPGraph.IsSynchronizeXAxes = false;
            this.DOPGraph.IsSynchronizeYAxes = false;
            this.DOPGraph.IsZoomOnMouseCenter = false;
            this.DOPGraph.LinkButtons = System.Windows.Forms.MouseButtons.Left;
            this.DOPGraph.LinkModifierKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.None)));
            this.DOPGraph.Name = "DOPGraph";
            this.DOPGraph.PanButtons = System.Windows.Forms.MouseButtons.Left;
            this.DOPGraph.PanButtons2 = System.Windows.Forms.MouseButtons.Middle;
            this.DOPGraph.PanModifierKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Shift | System.Windows.Forms.Keys.None)));
            this.DOPGraph.PanModifierKeys2 = System.Windows.Forms.Keys.None;
            this.DOPGraph.PointDateFormat = "g";
            this.DOPGraph.PointValueFormat = "G";
            this.DOPGraph.ScrollMaxX = 0;
            this.DOPGraph.ScrollMaxY = 0;
            this.DOPGraph.ScrollMaxY2 = 0;
            this.DOPGraph.ScrollMinX = 0;
            this.DOPGraph.ScrollMinY = 0;
            this.DOPGraph.ScrollMinY2 = 0;
            this.DOPGraph.ZoomButtons = System.Windows.Forms.MouseButtons.Left;
            this.DOPGraph.ZoomButtons2 = System.Windows.Forms.MouseButtons.None;
            this.DOPGraph.ZoomModifierKeys = System.Windows.Forms.Keys.None;
            this.DOPGraph.ZoomModifierKeys2 = System.Windows.Forms.Keys.None;
            this.DOPGraph.ZoomStepFraction = 0.1;
            // 
            // AzElTab
            // 
            this.AzElTab.Controls.Add(this.panel2);
            this.AzElTab.Controls.Add(this.AzElGraph);
            resources.ApplyResources(this.AzElTab, "AzElTab");
            this.AzElTab.Name = "AzElTab";
            this.AzElTab.UseVisualStyleBackColor = true;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.label12);
            this.panel2.Controls.Add(this.azimuthAxis);
            this.panel2.Controls.Add(this.dateTimeXAxis);
            resources.ApplyResources(this.panel2, "panel2");
            this.panel2.Name = "panel2";
            // 
            // label12
            // 
            resources.ApplyResources(this.label12, "label12");
            this.label12.Name = "label12";
            // 
            // azimuthAxis
            // 
            resources.ApplyResources(this.azimuthAxis, "azimuthAxis");
            this.azimuthAxis.Name = "azimuthAxis";
            this.azimuthAxis.UseVisualStyleBackColor = true;
            this.azimuthAxis.CheckedChanged += new System.EventHandler(this.azimuthAxis_CheckedChanged);
            // 
            // dateTimeXAxis
            // 
            resources.ApplyResources(this.dateTimeXAxis, "dateTimeXAxis");
            this.dateTimeXAxis.Checked = true;
            this.dateTimeXAxis.Name = "dateTimeXAxis";
            this.dateTimeXAxis.TabStop = true;
            this.dateTimeXAxis.UseVisualStyleBackColor = true;
            this.dateTimeXAxis.CheckedChanged += new System.EventHandler(this.dateTimeXAxis_CheckedChanged);
            // 
            // AzElGraph
            // 
            resources.ApplyResources(this.AzElGraph, "AzElGraph");
            this.AzElGraph.EditButtons = System.Windows.Forms.MouseButtons.Left;
            this.AzElGraph.EditModifierKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.None)));
            this.AzElGraph.IsAutoScrollRange = false;
            this.AzElGraph.IsEnableHEdit = false;
            this.AzElGraph.IsEnableHPan = true;
            this.AzElGraph.IsEnableHZoom = true;
            this.AzElGraph.IsEnableVEdit = false;
            this.AzElGraph.IsEnableVPan = true;
            this.AzElGraph.IsEnableVZoom = true;
            this.AzElGraph.IsPrintFillPage = true;
            this.AzElGraph.IsPrintKeepAspectRatio = true;
            this.AzElGraph.IsScrollY2 = false;
            this.AzElGraph.IsShowContextMenu = true;
            this.AzElGraph.IsShowCopyMessage = true;
            this.AzElGraph.IsShowCursorValues = false;
            this.AzElGraph.IsShowHScrollBar = false;
            this.AzElGraph.IsShowPointValues = true;
            this.AzElGraph.IsShowVScrollBar = false;
            this.AzElGraph.IsSynchronizeXAxes = false;
            this.AzElGraph.IsSynchronizeYAxes = false;
            this.AzElGraph.IsZoomOnMouseCenter = false;
            this.AzElGraph.LinkButtons = System.Windows.Forms.MouseButtons.Left;
            this.AzElGraph.LinkModifierKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.None)));
            this.AzElGraph.Name = "AzElGraph";
            this.AzElGraph.PanButtons = System.Windows.Forms.MouseButtons.Left;
            this.AzElGraph.PanButtons2 = System.Windows.Forms.MouseButtons.Middle;
            this.AzElGraph.PanModifierKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Shift | System.Windows.Forms.Keys.None)));
            this.AzElGraph.PanModifierKeys2 = System.Windows.Forms.Keys.None;
            this.AzElGraph.PointDateFormat = "g";
            this.AzElGraph.PointValueFormat = "G";
            this.AzElGraph.ScrollMaxX = 0;
            this.AzElGraph.ScrollMaxY = 0;
            this.AzElGraph.ScrollMaxY2 = 0;
            this.AzElGraph.ScrollMinX = 0;
            this.AzElGraph.ScrollMinY = 0;
            this.AzElGraph.ScrollMinY2 = 0;
            this.AzElGraph.ZoomButtons = System.Windows.Forms.MouseButtons.Left;
            this.AzElGraph.ZoomButtons2 = System.Windows.Forms.MouseButtons.None;
            this.AzElGraph.ZoomModifierKeys = System.Windows.Forms.Keys.None;
            this.AzElGraph.ZoomModifierKeys2 = System.Windows.Forms.Keys.None;
            this.AzElGraph.ZoomStepFraction = 0.1;
            // 
            // NavAccTab
            // 
            this.NavAccTab.Controls.Add(this.panel3);
            this.NavAccTab.Controls.Add(this.NavAccGraph);
            resources.ApplyResources(this.NavAccTab, "NavAccTab");
            this.NavAccTab.Name = "NavAccTab";
            this.NavAccTab.UseVisualStyleBackColor = true;
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.PredAccCheckBox);
            this.panel3.Controls.Add(this.AsAccCheckBox);
            resources.ApplyResources(this.panel3, "panel3");
            this.panel3.Name = "panel3";
            // 
            // PredAccCheckBox
            // 
            resources.ApplyResources(this.PredAccCheckBox, "PredAccCheckBox");
            this.PredAccCheckBox.Checked = true;
            this.PredAccCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.PredAccCheckBox.Name = "PredAccCheckBox";
            this.PredAccCheckBox.UseVisualStyleBackColor = true;
            this.PredAccCheckBox.CheckedChanged += new System.EventHandler(this.PredAccCheckBox_CheckedChanged);
            // 
            // AsAccCheckBox
            // 
            resources.ApplyResources(this.AsAccCheckBox, "AsAccCheckBox");
            this.AsAccCheckBox.Checked = true;
            this.AsAccCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.AsAccCheckBox.Name = "AsAccCheckBox";
            this.AsAccCheckBox.UseVisualStyleBackColor = true;
            this.AsAccCheckBox.CheckedChanged += new System.EventHandler(this.AsAccCheckBox_CheckedChanged);
            // 
            // NavAccGraph
            // 
            resources.ApplyResources(this.NavAccGraph, "NavAccGraph");
            this.NavAccGraph.EditButtons = System.Windows.Forms.MouseButtons.Left;
            this.NavAccGraph.EditModifierKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.None)));
            this.NavAccGraph.IsAutoScrollRange = false;
            this.NavAccGraph.IsEnableHEdit = false;
            this.NavAccGraph.IsEnableHPan = true;
            this.NavAccGraph.IsEnableHZoom = true;
            this.NavAccGraph.IsEnableVEdit = false;
            this.NavAccGraph.IsEnableVPan = true;
            this.NavAccGraph.IsEnableVZoom = true;
            this.NavAccGraph.IsPrintFillPage = true;
            this.NavAccGraph.IsPrintKeepAspectRatio = true;
            this.NavAccGraph.IsScrollY2 = false;
            this.NavAccGraph.IsShowContextMenu = true;
            this.NavAccGraph.IsShowCopyMessage = true;
            this.NavAccGraph.IsShowCursorValues = false;
            this.NavAccGraph.IsShowHScrollBar = false;
            this.NavAccGraph.IsShowPointValues = true;
            this.NavAccGraph.IsShowVScrollBar = false;
            this.NavAccGraph.IsSynchronizeXAxes = false;
            this.NavAccGraph.IsSynchronizeYAxes = false;
            this.NavAccGraph.IsZoomOnMouseCenter = false;
            this.NavAccGraph.LinkButtons = System.Windows.Forms.MouseButtons.Left;
            this.NavAccGraph.LinkModifierKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.None)));
            this.NavAccGraph.Name = "NavAccGraph";
            this.NavAccGraph.PanButtons = System.Windows.Forms.MouseButtons.Left;
            this.NavAccGraph.PanButtons2 = System.Windows.Forms.MouseButtons.Middle;
            this.NavAccGraph.PanModifierKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Shift | System.Windows.Forms.Keys.None)));
            this.NavAccGraph.PanModifierKeys2 = System.Windows.Forms.Keys.None;
            this.NavAccGraph.PointDateFormat = "g";
            this.NavAccGraph.PointValueFormat = "G";
            this.NavAccGraph.ScrollMaxX = 0;
            this.NavAccGraph.ScrollMaxY = 0;
            this.NavAccGraph.ScrollMaxY2 = 0;
            this.NavAccGraph.ScrollMinX = 0;
            this.NavAccGraph.ScrollMinY = 0;
            this.NavAccGraph.ScrollMinY2 = 0;
            this.NavAccGraph.ZoomButtons = System.Windows.Forms.MouseButtons.Left;
            this.NavAccGraph.ZoomButtons2 = System.Windows.Forms.MouseButtons.None;
            this.NavAccGraph.ZoomModifierKeys = System.Windows.Forms.Keys.None;
            this.NavAccGraph.ZoomModifierKeys2 = System.Windows.Forms.Keys.None;
            this.NavAccGraph.ZoomStepFraction = 0.1;
            this.NavAccGraph.Load += new System.EventHandler(this.NavAccGraph_Load);
            // 
            // openAlmanacDialog
            // 
            resources.ApplyResources(this.openAlmanacDialog, "openAlmanacDialog");
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.helpToolStripMenuItem});
            resources.ApplyResources(this.menuStrip1, "menuStrip1");
            this.menuStrip1.Name = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            resources.ApplyResources(this.fileToolStripMenuItem, "fileToolStripMenuItem");
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            resources.ApplyResources(this.exitToolStripMenuItem, "exitToolStripMenuItem");
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.OnExitToolStripMenuItemClick);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.contentsToolStripMenuItem,
            this.toolStripSeparator5,
            this.aboutToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            resources.ApplyResources(this.helpToolStripMenuItem, "helpToolStripMenuItem");
            // 
            // contentsToolStripMenuItem
            // 
            this.contentsToolStripMenuItem.Name = "contentsToolStripMenuItem";
            resources.ApplyResources(this.contentsToolStripMenuItem, "contentsToolStripMenuItem");
            this.contentsToolStripMenuItem.Click += new System.EventHandler(this.OnContentsToolStripMenuItemClick);
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            resources.ApplyResources(this.toolStripSeparator5, "toolStripSeparator5");
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            resources.ApplyResources(this.aboutToolStripMenuItem, "aboutToolStripMenuItem");
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.OnAboutToolStripMenuItemClick);
            // 
            // openPAFDialog
            // 
            resources.ApplyResources(this.openPAFDialog, "openPAFDialog");
            // 
            // openPSFDialog
            // 
            resources.ApplyResources(this.openPSFDialog, "openPSFDialog");
            // 
            // NavAnalyst
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.menuStrip1);
            this.Name = "NavAnalyst";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            this.splitContainer2.ResumeLayout(false);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.NumberOfChannels)).EndInit();
            this.tabPage3.ResumeLayout(false);
            this.tabPage3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ConfIntvlUpDown)).EndInit();
            this.graphtabs.ResumeLayout(false);
            this.DOPTab.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.AzElTab.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.NavAccTab.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.TreeView ObjectView;
        private System.Windows.Forms.Button AddReceiver;
        private System.Windows.Forms.Button DeleteReceiver;
        private System.Windows.Forms.OpenFileDialog openAlmanacDialog;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox almanacName;
        private System.Windows.Forms.Button SelectAlmanac;
        private System.Windows.Forms.ComboBox TimeStep;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.DateTimePicker StopTime;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.DateTimePicker StartTime;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox MaskAngle;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox Longitude;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox Latitude;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.NumericUpDown NumberOfChannels;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox ReceiverHeight;
        private System.Windows.Forms.RadioButton AllInViewSolType;
        private System.Windows.Forms.RadioButton BestNSolType;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Button exportEphemeris;
        private System.Windows.Forms.RadioButton GPSTimeRadio;
        private System.Windows.Forms.RadioButton UTCTimeRadio;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem contentsToolStripMenuItem;
        private System.Windows.Forms.TabControl graphtabs;
        private System.Windows.Forms.TabPage DOPTab;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.CheckBox DisplayGDOP;
        private System.Windows.Forms.CheckBox DisplayTDOP;
        private System.Windows.Forms.CheckBox DisplayPDOP;
        private System.Windows.Forms.CheckBox DisplayHDOP;
        private System.Windows.Forms.CheckBox DisplayVDOP;
        private System.Windows.Forms.CheckBox DisplayNDOP;
        private System.Windows.Forms.CheckBox DisplayEDOP;
        private ZedGraph.ZedGraphControl DOPGraph;
        private System.Windows.Forms.TabPage AzElTab;
        private ZedGraph.ZedGraphControl AzElGraph;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.RadioButton azimuthAxis;
        private System.Windows.Forms.RadioButton dateTimeXAxis;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.TabPage NavAccTab;
        private System.Windows.Forms.TabPage tabPage3;
        private ZedGraph.ZedGraphControl NavAccGraph;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.TextBox PSFName;
        private System.Windows.Forms.Button SelectPSF;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.TextBox PAFName;
        private System.Windows.Forms.Button SelectPAF;
        private System.Windows.Forms.OpenFileDialog openPAFDialog;
        private System.Windows.Forms.OpenFileDialog openPSFDialog;
        private System.Windows.Forms.ToolTip PAFtoolTip;
        private System.Windows.Forms.ToolTip PSFtoolTip;
        private System.Windows.Forms.CheckBox UseExtrapolationCheckBox;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.NumericUpDown ConfIntvlUpDown;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.CheckBox PredAccCheckBox;
        private System.Windows.Forms.CheckBox AsAccCheckBox;
    }
}

