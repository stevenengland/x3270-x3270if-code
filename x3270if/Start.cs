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
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;
using System.Text;

namespace x3270if
{
    /// <summary>
    /// The result of a Start operation.
    /// </summary>
    public class startResult
    {
        /// <summary>
        /// true if the operation succeeded.
        /// </summary>
        public bool Success;

        /// <summary>
        /// If the operation failed, text explaining why.
        /// </summary>
        public string FailReason;

        /// <summary>
        /// Empty constructor.
        /// </summary>
        public startResult()
        {
            Success = true;
        }

        /// <summary>
        /// Constructor for failure, given explanatory text.
        /// </summary>
        /// <param name="failReason"></param>
        public startResult(string failReason)
        {
            Success = false;
            FailReason = failReason;
        }
    }

    partial class Session
    {
        /// <summary>
        /// Connect to the emulator.
        /// </summary>
        /// <returns>Task returning success/failure and failure text</returns>
        private async Task<startResult> TryEmptyCommandAsync()
        {
            // Talk to the emulator with a null command.
            var result = await IoAsync(string.Empty, Config.HandshakeTimeoutMsec).ConfigureAwait(continueOnCapturedContext: false);
            if (result.Success)
            {
                return new startResult();
            }
            else
            {
                return new startResult(backEnd.GetErrorOutput("Initial communication failed"));
            }
        }

        /// <summary>
        /// Start a session, asynchronous version.
        /// </summary>
        /// <returns>Task returning success/failure and failure text</returns>
        public async Task<startResult> StartAsync()
        {
            if (Running)
            {
                throw new InvalidOperationException("Already running");
            }

            // Start it up.
            var result = await backEnd.StartAsync().ConfigureAwait(continueOnCapturedContext: false);
            if (!result.Success)
            {
                return result;
            }

            // Provisionally mark the session as running.
            Running = true;

            // Try an empty command.
            result = await TryEmptyCommandAsync().ConfigureAwait(continueOnCapturedContext: false);
            if (!result.Success)
            {
                Close(); // includes clearing the Running flag and history
                return result;
            }

            // Get the code page.
            var ioResult = await IoAsync("Query(LocalEncoding)").ConfigureAwait(continueOnCapturedContext: false);
            if (!ioResult.Success || ioResult.Result.Length != 1)
            {
                Close();
                return new startResult("Query(LocalEncoding) failed");
            }
            try
            {
                var e = ioResult.Result[0];
                if (e.StartsWith("CP"))
                {
                    Encoding = Encoding.GetEncoding(int.Parse(e.Substring(2)));
                }
                else
                {
                    Encoding = Encoding.GetEncoding(e);
                }
            }
            catch
            {
                Close();
                return new startResult("No matching encoding");
            }

            // Success.
            return new startResult();
        }

        /// <summary>
        /// Start an emulator session, synchronous version.
        /// </summary>
        /// <returns>success/failure and failure text</returns>
        public startResult Start()
        {
            try
            {
                return StartAsync().Result;
            }
            catch (AggregateException e)
            {
                throw e.InnerException;
            }
        }

        /// <summary>
        /// Stop the emulator session.
        /// </summary>
        /// <param name="saveHistory">If true, recent commands will be saved</param>
        public void Close(bool saveHistory = false)
        {
            // Stop the underlying session.
            backEnd.Close();

            // Reset the state.
            Running = false;
            ExceptionMode = false;

            // Wipe out history, if we're asked to.
            if (!saveHistory)
            {
                lock (recentCommandsLock)
                {
                    recentCommands = new LinkedList<IoResult>();
                }
            }
        }
    }

    /// <summary>
    /// Utility class used to encapsulate common socket connect/retry logic.
    /// </summary>
    public static class SessionUtil
    {
        /// <summary>
        /// Connection result (attempting TCP connection to emulator TCP port).
        /// </summary>
        public struct ConnectResult
        {
            /// <summary>
            /// Success (true) or failure.
            /// </summary>
            public bool Success;

            /// <summary>
            /// If failed, the reason why.
            /// </summary>
            public string FailReason;

            /// <summary>
            /// If succeeded, the new connection.
            /// </summary>
            public TcpClient Client;
        }

        /// <summary>
        /// Start (and retry) the connection to an emulator.
        /// </summary>
        /// <param name="port"></param>
        /// <param name="retryMsec">Connect retry timeout</param>
        /// <returns>Start result</returns>
        public static async Task<ConnectResult> TryConnect(int port, int? retryMsec = null)
        {
            var client = new TcpClient(AddressFamily.InterNetwork);
            const int maxTries = 3;

            // Try three times to connect to the emulator, with a default 1s sleep in between.
            // This gives a newly-started copy of ws3270 time to initialize.
            int tries = maxTries;
            bool connected = false;
            int realRetryMsec = retryMsec ?? 1000;
            
            while (tries-- > 0)
            {
                try
                {
                    await client.ConnectAsync(IPAddress.Loopback, port).ConfigureAwait(continueOnCapturedContext: false);
                    connected = true;
                    break;
                }
                catch (Exception)
                {
                }

                // If we're going to try again, snooze for a bit.
                if (tries > 0)
                {
                    await Task.Delay(realRetryMsec).ConfigureAwait(continueOnCapturedContext: false);
                }
            }
            if (connected)
            {
                return new ConnectResult
                {
                    Success = true,
                    Client = client
                };
            }
            else
            {
                return new ConnectResult
                {
                    Success = false,
                    FailReason = "Could not connect to emulator on port " + port
                };
            }
        }
    }
}
