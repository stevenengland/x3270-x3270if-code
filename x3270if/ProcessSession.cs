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
using System.Diagnostics;
using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;

namespace x3270if
{
    /// <summary>
    /// The startup class for a process-based session.
    /// A common start-up object, plus the process name.
    /// </summary>
    public class ProcessConfig : Config
    {
        /// <summary>
        /// Emulator process name, e.g., ws3270.exe.
        /// </summary>
        public string ProcessName = "ws3270.exe";

        /// <summary>
        /// Extra options added to the emulator process command line.
        /// </summary>
        public string ExtraOptions;

        /// <summary>
        /// First options. Used to force the mock emulator to fall over by not making
        /// -scriptport the first option.
        /// </summary>
        public string TestFirstOptions;

        private int model = 4;

        /// <summary>
        /// 3270 model number, default is 4.
        /// </summary>
        public int Model
        {
            get
            {
                return model;
            }
            set
            {
                switch (value)
                {
                    case 2:
                    case 3:
                    case 4:
                    case 5:
                        model = value;
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
        /// Constructor, given a configuration.
        /// </summary>
        /// <param name="config">Configuration.</param>
        public ProcessSession(ProcessConfig config = null) : base(config, new ProcessBackEnd(config))
        {
        }

        /// <summary>
        /// Format an "-xrm" option for passing to ws3270.
        /// </summary>
        /// <param name="resource">Resource name.</param>
        /// <param name="value">Resource value.</param>
        /// <returns>Formatted string.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="resource"/> or <paramref name="value"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="resource"/> or <paramref name="value"/> contains an invalid character.</exception>
        public static string XrmOption(string resource, string value)
        {
            if (resource == null)
            {
                throw new ArgumentNullException("resource");
            }
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            if (resource.Contains(" ") || resource.Contains("\"") || value.Contains("\""))
            {
                throw new ArgumentException("contains double quote");
            }
            return "-xrm " + QuoteString("ws3270." + resource + ": " + value);
        }
    }

    /// <summary>
    /// Process-based emulator connection.
    /// The emulator might be a real copy of ws3270.exe, or could be a mock.
    /// </summary>
    public class ProcessBackEnd : IBackEnd
    {
        // Has Dispose already been called? 
        private bool disposed = false;

        // Instantiate a SafeHandle instance.
        private SafeHandle handle = new SafeFileHandle(IntPtr.Zero, true);

        // TCP client (socket)
        private TcpClient Client = null;

        // Configuration.
        private ProcessConfig ProcessConfig = new ProcessConfig();

        // Started emulator process.
        private Process Process = null;

        /// <summary>
        /// Constructor, given a configuration.
        /// </summary>
        /// <param name="config">Process configuration.</param>
        public ProcessBackEnd(ProcessConfig config)
        {
            this.ProcessConfig = config ?? new ProcessConfig();
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
        /// <param name="disposing">True if called from public Dispose method.</param>
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

        /// <summary>
        /// Kill the emulator process.
        /// </summary>
        private void ZapProcess()
        {
            if (Process != null)
            {
                try
                {
                    Process.Kill();
                }
                catch
                {
                }
                Process.Dispose();
                Process = null;
            }
        }

        /// <summary>
        /// Clean up the TcpClient.
        /// </summary>
        private void ZapClient()
        {
            if (Client != null)
            {
                Client.Close();
                Client = null;
            }
        }

        /// <summary>
        /// Start a new emulator process, async version.
        /// </summary>
        /// <returns>Success/failure and failure reason.</returns>
        /// <exception cref="InvalidOperationException">Arguments are too long.</exception>
        public async Task<startResult> StartAsync()
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
                var arguments = string.Format("-utf8 -model {0} -scriptportonce", ProcessConfig.Model);

                // Add arbitrary extra options.
                arguments += arguments.JoinNonEmpty(" ", ProcessConfig.ExtraOptions);

                // Build up the parameters for ws3270.
                var info = new ProcessStartInfo(ProcessConfig.ProcessName);
                info.UseShellExecute = false;
                info.CreateNoWindow = true;
                info.RedirectStandardError = true;
                info.RedirectStandardOutput = true;
                info.Arguments = string.Empty;

                // Put special debug options first, intended to throw off the "-scriptport"-first logic below.
                if (ProcessConfig.TestFirstOptions != null)
                {
                    info.Arguments = ProcessConfig.TestFirstOptions;
                }
                // It's important to put "-scriptport" first, because the Mock server looks for it and ignores everything else.
                info.Arguments += info.Arguments.JoinNonEmpty(" ", string.Format("-scriptport {0}", port));
                info.Arguments += info.Arguments.JoinNonEmpty(" ", arguments);

                // Check for argument overflow.
                // At some point, we could automatically put most arguments into a temporary session file, but for now,
                // we blow up, so arguments aren't silently ignored.
                var argsMax = (Environment.OSVersion.Version.Major >= 6 && Environment.OSVersion.Version.Minor >= 1) ? 32699 : 2080;
                if (ProcessConfig.ProcessName.Length + 1 + info.Arguments.Length > argsMax)
                {
                    throw new InvalidOperationException("Arguments too long");
                }

                // Try starting it.
                try
                {
                    Process = Process.Start(info);
                }
                catch (System.ComponentModel.Win32Exception e)
                {
                    // Typically Start errors are Win32 errors
                    return new startResult(e.Message);
                }
                catch (Exception e)
                {
                    return new startResult(string.Format("Caught exception {0}", e));
                }

                var result = await SessionUtil.TryConnect(port, ProcessConfig.ConnectRetryMsec).ConfigureAwait(continueOnCapturedContext: false);
                if (!result.Success)
                {
                    var emulatorErrorMessage = GetErrorOutput(result.FailReason);
                    ZapProcess();
                    return new startResult(result.FailReason);
                }
                else
                {
                    Client = result.Client;
                    return new startResult();
                }
            }
        }

        /// <summary>
        /// Get the TCP client for a session.
        /// </summary>
        /// <returns>TcpClient object.</returns>
        public TcpClient GetClient()
        {
            return Client;
        }

        /// <summary>
        /// Get stderr and stdout from the emulator, in case initial communication failed.
        /// </summary>
        /// <param name="fallbackText">Text to return if there is no output.</param>
        /// <returns>Error text.</returns>
        public string GetErrorOutput(string fallbackText)
        {
            string fail = string.Empty;
            if (Process != null)
            {
                var s = Process.StandardError.ReadToEnd();
                if (!string.IsNullOrEmpty(s))
                {
                    fail = "(stderr): " + s;
                }
                s = Process.StandardOutput.ReadToEnd();
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
            ZapClient();
            ZapProcess();
        }
    }
}