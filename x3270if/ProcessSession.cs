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
using System.Linq;
using System.Collections.Generic;
using x3270if.ProcessOptions;

namespace x3270if
{
    namespace ProcessOptions
    {
        /// <summary>
        /// Abstract class for defining additional ws3270 process options.
        /// </summary>
        public abstract class ProcessOption
        {
            private string optionName;
            /// <summary>
            /// The option name.
            /// </summary>
            protected string OptionName
            {
                get
                {
                    return optionName;
                }
                set
                {
                    if (value == null)
                    {
                        throw new ArgumentNullException("name");
                    }
                    if (String.IsNullOrWhiteSpace(value) ||
                        value.Substring(0, 1) == "-" ||
                        value.ToCharArray().Any(c => c == '"' || c == '\\' || Char.IsWhiteSpace(c) || Char.IsControl(c)))
                    {
                        throw new ArgumentException("name");
                    }
                    optionName = value;
                }
            }

            /// <summary>
            /// Expand an option into a properly-quoted string to pass on the command line.
            /// </summary>
            /// <returns>Quoted string.</returns>
            public abstract string Quote();
        }

        /// <summary>
        /// Extra command-line option without a paramter.
        /// </summary>
        public class ProcessOptionWithoutValue : ProcessOption
        {
            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="optionName">Option name. Must not begin with '-'.</param>
            public ProcessOptionWithoutValue(string optionName)
            {
                OptionName = optionName;
            }
            /// <summary>
            /// Expand an option into a properly-quoted string to pass on the command line.
            /// </summary>
            /// <returns>Quoted string.</returns>
            public override string Quote()
            {
                return "-" + OptionName;
            }
        }

        /// <summary>
        /// Extra command-line option with a parameter.
        /// </summary>
        public class ProcessOptionWithValue : ProcessOption
        {
            private string optionValue;
            /// <summary>
            /// Option value.
            /// </summary>
            protected string OptionValue
            {
                get
                {
                    return optionValue;
                }
                set
                {
                    if (value == null)
                    {
                        throw new ArgumentNullException("value");
                    }
                    if (value.ToCharArray().Any(c => c == '"' || Char.IsControl(c)))
                    {
                        throw new ArgumentException("value");
                    }
                    optionValue = value;
                }
            }

            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="option">Option name. Must not begin with '-'.</param>
            /// <param name="value">Option value.</param>
            public ProcessOptionWithValue(string option, string value)
            {
                OptionName = option;
                OptionValue = value;
            }

            /// <summary>
            /// Set the optionValue, allowing certain control codes through.
            /// </summary>
            /// <param name="value"></param>
            private void SetValueWithControl(string value)
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                if (value.ToCharArray().Any(c => c == '"' || (Char.IsControl(c) && !"\r\n\b\f\t".Contains(c))))
                {
                    throw new ArgumentException("value");
                }
                optionValue = value;
            }

            /// <summary>
            /// Constructor, allowing certain C control characters in the value.
            /// </summary>
            /// <param name="option">Option name. Must not begin with '-'.</param>
            /// <param name="value">Option value.</param>
            /// <param name="allowCControl">If true, allow certain C control characters in <paramref name="value"/>.</param>
            protected ProcessOptionWithValue(string option, string value, bool allowCControl = true)
            {
                OptionName = option;
                if (allowCControl)
                {
                    SetValueWithControl(value);
                }
                else
                {
                    OptionValue = value;
                }
            }

            /// <summary>
            /// Expand an option into a properly-quoted string to pass on the command line.
            /// </summary>
            /// <returns>Quoted string.</returns>
            public override string Quote()
            {
                // Note: Command-line options do not generally need quoted backslashes.
                return String.Format("-{0} {1}", OptionName, Session.QuoteString(OptionValue, quoteBackslashes: false));
            }
        }

        /// <summary>
        /// Extra command-line "-xrm" option.
        /// </summary>
        public class ProcessOptionXrm : ProcessOptionWithValue
        {
            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="resource">Resource name. Do not include "ws3270." in the name.</param>
            /// <param name="value">Resource value.</param>
            public ProcessOptionXrm(string resource, string value)
                : base("xrm", String.Format("ws3270.{0}: {1}", resource, value), allowCControl: true)
            {
            }

            /// <summary>
            /// Expand an option into a properly-quoted string to pass on the command line.
            /// </summary>
            /// <returns>Quoted string.</returns>
            public override string Quote()
            {
                // Note: -xrm options *do* need quoted backslashes.
                return String.Format("-{0} {1}", OptionName, Session.QuoteString(OptionValue, quoteBackslashes: true));
            }
        }
    }

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
        public IEnumerable<ProcessOption> ExtraOptions;

        /// <summary>
        /// First options. Used to force the mock emulator to fall over by not making
        /// -scriptport the first option.
        /// <para>This option is present for unit testing, not for general use.</para>
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
                if (ProcessConfig.ExtraOptions != null)
                {
                    arguments += " " + String.Join(" ", ProcessConfig.ExtraOptions.Select(o => o.Quote()));
                }

                // Build up the parameters for ws3270.
                var info = new ProcessStartInfo(ProcessConfig.ProcessName);
                info.UseShellExecute = false;
                info.CreateNoWindow = true;
                info.RedirectStandardError = true;
                info.RedirectStandardOutput = true;
                info.Arguments = string.Empty;

                // Put special unit test options first, intended to throw off the "-scriptport"-first logic below.
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

                Util.Log("ProcessSession Start: ProcessName '{0}', arguments '{1}'", ProcessConfig.ProcessName, info.Arguments);

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
                    return new startResult(emulatorErrorMessage);
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