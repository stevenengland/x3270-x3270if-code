// Copyright (c) 2015 Paul Mattes.
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//     * Redistributions of source code must retain the above copyright
//       notice, this list of conditions and the following disclaimer.
//     * Redistributions in binary form must reproduce the above copyright
//       notice, this list of conditions and the following disclaimer in the
//       documentation and/or other materials provided with the distribution.
//     * Neither the names of Paul Mattes nor the names of his contributors
//       may be used to endorse or promote products derived from this software
//       without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY PAUL MATTES "AS IS" AND ANY EXPRESS OR IMPLIED
// WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
// MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
// EVENT SHALL PAUL MATTES BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
// SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
// PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS;
// OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
// WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR
// OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
// ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

// GUI app to exercise the x3270if DLL.
namespace x3270ifGuiTest
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data;
    using System.Drawing;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows.Forms;
    using System.Text.RegularExpressions;
    using System.Timers;
    using X3270if;
    using X3270if.Transfer;
    using X3270if.ProcessOptions;

    /// <summary>
    /// GUI test for x3270if.
    /// </summary>
    public partial class x3270ifGuiTest : Form
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="x3270ifGuiTest"/> class.
        /// </summary>
        public x3270ifGuiTest()
        {
            InitializeComponent();
        }

        #region GUI thread event handlers
        private void x3270ifGuiTest_Load(object sender, EventArgs e)
        {
            stateLabel.Text = string.Empty;
            resultLabel.Text = string.Empty;

            // Set up the idle timer.
            timer.Elapsed += timer_Elapsed;
            timer.Interval = 15000;
            timer.Enabled = false;
        }

        void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            this.Invoke(new MethodInvoker(() => backgroundWorker1.RunWorkerAsync(QueryAction.Timeout)));
        }

        private void runQueryButton_Click(object sender, EventArgs e)
        {
            if (!backgroundWorker1.IsBusy)
            {
                backgroundWorker1.RunWorkerAsync(QueryAction.StartQuery);
            }
        }

        private void stopButton_Click(object sender, EventArgs e)
        {
            if (!backgroundWorker1.IsBusy)
            {
                backgroundWorker1.RunWorkerAsync(QueryAction.Stop);
            }
            else
            {
                backgroundWorker1.CancelAsync();
            }
        }

        private void quitButton_Click(object sender, EventArgs e)
        {
            if (!backgroundWorker1.IsBusy)
            {
                // Have the background worker kill us
                backgroundWorker1.RunWorkerAsync(QueryAction.Quit);
            }
            else
            {
                // Cancel the background, and quit when it completes.
                quitting = true;
                backgroundWorker1.CancelAsync();
            }
        }

        private void x3270ifGuiTest_FormClosing(object sender, FormClosingEventArgs e)
        {
            quitButton_Click(sender, e);
        }
        #endregion

        private void fileBrowseButton_Click(object sender, EventArgs e)
        {
            openFileDialog1.CheckFileExists = directionSendButton.Checked;
            openFileDialog1.FileName = localFileTextBox.Text;
            openFileDialog1.ShowDialog();
        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            localFileTextBox.Text = openFileDialog1.FileName;
        }

        private void modeBinaryButton_CheckedChanged(object sender, EventArgs e)
        {
            asciiBox.Enabled = modeAsciiButton.Checked;
        }

        private void remapCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            windowsCodePageLabel.Enabled = remapCheckBox.Checked;
            windowsCodePageTextBox.Enabled = remapCheckBox.Checked;
        }


        private void directionSendButton_CheckedChanged(object sender, EventArgs e)
        {
            recfmBox.Enabled = directionSendButton.Checked &&
                               !hostCicsButton.Checked &&
                               !existsAppendButton.Checked;
            tsoAllocationBox.Enabled = directionSendButton.Checked &&
                                       hostTsoButton.Checked &&
                                       !existsAppendButton.Checked;
            blockSizeLabel.Enabled = blockSizeTextBox.Enabled = hostTsoButton.Checked;
        }

        private void allocTracksButton_CheckedChanged(object sender, EventArgs e)
        {
            avblockLabel.Enabled = avblockTextBox.Enabled = allocAvblockButton.Checked;
        }

        private void refcmDefaultButton_CheckedChanged(object sender, EventArgs e)
        {
            lreclLabel.Enabled = lreclTextBox.Enabled = !recfmDefaultButton.Checked;
        }

        private void transferButton_Click(object sender, EventArgs e)
        {
            if (!backgroundWorker1.IsBusy)
            {
                backgroundWorker1.RunWorkerAsync(QueryAction.StartTransfer);
            }

        }

        private void queryTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r')
            {
                runQueryButton_Click(sender, e);
                e.Handled = true;
            }
        }
    }
}
