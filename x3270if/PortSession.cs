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
using System.Net;
using System.Net.Sockets;
using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;

namespace x3270if
{
    /// <summary>
    /// The startup class for a port-based session.
    /// A common start-up object, plus the TCP port.
    /// </summary>
    public class PortConfig : Config
    {
        /// <summary>
        /// TCP port to connect to.
        /// </summary>
        public int Port;
        /// <summary>
        /// Automatically start the session when constructing an instance.
        /// </summary>
        public bool AutoStart = true;
    }

    /// <summary>
    /// A port-based session.
    /// </summary>
    public class PortSession : Session
    {
        /// <summary>
        /// Constructor, given a configuration.
        /// </summary>
        /// <param name="config">Configuration.</param>
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
        // Has Dispose already been called? 
        private bool Disposed = false;

        // SafeHandle instance for Dispose.
        private SafeHandle Handle = new SafeFileHandle(IntPtr.Zero, true);

        // TCP client (socket)
        private TcpClient Client = null;

        // Configuration parameters.
        private PortConfig PortConfig;

        /// <summary>
        /// Constructor, given a configuration.
        /// </summary>
        /// <param name="config">Configuration</param>
        public PortBackEnd(PortConfig config)
        {
            PortConfig = config ?? new PortConfig();
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
        protected void Dispose(bool disposing)
        {
            if (Disposed)
            {
                return;
            }

            if (disposing)
            {
                Handle.Dispose();
                // Free other managed objects.
                Close();
            }
            Disposed = true;
        }

        /// <summary>
        /// Connect to an existing emulator process
        /// </summary>
        /// <returns>Success indication and error message</returns>
        public async Task<startResult> StartAsync()
        {
            int port;

            if (PortConfig.Port != 0)
            {
                port = PortConfig.Port;
            }
            else
            {
                var portString = Environment.GetEnvironmentVariable(Util.x3270Port);
                if (portString == null)
                {
                    return new startResult(Util.x3270Port + " not found in the environment");
                }

                // Parse it.
                UInt16 port16;
                if (!UInt16.TryParse(portString, out port16))
                {
                    return new startResult("Invalid " + Util.x3270Port + " in the environment");
                }
                port = port16;
            }

            var result = await SessionUtil.TryConnect(port, PortConfig.ConnectRetryMsec).ConfigureAwait(continueOnCapturedContext: false);
            if (result.Success)
            {
                Client = result.Client;
                return new startResult();
            }
            else
	        {
                return new startResult(result.FailReason);
	        }
        }

        /// <summary>
        /// Get the TCP client for a session.
        /// </summary>
        /// <returns>TcpClient object</returns>
        public TcpClient GetClient()
        {
            return Client;
        }
        
        /// <summary>
        /// Fetch the error output from the emulator.
        /// </summary>
        /// <param name="fallbackText">Text to return if there is nothing from the emulator</param>
        /// <returns>Error text</returns>
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
            if (Client != null)
            {
                Client.Close();
                Client = null;
            }
        }
    }
}
