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

namespace X3270if
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Definitions of the fields on the status line.
    /// </summary>
    public enum StatusLineField
    {
        /// <summary>
        /// Keyboard: unlocked (U), locked (L), error lock (E)
        /// </summary>
        KeyboardLock,

        /// <summary>
        /// Formatting: formatted (F), unformatted (U)
        /// </summary>
        Formatting,

        /// <summary>
        /// Protection status of current field: unprotected (U), protected (P)
        /// </summary>
        Protection,

        /// <summary>
        /// Connection status: not connected (N), connected (C), plus hostname
        /// </summary>
        Connection,

        /// <summary>
        /// Mode: not connected (N), connected in NVT character mode (C),
        ///  connected in NVT line mode (L), 3270 negotiation pending (P),
        ///  connected in 3270 mode (I).
        /// </summary>
        Mode,

        /// <summary>
        /// Model number (2/3/4/5)
        /// </summary>
        Model,

        /// <summary>
        /// Number of rows on current screen.
        /// </summary>
        Rows,

        /// <summary>
        /// Number of columns on current screen.
        /// </summary>
        Columns,

        /// <summary>
        /// Row containing cursor.
        /// </summary>
        CursorRow,

        /// <summary>
        /// Column containing cursor.
        /// </summary>
        CursorColumn,

        /// <summary>
        /// X11 window ID of main window, of 0 if not applicable
        /// </summary>
        WindowID,

        /// <summary>
        /// Time that last command took to execute, or -
        /// </summary>
        Timing
    }

    /// <summary>
    /// The result of an I/O operation to the emulator.
    /// </summary>
    public class IoResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IoResult"/> class.
        /// Empty constructor.
        /// </summary>
        public IoResult()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IoResult"/> class.
        /// Cloning constructor.
        /// </summary>
        /// <param name="r">IoResult to clone.</param>
        public IoResult(IoResult r)
        {
            this.Success = r.Success;
            this.Result = r.Result;
            this.Command = r.Command;
            this.StatusLine = r.StatusLine;
            this.ExecutionTime = r.ExecutionTime;
            this.Encoding = r.Encoding;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the operation succeeded.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Gets or sets the output from the operation, one element per line.
        /// If the operation failed, this may contain error text.
        /// </summary>
        public string[] Result { get; set; }

        /// <summary>
        /// Gets or sets the command that was sent.
        /// </summary>
        public string Command { get; set; }

        /// <summary>
        /// Gets or sets the status line.
        /// </summary>
        public string StatusLine { get; set; }

        /// <summary>
        /// Gets or sets how long it took to execute.
        /// </summary>
        public TimeSpan ExecutionTime { get; set; }

        /// <summary>
        /// Gets or sets the encoding of the result.
        /// </summary>
        public Encoding Encoding { get; set; }
    }

    /// <summary>
    /// Session class.
    /// </summary>
    public partial class Session
    {
        /// <summary>
        /// Processing states for the emulator while a command is in progress.
        /// </summary>
        private enum IoStates
        {
            /// <summary>
            /// Waiting for completion.
            /// </summary>
            Waiting,

            /// <summary>
            /// Got the 'ok' prompt.
            /// </summary>
            Succeeded,

            /// <summary>
            /// Got the 'error' prompt.
            /// </summary>
            Failed,

            /// <summary>
            /// Timed out or got EOF.
            /// </summary>
            Crashed
        }

        /// <summary>
        /// Basic emulator I/O function, asynchronous version.
        /// Given a command string and an optional timeout, send it and return the reply.
        /// The emulator status is saved in the session and can be queried after.
        /// </summary>
        /// <param name="command">The command and parameters to pass to the emulator.
        /// Must be formatted correctly for the emulator. This method does no translation.</param>
        /// <param name="timeoutMsec">Optional timeout. The emulator session will be stopped if the timeout expires, so this
        /// is a dead-man timer, not to be used casually.</param>
        /// <param name="isModify">True if command modifies the host</param>
        /// <returns>I/O result.</returns>
        /// <exception cref="InvalidOperationException">Session is not started.</exception>
        /// <exception cref="X3270ifCommandException"><see cref="ExceptionMode"/> is enabled and the command fails.</exception>
        /// <exception cref="ArgumentException"><paramref name="command"/> contains control characters.</exception>
        public async Task<IoResult> IoAsync(string command, int? timeoutMsec = null, bool isModify = false)
        {
            string[] reply = null;

            if (!this.EmulatorRunning)
            {
                throw new InvalidOperationException("Not running");
            }
            
            // If this is a screen-modifying command, see if a forced failure is in order.
            if (isModify)
            {
                var result = this.ForceModifyFailure();
                if (!result.Success)
                {
                    if (this.ExceptionMode)
                    {
                        throw new X3270ifCommandException(result.Result[0]);
                    }

                    return result;
                }
            }

            // Control characters are verboten.
            if (command.Any(c => char.IsControl(c)))
            {
                throw new ArgumentException("command contains control character(s)");
            }

            Util.Log("Io: command '{0}'", command);

            // Write out the command.
            byte[] nl = this.encoding.GetBytes(command + "\n");
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var state = IoStates.Waiting;
            var accum = new StringBuilder();

            var tokenSource = new CancellationTokenSource();
            Task timeoutTask = null;

            try
            {
                var ns = this.Client.GetStream();
                await ns.WriteAsync(nl, 0, nl.Length).ConfigureAwait(continueOnCapturedContext: false);

                // Create a task to time out the read operations after <n> seconds, in case the emulator hangs or we get confused.
                int thisTimeoutMsec = timeoutMsec ?? this.Config.DefaultTimeoutMsec;
                if (thisTimeoutMsec > 0)
                {
                    timeoutTask = Task.Run(async delegate
                    {
                        // When the timeout expires, close the socket.
                        await Task.Delay(thisTimeoutMsec, tokenSource.Token).ConfigureAwait(continueOnCapturedContext: false);
                        this.Client.Close();
                    });
                }

                // Read until we get a prompt.
                var buf = new byte[1024];
                while (state == IoStates.Waiting)
                {
                    var nr = await ns.ReadAsync(buf, 0, buf.Length).ConfigureAwait(continueOnCapturedContext: false);
                    if (nr == 0)
                    {
                        state = IoStates.Crashed;
                        break;
                    }

                    accum.Append(this.encoding.GetString(buf, 0, nr));
                    if (accum.ToString().EndsWith("\nok\n"))
                    {
                        state = IoStates.Succeeded;
                    }
                    else if (accum.ToString().EndsWith("\nerror\n"))
                    {
                        state = IoStates.Failed;
                    }
                }
            }
            catch (ObjectDisposedException)
            {
                // This seems to happen when we Close a TcpClient.
                reply = new[] { "Operation timed out" };
                state = IoStates.Crashed;
            }
            catch (SocketException)
            {
                // Server process died, for example.
                reply = new[] { "Socket exception" };
                state = IoStates.Crashed;
            }
            catch (InvalidOperationException)
            {
                // Also happens when the server process dies; the socket is no longer valid.
                reply = new[] { "Invalid operation exception" };
                state = IoStates.Crashed;
            }
            catch (System.IO.IOException)
            {
                // Also happens when the server process dies; the socket is no longer valid.
                reply = new[] { "I/O exception" };
                state = IoStates.Crashed;
            }

            // All done talking to the server. Stop timing.
            stopwatch.Stop();

            if (timeoutTask != null)
            {
                // Cancel the timeout. Yes, there is a race here, but if we lose, it means that
                // the emulator took a long time to answer, and something is wrong.
                tokenSource.Cancel();

                // Collect the status of the timeout task.
                try
                {
                    timeoutTask.Wait();
                }
                catch (Exception)
                {
                }
            }

            Util.Log("Io: {0}, got '{1}'", state, accum.ToString().Replace("\n", "<\\n>"));

            string statusLine = null;

            if (state != IoStates.Crashed)
            {
                // The array looks like:
                //  data: xxx
                //  data: ...
                //  status line
                //  ok or error
                //  (empty line)

                // Remember the status.
                reply = accum.ToString().Split('\n');
                var nlines = reply.Length;
                statusLine = reply[nlines - 3];

                // Return everything else, removing the "data: " from the beginning.
                Array.Resize(ref reply, nlines - 3);
                for (int i = 0; i < reply.Length; i++)
                {
                    const string DataPrefix = "data: ";
                    if (reply[i].StartsWith(DataPrefix))
                    {
                        reply[i] = reply[i].Substring(DataPrefix.Length);
                    }
                }
            }

            // In Exception Mode, throw a descriptive error if anything ever fails.
            try
            {
                if (this.ExceptionMode && state != IoStates.Succeeded)
                {
                    var commandAndArgs = command.Split(new char[] { ' ', '(', ')' }, StringSplitOptions.RemoveEmptyEntries);
                    string commandName;
                    if (commandAndArgs.Length > 0 && !string.IsNullOrEmpty(commandAndArgs[0]))
                    {
                        commandName = commandAndArgs[0];
                    }
                    else
                    {
                        commandName = "(empty)";
                    }

                    string failureMessage = string.Format("Command {0} failed:", commandName);
                    if (state == IoStates.Failed)
                    {
                        foreach (string r in reply)
                        {
                            failureMessage += " " + r;
                        }
                    }
                    else
                    {
                        failureMessage += " Timeout or socket EOF";
                    }

                    throw new X3270ifCommandException(failureMessage);
                }
            }
            finally
            {
                // Close the session, whether or not we throw an exception (but after
                // we test for an exception), if the server crashed.
                if (state == IoStates.Crashed)
                {
                    this.Close(saveHistory: true);
                }
            }

            // Save the original response in history.
            var ioResult = new IoResult
            {
                Success = state == IoStates.Succeeded,
                Result = reply,
                Command = command,
                StatusLine = statusLine,
                ExecutionTime = stopwatch.Elapsed,
                Encoding = this.encoding
            };
            this.SaveRecentCommand(ioResult);
            this.lastStatusLine = statusLine;

            if (this.Config.Origin == 0 || statusLine == null)
            {
                return ioResult;
            }

            // Edit the cursor position in the status line for Origin and return it.
            var statusFields = statusLine.Split(' ');
            statusFields[(int)StatusLineField.CursorRow] = (int.Parse(statusFields[(int)StatusLineField.CursorRow]) + this.Config.Origin).ToString();
            statusFields[(int)StatusLineField.CursorColumn] = (int.Parse(statusFields[(int)StatusLineField.CursorColumn]) + this.Config.Origin).ToString();
            this.lastStatusLine = string.Join(" ", statusFields);
            var retIoResult = new IoResult(ioResult);
            retIoResult.StatusLine = this.lastStatusLine;
            return retIoResult;
        }

        /// <summary>
        /// Basic emulator I/O function.
        /// Given a command and an optional timeout, send it and return the reply.
        /// The emulator status is saved in the session and can be queried after.
        /// </summary>
        /// <param name="command">The command and parameters to pass to the emulator.
        /// Must be formatted correctly for the emulator. This method does no translation.</param>
        /// <param name="timeoutMsec">Optional timeout. The emulator session will be stopped if the timeout expires, so this
        /// is a dead-man timer, not to be used casually.</param>
        /// <returns>Success/failure and result or error text.</returns>
        /// <exception cref="InvalidOperationException">Session is not started.</exception>
        /// <exception cref="X3270ifCommandException"><see cref="ExceptionMode"/> is enabled and the command fails.</exception>
        public IoResult Io(string command, int? timeoutMsec = null)
        {
            try
            {
                return this.IoAsync(command, timeoutMsec).Result;
            }
            catch (AggregateException e)
            {
                throw e.InnerException;
            }
        }

        /// <summary>
        /// Check for a force-modify failure.
        /// </summary>
        /// <returns>Bad IoResult if a modify command should fail.</returns>
        private IoResult ForceModifyFailure()
        {
            switch (this.Config.ModifyFail)
            {
                case ModifyFailType.Never:
                    break;
                case ModifyFailType.RequireConnection:
                    if (!this.HostConnected)
                    {
                        return new IoResult { Success = false, Result = new[] { "Not connected" } };
                    }

                    break;
                case ModifyFailType.Require3270:
                    if (this.StatusField(StatusLineField.Mode)[0] != 'I')
                    {
                        return new IoResult { Success = false, Result = new[] { "Not in 3270 mode" } };
                    }

                    break;
            }

            // Nothing to fail.
            return new IoResult { Success = true };
        }
    }
}
