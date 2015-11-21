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
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using System.Timers;
    using System.Windows.Forms;

    using X3270if;
    using X3270if.ProcessOptions;
    using X3270if.Transfer;

    /// <summary>
    /// GUI test for X3270if.
    /// This is the real meat of the form, and is not generated, so it can be subject to StyleCop checks.
    /// </summary>
    public partial class x3270ifGuiTest : Form
    {
        /// <summary>
        /// The ws3270 session.
        /// </summary>
        private Session session;

        /// <summary>
        /// True if we are logged on.
        /// </summary>
        private bool loggedOn;

        /// <summary>
        /// The most recent display buffer.
        /// </summary>
        private DisplayBuffer displayBuffer;

        /// <summary>
        /// The ASCII decode of <see cref="displayBuffer"/>.
        /// </summary>
        private string[] ascii;

        /// <summary>
        /// True if we are exiting the application.
        /// </summary>
        private bool quitting;

        /// <summary>
        /// The idle timeout, used to tear down the logon session after inactivity.
        /// </summary>
        private System.Timers.Timer timer = new System.Timers.Timer();

        /// <summary>
        /// Actions for the background worker process
        /// </summary>
        private enum QueryAction
        {
            /// <summary>
            /// Start running the query.
            /// </summary>
            StartQuery,

            /// <summary>
            /// Start a file transfer.
            /// </summary>
            StartTransfer,

            /// <summary>
            /// Time out the session.
            /// </summary>
            Timeout,

            /// <summary>
            /// Stop the session.
            /// </summary>
            Stop,

            /// <summary>
            /// Stop the app.
            /// </summary>
            Quit
        }

        /// <summary>
        /// Worker status indications.
        /// </summary>
        private enum WorkerStatusIndication
        {
            /// <summary>
            /// Initial state.
            /// </summary>
            Idle,

            /// <summary>
            /// Waiting for host output.
            /// </summary>
            Waiting,

            /// <summary>
            /// Got host output.
            /// </summary>
            Found,

            /// <summary>
            /// An error occurred.
            /// </summary>
            Error,

            /// <summary>
            /// Query is complete.
            /// </summary>
            Complete,

            /// <summary>
            /// ws3270 running or stopped
            /// </summary>
            Running,

            /// <summary>
            /// Connected to host.
            /// </summary>
            Connected,

            /// <summary>
            /// Logged on to host.
            /// </summary>
            LoggedOn,

            /// <summary>
            /// Processing request.
            /// </summary>
            Processing,

            /// <summary>
            /// Updated screen image.
            /// </summary>
            Screen
        }

        #region Worker subroutines

        /// <summary>
        /// Downgrade the session by one step.
        /// </summary>
        /// <param name="worker">Worker context.</param>
        /// <param name="andClose">If true, close the session.</param>
        /// <returns>True if there is more to tear down.</returns>
        private bool DowngradeSession(BackgroundWorker worker, bool andClose)
        {
            if (this.session == null)
            {
                // Nothing to downgrade.
                return false;
            }

            if (this.loggedOn)
            {
                this.session.Clear();
                this.session.String("LOGOFF\n");
                this.loggedOn = false;
                worker.ReportProgress(0, new WorkerStatusLoggedOn(false));
                worker.ReportProgress(0, new WorkerStatusConnected(false));
                worker.ReportProgress(0, new WorkerStatusScreen(string.Empty));
                return true;
            }

            if (this.session.HostConnected)
            {
                this.session.Disconnect();
                worker.ReportProgress(0, new WorkerStatusConnected(false));
                return true;
            }
            else
            {
                worker.ReportProgress(0, new WorkerStatusConnected(false));
            }

            if (andClose && this.session.EmulatorRunning)
            {
                this.session.Close();
                worker.ReportProgress(0, new WorkerStatusRunning(false));
                return false;
            }

            return false;
        }

        /// <summary>
        /// Abort the session due to an error.
        /// </summary>
        /// <param name="worker">Worker context</param>
        private void AbortSession(BackgroundWorker worker)
        {
            this.session.ExceptionMode = false;
            while (this.DowngradeSession(worker, andClose: false))
            {
            }
        }

        /// <summary>
        /// Grab the image of the next screen from the host.
        /// Send the image to the GUI for display.
        /// </summary>
        /// <param name="worker">Worker context</param>
        private void NextScreen(BackgroundWorker worker)
        {
            this.displayBuffer = new DisplayBuffer(this.session.ReadBuffer());
            this.ascii = this.displayBuffer.Ascii();
            worker.ReportProgress(0, new WorkerStatusScreen(string.Join("\n", this.ascii)));
        }

        /// <summary>
        /// Scan the screen for a predicate.
        /// Checks the predicate, and if it fails, waits for output and tries again.
        /// Gives up after the specified number of seconds.
        /// </summary>
        /// <param name="worker">Worker context.</param>
        /// <param name="d">Predicate to test.</param>
        /// <param name="secs">Seconds to wait.</param>
        /// <returns>True if predicate succeeds.</returns>
        private bool RescanUntil(BackgroundWorker worker, Func<bool> d, int secs = 10)
        {
            return this.RescanUntil(worker, new List<Func<bool>> { d }, secs) >= 0;
        }

        /// <summary>
        /// Scan the screen for a predicate.
        /// Checks the predicate, and if it fails, waits for output and tries again.
        /// Gives up after the specified number of seconds.
        /// </summary>
        /// <param name="worker">Worker context.</param>
        /// <param name="d">Set of predicates.</param>
        /// <param name="secs">Seconds to wait.</param>
        /// <returns>Index of successful predicate, or -1 if it timed out.</returns>
        private int RescanUntil(BackgroundWorker worker, IEnumerable<Func<bool>> d, int secs = 10)
        {
            if (worker.CancellationPending)
            {
                return -1;
            }

            var timeout = new System.Diagnostics.Stopwatch();
            timeout.Start();

            while (true)
            {
                int ret = 0;
                foreach (var f in d)
                {
                    if (f())
                    {
                        return ret;
                    }

                    ret++;
                }

                var secondsToWait = secs - (timeout.ElapsedMilliseconds / 1000);
                if (secondsToWait <= 0 || worker.CancellationPending)
                {
                    return -1;
                }

                var result = this.session.Wait(WaitMode.Output, (int)secondsToWait);
                if (!result.Success || timeout.ElapsedMilliseconds > secs * 1000 || worker.CancellationPending)
                {
                    return -1;
                }

                this.NextScreen(worker);
            }
        }

        /// <summary>
        /// Wait until the host displays a particular string.
        /// Requires that displayBuffer is valid. Will wait for more output and update displayBuffer if necessary.
        /// Times out after 10 seconds.
        /// </summary>
        /// <param name="worker">Worker context.</param>
        /// <param name="row">Row where text needs to appear.</param>
        /// <param name="col">Column where text needs to appear.</param>
        /// <param name="text">Desired text.</param>
        /// <returns>True if text was found.</returns>
        private bool WaitForString(BackgroundWorker worker, int row, int col, string text)
        {
            if (!this.RescanUntil(worker, () => this.displayBuffer.AsciiEquals(row, col, text), 10))
            {
                worker.ReportProgress(0, new WorkerStatusError(worker, "Could not find '" + text + "'"));
                return false;
            }

            worker.ReportProgress(0, new WorkerStatusFound("'" + text + "'"));
            return true;
        }

        /// <summary>
        /// Reboot the CMS virtual machine.
        /// </summary>
        /// <param name="worker">Worker context.</param>
        /// <returns>True if CMS rebooted successfully.</returns>
        private bool RebootCMS(BackgroundWorker worker)
        {
            worker.ReportProgress(0, new WorkerStatusError("Attempting CMS reboot"));

            this.session.String("IPL CMS\n");
            if (!this.RescanUntil(worker, () => this.ScanFor(1, "z/VM"), 10))
            {
                worker.ReportProgress(0, new WorkerStatusError(worker, "Reboot failed"));
                this.AbortSession(worker);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Check the screen for the MORE... prompt.
        /// displayBuffer must be valid.
        /// </summary>
        /// <returns>True if MORE... was found.</returns>
        private bool IsMore()
        {
            return this.displayBuffer.AsciiEquals(43, 61, "MORE...");
        }

        /// <summary>
        /// Scan the screen buffer for text at the beginning of the line.
        /// </summary>
        /// <param name="startRow">First row to scan.</param>
        /// <param name="text">Text to scan for.</param>
        /// <returns>True if <paramref name="text"/> found.</returns>
        private bool ScanFor(int startRow, string text)
        {
            for (var i = startRow; i < 42; i++)
            {
                if (this.displayBuffer.AsciiEquals(i, 1, text))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Check logon parameters in the form.
        /// </summary>
        /// <param name="worker">Worker context.</param>
        /// <returns>True if logon parameters are valid.</returns>
        private bool CheckLogonFields(BackgroundWorker worker)
        {
            if (string.IsNullOrEmpty(hostnameTextBox.Text))
            {
                worker.ReportProgress(0, new WorkerStatusError("Empty hostname"));
                return false;
            }

            if (string.IsNullOrEmpty(usernameTextBox.Text))
            {
                worker.ReportProgress(0, new WorkerStatusError("Empty username"));
                return false;
            }

            if (string.IsNullOrEmpty(passwordTextBox.Text))
            {
                worker.ReportProgress(0, new WorkerStatusError("Empty password"));
                return false;
            }

            return true;
        }

        /// <summary>
        /// Log on to the host. Creates the session and connects to the host if necessary.
        /// If successful, leaves exception mode enabled on the session.
        /// </summary>
        /// <param name="worker">Worker context.</param>
        /// <param name="username">User name.</param>
        /// <returns>True if logon succeeded.</returns>
        private bool LogOn(BackgroundWorker worker, string username)
        {
            if (this.session == null)
            {
                var config = new ProcessConfig { Origin = 1, ExtraOptions = new[] { new ProcessOptionWithoutValue("trace") } };
                this.session = new ProcessSession(config);
            }

            if (!this.session.EmulatorRunning)
            {
                var startResult = this.session.Start();
                if (!startResult.Success)
                {
                    worker.ReportProgress(0, new WorkerStatusError("Connect failed: " + startResult.FailReason));
                    return false;
                }

                worker.ReportProgress(0, new WorkerStatusRunning(true));
            }

            var ioResult = new IoResult();
            try
            {
                ioResult = this.session.Connect(hostnameTextBox.Text, port: portTextBox.Text, flags: secureCheckBox.Checked ? ConnectFlags.Secure : ConnectFlags.None);
            }
            catch (ArgumentException)
            {
                ioResult.Success = false;
                ioResult.Result = new[] { "Invalid hostname or port" };
            }

            if (ioResult.Success)
            {
                worker.ReportProgress(0, new WorkerStatusConnected(true));
            }
            else
            {
                worker.ReportProgress(0, new WorkerStatusError("Connect failed: " + ioResult.Result[0]));
                return false;
            }

            // Catch errors with exceptions from here on out.
            this.session.ExceptionMode = true;
            try
            {
                // Wait for the initial logon screen.
                this.NextScreen(worker);
                if (!this.WaitForString(worker, 39, 2, "USERID"))
                {
                    this.AbortSession(worker);
                    return false;
                }

                // Fill in username and password, do an Enter.
                this.session.StringAt(
                    new[]
                    {
                        new StringAtBlock { Row = 39, Column = 17, Text = username },
                        new StringAtBlock { Row = 40, Column = 17, Text = passwordTextBox.Text + "\n" }
                    });

                this.NextScreen(worker);

                // Check for successful logon.
                switch (this.RescanUntil(
                    worker,
                    new List<Func<bool>>
                    {
                        () => this.displayBuffer.AsciiMatches(2, 1, 80, ".*not in CP directory.*"), // bad username
                        () => this.displayBuffer.AsciiMatches(2, 1, 80, ".*unsuccessful.*"),        // bad password
                        () => this.displayBuffer.AsciiMatches(2, 1, 80, ".*Already logged on.*"),   // already logged on
                        () => this.ScanFor(3, "LOGON AT"),                                          // fresh logon
                        () => this.ScanFor(3, "RECONNECTED AT")                                     // reconnect
                    }))
                {
                    case 0:
                    case 1:
                    case 2:
                        // Failed logon
                        worker.ReportProgress(0, new WorkerStatusError("Logon failed: " + this.displayBuffer.Ascii(2, 1, 80).Trim()));
                        this.AbortSession(worker);
                        return false;

                    case 3: // LOGON AT (success)
                        break;

                    case 4: // RECONNECTED (disconnected without logoff; need reboot)
                        if (!this.RebootCMS(worker))
                        {
                            return false;
                        }

                        break;

                    default: // Timed out without a match
                        worker.ReportProgress(0, new WorkerStatusError(worker, "Logon failed"));
                        this.AbortSession(worker);
                        return false;
                }

                this.loggedOn = true;
                worker.ReportProgress(0, new WorkerStatusLoggedOn(true));

                // Get ready for the command.
                this.session.Clear();
            }
            catch (X3270ifCommandException e)
            {
                // One of the commands failed, likely because something broke (network, emulator).
                worker.ReportProgress(0, new WorkerStatusError(worker, "Session error: " + e.Message));
                this.AbortSession(worker);
                return false;
            }

            return true;
        }
        #endregion

        #region Background worker handlers
        /// <summary>
        /// Start button action. Runs the query.
        /// </summary>
        /// <param name="worker">Worker context.</param>
        private void DoStartQuery(BackgroundWorker worker)
        {
            System.Diagnostics.Stopwatch overall = new System.Diagnostics.Stopwatch();
            overall.Start();

            worker.ReportProgress(100, new WorkerStatusIdle());

            // Make sure we have the fields we need.
            if (!this.CheckLogonFields(worker))
            {
                return;
            }

            var username = usernameTextBox.Text.ToUpper();
            var lines = new List<string>();

            // Do the rest of the work catching command errors (socket or emulator process failures).
            try
            {
                var wasLoggedOn = this.loggedOn;
                if (!this.loggedOn)
                {
                    if (!this.LogOn(worker, username))
                    {
                        // Leaves exception mode on.
                        return;
                    }
                }
                else
                {
                    this.session.ExceptionMode = true;
                    this.session.Clear();
                }

                // Send the command we want to capture.
                string query = queryTextBox.Text;
                if (string.IsNullOrEmpty(query))
                {
                    query = "LISTFILE";
                }

                query = query.ToUpper();
                this.session.String(query + "\n");

                //// This will (artificially) trigger an X3270ifCommandException.
                // session.Io("woof");

                // Look for:
                //  query echoed on line 1
                //   DMS on line 2
                //   USERNAME Ready; on line 3
                //  file names dribbling out
                //   a second USERNAME Ready; somewhere, -or- MORE... at 43,61
                //  If MORE..., hit clear and repeat until we get USERNAME Ready;
                var ready = " " + username + " Ready;";
                this.NextScreen(worker);

                if (!this.WaitForString(worker, 1, 1, query) ||
                    (!wasLoggedOn && (!this.WaitForString(worker, 1, 1, query) ||
                                      !this.WaitForString(worker, 2, 1, "DMS") ||
                                      !this.WaitForString(worker, 3, 1, ready))))
                {
                    worker.ReportProgress(0, new WorkerStatusError(worker, "Command failed"));
                    this.AbortSession(worker);
                    return;
                }

                var dataRow = wasLoggedOn ? 2 : 4;
                bool first = true;
                do
                {
                    // Snap the screen.
                    this.NextScreen(worker);

                    // If the MORE... prompt is up, clear the screen and start scanning at the top.
                    if (!first && this.IsMore())
                    {
                        this.session.Clear();
                        dataRow = 1;
                        this.NextScreen(worker);
                    }

                    first = false;

                    // Wait for the concluding prompt or MORE...
                    if (!this.RescanUntil(worker, () => this.ScanFor(dataRow, ready) || this.IsMore(), 30))
                    {
                        this.AbortSession(worker);
                        worker.ReportProgress(0, new WorkerStatusError(worker, "Result not found"));
                        return;
                    }

                    // Collect the file names.
                    for (var i = dataRow; i < 42; i++)
                    {
                        if (this.displayBuffer.AsciiEquals(i, 1, ready))
                        {
                            break;
                        }

                        var filename = this.displayBuffer.Ascii(i, 1, 80).Trim();
                        if (!string.IsNullOrEmpty(filename))
                        {
                            lines.Add(filename);
                            worker.ReportProgress(0, new WorkerStatusFound(lines.Count.ToString() + " lines"));
                        }
                    }
                }
                while (this.IsMore());
            }
            catch (X3270ifCommandException e)
            {
                this.AbortSession(worker);
                worker.ReportProgress(0, new WorkerStatusError(worker, "Session failure: " + e.Message));
                return;
            }
            finally
            {
                // All done with exceptions.
                this.session.ExceptionMode = false;
            }

            // Report results.
            overall.Stop();
            worker.ReportProgress(
                100,
                new WorkerStatusComplete(
                    string.Format("Done: {0} lines in {1}", lines.Count, overall.Elapsed),
                    string.Join("\n", lines.ToArray())));
        }

        /// <summary>
        /// Find the checked RadioButton in a GroupBox and return its tag as an enumeration.
        /// </summary>
        /// <param name="groupBox">Box to search; must contain <see cref="RadioButton"/>s.</param>
        /// <typeparam name="T">Type of enumeration</typeparam>
        /// <returns>Enumeration value, or null.</returns>
        private T? CheckedTag<T>(GroupBox groupBox) where T : struct, IConvertible
        {
            foreach (var control in groupBox.Controls)
            {
                var radioButton = control as RadioButton;
                if (radioButton != null && radioButton.Checked)
                {
                    return (T)Enum.Parse(typeof(T), (string)radioButton.Tag);
                }
            }

            return null;
        }

        /// <summary>
        /// Transfer button action. Runs the transfer.
        /// </summary>
        /// <param name="worker">Worker context.</param>
        private void DoStartTransfer(BackgroundWorker worker)
        {
            worker.ReportProgress(100, new WorkerStatusIdle());

            // Make sure we have the fields we need.
            if (!this.CheckLogonFields(worker))
            {
                return;
            }

            if (string.IsNullOrEmpty(localFileTextBox.Text))
            {
                worker.ReportProgress(0, new WorkerStatusError("Empty local file name"));
                return;
            }

            if (string.IsNullOrEmpty(hostFileTextBox.Text))
            {
                worker.ReportProgress(0, new WorkerStatusError("Empty host file name"));
                return;
            }

            uint codePage = 0;
            if (windowsCodePageTextBox.Enabled &&
                !string.IsNullOrEmpty(windowsCodePageTextBox.Text) &&
                (!uint.TryParse(windowsCodePageTextBox.Text, out codePage) || codePage == 0))
            {
                worker.ReportProgress(0, new WorkerStatusError("Invalid Windows code page"));
                return;
            }

            uint lrecl = 0;
            if (lreclTextBox.Enabled &&
                !string.IsNullOrEmpty(lreclTextBox.Text) &&
                (!uint.TryParse(lreclTextBox.Text, out lrecl) || lrecl == 0))
            {
                worker.ReportProgress(0, new WorkerStatusError("Invalid logical record length"));
                return;
            }

            uint primarySpace = 0;
            if (primarySpaceTextBox.Enabled &&
                !string.IsNullOrEmpty(primarySpaceTextBox.Text) &&
                (!uint.TryParse(primarySpaceTextBox.Text, out primarySpace) || primarySpace == 0))
            {
                worker.ReportProgress(0, new WorkerStatusError("Invalid primary space"));
                return;
            }

            uint secondarySpace = 0;
            if (secondarySpaceTextBox.Enabled && !string.IsNullOrEmpty(secondarySpaceTextBox.Text))
            {
                if (string.IsNullOrEmpty(primarySpaceTextBox.Text))
                {
                    worker.ReportProgress(0, new WorkerStatusError("Missing primary space"));
                    return;
                }

                if (!uint.TryParse(secondarySpaceTextBox.Text, out secondarySpace) || secondarySpace == 0)
                {
                    worker.ReportProgress(0, new WorkerStatusError("Invalid secondary space"));
                    return;
                }
            }

            uint avblock = 0;
            if (avblockTextBox.Enabled &&
                !string.IsNullOrEmpty(avblockTextBox.Text) &&
                (!uint.TryParse(avblockTextBox.Text, out avblock) || avblock == 0))
            {
                worker.ReportProgress(0, new WorkerStatusError("Invalid avblock"));
                return;
            }

            uint blockSize = 0;
            if (blockSizeTextBox.Enabled &&
                !string.IsNullOrEmpty(blockSizeTextBox.Text) &&
                (!uint.TryParse(blockSizeTextBox.Text, out blockSize) || blockSize == 0))
            {
                worker.ReportProgress(0, new WorkerStatusError("Invalid block size"));
                return;
            }

            uint bufferSize = 0;
            if (!string.IsNullOrEmpty(bufferSizeTextBox.Text) &&
                (!uint.TryParse(bufferSizeTextBox.Text, out bufferSize) || bufferSize == 0))
            {
                worker.ReportProgress(0, new WorkerStatusError("Invalid buffer size"));
                return;
            }

            System.Diagnostics.Stopwatch overall = new System.Diagnostics.Stopwatch();
            overall.Start();

            // Log in
            var username = usernameTextBox.Text.ToUpper();
            bool wasLoggedOn = this.loggedOn;

            try
            {
                if (!this.loggedOn)
                {
                    if (!this.LogOn(worker, username))
                    {
                        // Leaves exception mode set.
                        return;
                    }
                }
                else
                {
                    this.session.ExceptionMode = true;
                    this.session.Clear();
                }

                // Send a harmless command and wait for the response.
                var result = this.session.String("SET APL OFF\n");
                if (!result.Success)
                {
                    worker.ReportProgress(0, new WorkerStatusError(result.Result[0]));
                    this.AbortSession(worker);
                    return;
                }

                var ready = " " + username + " Ready;";
                this.NextScreen(worker);
                if (!this.WaitForString(worker, 1, 1, "SET APL OFF") ||
                    (wasLoggedOn && !this.WaitForString(worker, 2, 1, ready)) ||
                    (!wasLoggedOn && (!this.WaitForString(worker, 2, 1, "DMS") ||
                                      !this.WaitForString(worker, 3, 1, ready) ||
                                      !this.WaitForString(worker, 4, 1, ready))))
                {
                    worker.ReportProgress(0, new WorkerStatusError(worker, "Setup failed"));
                    this.AbortSession(worker);
                    return;
                }

                // Marshal transfer options
                var transferParams = new List<Parameter>();
                transferParams.Add(new ParameterExistAction(this.CheckedTag<ExistAction>(existsBox).Value));
                if (modeAsciiButton.Checked)
                {
                    transferParams.Add(new ParameterAsciiCr(crCheckBox.Checked));
                    transferParams.Add(new ParameterAsciiRemap(remapCheckBox.Checked, (codePage != 0) ? (uint?)codePage : null));
                }

                if (recfmBox.Enabled && !recfmDefaultButton.Checked)
                {
                    transferParams.Add(new ParameterSendRecordFormat(this.CheckedTag<RecordFormat>(recfmBox).Value));
                    if (lrecl != 0)
                    {
                        transferParams.Add(new ParameterSendLogicalRecordLength(lrecl));
                    }
                }

                if (tsoAllocationBox.Enabled && primarySpace != 0)
                {
                    var allocationUnits = this.CheckedTag<TsoAllocationUnits>(tsoAllocationBox).Value;
                    transferParams.Add(new ParameterTsoSendAllocation(
                        allocationUnits,
                        primarySpace,
                        secondarySpace > 0 ? (uint?)secondarySpace : null,
                        allocationUnits == TsoAllocationUnits.Avblock ? (uint?)avblock : null));
                }

                var hostType = this.CheckedTag<HostType>(hostTypeBox);
                worker.ReportProgress(0, new WorkerStatusProcessing("Transfer in progress"));

                // Do the transfer.
                result = this.session.Transfer(
                    localFileTextBox.Text,
                    hostFileTextBox.Text,
                    this.CheckedTag<Direction>(directionBox).Value,
                    this.CheckedTag<Mode>(modeBox).Value,
                    this.CheckedTag<HostType>(hostTypeBox).Value,
                    transferParams);
                if (!result.Success)
                {
                    worker.ReportProgress(0, new WorkerStatusError("Transfer failed: " + result.Result[0]));
                    this.AbortSession(worker);
                    return;
                }
            }
            catch (X3270ifCommandException e)
            {
                this.AbortSession(worker);
                worker.ReportProgress(0, new WorkerStatusError("Session error: " + e.Message));
            }
            finally
            {
                // All done with exceptions.
                this.session.ExceptionMode = false;
            }

            // Report results.
            overall.Stop();
            worker.ReportProgress(
                100,
                new WorkerStatusComplete(
                    string.Format("Done in {0}", overall.Elapsed),
                    string.Empty));
        }

        /// <summary>
        /// Main background worker method.
        /// </summary>
        /// <param name="sender">Sender of event</param>
        /// <param name="e">Parameters to the action</param>
        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            // Stop the timer.
            this.timer.Enabled = false;

            BackgroundWorker worker = sender as BackgroundWorker;
            var action = (QueryAction)e.Argument;

            switch (action)
            {
                case QueryAction.StartQuery:
                    // Run the query.
                    this.DoStartQuery(worker);
                    if (this.session != null && this.session.EmulatorRunning)
                    {
                        this.timer.Enabled = true;
                    }

                    break;
                case QueryAction.StartTransfer:
                    // Run the file transfer.
                    this.DoStartTransfer(worker);
                    if (this.session != null && this.session.EmulatorRunning)
                    {
                        this.timer.Enabled = true;
                    }

                    break;
                case QueryAction.Timeout:
                    // Step the teardown.
                    if (this.DowngradeSession(worker, andClose: true))
                    {
                        this.timer.Enabled = true;
                    }

                    break;
                case QueryAction.Stop:
                case QueryAction.Quit:
                    // Stop the ws3270 session.
                    while (this.DowngradeSession(worker, andClose: true))
                    {
                    }

                    if (action == QueryAction.Quit)
                    {
                        // Exit the program.
                        Environment.Exit(0);
                    }

                    break;
            }
        }
        #endregion

        #region Worker thread handlers in the GUI thread
        /// <summary>
        /// Clear the 'Started' indicator
        /// </summary>
        private void NotRunning()
        {
            startedLabel.Text = "Not started";
            startedPictureBox.Image = global::x3270ifGuiTest.Properties.Resources.red_light;
        }

        /// <summary>
        /// Background worker thread progress indicator.
        /// The parameters are pretty badly overloaded:
        /// </summary>
        /// <param name="sender">Sending object.</param>
        /// <param name="e">Parameters; <see cref="ProgressChangedEventArgs.UserState"/> is a <see cref="WorkerStatus"/>.</param>
        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            WorkerStatus status = (WorkerStatus)e.UserState;

            switch (status.Status)
            {
                case WorkerStatusIndication.Idle:
                    stateLabel.Text = string.Empty;
                    resultLabel.Text = string.Empty;
                    screenLabel.Text = string.Empty;
                    break;
                case WorkerStatusIndication.Complete:
                    stateLabel.Text = ((WorkerStatusComplete)status).CompleteText;
                    stateLabel.ForeColor = Color.Black;
                    resultLabel.Text = ((WorkerStatusComplete)status).ResultText;
                    break;
                case WorkerStatusIndication.Error:
                    stateLabel.Text = "Error: " + ((WorkerStatusError)status).Text;
                    stateLabel.ForeColor = Color.Red;
                    if (this.session != null && !this.session.EmulatorRunning)
                    {
                        this.NotRunning();
                    }

                    break;
                case WorkerStatusIndication.LoggedOn:
                    var loggedOn = status as WorkerStatusLoggedOn;
                    if (loggedOn.LoggedOn)
                    {
                        loggedOnLabel.Text = "Logged on";
                        loggedOnPictureBox.Image = global::x3270ifGuiTest.Properties.Resources.green_light;
                    }
                    else
                    {
                        loggedOnLabel.Text = "Not logged on";
                        loggedOnPictureBox.Image = global::x3270ifGuiTest.Properties.Resources.red_light;
                    }

                    break;
                case WorkerStatusIndication.Running:
                    var running = status as WorkerStatusRunning;
                    if (running.Running)
                    {
                        startedLabel.Text = "Started";
                        startedPictureBox.Image = global::x3270ifGuiTest.Properties.Resources.green_light;
                    }
                    else
                    {
                        NotRunning();
                    }

                    break;
                case WorkerStatusIndication.Connected:
                    var connected = status as WorkerStatusConnected;
                    if (connected.Connected)
                    {
                        connectedLabel.Text = "Connected";
                        connectedPictureBox.Image = global::x3270ifGuiTest.Properties.Resources.green_light;
                    }
                    else
                    {
                        connectedLabel.Text = "Not connected";
                        connectedPictureBox.Image = global::x3270ifGuiTest.Properties.Resources.red_light;
                    }

                    break;
                case WorkerStatusIndication.Waiting:
                    stateLabel.Text = "Waiting for '" + ((WorkerStatusWaiting)status).Text + "'";
                    stateLabel.ForeColor = Color.Black;
                    break;
                case WorkerStatusIndication.Found:
                    stateLabel.Text = "Found " + ((WorkerStatusFound)status).Text;
                    stateLabel.ForeColor = Color.Black;
                    break;
                case WorkerStatusIndication.Processing:
                    stateLabel.Text = ((WorkerStatusProcessing)status).Text;
                    stateLabel.ForeColor = Color.Black;
                    break;
                case WorkerStatusIndication.Screen:
                    screenLabel.Text = ((WorkerStatusScreen)status).Text;
                    break;
            }
        }

        /// <summary>
        /// Background worker completion function.
        /// If we are quitting, makes sure the session is closed and then exits. 
        /// </summary>
        /// <param name="sender">Worker context.</param>
        /// <param name="e">Completion arguments.</param>
        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (this.quitting)
            {
                if (this.session != null)
                {
                    this.session.Close();
                }

                Environment.Exit(0);
            }
        }
        #endregion

        #region worker status messages
        /// <summary>
        /// Parent class for transmitting status from the worker thread to the GUI thread.
        /// </summary>
        private abstract class WorkerStatus
        {
            /// <summary>
            /// Gets or sets what kind of status indication this is.
            /// </summary>
            public WorkerStatusIndication Status { get; set; }
        }

        /// <summary>
        /// Worker thread is idle.
        /// </summary>
        private class WorkerStatusIdle : WorkerStatus
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="WorkerStatusIdle"/> class.
            /// </summary>
            public WorkerStatusIdle()
            {
                this.Status = WorkerStatusIndication.Idle;
            }
        }

        /// <summary>
        /// Worker thread is waiting for some string.
        /// </summary>
        private class WorkerStatusWaiting : WorkerStatus
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="WorkerStatusWaiting"/> class.
            /// </summary>
            /// <param name="text">Text we are waiting for</param>
            public WorkerStatusWaiting(string text)
            {
                this.Status = WorkerStatusIndication.Waiting;
                this.Text = text;
            }

            /// <summary>
            /// Gets or sets the string we are waiting for.
            /// </summary>
            public string Text { get; set; }
        }

        /// <summary>
        /// Worker thread has found a string it was waiting for.
        /// </summary>
        private class WorkerStatusFound : WorkerStatus
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="WorkerStatusFound"/> class.
            /// </summary>
            /// <param name="text">Text that we found</param>
            public WorkerStatusFound(string text)
            {
                this.Status = WorkerStatusIndication.Found;
                this.Text = text;
            }

            /// <summary>
            /// Gets or sets the text we found.
            /// </summary>
            public string Text { get; set; }
        }

        /// <summary>
        /// Worker thread has encountered an error.
        /// </summary>
        private class WorkerStatusError : WorkerStatus
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="WorkerStatusError"/> class.
            /// </summary>
            /// <param name="text">Error text</param>
            public WorkerStatusError(string text)
            {
                this.Status = WorkerStatusIndication.Error;
                this.Text = text;
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="WorkerStatusError"/> class.
            /// </summary>
            /// <param name="worker">Worker context</param>
            /// <param name="text">Error text.</param>
            public WorkerStatusError(BackgroundWorker worker, string text)
            {
                this.Status = WorkerStatusIndication.Error;
                this.Text = worker.CancellationPending ? "Canceled" : text;
            }

            /// <summary>
            /// Gets or sets the error text.
            /// </summary>
            public string Text { get; set; }
        }

        /// <summary>
        /// Worker thread has completed an operation.
        /// </summary>
        private class WorkerStatusComplete : WorkerStatus
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="WorkerStatusComplete"/> class.
            /// </summary>
            /// <param name="completeText">Completion text (status message)</param>
            /// <param name="resultText">Result of the operation</param>
            public WorkerStatusComplete(string completeText, string resultText)
            {
                this.Status = WorkerStatusIndication.Complete;
                this.CompleteText = completeText;
                this.ResultText = resultText;
            }

            /// <summary>
            /// Gets or sets the completion text (status message).
            /// </summary>
            public string CompleteText { get; set; }

            /// <summary>
            /// Gets or sets the result of the operation.
            /// </summary>
            public string ResultText { get; set; }
        }

        /// <summary>
        /// ws3270 process running status has changed.
        /// </summary>
        private class WorkerStatusRunning : WorkerStatus
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="WorkerStatusRunning"/> class.
            /// </summary>
            /// <param name="running">Is ws3270 running?</param>
            public WorkerStatusRunning(bool running)
            {
                this.Status = WorkerStatusIndication.Running;
                this.Running = running;
            }

            /// <summary>
            /// Gets or sets a value indicating whether ws3270 is running
            /// </summary>
            public bool Running { get; set; }
        }

        /// <summary>
        /// Connected to host.
        /// </summary>
        private class WorkerStatusConnected : WorkerStatus
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="WorkerStatusConnected"/> class.
            /// </summary>
            /// <param name="connected">Are we connected?</param>
            public WorkerStatusConnected(bool connected)
            {
                this.Status = WorkerStatusIndication.Connected;
                this.Connected = connected;
            }

            /// <summary>
            /// Gets or sets a value indicating whether we are connected
            /// </summary>
            public bool Connected { get; set; }
        }

        /// <summary>
        /// Logged on to host.
        /// </summary>
        private class WorkerStatusLoggedOn : WorkerStatus
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="WorkerStatusLoggedOn"/> class.
            /// </summary>
            /// <param name="loggedOn">Are we logged on?</param>
            public WorkerStatusLoggedOn(bool loggedOn)
            {
                this.Status = WorkerStatusIndication.LoggedOn;
                this.LoggedOn = loggedOn;
            }

            /// <summary>
            /// Gets or sets a value indicating whether we are logged on
            /// </summary>
            public bool LoggedOn { get; set; }
        }

        /// <summary>
        /// Processing some operation.
        /// </summary>
        private class WorkerStatusProcessing : WorkerStatus
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="WorkerStatusProcessing"/> class.
            /// </summary>
            /// <param name="text">Text to display</param>
            public WorkerStatusProcessing(string text)
            {
                this.Status = WorkerStatusIndication.Processing;
                this.Text = text;
            }

            /// <summary>
            /// Gets or sets the text to display.
            /// </summary>
            public string Text { get; set; }
        }

        /// <summary>
        /// Used to update the miniature screen image.
        /// </summary>
        private class WorkerStatusScreen : WorkerStatus
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="WorkerStatusScreen"/> class.
            /// </summary>
            /// <param name="text">Text to display</param>
            public WorkerStatusScreen(string text)
            {
                this.Status = WorkerStatusIndication.Screen;
                this.Text = text;
            }

            /// <summary>
            /// Gets or sets the text to display
            /// </summary>
            public string Text { get; set; }
        }
        #endregion
    }
}