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
using x3270if;
using x3270if.Transfer;
using x3270if.ProcessOptions;

// GUI app to exercise the x3270if DLL.
namespace x3270ifGuiTest
{
    public partial class x3270ifGuiTest : Form
    {
        public x3270ifGuiTest()
        {
            InitializeComponent();
        }

        // Actions for the background worker process
        private enum queryAction
        {
            StartQuery,
            StartTransfer,
            Timeout,
            Stop,
            Quit
        }

        #region Worker status messages
        enum WorkerStatusIndication
        {
            Idle,       // initial state
            Waiting,    // waiting for host output
            Found,      // got host output
            Error,      // error occurred
            Complete,   // query complete
            Running,    // x3270if running or stopped
            Connected,  // connected to host
            LoggedOn,   // logged on to host
            Processing, // processing request
            Screen      // updated screen image
        }

        class WorkerStatus
        {
            public WorkerStatusIndication Status;
        }

        class WorkerStatusIdle : WorkerStatus
        {
            public WorkerStatusIdle()
            {
                Status = WorkerStatusIndication.Idle;
            }
        }

        class WorkerStatusWaiting : WorkerStatus
        {
            public string Text;
            public WorkerStatusWaiting(string text)
            {
                Status = WorkerStatusIndication.Waiting;
                Text = text;
            }
        }

        class WorkerStatusFound : WorkerStatus
        {
            public string Text;
            public WorkerStatusFound(string text)
            {
                Status = WorkerStatusIndication.Found;
                Text = text;
            }
        }

        class WorkerStatusError : WorkerStatus
        {
            public string Text;
            public WorkerStatusError(string text)
            {
                Status = WorkerStatusIndication.Error;
                Text = text;
            }

            public WorkerStatusError(BackgroundWorker worker, string text)
            {
                Status = WorkerStatusIndication.Error;
                Text = worker.CancellationPending ? "Canceled" : text;
            }
        }

        class WorkerStatusComplete : WorkerStatus
        {
            public string CompleteText;
            public string ResultText;
            public WorkerStatusComplete(string completeText, string resultText)
            {
                Status = WorkerStatusIndication.Complete;
                CompleteText = completeText;
                ResultText = resultText;
            }
        }

        class WorkerStatusRunning : WorkerStatus
        {
            public bool Running;
            public WorkerStatusRunning(bool running)
            {
                Status = WorkerStatusIndication.Running;
                Running = running;
            }
        }

        class WorkerStatusConnected : WorkerStatus
        {
            public bool Connected;
            public WorkerStatusConnected(bool connected)
            {
                Status = WorkerStatusIndication.Connected;
                Connected = connected;
            }
        }

        class WorkerStatusLoggedOn : WorkerStatus
        {
            public bool LoggedOn;
            public WorkerStatusLoggedOn(bool loggedOn)
            {
                Status = WorkerStatusIndication.LoggedOn;
                LoggedOn = loggedOn;
            }
        }

        class WorkerStatusProcessing : WorkerStatus
        {
            public string Text;
            public WorkerStatusProcessing(string text)
            {
                Status = WorkerStatusIndication.Processing;
                Text = text;
            }
        }

        class WorkerStatusScreen : WorkerStatus
        {
            public string Text;
            public WorkerStatusScreen(string text)
            {
                Status = WorkerStatusIndication.Screen;
                Text = text;
            }
        }
        #endregion

        private Session session;

        private bool loggedOn;
        private DisplayBuffer displayBuffer;
        private bool quitting;

        private System.Timers.Timer timer = new System.Timers.Timer();

        #region Worker subroutines

        /// <summary>
        /// Downgrade the session by one step.
        /// </summary>
        /// <param name="worker">Context.</param>
        /// <param name="andClose">If true, close the session.</param>
        /// <returns>True if there is more to tear down.</returns>
        private bool DowngradeSession(BackgroundWorker worker, bool andClose)
        {
            if (session == null)
            {
                // Nothing to downgrade.
                return false;
            }

            if (loggedOn)
            {
                session.Clear();
                session.String("LOGOFF\n");
                loggedOn = false;
                worker.ReportProgress(0, new WorkerStatusLoggedOn(false));
                worker.ReportProgress(0, new WorkerStatusConnected(false));
                worker.ReportProgress(0, new WorkerStatusScreen(string.Empty));
                return true;
            }

            if (session.HostConnected)
            {
                session.Disconnect();
                worker.ReportProgress(0, new WorkerStatusConnected(false));
                return true;
            }
            else
            {
                worker.ReportProgress(0, new WorkerStatusConnected(false));
            }

            if (andClose && session.EmulatorRunning)
            {
                session.Close();
                worker.ReportProgress(0, new WorkerStatusRunning(false));
                return false;
            }

            return false;
        }

        private void AbortSession(BackgroundWorker worker)
        {
            session.ExceptionMode = false;
            while (DowngradeSession(worker, andClose: false))
            {
            }
        }

        private void NextScreen(BackgroundWorker worker)
        {
            displayBuffer = new DisplayBuffer(session.ReadBuffer());
            worker.ReportProgress(0, new WorkerStatusScreen(string.Join("\n", displayBuffer.Ascii())));
        }

        /// <summary>
        /// Scan the screen for a predicate.
        /// Checks the predicate, and if it fails, waits for output and tries again.
        /// Gives up after the specified number of seconds.
        /// </summary>
        /// <param name="worker">Context.</param>
        /// <param name="d">Predicate.</param>
        /// <param name="secs">Seconds to wait.</param>
        /// <returns>True if predicate succeeds.</returns>
        private bool RescanUntil(BackgroundWorker worker, Func<Boolean> d, int secs = 10)
        {
            return RescanUntil(worker, new List<Func<Boolean>> { d }, secs) >= 0;
        }

        /// <summary>
        /// Scan the screen for a predicate.
        /// Checks the predicate, and if it fails, waits for output and tries again.
        /// Gives up after the specified number of seconds.
        /// </summary>
        /// <param name="worker">Context.</param>
        /// <param name="d">Set of predicates.</param>
        /// <param name="secs">Seconds to wait.</param>
        /// <returns>True if predicate succeeds.</returns>
        private int RescanUntil(BackgroundWorker worker, IEnumerable<Func<Boolean>> d, int secs = 10)
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
                var result = session.Wait(WaitMode.Output, (int)secondsToWait);
                if (!result.Success || timeout.ElapsedMilliseconds > secs * 1000 || worker.CancellationPending)
                {
                    return -1;
                }
                NextScreen(worker);
            }
        }

        /// <summary>
        /// Wait until the host displays a particular string.
        /// Requires that displayBuffer is valid. Will wait for more output and update displayBuffer if necessary.
        /// Times out after 10 seconds.
        /// </summary>
        /// <param name="worker">Context.</param>
        /// <param name="row">Row where text needs to appear.</param>
        /// <param name="col">Column.</param>
        /// <param name="text">Desired text.</param>
        /// <returns>True if text was found.</returns>
        private bool WaitForString(BackgroundWorker worker, int row, int col, string text)
        {
            if (!RescanUntil(worker, () => displayBuffer.AsciiEquals(row, col, text), 10))
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
        /// <param name="worker">Context.</param>
        /// <returns>True if CMS rebooted successfully.</returns>
        private bool RebootCMS(BackgroundWorker worker)
        {
            worker.ReportProgress(0, new WorkerStatusError("Attempting CMS reboot"));

            session.String("IPL CMS\n");
            if (!WaitForString(worker, 6, 1, "z/VM"))
            {
                worker.ReportProgress(0, new WorkerStatusError(worker, "Reboot failed"));
                AbortSession(worker);
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
            return displayBuffer.AsciiEquals(43, 61, "MORE...");
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
                if (displayBuffer.AsciiEquals(i, 1, text))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Check logon parameters.
        /// </summary>
        /// <param name="worker">Context.</param>
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
        /// <param name="worker">Context.</param>
        /// <param name="username">Username.</param>
        /// <returns>True if logon succeeded.</returns>
        private bool LogOn(BackgroundWorker worker, string username)
        {
            if (session == null)
            {
                var config = new ProcessConfig { Origin = 1, ExtraOptions = new[] { new ProcessOptionWithoutValue("trace") } };
                session = new ProcessSession(config);
            }
            if (!session.EmulatorRunning)
            {
                var startResult = session.Start();
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
                ioResult = session.Connect(hostnameTextBox.Text, port: portTextBox.Text, flags: secureCheckBox.Checked ? ConnectFlags.Secure : ConnectFlags.None);
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
            session.ExceptionMode = true;
            try
            {
                // Wait for the initial logon screen.
                NextScreen(worker);
                if (!WaitForString(worker, 39, 2, "USERID"))
                {
                    AbortSession(worker);
                    return false;
                }

                // Fill in username and password, do an Enter.
                session.StringAt(
                    new[] {
                    new StringAtBlock { Row = 39, Column = 17, Text = username },
                    new StringAtBlock { Row = 40, Column = 17, Text = passwordTextBox.Text + "\n" }
                });

                NextScreen(worker);
                switch (RescanUntil(worker, new List<Func<bool>>
            {
                () => displayBuffer.AsciiMatches(2, 1, 80, ".*not in CP directory.*"),
                () => displayBuffer.AsciiMatches(2, 1, 80, ".*unsuccessful.*"),
                () => displayBuffer.AsciiEquals(4, 1, "RECONNECTED"),
                () => displayBuffer.AsciiEquals(5, 1, "z/VM"),
            }))
                {
                    case 0:
                    case 1:
                        // Failed logon
                        worker.ReportProgress(0, new WorkerStatusError("Logon failed: " + displayBuffer.Ascii(2, 1, 80).Trim()));
                        AbortSession(worker);
                        return false;
                    case 2: // RECONNECTED (disconnected without logoff; need reboot)
                        if (!RebootCMS(worker))
                        {
                            return false;
                        }
                        break;
                    case 3: // z/VM (success)
                        break;
                    default: // Timed out without a match
                        worker.ReportProgress(0, new WorkerStatusError(worker, "Logon failed"));
                        AbortSession(worker);
                        return false;
                }

                loggedOn = true;
                worker.ReportProgress(0, new WorkerStatusLoggedOn(true));

                // Send the command we want to capture.
                session.Clear();
            }
            catch (X3270ifCommandException e)
            {
                // One of the commands failed, likely because something broke (network, emulator).
                worker.ReportProgress(0, new WorkerStatusError(worker, "Session error: " + e.Message));
                AbortSession(worker);
                return false;
            }

            return true;
        }
        #endregion

        #region Background worker handlers
        /// <summary>
        /// Start button action. Runs the query.
        /// </summary>
        /// <param name="worker">Context.</param>
        private void DoStartQuery(BackgroundWorker worker)
        {
            System.Diagnostics.Stopwatch overall = new System.Diagnostics.Stopwatch();
            overall.Start();

            worker.ReportProgress(100, new WorkerStatusIdle());

            // Make sure we have the fields we need.
            if (!CheckLogonFields(worker))
            {
                return;
            }

            var username = usernameTextBox.Text.ToUpper();
            List<string> lines = new List<String>();

            // Do the rest of the work catching command errors (socket or emulator process failures).
            try
            {
                var wasLoggedOn = loggedOn;
                if (!loggedOn)
                {
                    if (!LogOn(worker, username))
                    {
                        return;
                    }
                    // Leaves exception mode on.
                }
                else
                {
                    session.ExceptionMode = true;
                    session.Clear();
                }

                // Send the command we want to capture.
                string query = queryTextBox.Text;
                if (string.IsNullOrEmpty(query))
                {
                    query = "LISTFILE";
                }
                query = query.ToUpper();
                session.String(query + "\n");

                // This will (artificially) trigger an X3270ifCommandException.
                //session.Io("woof");

                // Look for:
                //  query echoed on line 1
                //   DMS on line 2
                //   USERNAME Ready; on line 3
                //  file names dribbling out
                //   a second USERNAME Ready; somewhere, -or- MORE... at 43,61
                //  If MORE..., hit clear and repeat until we get USERNAME Ready;

                var ready = " " + username + " Ready;";
                NextScreen(worker);

                if (!WaitForString(worker, 1, 1, query) ||
                    (!wasLoggedOn && (!WaitForString(worker, 1, 1, query) ||
                                      !WaitForString(worker, 2, 1, "DMS") ||
                                      !WaitForString(worker, 3, 1, ready))))
                {
                    worker.ReportProgress(0, new WorkerStatusError(worker, "Command failed"));
                    AbortSession(worker);
                    return;
                }

                var dataRow = wasLoggedOn ? 2 : 4;
                bool first = true;
                do
                {
                    // Snap the screen.
                    NextScreen(worker);

                    // If the MORE... prompt is up, clear the screen and start scanning at the top.
                    if (!first && IsMore())
                    {
                        session.Clear();
                        dataRow = 1;
                        NextScreen(worker);
                    }
                    first = false;

                    // Wait for the concluding prompt or MORE...
                    if (!RescanUntil(worker, () => ScanFor(dataRow, ready) || IsMore(), 30))
                    {
                        AbortSession(worker);
                        worker.ReportProgress(0, new WorkerStatusError(worker, "Result not found"));
                        return;
                    }

                    // Collect the file names.
                    for (var i = dataRow; i < 42; i++)
                    {
                        if (displayBuffer.AsciiEquals(i, 1, ready))
                        {
                            break;
                        }
                        var filename = displayBuffer.Ascii(i, 1, 80).Trim();
                        if (!string.IsNullOrEmpty(filename))
                        {
                            lines.Add(filename);
                            worker.ReportProgress(0, new WorkerStatusFound(lines.Count.ToString() + " lines"));
                        }
                    }
                } while (IsMore());
            }
            catch (X3270ifCommandException e)
            {
                AbortSession(worker);
                worker.ReportProgress(0, new WorkerStatusError(worker, "Session failure: " + e.Message));
                return;
            }
            finally
            {
                // All done with exceptions.
                session.ExceptionMode = false;
            }

            // Report results.
            overall.Stop();
            worker.ReportProgress(100, new WorkerStatusComplete(
                string.Format("Done: {0} lines in {1}", lines.Count, overall.Elapsed),
                string.Join("\n", lines.ToArray())));
        }

        /// <summary>
        /// Find the checked RadioButton in a GroupBox and return its tag as an Enum.
        /// </summary>
        /// <param name="groupBox">Box to search; must contain <see cref="RadioButton"/>s.</param>
        /// <returns>Nullable Enum value.</returns>
        private Nullable<T> checkedTag<T>(GroupBox groupBox) where T : struct, IConvertible
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
        /// <param name="worker">Context.</param>
        private void DoStartTransfer(BackgroundWorker worker)
        {
            worker.ReportProgress(100, new WorkerStatusIdle());

            // Make sure we have the fields we need.
            if (!CheckLogonFields(worker))
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
            bool wasLoggedOn = loggedOn;

            try
            {
                if (!loggedOn)
                {
                    if (!LogOn(worker, username))
                    {
                        return;
                    }
                    // Leaves exception mode set.
                }
                else
                {
                    session.ExceptionMode = true;
                    session.Clear();
                }

                // Send a harmless command and wait for the response.
                var result = session.String("SET APL OFF\n");
                if (!result.Success)
                {
                    worker.ReportProgress(0, new WorkerStatusError(result.Result[0]));
                    AbortSession(worker);
                    return;
                }
                var ready = " " + username + " Ready;";
                NextScreen(worker);
                if (!WaitForString(worker, 1, 1, "SET APL OFF") ||
                    (wasLoggedOn && !WaitForString(worker, 2, 1, ready)) ||
                    (!wasLoggedOn && (!WaitForString(worker, 2, 1, "DMS") ||
                                      !WaitForString(worker, 3, 1, ready) ||
                                      !WaitForString(worker, 4, 1, ready))))
                {
                    worker.ReportProgress(0, new WorkerStatusError(worker, "Setup failed"));
                    AbortSession(worker);
                    return;
                }

                // Marshal transfer options
                var transferParams = new List<Parameter>();
                transferParams.Add(new ParameterExistAction(checkedTag<ExistAction>(existsBox).Value));
                if (modeAsciiButton.Checked)
                {
                    transferParams.Add(new ParameterAsciiCr(crCheckBox.Checked));
                    transferParams.Add(new ParameterAsciiRemap(remapCheckBox.Checked, (codePage != 0) ? (uint?)codePage : null));
                }
                if (recfmBox.Enabled && !recfmDefaultButton.Checked)
                {
                    transferParams.Add(new ParameterSendRecordFormat(checkedTag<RecordFormat>(recfmBox).Value));
                    if (lrecl != 0)
                    {
                        transferParams.Add(new ParameterSendLogicalRecordLength(lrecl));
                    }
                }
                if (tsoAllocationBox.Enabled && primarySpace != 0)
                {
                    var allocationUnits = checkedTag<TsoAllocationUnits>(tsoAllocationBox).Value;
                    transferParams.Add(new ParameterTsoSendAllocation(
                        allocationUnits,
                        primarySpace,
                        secondarySpace > 0 ? (uint?)secondarySpace : null,
                        allocationUnits == TsoAllocationUnits.Avblock ? (uint?)avblock : null));
                }

                var hostType = checkedTag<HostType>(hostTypeBox);
                worker.ReportProgress(0, new WorkerStatusProcessing("Transfer in progress"));

                // Do the transfer.
                result = session.Transfer(
                    localFileTextBox.Text,
                    hostFileTextBox.Text,
                    checkedTag<Direction>(directionBox).Value,
                    checkedTag<Mode>(modeBox).Value,
                    checkedTag<HostType>(hostTypeBox).Value,
                    transferParams);
                if (!result.Success)
                {
                    worker.ReportProgress(0, new WorkerStatusError("Transfer failed: " + result.Result[0]));
                    AbortSession(worker);
                    return;
                }

            }
            catch (X3270ifCommandException e)
            {
                AbortSession(worker);
                worker.ReportProgress(0, new WorkerStatusError("Session error: " + e.Message));
            }
            finally
            {
                // All done with exceptions.
                session.ExceptionMode = false;
            }

            // Report results.
            overall.Stop();
            worker.ReportProgress(100, new WorkerStatusComplete(
                string.Format("Done in {0}", overall.Elapsed),
                string.Empty));
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            // Stop the timer.
            timer.Enabled = false;

            BackgroundWorker worker = sender as BackgroundWorker;
            var action = (queryAction)e.Argument;

            switch (action)
            {
                case queryAction.StartQuery:
                    // Run the query.
                    DoStartQuery(worker);
                    if (session != null && session.EmulatorRunning)
                    {
                        timer.Enabled = true;
                    }
                    break;
                case queryAction.StartTransfer:
                    // Run the file transfer.
                    DoStartTransfer(worker);
                    if (session != null && session.EmulatorRunning)
                    {
                        timer.Enabled = true;
                    }
                    break;
                case queryAction.Timeout:
                    // Step the teardown.
                    if (DowngradeSession(worker, andClose: true))
                    {
                        timer.Enabled = true;
                    }
                    break;
                case queryAction.Stop:
                case queryAction.Quit:
                    // Stop the ws3270 session.
                    while (DowngradeSession(worker, andClose: true))
                    {
                    }
                    if (action == queryAction.Quit)
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
        private void notRunning()
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
                    if (session != null && !session.EmulatorRunning)
                    {
                        notRunning();
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
                        notRunning();
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
        /// <param name="sender">Context.</param>
        /// <param name="e">Completion arguments.</param>
        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (quitting)
            {
                if (session != null)
                {
                    session.Close();
                }
                Environment.Exit(0);
            }
        }
        #endregion

        #region GUI thread event handlers
        private void x3270ifGuiTest_Load(object sender, EventArgs e)
        {
            stateLabel.Text = "";
            resultLabel.Text = "";

            // Set up the idle timer.
            timer.Elapsed += timer_Elapsed;
            timer.Interval = 15000;
            timer.Enabled = false;
        }

        void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            this.Invoke(new MethodInvoker(() => backgroundWorker1.RunWorkerAsync(queryAction.Timeout)));
        }

        private void runQueryButton_Click(object sender, EventArgs e)
        {
            if (!backgroundWorker1.IsBusy)
            {
                backgroundWorker1.RunWorkerAsync(queryAction.StartQuery);
            }
        }

        private void stopButton_Click(object sender, EventArgs e)
        {
            if (!backgroundWorker1.IsBusy)
            {
                backgroundWorker1.RunWorkerAsync(queryAction.Stop);
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
                backgroundWorker1.RunWorkerAsync(queryAction.Quit);
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
                backgroundWorker1.RunWorkerAsync(queryAction.StartTransfer);
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
