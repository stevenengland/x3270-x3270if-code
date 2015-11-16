namespace x3270ifGuiTest
{
    partial class x3270ifGuiTest
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

            if (disposing && session != null)
            {
                session.Close();
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(x3270ifGuiTest));
            this.hostnameTextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.portTextBox = new System.Windows.Forms.TextBox();
            this.secureCheckBox = new System.Windows.Forms.CheckBox();
            this.label3 = new System.Windows.Forms.Label();
            this.usernameTextBox = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.passwordTextBox = new System.Windows.Forms.TextBox();
            this.runQueryButton = new System.Windows.Forms.Button();
            this.stateLabel = new System.Windows.Forms.Label();
            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            this.quitButton = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.resultLabel = new System.Windows.Forms.Label();
            this.loggedOnLabel = new System.Windows.Forms.Label();
            this.startedLabel = new System.Windows.Forms.Label();
            this.connectedLabel = new System.Windows.Forms.Label();
            this.stopButton = new System.Windows.Forms.Button();
            this.localFileLabel = new System.Windows.Forms.Label();
            this.localFileTextBox = new System.Windows.Forms.TextBox();
            this.hostFileLabel = new System.Windows.Forms.Label();
            this.hostFileTextBox = new System.Windows.Forms.TextBox();
            this.directionSendButton = new System.Windows.Forms.RadioButton();
            this.directionReceiveButton = new System.Windows.Forms.RadioButton();
            this.directionBox = new System.Windows.Forms.GroupBox();
            this.modeBox = new System.Windows.Forms.GroupBox();
            this.modeAsciiButton = new System.Windows.Forms.RadioButton();
            this.asciiBox = new System.Windows.Forms.GroupBox();
            this.windowsCodePageTextBox = new System.Windows.Forms.TextBox();
            this.crCheckBox = new System.Windows.Forms.CheckBox();
            this.remapCheckBox = new System.Windows.Forms.CheckBox();
            this.windowsCodePageLabel = new System.Windows.Forms.Label();
            this.modeBinaryButton = new System.Windows.Forms.RadioButton();
            this.hostTypeBox = new System.Windows.Forms.GroupBox();
            this.hostCicsButton = new System.Windows.Forms.RadioButton();
            this.hostTsoButton = new System.Windows.Forms.RadioButton();
            this.hostVmButton = new System.Windows.Forms.RadioButton();
            this.existsBox = new System.Windows.Forms.GroupBox();
            this.existsAppendButton = new System.Windows.Forms.RadioButton();
            this.existsKeepButton = new System.Windows.Forms.RadioButton();
            this.existsReplaceButton = new System.Windows.Forms.RadioButton();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.recfmBox = new System.Windows.Forms.GroupBox();
            this.lreclTextBox = new System.Windows.Forms.TextBox();
            this.lreclLabel = new System.Windows.Forms.Label();
            this.recfmDefaultButton = new System.Windows.Forms.RadioButton();
            this.recfmUndefinedButton = new System.Windows.Forms.RadioButton();
            this.recfmFixedButton = new System.Windows.Forms.RadioButton();
            this.recfmVariableButton = new System.Windows.Forms.RadioButton();
            this.tsoAllocationBox = new System.Windows.Forms.GroupBox();
            this.avblockTextBox = new System.Windows.Forms.TextBox();
            this.secondarySpaceTextBox = new System.Windows.Forms.TextBox();
            this.primarySpaceTextBox = new System.Windows.Forms.TextBox();
            this.avblockLabel = new System.Windows.Forms.Label();
            this.allocTracksButton = new System.Windows.Forms.RadioButton();
            this.allocCylindersButton = new System.Windows.Forms.RadioButton();
            this.allocAvblockButton = new System.Windows.Forms.RadioButton();
            this.label8 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.transferButton = new System.Windows.Forms.Button();
            this.fileBrowseButton = new System.Windows.Forms.Button();
            this.filesBox = new System.Windows.Forms.GroupBox();
            this.sizesBox = new System.Windows.Forms.GroupBox();
            this.bufferSizeTextBox = new System.Windows.Forms.TextBox();
            this.blockSizeTextBox = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.blockSizeLabel = new System.Windows.Forms.Label();
            this.queryTextBox = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.queryGroupBox = new System.Windows.Forms.GroupBox();
            this.fileTransferBox = new System.Windows.Forms.GroupBox();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.loggedOnPictureBox = new System.Windows.Forms.PictureBox();
            this.connectedPictureBox = new System.Windows.Forms.PictureBox();
            this.startedPictureBox = new System.Windows.Forms.PictureBox();
            this.screenLabel = new System.Windows.Forms.Label();
            this.panel1.SuspendLayout();
            this.directionBox.SuspendLayout();
            this.modeBox.SuspendLayout();
            this.asciiBox.SuspendLayout();
            this.hostTypeBox.SuspendLayout();
            this.existsBox.SuspendLayout();
            this.recfmBox.SuspendLayout();
            this.tsoAllocationBox.SuspendLayout();
            this.filesBox.SuspendLayout();
            this.sizesBox.SuspendLayout();
            this.queryGroupBox.SuspendLayout();
            this.fileTransferBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.loggedOnPictureBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.connectedPictureBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.startedPictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // hostnameTextBox
            // 
            this.hostnameTextBox.Location = new System.Drawing.Point(76, 12);
            this.hostnameTextBox.Name = "hostnameTextBox";
            this.hostnameTextBox.Size = new System.Drawing.Size(100, 20);
            this.hostnameTextBox.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(15, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(55, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Hostname";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(15, 46);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(26, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "Port";
            // 
            // portTextBox
            // 
            this.portTextBox.Location = new System.Drawing.Point(76, 43);
            this.portTextBox.Name = "portTextBox";
            this.portTextBox.Size = new System.Drawing.Size(99, 20);
            this.portTextBox.TabIndex = 3;
            // 
            // secureCheckBox
            // 
            this.secureCheckBox.AutoSize = true;
            this.secureCheckBox.Location = new System.Drawing.Point(182, 14);
            this.secureCheckBox.Name = "secureCheckBox";
            this.secureCheckBox.Size = new System.Drawing.Size(60, 17);
            this.secureCheckBox.TabIndex = 2;
            this.secureCheckBox.Text = "Secure";
            this.secureCheckBox.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(15, 79);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(55, 13);
            this.label3.TabIndex = 0;
            this.label3.Text = "Username";
            // 
            // usernameTextBox
            // 
            this.usernameTextBox.Location = new System.Drawing.Point(76, 76);
            this.usernameTextBox.MaxLength = 8;
            this.usernameTextBox.Name = "usernameTextBox";
            this.usernameTextBox.Size = new System.Drawing.Size(98, 20);
            this.usernameTextBox.TabIndex = 4;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(15, 114);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(53, 13);
            this.label4.TabIndex = 0;
            this.label4.Text = "Password";
            // 
            // passwordTextBox
            // 
            this.passwordTextBox.Location = new System.Drawing.Point(76, 111);
            this.passwordTextBox.MaxLength = 8;
            this.passwordTextBox.Name = "passwordTextBox";
            this.passwordTextBox.Size = new System.Drawing.Size(98, 20);
            this.passwordTextBox.TabIndex = 5;
            this.passwordTextBox.UseSystemPasswordChar = true;
            // 
            // runQueryButton
            // 
            this.runQueryButton.ForeColor = System.Drawing.Color.Green;
            this.runQueryButton.Location = new System.Drawing.Point(532, 14);
            this.runQueryButton.Name = "runQueryButton";
            this.runQueryButton.Size = new System.Drawing.Size(76, 41);
            this.runQueryButton.TabIndex = 7;
            this.runQueryButton.Text = "Run Query";
            this.runQueryButton.UseVisualStyleBackColor = true;
            this.runQueryButton.Click += new System.EventHandler(this.runQueryButton_Click);
            // 
            // stateLabel
            // 
            this.stateLabel.AutoSize = true;
            this.stateLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.stateLabel.Location = new System.Drawing.Point(18, 158);
            this.stateLabel.Name = "stateLabel";
            this.stateLabel.Size = new System.Drawing.Size(37, 13);
            this.stateLabel.TabIndex = 0;
            this.stateLabel.Text = "State";
            // 
            // backgroundWorker1
            // 
            this.backgroundWorker1.WorkerReportsProgress = true;
            this.backgroundWorker1.WorkerSupportsCancellation = true;
            this.backgroundWorker1.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorker1_DoWork);
            this.backgroundWorker1.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.backgroundWorker1_ProgressChanged);
            this.backgroundWorker1.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.backgroundWorker1_RunWorkerCompleted);
            // 
            // quitButton
            // 
            this.quitButton.ForeColor = System.Drawing.SystemColors.ControlText;
            this.quitButton.Location = new System.Drawing.Point(544, 12);
            this.quitButton.Name = "quitButton";
            this.quitButton.Size = new System.Drawing.Size(76, 38);
            this.quitButton.TabIndex = 21;
            this.quitButton.Text = "Quit";
            this.quitButton.UseVisualStyleBackColor = true;
            this.quitButton.Click += new System.EventHandler(this.quitButton_Click);
            // 
            // panel1
            // 
            this.panel1.AutoScroll = true;
            this.panel1.Controls.Add(this.resultLabel);
            this.panel1.Location = new System.Drawing.Point(6, 69);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(602, 158);
            this.panel1.TabIndex = 0;
            // 
            // resultLabel
            // 
            this.resultLabel.AutoSize = true;
            this.resultLabel.Font = new System.Drawing.Font("Lucida Console", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.resultLabel.Location = new System.Drawing.Point(13, 0);
            this.resultLabel.Name = "resultLabel";
            this.resultLabel.Size = new System.Drawing.Size(47, 11);
            this.resultLabel.TabIndex = 0;
            this.resultLabel.Text = "Result";
            // 
            // loggedOnLabel
            // 
            this.loggedOnLabel.AutoSize = true;
            this.loggedOnLabel.Location = new System.Drawing.Point(459, 66);
            this.loggedOnLabel.Name = "loggedOnLabel";
            this.loggedOnLabel.Size = new System.Drawing.Size(74, 13);
            this.loggedOnLabel.TabIndex = 0;
            this.loggedOnLabel.Text = "Not logged on";
            // 
            // startedLabel
            // 
            this.startedLabel.AutoSize = true;
            this.startedLabel.Location = new System.Drawing.Point(458, 19);
            this.startedLabel.Name = "startedLabel";
            this.startedLabel.Size = new System.Drawing.Size(59, 13);
            this.startedLabel.TabIndex = 0;
            this.startedLabel.Text = "Not started";
            // 
            // connectedLabel
            // 
            this.connectedLabel.AutoSize = true;
            this.connectedLabel.Location = new System.Drawing.Point(459, 43);
            this.connectedLabel.Name = "connectedLabel";
            this.connectedLabel.Size = new System.Drawing.Size(78, 13);
            this.connectedLabel.TabIndex = 0;
            this.connectedLabel.Text = "Not connected";
            // 
            // stopButton
            // 
            this.stopButton.ForeColor = System.Drawing.Color.Red;
            this.stopButton.Location = new System.Drawing.Point(544, 56);
            this.stopButton.Name = "stopButton";
            this.stopButton.Size = new System.Drawing.Size(76, 38);
            this.stopButton.TabIndex = 20;
            this.stopButton.Text = "Stop";
            this.stopButton.UseVisualStyleBackColor = true;
            this.stopButton.Click += new System.EventHandler(this.stopButton_Click);
            // 
            // localFileLabel
            // 
            this.localFileLabel.AutoSize = true;
            this.localFileLabel.Location = new System.Drawing.Point(10, 24);
            this.localFileLabel.Name = "localFileLabel";
            this.localFileLabel.Size = new System.Drawing.Size(52, 13);
            this.localFileLabel.TabIndex = 0;
            this.localFileLabel.Text = "Local File";
            // 
            // localFileTextBox
            // 
            this.localFileTextBox.Location = new System.Drawing.Point(65, 21);
            this.localFileTextBox.Name = "localFileTextBox";
            this.localFileTextBox.Size = new System.Drawing.Size(151, 20);
            this.localFileTextBox.TabIndex = 1;
            // 
            // hostFileLabel
            // 
            this.hostFileLabel.AutoSize = true;
            this.hostFileLabel.Location = new System.Drawing.Point(10, 51);
            this.hostFileLabel.Name = "hostFileLabel";
            this.hostFileLabel.Size = new System.Drawing.Size(48, 13);
            this.hostFileLabel.TabIndex = 0;
            this.hostFileLabel.Text = "Host File";
            // 
            // hostFileTextBox
            // 
            this.hostFileTextBox.Location = new System.Drawing.Point(65, 48);
            this.hostFileTextBox.Name = "hostFileTextBox";
            this.hostFileTextBox.Size = new System.Drawing.Size(210, 20);
            this.hostFileTextBox.TabIndex = 3;
            // 
            // directionSendButton
            // 
            this.directionSendButton.AutoSize = true;
            this.directionSendButton.Checked = true;
            this.directionSendButton.Location = new System.Drawing.Point(15, 19);
            this.directionSendButton.Name = "directionSendButton";
            this.directionSendButton.Size = new System.Drawing.Size(85, 17);
            this.directionSendButton.TabIndex = 0;
            this.directionSendButton.TabStop = true;
            this.directionSendButton.Tag = "Send";
            this.directionSendButton.Text = "Send to host";
            this.directionSendButton.UseVisualStyleBackColor = true;
            this.directionSendButton.CheckedChanged += new System.EventHandler(this.directionSendButton_CheckedChanged);
            // 
            // directionReceiveButton
            // 
            this.directionReceiveButton.AutoSize = true;
            this.directionReceiveButton.Location = new System.Drawing.Point(15, 42);
            this.directionReceiveButton.Name = "directionReceiveButton";
            this.directionReceiveButton.Size = new System.Drawing.Size(111, 17);
            this.directionReceiveButton.TabIndex = 1;
            this.directionReceiveButton.Tag = "Receive";
            this.directionReceiveButton.Text = "Receive from host";
            this.directionReceiveButton.UseVisualStyleBackColor = true;
            this.directionReceiveButton.CheckedChanged += new System.EventHandler(this.directionSendButton_CheckedChanged);
            // 
            // directionBox
            // 
            this.directionBox.Controls.Add(this.directionSendButton);
            this.directionBox.Controls.Add(this.directionReceiveButton);
            this.directionBox.Location = new System.Drawing.Point(311, 440);
            this.directionBox.Name = "directionBox";
            this.directionBox.Size = new System.Drawing.Size(145, 81);
            this.directionBox.TabIndex = 14;
            this.directionBox.TabStop = false;
            this.directionBox.Text = "Direction";
            // 
            // modeBox
            // 
            this.modeBox.Controls.Add(this.modeAsciiButton);
            this.modeBox.Controls.Add(this.asciiBox);
            this.modeBox.Controls.Add(this.modeBinaryButton);
            this.modeBox.Location = new System.Drawing.Point(158, 527);
            this.modeBox.Name = "modeBox";
            this.modeBox.Size = new System.Drawing.Size(147, 196);
            this.modeBox.TabIndex = 13;
            this.modeBox.TabStop = false;
            this.modeBox.Text = "Mode";
            // 
            // modeAsciiButton
            // 
            this.modeAsciiButton.AutoSize = true;
            this.modeAsciiButton.Checked = true;
            this.modeAsciiButton.Location = new System.Drawing.Point(15, 42);
            this.modeAsciiButton.Name = "modeAsciiButton";
            this.modeAsciiButton.Size = new System.Drawing.Size(72, 17);
            this.modeAsciiButton.TabIndex = 1;
            this.modeAsciiButton.TabStop = true;
            this.modeAsciiButton.Tag = "Ascii";
            this.modeAsciiButton.Text = "ASCII text";
            this.modeAsciiButton.UseVisualStyleBackColor = true;
            this.modeAsciiButton.CheckedChanged += new System.EventHandler(this.modeBinaryButton_CheckedChanged);
            // 
            // asciiBox
            // 
            this.asciiBox.Controls.Add(this.windowsCodePageTextBox);
            this.asciiBox.Controls.Add(this.crCheckBox);
            this.asciiBox.Controls.Add(this.remapCheckBox);
            this.asciiBox.Controls.Add(this.windowsCodePageLabel);
            this.asciiBox.Location = new System.Drawing.Point(7, 68);
            this.asciiBox.Name = "asciiBox";
            this.asciiBox.Size = new System.Drawing.Size(134, 118);
            this.asciiBox.TabIndex = 2;
            this.asciiBox.TabStop = false;
            this.asciiBox.Text = "Ascii options";
            // 
            // windowsCodePageTextBox
            // 
            this.windowsCodePageTextBox.Location = new System.Drawing.Point(10, 87);
            this.windowsCodePageTextBox.Name = "windowsCodePageTextBox";
            this.windowsCodePageTextBox.Size = new System.Drawing.Size(116, 20);
            this.windowsCodePageTextBox.TabIndex = 3;
            // 
            // crCheckBox
            // 
            this.crCheckBox.AutoSize = true;
            this.crCheckBox.Checked = true;
            this.crCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.crCheckBox.Location = new System.Drawing.Point(10, 19);
            this.crCheckBox.Name = "crCheckBox";
            this.crCheckBox.Size = new System.Drawing.Size(108, 17);
            this.crCheckBox.TabIndex = 1;
            this.crCheckBox.Text = "Add/remove CRs";
            this.crCheckBox.UseVisualStyleBackColor = true;
            // 
            // remapCheckBox
            // 
            this.remapCheckBox.AutoSize = true;
            this.remapCheckBox.Checked = true;
            this.remapCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.remapCheckBox.Location = new System.Drawing.Point(10, 42);
            this.remapCheckBox.Name = "remapCheckBox";
            this.remapCheckBox.Size = new System.Drawing.Size(112, 17);
            this.remapCheckBox.TabIndex = 2;
            this.remapCheckBox.Text = "Map character set";
            this.remapCheckBox.UseVisualStyleBackColor = true;
            this.remapCheckBox.CheckedChanged += new System.EventHandler(this.remapCheckBox_CheckedChanged);
            // 
            // windowsCodePageLabel
            // 
            this.windowsCodePageLabel.AutoSize = true;
            this.windowsCodePageLabel.Location = new System.Drawing.Point(7, 71);
            this.windowsCodePageLabel.Name = "windowsCodePageLabel";
            this.windowsCodePageLabel.Size = new System.Drawing.Size(105, 13);
            this.windowsCodePageLabel.TabIndex = 0;
            this.windowsCodePageLabel.Text = "Windows code page";
            // 
            // modeBinaryButton
            // 
            this.modeBinaryButton.AutoSize = true;
            this.modeBinaryButton.Location = new System.Drawing.Point(15, 19);
            this.modeBinaryButton.Name = "modeBinaryButton";
            this.modeBinaryButton.Size = new System.Drawing.Size(78, 17);
            this.modeBinaryButton.TabIndex = 0;
            this.modeBinaryButton.Tag = "Binary";
            this.modeBinaryButton.Text = "Binary data";
            this.modeBinaryButton.UseVisualStyleBackColor = true;
            this.modeBinaryButton.CheckedChanged += new System.EventHandler(this.modeBinaryButton_CheckedChanged);
            // 
            // hostTypeBox
            // 
            this.hostTypeBox.Controls.Add(this.hostCicsButton);
            this.hostTypeBox.Controls.Add(this.hostTsoButton);
            this.hostTypeBox.Controls.Add(this.hostVmButton);
            this.hostTypeBox.Location = new System.Drawing.Point(22, 527);
            this.hostTypeBox.Name = "hostTypeBox";
            this.hostTypeBox.Size = new System.Drawing.Size(130, 92);
            this.hostTypeBox.TabIndex = 11;
            this.hostTypeBox.TabStop = false;
            this.hostTypeBox.Text = "Host type";
            // 
            // hostCicsButton
            // 
            this.hostCicsButton.AutoSize = true;
            this.hostCicsButton.Location = new System.Drawing.Point(15, 65);
            this.hostCicsButton.Name = "hostCicsButton";
            this.hostCicsButton.Size = new System.Drawing.Size(49, 17);
            this.hostCicsButton.TabIndex = 2;
            this.hostCicsButton.Tag = "Cics";
            this.hostCicsButton.Text = "CICS";
            this.hostCicsButton.UseVisualStyleBackColor = true;
            this.hostCicsButton.CheckedChanged += new System.EventHandler(this.directionSendButton_CheckedChanged);
            // 
            // hostTsoButton
            // 
            this.hostTsoButton.AutoSize = true;
            this.hostTsoButton.Checked = true;
            this.hostTsoButton.Location = new System.Drawing.Point(15, 19);
            this.hostTsoButton.Name = "hostTsoButton";
            this.hostTsoButton.Size = new System.Drawing.Size(47, 17);
            this.hostTsoButton.TabIndex = 0;
            this.hostTsoButton.TabStop = true;
            this.hostTsoButton.Tag = "Tso";
            this.hostTsoButton.Text = "TSO";
            this.hostTsoButton.UseVisualStyleBackColor = true;
            this.hostTsoButton.CheckedChanged += new System.EventHandler(this.directionSendButton_CheckedChanged);
            // 
            // hostVmButton
            // 
            this.hostVmButton.AutoSize = true;
            this.hostVmButton.Location = new System.Drawing.Point(15, 42);
            this.hostVmButton.Name = "hostVmButton";
            this.hostVmButton.Size = new System.Drawing.Size(69, 17);
            this.hostVmButton.TabIndex = 1;
            this.hostVmButton.Tag = "Vm";
            this.hostVmButton.Text = "VM/CMS";
            this.hostVmButton.UseVisualStyleBackColor = true;
            this.hostVmButton.CheckedChanged += new System.EventHandler(this.directionSendButton_CheckedChanged);
            // 
            // existsBox
            // 
            this.existsBox.Controls.Add(this.existsAppendButton);
            this.existsBox.Controls.Add(this.existsKeepButton);
            this.existsBox.Controls.Add(this.existsReplaceButton);
            this.existsBox.Location = new System.Drawing.Point(22, 625);
            this.existsBox.Name = "existsBox";
            this.existsBox.Size = new System.Drawing.Size(130, 98);
            this.existsBox.TabIndex = 12;
            this.existsBox.TabStop = false;
            this.existsBox.Text = "If destination file exists";
            // 
            // existsAppendButton
            // 
            this.existsAppendButton.AutoSize = true;
            this.existsAppendButton.Location = new System.Drawing.Point(15, 65);
            this.existsAppendButton.Name = "existsAppendButton";
            this.existsAppendButton.Size = new System.Drawing.Size(82, 17);
            this.existsAppendButton.TabIndex = 2;
            this.existsAppendButton.Tag = "Append";
            this.existsAppendButton.Text = "Append to it";
            this.existsAppendButton.UseVisualStyleBackColor = true;
            this.existsAppendButton.CheckedChanged += new System.EventHandler(this.directionSendButton_CheckedChanged);
            // 
            // existsKeepButton
            // 
            this.existsKeepButton.AutoSize = true;
            this.existsKeepButton.Checked = true;
            this.existsKeepButton.Location = new System.Drawing.Point(15, 19);
            this.existsKeepButton.Name = "existsKeepButton";
            this.existsKeepButton.Size = new System.Drawing.Size(58, 17);
            this.existsKeepButton.TabIndex = 0;
            this.existsKeepButton.TabStop = true;
            this.existsKeepButton.Tag = "Keep";
            this.existsKeepButton.Text = "Keep it";
            this.existsKeepButton.UseVisualStyleBackColor = true;
            this.existsKeepButton.CheckedChanged += new System.EventHandler(this.directionSendButton_CheckedChanged);
            // 
            // existsReplaceButton
            // 
            this.existsReplaceButton.AutoSize = true;
            this.existsReplaceButton.Location = new System.Drawing.Point(15, 42);
            this.existsReplaceButton.Name = "existsReplaceButton";
            this.existsReplaceButton.Size = new System.Drawing.Size(73, 17);
            this.existsReplaceButton.TabIndex = 1;
            this.existsReplaceButton.Tag = "Replace";
            this.existsReplaceButton.Text = "Replace it";
            this.existsReplaceButton.UseVisualStyleBackColor = true;
            this.existsReplaceButton.CheckedChanged += new System.EventHandler(this.directionSendButton_CheckedChanged);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.CheckFileExists = false;
            this.openFileDialog1.Title = "Local file";
            this.openFileDialog1.FileOk += new System.ComponentModel.CancelEventHandler(this.openFileDialog1_FileOk);
            // 
            // recfmBox
            // 
            this.recfmBox.Controls.Add(this.lreclTextBox);
            this.recfmBox.Controls.Add(this.lreclLabel);
            this.recfmBox.Controls.Add(this.recfmDefaultButton);
            this.recfmBox.Controls.Add(this.recfmUndefinedButton);
            this.recfmBox.Controls.Add(this.recfmFixedButton);
            this.recfmBox.Controls.Add(this.recfmVariableButton);
            this.recfmBox.Location = new System.Drawing.Point(311, 527);
            this.recfmBox.Name = "recfmBox";
            this.recfmBox.Size = new System.Drawing.Size(145, 196);
            this.recfmBox.TabIndex = 15;
            this.recfmBox.TabStop = false;
            this.recfmBox.Text = "Record format";
            // 
            // lreclTextBox
            // 
            this.lreclTextBox.Enabled = false;
            this.lreclTextBox.Location = new System.Drawing.Point(18, 155);
            this.lreclTextBox.Name = "lreclTextBox";
            this.lreclTextBox.Size = new System.Drawing.Size(116, 20);
            this.lreclTextBox.TabIndex = 5;
            // 
            // lreclLabel
            // 
            this.lreclLabel.AutoSize = true;
            this.lreclLabel.Enabled = false;
            this.lreclLabel.Location = new System.Drawing.Point(15, 139);
            this.lreclLabel.Name = "lreclLabel";
            this.lreclLabel.Size = new System.Drawing.Size(106, 13);
            this.lreclLabel.TabIndex = 0;
            this.lreclLabel.Text = "Logical record length";
            // 
            // recfmDefaultButton
            // 
            this.recfmDefaultButton.AutoSize = true;
            this.recfmDefaultButton.Checked = true;
            this.recfmDefaultButton.Location = new System.Drawing.Point(15, 19);
            this.recfmDefaultButton.Name = "recfmDefaultButton";
            this.recfmDefaultButton.Size = new System.Drawing.Size(59, 17);
            this.recfmDefaultButton.TabIndex = 1;
            this.recfmDefaultButton.TabStop = true;
            this.recfmDefaultButton.Tag = "Default";
            this.recfmDefaultButton.Text = "Default";
            this.recfmDefaultButton.UseVisualStyleBackColor = true;
            this.recfmDefaultButton.CheckedChanged += new System.EventHandler(this.refcmDefaultButton_CheckedChanged);
            // 
            // recfmUndefinedButton
            // 
            this.recfmUndefinedButton.AutoSize = true;
            this.recfmUndefinedButton.Location = new System.Drawing.Point(15, 88);
            this.recfmUndefinedButton.Name = "recfmUndefinedButton";
            this.recfmUndefinedButton.Size = new System.Drawing.Size(74, 17);
            this.recfmUndefinedButton.TabIndex = 4;
            this.recfmUndefinedButton.Tag = "Undefined";
            this.recfmUndefinedButton.Text = "Undefined";
            this.recfmUndefinedButton.UseVisualStyleBackColor = true;
            // 
            // recfmFixedButton
            // 
            this.recfmFixedButton.AutoSize = true;
            this.recfmFixedButton.Location = new System.Drawing.Point(15, 42);
            this.recfmFixedButton.Name = "recfmFixedButton";
            this.recfmFixedButton.Size = new System.Drawing.Size(50, 17);
            this.recfmFixedButton.TabIndex = 2;
            this.recfmFixedButton.Tag = "Fixed";
            this.recfmFixedButton.Text = "Fixed";
            this.recfmFixedButton.UseVisualStyleBackColor = true;
            // 
            // recfmVariableButton
            // 
            this.recfmVariableButton.AutoSize = true;
            this.recfmVariableButton.Location = new System.Drawing.Point(15, 65);
            this.recfmVariableButton.Name = "recfmVariableButton";
            this.recfmVariableButton.Size = new System.Drawing.Size(63, 17);
            this.recfmVariableButton.TabIndex = 3;
            this.recfmVariableButton.Tag = "Variable";
            this.recfmVariableButton.Text = "Variable";
            this.recfmVariableButton.UseVisualStyleBackColor = true;
            // 
            // tsoAllocationBox
            // 
            this.tsoAllocationBox.Controls.Add(this.avblockTextBox);
            this.tsoAllocationBox.Controls.Add(this.secondarySpaceTextBox);
            this.tsoAllocationBox.Controls.Add(this.primarySpaceTextBox);
            this.tsoAllocationBox.Controls.Add(this.avblockLabel);
            this.tsoAllocationBox.Controls.Add(this.allocTracksButton);
            this.tsoAllocationBox.Controls.Add(this.allocCylindersButton);
            this.tsoAllocationBox.Controls.Add(this.allocAvblockButton);
            this.tsoAllocationBox.Controls.Add(this.label8);
            this.tsoAllocationBox.Controls.Add(this.label7);
            this.tsoAllocationBox.Location = new System.Drawing.Point(462, 440);
            this.tsoAllocationBox.Name = "tsoAllocationBox";
            this.tsoAllocationBox.Size = new System.Drawing.Size(148, 283);
            this.tsoAllocationBox.TabIndex = 16;
            this.tsoAllocationBox.TabStop = false;
            this.tsoAllocationBox.Text = "TSO file allocation";
            // 
            // avblockTextBox
            // 
            this.avblockTextBox.Enabled = false;
            this.avblockTextBox.Location = new System.Drawing.Point(19, 242);
            this.avblockTextBox.Name = "avblockTextBox";
            this.avblockTextBox.Size = new System.Drawing.Size(116, 20);
            this.avblockTextBox.TabIndex = 6;
            // 
            // secondarySpaceTextBox
            // 
            this.secondarySpaceTextBox.Location = new System.Drawing.Point(20, 94);
            this.secondarySpaceTextBox.Name = "secondarySpaceTextBox";
            this.secondarySpaceTextBox.Size = new System.Drawing.Size(116, 20);
            this.secondarySpaceTextBox.TabIndex = 2;
            // 
            // primarySpaceTextBox
            // 
            this.primarySpaceTextBox.Location = new System.Drawing.Point(19, 48);
            this.primarySpaceTextBox.Name = "primarySpaceTextBox";
            this.primarySpaceTextBox.Size = new System.Drawing.Size(116, 20);
            this.primarySpaceTextBox.TabIndex = 1;
            // 
            // avblockLabel
            // 
            this.avblockLabel.AutoSize = true;
            this.avblockLabel.Enabled = false;
            this.avblockLabel.Location = new System.Drawing.Point(16, 226);
            this.avblockLabel.Name = "avblockLabel";
            this.avblockLabel.Size = new System.Drawing.Size(93, 13);
            this.avblockLabel.TabIndex = 0;
            this.avblockLabel.Text = "Bytes per Avblock";
            // 
            // allocTracksButton
            // 
            this.allocTracksButton.AutoSize = true;
            this.allocTracksButton.Checked = true;
            this.allocTracksButton.Location = new System.Drawing.Point(18, 133);
            this.allocTracksButton.Name = "allocTracksButton";
            this.allocTracksButton.Size = new System.Drawing.Size(58, 17);
            this.allocTracksButton.TabIndex = 3;
            this.allocTracksButton.TabStop = true;
            this.allocTracksButton.Tag = "Tracks";
            this.allocTracksButton.Text = "Tracks";
            this.allocTracksButton.UseVisualStyleBackColor = true;
            this.allocTracksButton.CheckedChanged += new System.EventHandler(this.allocTracksButton_CheckedChanged);
            // 
            // allocCylindersButton
            // 
            this.allocCylindersButton.AutoSize = true;
            this.allocCylindersButton.Location = new System.Drawing.Point(18, 156);
            this.allocCylindersButton.Name = "allocCylindersButton";
            this.allocCylindersButton.Size = new System.Drawing.Size(67, 17);
            this.allocCylindersButton.TabIndex = 4;
            this.allocCylindersButton.Tag = "Cylinders";
            this.allocCylindersButton.Text = "Cylinders";
            this.allocCylindersButton.UseVisualStyleBackColor = true;
            this.allocCylindersButton.CheckedChanged += new System.EventHandler(this.allocTracksButton_CheckedChanged);
            // 
            // allocAvblockButton
            // 
            this.allocAvblockButton.AutoSize = true;
            this.allocAvblockButton.Location = new System.Drawing.Point(18, 179);
            this.allocAvblockButton.Name = "allocAvblockButton";
            this.allocAvblockButton.Size = new System.Drawing.Size(64, 17);
            this.allocAvblockButton.TabIndex = 5;
            this.allocAvblockButton.Tag = "Avblock";
            this.allocAvblockButton.Text = "Avblock";
            this.allocAvblockButton.UseVisualStyleBackColor = true;
            this.allocAvblockButton.CheckedChanged += new System.EventHandler(this.allocTracksButton_CheckedChanged);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(17, 78);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(90, 13);
            this.label8.TabIndex = 0;
            this.label8.Text = "Secondary space";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(15, 33);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(73, 13);
            this.label7.TabIndex = 0;
            this.label7.Text = "Primary space";
            // 
            // transferButton
            // 
            this.transferButton.ForeColor = System.Drawing.Color.Green;
            this.transferButton.Location = new System.Drawing.Point(522, 311);
            this.transferButton.Name = "transferButton";
            this.transferButton.Size = new System.Drawing.Size(76, 38);
            this.transferButton.TabIndex = 18;
            this.transferButton.Text = "Transfer";
            this.transferButton.UseVisualStyleBackColor = true;
            this.transferButton.Click += new System.EventHandler(this.transferButton_Click);
            // 
            // fileBrowseButton
            // 
            this.fileBrowseButton.Location = new System.Drawing.Point(222, 19);
            this.fileBrowseButton.Name = "fileBrowseButton";
            this.fileBrowseButton.Size = new System.Drawing.Size(53, 23);
            this.fileBrowseButton.TabIndex = 2;
            this.fileBrowseButton.Text = "Browse";
            this.fileBrowseButton.UseVisualStyleBackColor = true;
            this.fileBrowseButton.Click += new System.EventHandler(this.fileBrowseButton_Click);
            // 
            // filesBox
            // 
            this.filesBox.Controls.Add(this.fileBrowseButton);
            this.filesBox.Controls.Add(this.localFileLabel);
            this.filesBox.Controls.Add(this.hostFileLabel);
            this.filesBox.Controls.Add(this.localFileTextBox);
            this.filesBox.Controls.Add(this.hostFileTextBox);
            this.filesBox.Location = new System.Drawing.Point(24, 440);
            this.filesBox.Name = "filesBox";
            this.filesBox.Size = new System.Drawing.Size(281, 81);
            this.filesBox.TabIndex = 10;
            this.filesBox.TabStop = false;
            this.filesBox.Text = "Files";
            // 
            // sizesBox
            // 
            this.sizesBox.Controls.Add(this.bufferSizeTextBox);
            this.sizesBox.Controls.Add(this.blockSizeTextBox);
            this.sizesBox.Controls.Add(this.label11);
            this.sizesBox.Controls.Add(this.blockSizeLabel);
            this.sizesBox.Location = new System.Drawing.Point(22, 727);
            this.sizesBox.Name = "sizesBox";
            this.sizesBox.Size = new System.Drawing.Size(434, 46);
            this.sizesBox.TabIndex = 17;
            this.sizesBox.TabStop = false;
            // 
            // bufferSizeTextBox
            // 
            this.bufferSizeTextBox.Location = new System.Drawing.Point(279, 16);
            this.bufferSizeTextBox.Name = "bufferSizeTextBox";
            this.bufferSizeTextBox.Size = new System.Drawing.Size(116, 20);
            this.bufferSizeTextBox.TabIndex = 2;
            // 
            // blockSizeTextBox
            // 
            this.blockSizeTextBox.Location = new System.Drawing.Point(77, 16);
            this.blockSizeTextBox.Name = "blockSizeTextBox";
            this.blockSizeTextBox.Size = new System.Drawing.Size(116, 20);
            this.blockSizeTextBox.TabIndex = 1;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(217, 19);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(56, 13);
            this.label11.TabIndex = 0;
            this.label11.Text = "Buffer size";
            // 
            // blockSizeLabel
            // 
            this.blockSizeLabel.AutoSize = true;
            this.blockSizeLabel.Location = new System.Drawing.Point(16, 19);
            this.blockSizeLabel.Name = "blockSizeLabel";
            this.blockSizeLabel.Size = new System.Drawing.Size(55, 13);
            this.blockSizeLabel.TabIndex = 0;
            this.blockSizeLabel.Text = "Block size";
            // 
            // queryTextBox
            // 
            this.queryTextBox.Location = new System.Drawing.Point(9, 35);
            this.queryTextBox.MaxLength = 32768;
            this.queryTextBox.Name = "queryTextBox";
            this.queryTextBox.Size = new System.Drawing.Size(516, 20);
            this.queryTextBox.TabIndex = 6;
            this.queryTextBox.Text = "LISTFILE";
            this.queryTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.queryTextBox_KeyPress);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(6, 19);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(54, 13);
            this.label5.TabIndex = 18;
            this.label5.Text = "Command";
            // 
            // queryGroupBox
            // 
            this.queryGroupBox.Controls.Add(this.label5);
            this.queryGroupBox.Controls.Add(this.queryTextBox);
            this.queryGroupBox.Controls.Add(this.runQueryButton);
            this.queryGroupBox.Controls.Add(this.panel1);
            this.queryGroupBox.Location = new System.Drawing.Point(12, 183);
            this.queryGroupBox.Name = "queryGroupBox";
            this.queryGroupBox.Size = new System.Drawing.Size(614, 233);
            this.queryGroupBox.TabIndex = 8;
            this.queryGroupBox.TabStop = false;
            this.queryGroupBox.Text = "Query";
            // 
            // fileTransferBox
            // 
            this.fileTransferBox.Controls.Add(this.transferButton);
            this.fileTransferBox.Location = new System.Drawing.Point(12, 422);
            this.fileTransferBox.Name = "fileTransferBox";
            this.fileTransferBox.Size = new System.Drawing.Size(614, 367);
            this.fileTransferBox.TabIndex = 19;
            this.fileTransferBox.TabStop = false;
            this.fileTransferBox.Text = "File Transfer";
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "green_light.png");
            // 
            // loggedOnPictureBox
            // 
            this.loggedOnPictureBox.Image = global::x3270ifGuiTest.Properties.Resources.red_light;
            this.loggedOnPictureBox.Location = new System.Drawing.Point(432, 63);
            this.loggedOnPictureBox.Name = "loggedOnPictureBox";
            this.loggedOnPictureBox.Size = new System.Drawing.Size(20, 20);
            this.loggedOnPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.loggedOnPictureBox.TabIndex = 23;
            this.loggedOnPictureBox.TabStop = false;
            // 
            // connectedPictureBox
            // 
            this.connectedPictureBox.Image = global::x3270ifGuiTest.Properties.Resources.red_light;
            this.connectedPictureBox.Location = new System.Drawing.Point(432, 39);
            this.connectedPictureBox.Name = "connectedPictureBox";
            this.connectedPictureBox.Size = new System.Drawing.Size(20, 20);
            this.connectedPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.connectedPictureBox.TabIndex = 22;
            this.connectedPictureBox.TabStop = false;
            // 
            // startedPictureBox
            // 
            this.startedPictureBox.Image = global::x3270ifGuiTest.Properties.Resources.red_light;
            this.startedPictureBox.Location = new System.Drawing.Point(432, 15);
            this.startedPictureBox.Name = "startedPictureBox";
            this.startedPictureBox.Size = new System.Drawing.Size(20, 20);
            this.startedPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.startedPictureBox.TabIndex = 21;
            this.startedPictureBox.TabStop = false;
            // 
            // screenLabel
            // 
            this.screenLabel.BackColor = System.Drawing.Color.Black;
            this.screenLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.screenLabel.Font = new System.Drawing.Font("Consolas", 1.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.screenLabel.ForeColor = System.Drawing.Color.LightGreen;
            this.screenLabel.Location = new System.Drawing.Point(332, 12);
            this.screenLabel.Name = "screenLabel";
            this.screenLabel.Size = new System.Drawing.Size(85, 137);
            this.screenLabel.TabIndex = 24;
            // 
            // x3270ifGuiTest
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(633, 796);
            this.Controls.Add(this.screenLabel);
            this.Controls.Add(this.loggedOnPictureBox);
            this.Controls.Add(this.connectedPictureBox);
            this.Controls.Add(this.startedPictureBox);
            this.Controls.Add(this.connectedLabel);
            this.Controls.Add(this.loggedOnLabel);
            this.Controls.Add(this.startedLabel);
            this.Controls.Add(this.queryGroupBox);
            this.Controls.Add(this.sizesBox);
            this.Controls.Add(this.tsoAllocationBox);
            this.Controls.Add(this.recfmBox);
            this.Controls.Add(this.existsBox);
            this.Controls.Add(this.hostTypeBox);
            this.Controls.Add(this.modeBox);
            this.Controls.Add(this.directionBox);
            this.Controls.Add(this.stopButton);
            this.Controls.Add(this.quitButton);
            this.Controls.Add(this.stateLabel);
            this.Controls.Add(this.passwordTextBox);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.usernameTextBox);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.secureCheckBox);
            this.Controls.Add(this.portTextBox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.hostnameTextBox);
            this.Controls.Add(this.filesBox);
            this.Controls.Add(this.fileTransferBox);
            this.Name = "x3270ifGuiTest";
            this.Text = "x3270if GUI Test";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.x3270ifGuiTest_FormClosing);
            this.Load += new System.EventHandler(this.x3270ifGuiTest_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.directionBox.ResumeLayout(false);
            this.directionBox.PerformLayout();
            this.modeBox.ResumeLayout(false);
            this.modeBox.PerformLayout();
            this.asciiBox.ResumeLayout(false);
            this.asciiBox.PerformLayout();
            this.hostTypeBox.ResumeLayout(false);
            this.hostTypeBox.PerformLayout();
            this.existsBox.ResumeLayout(false);
            this.existsBox.PerformLayout();
            this.recfmBox.ResumeLayout(false);
            this.recfmBox.PerformLayout();
            this.tsoAllocationBox.ResumeLayout(false);
            this.tsoAllocationBox.PerformLayout();
            this.filesBox.ResumeLayout(false);
            this.filesBox.PerformLayout();
            this.sizesBox.ResumeLayout(false);
            this.sizesBox.PerformLayout();
            this.queryGroupBox.ResumeLayout(false);
            this.queryGroupBox.PerformLayout();
            this.fileTransferBox.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.loggedOnPictureBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.connectedPictureBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.startedPictureBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox hostnameTextBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox portTextBox;
        private System.Windows.Forms.CheckBox secureCheckBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox usernameTextBox;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox passwordTextBox;
        private System.Windows.Forms.Button runQueryButton;
        private System.Windows.Forms.Label stateLabel;
        private System.ComponentModel.BackgroundWorker backgroundWorker1;
        private System.Windows.Forms.Button quitButton;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label resultLabel;
        private System.Windows.Forms.Label loggedOnLabel;
        private System.Windows.Forms.Label startedLabel;
        private System.Windows.Forms.Label connectedLabel;
        private System.Windows.Forms.Button stopButton;
        private System.Windows.Forms.Label localFileLabel;
        private System.Windows.Forms.TextBox localFileTextBox;
        private System.Windows.Forms.Label hostFileLabel;
        private System.Windows.Forms.TextBox hostFileTextBox;
        private System.Windows.Forms.RadioButton directionSendButton;
        private System.Windows.Forms.RadioButton directionReceiveButton;
        private System.Windows.Forms.GroupBox directionBox;
        private System.Windows.Forms.GroupBox modeBox;
        private System.Windows.Forms.RadioButton modeAsciiButton;
        private System.Windows.Forms.RadioButton modeBinaryButton;
        private System.Windows.Forms.GroupBox hostTypeBox;
        private System.Windows.Forms.RadioButton hostCicsButton;
        private System.Windows.Forms.RadioButton hostTsoButton;
        private System.Windows.Forms.RadioButton hostVmButton;
        private System.Windows.Forms.GroupBox existsBox;
        private System.Windows.Forms.RadioButton existsAppendButton;
        private System.Windows.Forms.RadioButton existsKeepButton;
        private System.Windows.Forms.RadioButton existsReplaceButton;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.CheckBox crCheckBox;
        private System.Windows.Forms.CheckBox remapCheckBox;
        private System.Windows.Forms.Label windowsCodePageLabel;
        private System.Windows.Forms.GroupBox recfmBox;
        private System.Windows.Forms.Label lreclLabel;
        private System.Windows.Forms.RadioButton recfmDefaultButton;
        private System.Windows.Forms.RadioButton recfmUndefinedButton;
        private System.Windows.Forms.RadioButton recfmFixedButton;
        private System.Windows.Forms.RadioButton recfmVariableButton;
        private System.Windows.Forms.GroupBox asciiBox;
        private System.Windows.Forms.GroupBox tsoAllocationBox;
        private System.Windows.Forms.Label avblockLabel;
        private System.Windows.Forms.RadioButton allocTracksButton;
        private System.Windows.Forms.RadioButton allocCylindersButton;
        private System.Windows.Forms.RadioButton allocAvblockButton;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Button transferButton;
        private System.Windows.Forms.Button fileBrowseButton;
        private System.Windows.Forms.GroupBox filesBox;
        private System.Windows.Forms.GroupBox sizesBox;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label blockSizeLabel;
        private System.Windows.Forms.TextBox windowsCodePageTextBox;
        private System.Windows.Forms.TextBox lreclTextBox;
        private System.Windows.Forms.TextBox avblockTextBox;
        private System.Windows.Forms.TextBox secondarySpaceTextBox;
        private System.Windows.Forms.TextBox primarySpaceTextBox;
        private System.Windows.Forms.TextBox bufferSizeTextBox;
        private System.Windows.Forms.TextBox blockSizeTextBox;
        private System.Windows.Forms.TextBox queryTextBox;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.GroupBox queryGroupBox;
        private System.Windows.Forms.GroupBox fileTransferBox;
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.PictureBox startedPictureBox;
        private System.Windows.Forms.PictureBox connectedPictureBox;
        private System.Windows.Forms.PictureBox loggedOnPictureBox;
        private System.Windows.Forms.Label screenLabel;
    }
}

