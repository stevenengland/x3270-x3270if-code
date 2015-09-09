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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace x3270if
{
    public partial class Session
    {
        /// <summary>
        /// Check a name (host, port or LU) for the presence of metacharacters that could confuse the hostname parser.
        /// </summary>
        /// <param name="name">Name to check.</param>
        /// <param name="extraChars">Additional illegal characters.</param>
        private void CheckName(string name, string extraChars = null)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Empty name");
            }

            var invalidNameChars = "[]@/";
            if (extraChars != null)
            {
                invalidNameChars += extraChars;
            }
            if (name.Any(c => invalidNameChars.Contains(c)))
            {
                throw new ArgumentException(string.Format("name '{0}' contains invalid character(s)", name));
            }
        }

        private const string flagsTranslate = "CLNPSB";

        /// <summary>
        /// Expand a hostname and parameters into a host string that ws3270 understands.
        /// </summary>
        /// <param name="host">Hostname, can be symbolic or numeric.</param>
        /// <param name="port">TCP port number.</param>
        /// <param name="lus">Set of LU names to try to connect to.</param>
        /// <param name="flags">Connection flags (SSL, etc.).</param>
        /// <returns>Encoded host string.</returns>
        public string ExpandHostName(string host, string port = null, IEnumerable<string> lus = null, ConnectFlags flags = ConnectFlags.None)
        {
            string hostString = string.Empty;

            // Map the symbolic flags onto options.
            ConnectFlags thisConnectFlags = (flags != ConnectFlags.None) ? flags : Config.DefaultConnectFlags;
            for (int i = 0; i < flagsTranslate.Length; i++)
            {
                if ((thisConnectFlags & (ConnectFlags)(1 << i)) != 0)
                {
                    hostString += flagsTranslate.Substring(i, 1) + ":";
                }
            }

            // Add the LUs.
            if (lus != null)
            {
                foreach (string lu in lus)
                {
                    CheckName(lu, ":");
                }
                hostString += System.String.Join(",", lus) + "@";
            }

            // Now the host name.
            CheckName(host);
            if (host.Contains(':'))
            {
                hostString += "[" + host + "]";
            }
            else
            {
                hostString += host;
            }

            // Add the port.
            if (!string.IsNullOrEmpty(port))
            {
                CheckName(port, ":.");
                hostString += ":" + port;
            }

            return QuoteString(hostString);
        }

        /// <summary>
        /// Connect to a host. Asynchronous version.
        /// </summary>
        /// <param name="host">Hostname.</param>
        /// <param name="port">Optional TCP port number or service name.</param>
        /// <param name="lu">Optional set of LU names to try to connect to.</param>
        /// <param name="flags">Connection flags (SSL, etc.).</param>
        /// <returns>Task returning success/failure and failure text.</returns>
        /// <exception cref="InvalidOperationException">Session is not started.</exception>
        /// <exception cref="X3270ifCommandException"><see cref="ExceptionMode"/> is enabled and the command fails.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="host"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="host"/>, <paramref name="port"/> or <paramref name="lu"/> contain invalid characters.</exception>
        public async Task<IoResult> ConnectAsync(string host, string port = null, IEnumerable<string> lu = null, ConnectFlags flags = ConnectFlags.None)
        {
            if (host == null)
            {
                throw new ArgumentNullException("host");
            }
            if (string.IsNullOrEmpty(host))
            {
                throw new ArgumentException("host");
            }
            return await IoAsync("Connect(" + ExpandHostName(host, port, lu, flags) + ")").ConfigureAwait(continueOnCapturedContext: false);
        }

        /// <summary>
        /// Disconnect from the host. Asynchronous version.
        /// </summary>
        /// <returns>Task returning success/failure and failure text.</returns>
        /// <exception cref="InvalidOperationException">Session is not started.</exception>
        /// <exception cref="X3270ifCommandException"><see cref="ExceptionMode"/> is enabled and the command fails.</exception>
        public async Task<IoResult> DisconnectAsync()
        {
            return await IoAsync("Disconnect()").ConfigureAwait(continueOnCapturedContext: false);
        }
        
        /// <summary>
        /// Connect to a host.
        /// </summary>
        /// <param name="host">Hostname.</param>
        /// <param name="port">Optional TCP port number or service name.</param>
        /// <param name="lu">Optional set of LU names to try to connect to.</param>
        /// <param name="flags">Connection flags (SSL, etc.).</param>
        /// <returns>Success/failure and failure text.</returns>
        /// <exception cref="InvalidOperationException">Session is not started.</exception>
        /// <exception cref="X3270ifCommandException"><see cref="ExceptionMode"/> is enabled and the command fails.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="host"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="host"/>, <paramref name="port"/> or <paramref name="lu"/> contain invalid characters.</exception>
        public IoResult Connect(string host, string port = null, IEnumerable<string> lu = null, ConnectFlags flags = ConnectFlags.None)
        {
            try
            {
                return ConnectAsync(host, port, lu, flags).Result;
            }
            catch (AggregateException e)
            {
                throw e.InnerException;
            }
        }

        /// <summary>
        /// Disconnect from a host.
        /// </summary>
        /// <returns>Success/failure and failure text.</returns>
        /// <exception cref="InvalidOperationException">Session is not started.</exception>
        /// <exception cref="X3270ifCommandException"><see cref="ExceptionMode"/> is enabled and the command fails.</exception>
        public IoResult Disconnect()
        {
            try
            {
                return DisconnectAsync().Result;
            }
            catch (AggregateException e)
            {
                throw e.InnerException;
            }
        }
    }
}
