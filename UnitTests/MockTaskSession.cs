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

namespace UnitTests
{
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Runtime.InteropServices;
    using System.Threading.Tasks;

    using Microsoft.Win32.SafeHandles;

    using Mock;

    using X3270if;

    /// <summary>
    /// Startup class for task-based mock back-end
    /// </summary>
    public class MockTaskConfig : Config
    {
        // No mock thread-specific config info yet.
    }

    /// <summary>
    /// Task-based mock back-end
    /// </summary>
    public class MockTaskSession : Session
    {
        /// <summary>
        /// The back end.
        /// </summary>
        private MockBackEnd mockBackEnd;

        /// <summary>
        /// Initializes a new instance of the <see cref="MockTaskSession"/> class.
        /// </summary>
        /// <param name="config">The configuration.</param>
        public MockTaskSession(MockTaskConfig config = null) : base(config, new MockBackEnd(config))
        {
            // Remember the back end.
            this.mockBackEnd = (MockBackEnd)BackEnd;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MockTaskSession"/> class.
        /// Forces the base constructor to crash.
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <param name="forceChaos">Flag indicating that a crash should be forced</param>
        public MockTaskSession(MockTaskConfig config, bool forceChaos) : base(config, null)
        {
            // Base constructor should have blown up at this point.
        }

        /// <summary>
        /// Gets the last command processed.
        /// </summary>
        public string LastCommandProcessed
        {
            get { return this.mockBackEnd.LastCommandProcessed; }
        }

        /// <summary>
        /// Sets a value indicating whether all commands should fail.
        /// </summary>
        public bool AllFail
        {
            set { this.mockBackEnd.AllFail = value; }
        }

        /// <summary>
        /// Sets a value in milliseconds that the next command should hang for.
        /// </summary>
        public int HangMsec
        {
            set { this.mockBackEnd.HangMsec = value; }
        }

        /// <summary>
        /// Sets the server code page.
        /// </summary>
        public string CodePage
        {
            set { this.mockBackEnd.CodePage = value; }
        }

        /// <summary>
        /// Sets a value indicating whether the server should fail a code page query.
        /// </summary>
        public bool CodePageFail
        {
            set { this.mockBackEnd.CodePageFail = value; }
        }

        /// <summary>
        /// Sets a value indicating whether the server is connected.
        /// </summary>
        public bool Connected
        {
            set { this.mockBackEnd.Connected = value; }
        }
    }

    /// <summary>
    /// A mock version of a 3270 emulator session, implemented with a Task.
    /// Has some special knobs for forcing particular kinds of error conditions.
    /// </summary>
    public class MockBackEnd : IBackEnd
    {
        /// <summary>
        /// Has Dispose already been called?
        /// </summary>
        private bool disposed = false;

        /// <summary>
        /// SafeHandle instance for Dispose.
        /// </summary>
        private SafeHandle handle = new SafeFileHandle(IntPtr.Zero, true);

        /// <summary>
        /// The configuration.
        /// </summary>
        private MockTaskConfig config;

        /// <summary>
        /// The mock server object.
        /// </summary>
        private Server mockServer = null;

        /// <summary>
        /// TCP connection to the mock session.
        /// </summary>
        private TcpClient client = null;

        /// <summary>
        /// The server task.
        /// </summary>
        private Task server = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="MockBackEnd"/> class.
        /// Constructor, which ensures there is a Server object.
        /// </summary>
        /// <param name="config">The configuration</param>
        public MockBackEnd(MockTaskConfig config)
        {
            this.config = config ?? new MockTaskConfig();
            this.mockServer = new Server();
        }

        /// <summary>
        /// Gets the Last command received by the server.
        /// </summary>
        public string LastCommandProcessed
        {
            get
            {
                return (this.mockServer != null) ? this.mockServer.LastCommandProcessed : null;
            }
        }

        /// <summary>
        /// Sets a value indicating whether all commands should fail.
        /// </summary>
        public bool AllFail
        {
            set
            {
                if (this.mockServer != null)
                {
                    this.mockServer.AllFail = value;
                }
            }
        }

        /// <summary>
        /// Sets a value in milliseconds to force the next command to hang.
        /// </summary>
        public int HangMsec
        {
            set
            {
                if (this.mockServer != null)
                {
                    this.mockServer.HangMsec = value;
                }
            }
        }

        /// <summary>
        /// Sets the workstation code page.
        /// </summary>
        public string CodePage
        {
            set
            {
                if (this.mockServer != null)
                {
                    this.mockServer.CodePage = value;
                }
            }
        }

        /// <summary>
        /// Sets a value indicating whether the code page query should fail.
        /// </summary>
        public bool CodePageFail
        {
            set
            {
                if (this.mockServer != null)
                {
                    this.mockServer.CodePageFail = value;
                }
            }
        }

        /// <summary>
        /// Sets a value indicating whether the server is connected.
        /// </summary>
        public bool Connected
        {
            set
            {
                if (this.mockServer != null)
                {
                    this.mockServer.Connected = value;
                }
            }
        }

        /// <summary>
        /// Asynchronous start method.
        /// </summary>
        /// <returns>Start result</returns>
        public async Task<StartResult> StartAsync()
        {
            // Create a listening socket, letting the system pick an unused port.
            var listener = new Socket(SocketType.Stream, ProtocolType.Tcp);
            listener.Bind(new IPEndPoint(IPAddress.Loopback, 0));
            listener.Listen(1);

            // Create a listener and accept a connection.
            // The task is intenionally not awaited, so it runs asynchronously.
            // The listener stops the listening socket as soon as it accepts one connection.
            this.server = Task.Run(() => this.mockServer.Ws3270(listener));

            // Create the client connection.
            var endPoint = (IPEndPoint)listener.LocalEndPoint;
            var result = await SessionUtil.TryConnect(endPoint.Port, this.config.ConnectRetryMsec).ConfigureAwait(continueOnCapturedContext: false);
            if (result.Success)
            {
                this.client = result.Client;
                return new StartResult();
            }
            else
            {
                this.Close();
                return new StartResult(result.FailReason);
            }
        }

        /// <summary>
        /// Return the TCP client associated with the session.
        /// </summary>
        /// <returns>TCP client object.</returns>
        public TcpClient GetClient()
        {
            return this.client;
        }

        /// <summary>
        /// Get the error output from the server.
        /// </summary>
        /// <param name="fallbackText">Text to return if there is no error output</param>
        /// <returns>The fallback test</returns>
        public string GetErrorOutput(string fallbackText)
        {
            return fallbackText;
        }

        /// <summary>
        /// Close the session.
        /// </summary>
        public void Close()
        {
            if (this.client != null)
            {
                this.client.Close();
                this.client = null;
            }

            // Wait for the server to stop.
            if (this.server != null)
            {
                this.server.Wait();
            }
        }

        /// <summary>
        /// Public Dispose method.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Internal Dispose method.
        /// </summary>
        /// <param name="disposing">True if called from public Dispose method.</param>
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
