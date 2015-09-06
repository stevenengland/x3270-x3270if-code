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
using System.Threading.Tasks;
using x3270if;
using System.Net;
using System.Net.Sockets;
using Mock;
using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;

namespace UnitTests
{
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
    public class MockTaskSession : Session, IDisposable
    {
        // Has Dispose already been called? 
        private bool disposed = false;

        // SafeHandle instance for Dispose.
        private SafeHandle handle = new SafeFileHandle(IntPtr.Zero, true);

        // The back end
        private MockBackEnd mockBackEnd;

        public MockTaskSession(MockTaskConfig config = null) : base(config, new MockBackEnd(config))
        {
            // Remember the back end.
            mockBackEnd = (MockBackEnd)base.backEnd;
        }

        public MockTaskSession(MockTaskConfig config, bool forceChaos) : base(config, null)
        {
            // Base constructor should have blown up at this point.
        }

        public string LastCommandProcessed
        {
            get { return mockBackEnd.LastCommandProcessed; }
        }

        public bool AllFail
        {
            set { mockBackEnd.AllFail = value; }
        }

        public int HangMsec
        {
            set { mockBackEnd.HangMsec = value; }
        }

        public string CodePage
        {
            set { mockBackEnd.CodePage = value; }
        }

        public bool CodePageFail
        {
            set { mockBackEnd.CodePageFail = value; }
        }


        // Dispose methods.
        /// <summary>
        /// Public Dispose method.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Private Dispose method.
        /// </summary>
        /// <param name="disposing">true if called from public Dispose.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }

            if (disposing)
            {
                handle.Dispose();
                // Free other managed objects.
                Close();
            }
            disposed = true;
        }
    }

    /// <summary>
    /// A mock version of a 3270 emulator session, implemented with a Task.
    /// Has some special knobs for forcing particular kinds of error conditions.
    /// </summary>
    public class MockBackEnd : IBackEnd
    {
        // Has Dispose already been called? 
        private bool disposed = false;

        // SafeHandle instance for Dispose.
        private SafeHandle handle = new SafeFileHandle(IntPtr.Zero, true);

        // Configuration.
        private MockTaskConfig Config;

        // The mock server object.
        Server MockServer = null;

        // TCP connection to the mock session.
        private TcpClient Client = null;

        // Last command received by the server.
        public string LastCommandProcessed
        {
            get
            {
                return (MockServer != null) ? MockServer.LastCommandProcessed : null;
            }
        }

        // Force all commands to fail.
        public bool AllFail
        {
            set
            {
                if (MockServer != null)
                {
                    MockServer.AllFail = value;
                }
            }
        }

        // Force commands to hang.
        public int HangMsec
        {
            set
            {
                if (MockServer != null)
                {
                    MockServer.HangMsec = value;
                }
            }
        }

        // Set the code page.
        public string CodePage
        {
            set
            {
                if (MockServer != null)
                {
                    MockServer.CodePage = value;
                }
            }
        }

        // Set the code page failure flag.
        public bool CodePageFail
        {
            set
            {
                if (MockServer != null)
                {
                    MockServer.CodePageFail = value;
                }
            }
        }

        // Server task.
        private Task Server = null;

        // Constructor, which ensures there is a Server object.
        public MockBackEnd(MockTaskConfig config)
        {
            this.Config = config ?? new MockTaskConfig();
            MockServer = new Server();
        }

        public async Task<startResult> StartAsync()
        {
            // Create a listening socket, letting the system pick an unused port.
            var listener = new Socket(SocketType.Stream, ProtocolType.Tcp);
            listener.Bind(new IPEndPoint(IPAddress.Loopback, 0));
            listener.Listen(1);

            // Create a listener and accept a connection.
            // The task is intenionally not awaited, so it runs asynchronously.
            // The listener stops the listening socket as soon as it accepts one connection.
            Server = Task.Run(() => MockServer.Ws3270(listener));

            // Create the client connection.
            var endPoint = (IPEndPoint)listener.LocalEndPoint;
            var result = await SessionUtil.TryConnect(endPoint.Port, Config.ConnectRetryMsec).ConfigureAwait(continueOnCapturedContext: false);
            if (result.Success)
            {
                Client = result.Client;
                return new startResult();
            }
            else
            {
                Close();
                return new startResult(result.FailReason);
            }
        }

        /// <summary>
        /// Return the TCP client associated with the session.
        /// </summary>
        /// <returns>TCP client object.</returns>
        public TcpClient GetClient()
        {
            return Client;
        }

        public string GetErrorOutput(string fallbackText)
        {
            return fallbackText;
        }

        public void Close()
        {
            if (Client != null)
            {
                Client.Close();
                Client = null;
            }

            // Wait for the server to stop.
            if (Server != null)
            {
                Server.Wait();
            }
        }

        /// <summary>
        /// Public Dispose method.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Internal Dispose method.
        /// </summary>
        /// <param name="disposing">true if called from public Dispose method</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }

            if (disposing)
            {
                handle.Dispose();
                // Free other managed objects.
                Close();
            }
            disposed = true;
        }
    }
}
