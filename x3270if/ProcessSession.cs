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
    using System.Diagnostics;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Runtime.InteropServices;
    using System.Threading.Tasks;

    using Microsoft.Win32.SafeHandles;

    using ProcessOptions;

    /// <summary>
    /// The startup class for a process-based session.
    /// A common start-up object, plus the process name.
    /// </summary>
    public class ProcessConfig : Config
    {
        /// <summary>
        /// Emulator process name, e.g., ws3270.exe.
        /// </summary>
        private string processName = "ws3270.exe";

        /// <summary>
        /// Backing field for <see cref="Model"/>.
        /// </summary>
        private int model = 4;

        /// <summary>
        /// Gets or sets the emulator process name.
        /// </summary>
        public string ProcessName
        {
            get
            {
                return this.processName;
            }

            set
            {
                this.processName = value;
            }
        }

        /// <summary>
        /// Gets or sets extra options added to the emulator process command line.
        /// </summary>
        public IEnumerable<ProcessOption> ExtraOptions { get; set; }

        /// <summary>
        /// Gets or sets the first options. Used to force the mock emulator to fall over by not making
        /// <c>-scriptport</c> the first option.
        /// <para>This option is present for unit testing, not for general use.</para>
        /// </summary>
        public string TestFirstOptions { get; set; }

        /// <summary>
        /// Gets or sets the 3270 model number. The default is 4.
        /// </summary>
        public int Model
        {
            get
            {
                return this.model;
            }

            set
            {
                switch (value)
                {
                    case 2:
                    case 3:
                    case 4:
                    case 5:
                        this.model = value;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("model");
                }
            }
        }
    }

    /// <summary>
    /// A process-based session.
    /// </summary>
    public class ProcessSession : Session
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessSession"/> class.
        /// Constructor, given a configuration.
        /// </summary>
        /// <param name="config">Configuration to use.</param>
        public ProcessSession(ProcessConfig config = null) : base(config, new ProcessBackEnd(config))
        {
        }
    }

    /// <summary>
    /// Process-based emulator connection.
    /// The emulator might be a real copy of ws3270.exe, or could be a mock.
    /// </summary>
    public class ProcessBackEnd : IBackEnd
    {
        /// <summary>
        /// Has Dispose already been called?
        /// </summary>
        private bool disposed = false;

        /// <summary>
        /// Instantiate a SafeHandle instance.
        /// </summary>
        private SafeHandle handle = new SafeFileHandle(IntPtr.Zero, true);

        /// <summary>
        /// TCP client (socket).
        /// </summary>
        private TcpClient client = null;

        /// <summary>
        /// The configuration.
        /// </summary>
        private ProcessConfig processConfig = new ProcessConfig();

        /// <summary>
        /// Started emulator process.
        /// </summary>
        private Process process = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessBackEnd"/> class.
        /// Constructor, given a configuration.
        /// </summary>
        /// <param name="config">Process configuration.</param>
        public ProcessBackEnd(ProcessConfig config)
        {
            this.processConfig = config ?? new ProcessConfig();
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
        /// Start a new emulator process. Asynchronous version.
        /// </summary>
        /// <returns>Success/failure and failure reason.</returns>
        /// <exception cref="InvalidOperationException">Arguments are too long.</exception>
        public async Task<StartResult> StartAsync()
        {
            using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.ReuseAddress, true);
                socket.Bind(new IPEndPoint(IPAddress.Any, 0));
                int port = ((IPEndPoint)socket.LocalEndPoint).Port;

                // Start with basic arguments:
                //  -utf8             UTF-8 mode
                //  -model n          Model number
                //  -scriptportonce   Make sure the emulator exists if the socket connection breaks
                var arguments = string.Format("-utf8 -model {0} -scriptportonce", this.processConfig.Model);

                // Add arbitrary extra options.
                if (this.processConfig.ExtraOptions != null)
                {
                    arguments += " " + string.Join(" ", this.processConfig.ExtraOptions.Select(o => o.Quote()));
                }

                // Build up the parameters for ws3270.
                var info = new ProcessStartInfo(this.processConfig.ProcessName);
                info.UseShellExecute = false;
                info.CreateNoWindow = true;
                info.RedirectStandardError = true;
                info.RedirectStandardOutput = true;
                info.Arguments = string.Empty;

                // Put special unit test options first, intended to throw off the "-scriptport"-first logic below.
                if (this.processConfig.TestFirstOptions != null)
                {
                    info.Arguments = this.processConfig.TestFirstOptions;
                }

                // It's important to put "-scriptport" first, because the Mock server looks for it and ignores everything else.
                info.Arguments += info.Arguments.JoinNonEmpty(" ", string.Format("-scriptport {0}", port));
                info.Arguments += info.Arguments.JoinNonEmpty(" ", arguments);

                // Check for argument overflow.
                // At some point, we could automatically put most arguments into a temporary session file, but for now,
                // we blow up, so arguments aren't silently ignored.
                var argsMax = (Environment.OSVersion.Version.Major >= 6 && Environment.OSVersion.Version.Minor >= 1) ? 32699 : 2080;
                if (this.processConfig.ProcessName.Length + 1 + info.Arguments.Length > argsMax)
                {
                    throw new InvalidOperationException("Arguments too long");
                }

                Util.Log("ProcessSession Start: ProcessName '{0}', arguments '{1}'", this.processConfig.ProcessName, info.Arguments);

                // Try starting it.
                try
                {
                    this.process = Process.Start(info);
                }
                catch (System.ComponentModel.Win32Exception e)
                {
                    // Typically Start errors are Win32 errors
                    return new StartResult(e.Message);
                }
                catch (Exception e)
                {
                    return new StartResult(string.Format("Caught exception {0}", e));
                }

                var result = await SessionUtil.TryConnect(port, this.processConfig.ConnectRetryMsec).ConfigureAwait(continueOnCapturedContext: false);
                if (!result.Success)
                {
                    var emulatorErrorMessage = this.GetErrorOutput(result.FailReason);
                    this.ZapProcess();
                    return new StartResult(emulatorErrorMessage);
                }
                else
                {
                    this.client = result.Client;
                    return new StartResult();
                }
            }
        }

        /// <summary>
        /// Get the TCP client for a session.
        /// </summary>
        /// <returns><see cref="TcpClient"/> object.</returns>
        public TcpClient GetClient()
        {
            return this.client;
        }

        /// <summary>
        /// Get <c>stderr</c> and <c>stdout</c> from the emulator, in case initial communication failed.
        /// </summary>
        /// <param name="fallbackText">Text to return if there is no output.</param>
        /// <returns>Error text.</returns>
        public string GetErrorOutput(string fallbackText)
        {
            string fail = string.Empty;
            if (this.process != null)
            {
                var s = this.process.StandardError.ReadToEnd();
                if (!string.IsNullOrEmpty(s))
                {
                    fail = "(stderr): " + s;
                }

                s = this.process.StandardOutput.ReadToEnd();
                if (!string.IsNullOrEmpty(s))
                {
                    fail += fail.JoinNonEmpty(", ", "(stdout): " + s);
                }
            }

            if (string.IsNullOrEmpty(fail))
            {
                fail = fallbackText;
            }

            return fail;
        }

        /// <summary>
        /// Close an emulator session.
        /// It can be restarted after this.
        /// </summary>
        public void Close()
        {
            this.ZapClient();
            this.ZapProcess();
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

        /// <summary>
        /// Kill the emulator process.
        /// </summary>
        private void ZapProcess()
        {
            if (this.process != null)
            {
                try
                {
                    this.process.Kill();
                }
                catch
                {
                }

                this.process.Dispose();
                this.process = null;
            }
        }

        /// <summary>
        /// Clean up the socket.
        /// </summary>
        private void ZapClient()
        {
            if (this.client != null)
            {
                this.client.Close();
                this.client = null;
            }
        }
    }
}