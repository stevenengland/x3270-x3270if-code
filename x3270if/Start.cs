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
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Sockets;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading.Tasks;

    using Microsoft.Win32.SafeHandles;

    /// <summary>
    /// Utility class used to encapsulate common socket connect/retry logic.
    /// </summary>
    public static class SessionUtil
    {
        /// <summary>
        /// Start (and retry) the connection to an emulator.
        /// </summary>
        /// <param name="port">TCP port number to connect to.</param>
        /// <param name="retryMsec">Optional connect retry timeout, in milliseconds.</param>
        /// <returns>Connect result.</returns>
        public static async Task<ConnectResult> TryConnect(int port, int? retryMsec = null)
        {
            var client = new TcpClient(AddressFamily.InterNetwork);
            const int MaxTries = 3;

            // Try three times to connect to the emulator, with a default 1s sleep in between.
            // This gives a newly-started copy of ws3270 time to initialize.
            int tries = MaxTries;
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
    }

    /// <summary>
    /// The result of a Start operation.
    /// </summary>
    public class StartResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StartResult"/> class.
        /// </summary>
        public StartResult()
        {
            this.Success = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StartResult"/> class.
        /// Constructor for failure, given explanatory text.
        /// </summary>
        /// <param name="failReason">Failure reason text.</param>
        public StartResult(string failReason)
        {
            this.Success = false;
            this.FailReason = failReason;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the operation succeeded.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Gets or sets a string explaining why the operation failed.
        /// </summary>
        public string FailReason { get; set; }
    }

    /// <summary>
    /// Session class.
    /// </summary>
    public partial class Session
    {
        /// <summary>
        /// Disposed flag.
        /// </summary>
        private bool disposed;

        /// <summary>
        /// SafeHandle instance for Dispose.
        /// </summary>
        private SafeHandle handle = new SafeFileHandle(IntPtr.Zero, true);

        /// <summary>
        /// Start an emulator session, asynchronous version.
        /// </summary>
        /// <returns>Task returning success/failure and failure text.</returns>
        /// <remarks>
        /// <note type="caution">
        /// <para>When an application is finished with the emulator, including when the application exits,
        /// it must call the <see cref="Close"/> method to clean up. Otherwise, system resources may be leaked, such as
        /// orphaned server processes.</para></note></remarks>
        public async Task<StartResult> StartAsync()
        {
            if (this.EmulatorRunning)
            {
                throw new InvalidOperationException("Already running");
            }

            // Start it up.
            var result = await this.BackEnd.StartAsync().ConfigureAwait(continueOnCapturedContext: false);
            if (!result.Success)
            {
                return result;
            }

            // Provisionally mark the session as running.
            this.EmulatorRunning = true;

            // Get the local encoding (Windows code page).
            var ioResult = await this.IoAsync("Query(LocalEncoding)", this.Config.HandshakeTimeoutMsec).ConfigureAwait(continueOnCapturedContext: false);
            if (!ioResult.Success || ioResult.Result.Length != 1)
            {
                this.Close();
                return new StartResult("Query(LocalEncoding) failed");
            }

            try
            {
                var e = ioResult.Result[0];
                if (e.StartsWith("CP"))
                {
                    this.encoding = Encoding.GetEncoding(int.Parse(e.Substring(2)));
                }
                else
                {
                    this.encoding = Encoding.GetEncoding(e);
                }
            }
            catch
            {
                this.Close();
                return new StartResult("No matching encoding");
            }

            // Success.
            return new StartResult();
        }

        /// <summary>
        /// Start an emulator session.
        /// </summary>
        /// <returns>Success/failure and failure text.</returns>
        /// <remarks>
        /// <note type="caution">
        /// <para>When an application is finished with the emulator, including when the application exits,
        /// it must call the <see cref="Close"/> method to clean up. Otherwise, system resources may be leaked, such as
        /// orphaned server processes.</para></note></remarks>
        public StartResult Start()
        {
            try
            {
                return this.StartAsync().Result;
            }
            catch (AggregateException e)
            {
                throw e.InnerException;
            }
        }

        /// <summary>
        /// Stop the emulator session and clean up resources it is using.
        /// </summary>
        /// <param name="saveHistory">If true, recent commands will be saved.</param>
        public void Close(bool saveHistory = false)
        {
            // Stop the underlying session.
            this.BackEnd.Close();

            // Reset the state.
            this.EmulatorRunning = false;
            this.ExceptionMode = false;

            // Wipe out history, if we're asked to.
            if (!saveHistory)
            {
                lock (this.recentCommandsLock)
                {
                    this.recentCommands = new LinkedList<IoResult>();
                }
            }
        }

        /// <summary>
        /// Dispose method.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Internal Dispose method.
        /// </summary>
        /// <param name="disposing">True if being called from the public Dispose method.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (this.disposed)
            {
                return;
            }

            if (disposing)
            {
                this.handle.Dispose();

                // Free other managed objects.
                this.Close();
            }

            this.disposed = true;
        }
    }
}
