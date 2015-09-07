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
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using System.Linq;

namespace x3270if
{
    /// <summary>
    /// Definitions of the fields on the status line.
    /// </summary>
    public enum StatusLineField
    {
        /// <summary>
        /// Keyboard: (U)nlocked, (L)ocked, (E)rror lock
        /// </summary>
        KeyboardLock,
        /// <summary>
        /// Formatting: (F)ormatted, (U)nformatted
        /// </summary>
        Formatting,
        /// <summary>
        /// Protection status of current field: (U)nprotected, (P)rotected
        /// </summary>
        Protection,
        /// <summary>
        /// Connection status: (N)ot connected, (C)onnected, plus hostname
        /// </summary>
        Connection,
        /// <summary>
        /// Mode: (N)ot connected, connected in NVT (C)haracter mode,
        ///  connected in NVT (L)ine mode, 3270 negotiation (P)ending,
        ///  connected (I)n 3270 mode.
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
        /// True if the operation succeeded.
        /// </summary>
        public bool Success;

        /// <summary>
        /// Output from the operation, one element per line.
        /// If the operation failed, this may contain error text.
        /// </summary>
        public string[] Result;

        /// <summary>
        /// The command that was sent.
        /// </summary>
        public string Command;

        /// <summary>
        /// The status line.
        /// </summary>
        public string StatusLine;

        /// <summary>
        /// How long it took to execute.
        /// </summary>
        public TimeSpan ExecutionTime;

        /// <summary>
        /// The encoding of the result.
        /// </summary>
        public Encoding Encoding;

        /// <summary>
        /// Empty constructor.
        /// </summary>
        public IoResult()
        {
        }

        /// <summary>
        /// Cloning constructor.
        /// </summary>
        /// <param name="r">IoResult to clone.</param>
        public IoResult(IoResult r)
        {
            Success = r.Success;
            Result = r.Result;
            Command = r.Command;
            StatusLine = r.StatusLine;
            ExecutionTime = r.ExecutionTime;
            Encoding = r.Encoding;
        }
    }

    public partial class Session
    {
        /// <summary>
        /// Processing states for the emulator while a command is in progress.
        /// </summary>
        enum IoStates
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
        };

        /// <summary>
        /// Basic emulator I/O function, asynchronous version.
        /// Given a command string and an optional timeout, send it and return the reply.
        /// The emulator status is saved in the session and can be queried after.
        /// </summary>
        /// <param name="command">The command and parameters to pass to the emulator.
        /// Must be formatted correctly for the emulator. This method does no translation.</param>
        /// <param name="timeoutMsec">Optional timeout. The emulator session will be stopped if the timeout expires, so this
        /// is a dead-man timer, not to be used casually.</param>
        /// <returns>I/O result.</returns>
        /// <exception cref="InvalidOperationException">Session is not started.</exception>
        /// <exception cref="X3270ifCommandException"><see cref="ExceptionMode"/> is enabled and the command fails.</exception>
        /// <exception cref="ArgumentException"><paramref name="command"/> contains control characters.</exception>
        public async Task<IoResult> IoAsync(string command, int? timeoutMsec = null)
        {
            string[] reply = null;

            if (!Running)
            {
                throw new InvalidOperationException("Not running");
            }

            // Control characters are verboten.
            if (command.Any(c => char.IsControl(c)))
            {
                throw new ArgumentException("command contains control character(s)");
            }

            Util.Log("Io: command '{0}'", command);

            // Write out the command.
            byte[] nl = Encoding.GetBytes(command + "\n");
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var state = IoStates.Waiting;
            var accum = new StringBuilder();

            var tokenSource = new CancellationTokenSource();
            Task timeoutTask = null;

            try
            {
                var ns = client.GetStream();
                await ns.WriteAsync(nl, 0, nl.Length).ConfigureAwait(continueOnCapturedContext: false);

                // Create a task to time out the read operations after <n> seconds, in case the emulator hangs or we get confused.

                int thisTimeoutMsec = timeoutMsec ?? Config.DefaultTimeoutMsec;
                if (thisTimeoutMsec > 0)
                {
                    timeoutTask = Task.Run(async delegate
                    {
                        // When the timeout expires, close the socket.
                        await Task.Delay(thisTimeoutMsec, tokenSource.Token).ConfigureAwait(continueOnCapturedContext: false);
                        client.Close();
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
                    accum.Append(Encoding.GetString(buf, 0, nr));
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
                    const string dataPrefix = "data: ";
                    if (reply[i].StartsWith(dataPrefix))
                    {
                        reply[i] = reply[i].Substring(dataPrefix.Length);
                    }
                }
            }

            // In Exception Mode, throw a descriptive error if anything ever fails.
            try
            {
                if (ExceptionMode && state != IoStates.Succeeded)
                {
                    var commandAndArgs = command.Split(new char[] {' ', '(', ')'}, StringSplitOptions.RemoveEmptyEntries);
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
                    Close(saveHistory: true);
                }
            }

            // Save the original response in history.
            var ioResult = new IoResult
            {
                Success = (state == IoStates.Succeeded),
                Result = reply,
                Command = command,
                StatusLine = statusLine,
                ExecutionTime = stopwatch.Elapsed,
                Encoding = this.Encoding
            };
            SaveRecentCommand(ioResult);
            lastStatusLine = statusLine;

            if (Config.Origin == 0 || statusLine == null)
            {
                return ioResult;
            }

            // Edit the cursor position in the status line for Origin and return it.
            var statusFields = statusLine.Split(' ');
            statusFields[(int)StatusLineField.CursorRow] = (int.Parse(statusFields[(int)StatusLineField.CursorRow]) + Config.Origin).ToString();
            statusFields[(int)StatusLineField.CursorColumn] = (int.Parse(statusFields[(int)StatusLineField.CursorColumn]) + Config.Origin).ToString();
            lastStatusLine = string.Join(" ", statusFields);
            var retIoResult = new IoResult(ioResult);
            retIoResult.StatusLine = lastStatusLine;
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
                return IoAsync(command, timeoutMsec).Result;
            }
            catch (AggregateException e)
            {
                throw e.InnerException;
            }
        }
    }
}
