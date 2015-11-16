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
    using System.Net;
    using System.Net.Sockets;
    using System.Runtime.InteropServices;
    using System.Threading.Tasks;

    using Microsoft.Win32.SafeHandles;

    /// <summary>
    /// The startup class for a port-based session.
    /// A common start-up object, plus the TCP port.
    /// </summary>
    public class PortConfig : Config
    {
        /// <summary>
        /// Backing field for <see cref="AutoStart"/>.
        /// </summary>
        private bool autoStart = true;

        /// <summary>
        /// Gets or sets the TCP port to connect to.
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to automatically start the session when constructing an instance.
        /// </summary>
        public bool AutoStart
        {
            get
            {
                return this.autoStart;
            }

            set
            {
                this.autoStart = value;
            }
        }
    }

    /// <summary>
    /// A port-based session.
    /// </summary>
    public class PortSession : Session
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PortSession"/> class.
        /// Constructor, given a configuration.
        /// </summary>
        /// <param name="config">The configuration to use.</param>
        public PortSession(PortConfig config = null) : base(config, new PortBackEnd(config))
        {
            // Because they are usually used in child scripts, port sessions are auto-start
            // by default.
            if (config == null || config.AutoStart)
            {
                var startResult = Start();
                if (!startResult.Success)
                {
                    throw new X3270ifInternalException("Session failed to start: " + startResult.FailReason);
                }
            }
        }
    }

    /// <summary>
    /// Port-based emulator connection.
    /// </summary>
    public class PortBackEnd : IBackEnd
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
        /// TCP client (socket)
        /// </summary>
        private TcpClient client = null;

        /// <summary>
        /// Configuration parameters.
        /// </summary>
        private PortConfig portConfig;

        /// <summary>
        /// Initializes a new instance of the <see cref="PortBackEnd"/> class.
        /// Constructor, given a configuration.
        /// </summary>
        /// <param name="config">The configuration to use.</param>
        public PortBackEnd(PortConfig config)
        {
            this.portConfig = config ?? new PortConfig();
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
        /// Connect to an existing emulator process
        /// </summary>
        /// <returns>Success indication and error message.</returns>
        public async Task<StartResult> StartAsync()
        {
            int port;

            if (this.portConfig.Port != 0)
            {
                port = this.portConfig.Port;
            }
            else
            {
                var portString = Environment.GetEnvironmentVariable(Util.X3270Port);
                if (portString == null)
                {
                    return new StartResult(Util.X3270Port + " not found in the environment");
                }

                // Parse it.
                ushort port16;
                if (!ushort.TryParse(portString, out port16))
                {
                    return new StartResult("Invalid " + Util.X3270Port + " in the environment");
                }

                port = port16;
            }

            var result = await SessionUtil.TryConnect(port, this.portConfig.ConnectRetryMsec).ConfigureAwait(continueOnCapturedContext: false);
            if (result.Success)
            {
                this.client = result.Client;
                return new StartResult();
            }
            else
            {
                return new StartResult(result.FailReason);
            }
        }

        /// <summary>
        /// Get the TCP client for a session.
        /// </summary>
        /// <returns>New object.</returns>
        public TcpClient GetClient()
        {
            return this.client;
        }
        
        /// <summary>
        /// Fetch the error output from the emulator.
        /// </summary>
        /// <param name="fallbackText">Text to return if there is nothing from the emulator.</param>
        /// <returns>Error text.</returns>
        public string GetErrorOutput(string fallbackText)
        {
            return fallbackText;
        }

        /// <summary>
        /// Close an emulator session.
        /// It can be restarted after this.
        /// </summary>
        public void Close()
        {
            if (this.client != null)
            {
                this.client.Close();
                this.client = null;
            }
        }

        /// <summary>
        /// Private Dispose method.
        /// </summary>
        /// <param name="disposing">true if called from public Dispose.</param>
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
