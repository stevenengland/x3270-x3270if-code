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
using System.Threading;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Runtime.Serialization;

namespace x3270if
{
    /// <summary>
    /// Base exception class for exception mode. Nothing should use this class directly.
    /// </summary>
    [Serializable]
    public class X3270ifException : Exception, ISerializable
    {
        /// <summary>
        /// Basic constructor.
        /// </summary>
        /// <param name="explanation">Explanatory text.</param>
        protected X3270ifException(string explanation) : base(explanation)
        {
        }
    }

    /// <summary>
    /// Exception class for command exceptions (generated when a command fails and
    /// exception mode is set).
    /// </summary>
    [Serializable]
    public class X3270ifCommandException : X3270ifException
    {
        /// <summary>
        /// Basic constructor.
        /// </summary>
        /// <param name="explanation">Explanatory text.</param>
        public X3270ifCommandException(string explanation) : base(explanation)
        {
        }
    }

    /// <summary>
    /// Exception class for x3270if operational errors (internal operation failed).
    /// </summary>
    [Serializable]
    public class X3270ifInternalException : X3270ifException
    {
        /// <summary>
        /// Basic constructor.
        /// </summary>
        /// <param name="explanation">Explanatory text.</param>
        public X3270ifInternalException(string explanation)
            : base(explanation)
        {
        }
    }

    /// <summary>
    /// Interface to the back end.
    /// </summary>
    public interface IBackEnd : IDisposable
    {
        // This Interface is abstracted for two reasons: to allow it to be mocked for testing purposes, and
        // because the separation makes the implementation easier to understand.

        /// <summary>
        /// Start a new emulator instance (if part of the class' behavior) and connect to it.
        /// </summary>
        /// <returns>Start result.</returns>
        Task<startResult> StartAsync();

        /// <summary>
        /// Get the TCP client for a successful connection.
        /// </summary>
        /// <returns>TCP client object.</returns>
        TcpClient GetClient();

        /// <summary>
        /// Get asynchronous error text from the emulator.
        /// </summary>
        /// <param name="fallbackText">Fallback text to return, in case there was no output.</param>
        /// <returns>Error text.</returns>
        string GetErrorOutput(string fallbackText);

        /// <summary>
        /// Stop emulator connection, and terminate any process that was started.
        /// </summary>
        void Close();
    }

    /// <summary>
    /// An emulator session.
    /// </summary>
    public partial class Session
    {
        /// <summary>
        /// Is this instance running?
        /// </summary>
        public bool Running
        {
            get;
            private set;
        }

        /// <summary>
        /// The initialization object, if any.
        /// </summary>
        public readonly Config Config = new Config();

        /// <summary>
        /// The connection object, real or emulated.
        /// </summary>
        protected IBackEnd backEnd = null;

        // The text encoding for talking to the emulator.
        private System.Text.Encoding Encoding = System.Text.Encoding.UTF8;

        // The TcpClient (socket) for communicating with the emulator.
        private TcpClient client
        {
            get { return backEnd.GetClient(); }
        }

        // The list of recent commands.
        private LinkedList<IoResult> recentCommands = new LinkedList<IoResult>();
        private object recentCommandsLock = new object();

        /// <summary>
        /// Fetch the last command.
        /// </summary>
        public IoResult LastCommand
        {
            get
            {
                lock (recentCommandsLock)
                {
                    if (recentCommands.Count() == 0)
                    {
                        return null;
                    }
                    return recentCommands.First();
                }
            }
        }

        /// <summary>
        /// Fetch the set of recent commands.
        /// <para>These are the actual commands sent and responses received from the emulator.
        /// The data reported by the <see cref="Status"/> attribute, the <see cref="Query"/>
        /// method and the <see cref="StatusField"/> method may be translated to conform to the
        /// session's <see cref="x3270if.Config.Origin"/>.</para>
        /// </summary>
        public IoResult[] RecentCommands
        {
            get
            {
                lock (recentCommandsLock)
                {
                    return recentCommands.ToArray();
                }
            }
        }

        private string lastStatus;
        /// <summary>
        /// Fetch the last status.
        /// This may be different from the StatusLine in the recent history, because of <see cref="x3270if.Config.Origin"/> translation.
        /// </summary>
        public string Status
        {
            get
            {
                return lastStatus;
            }
        }

        /// <summary>
        /// The limit on the number of commands to save.
        /// </summary>
        public const int MaxCommands = 5;

        // Store a new command status.
        private IoResult SaveRecentCommand(IoResult result)
        {
            lock (recentCommandsLock)
            {
                recentCommands.AddFirst(result);
                if (recentCommands.Count() > MaxCommands)
                {
                    recentCommands.RemoveLast();
                }
            }
            return result;
        }

        /// <summary>
        /// If true, failed commands will raise exceptions.
        /// </summary>
        public bool ExceptionMode = false;

        /// <summary>
        /// Constructor, given a configuration and a back end.
        /// </summary>
        /// <param name="config">Configuration</param>
        /// <param name="emulatorConnection">Back end</param>
        protected Session(Config config, IBackEnd emulatorConnection)
        {
            this.Config = config ?? new Config();

            if (emulatorConnection == null)
            {
                throw new ArgumentNullException("emulatorConnection");
            }
            this.backEnd = emulatorConnection;
        }

        /// <summary>
        /// Return a status field.
        /// <para>For cursor coordinates, the value may have been translated to conform to the session's <see cref="x3270if.Config.Origin"/>.</para>
        /// </summary>
        /// <param name="index">Zero-based index of field to return (int cast of StatusLineField).</param>
        /// <returns>Field value</returns>
        public string StatusField(StatusLineField index)
        {
            if (!Running)
            {
                throw new InvalidOperationException("Not running");
            }
            return Status.Split(' ')[(int)index];
        }
    }
}
