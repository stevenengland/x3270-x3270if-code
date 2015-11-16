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
    using System.Linq;
    using System.Net.Sockets;
    using System.Runtime.Serialization;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Interface to the type-specific emulator back end.
    /// </summary>
    public interface IBackEnd : IDisposable
    {
        // This Interface is abstracted for two reasons: to allow it to be mocked for testing purposes, and
        // because the separation makes the implementation easier to understand.

        /// <summary>
        /// Start a new emulator instance (if part of the class' behavior) and connect to it.
        /// </summary>
        /// <returns>Start result.</returns>
        Task<StartResult> StartAsync();

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
    /// Base exception class for x3270if exceptions.
    /// </summary>
    [Serializable]
    public abstract class X3270ifException : Exception, ISerializable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="X3270ifException"/> class.
        /// </summary>
        /// <param name="explanation">Explanatory text.</param>
        protected X3270ifException(string explanation) : base(explanation)
        {
        }
    }

    /// <summary>
    /// Exception class for command exceptions (generated when a command fails and
    /// <see cref="Session.ExceptionMode"/> is set).
    /// </summary>
    [Serializable]
    public class X3270ifCommandException : X3270ifException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="X3270ifCommandException"/> class.
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
        /// Initializes a new instance of the <see cref="X3270ifInternalException"/> class.
        /// </summary>
        /// <param name="explanation">Explanatory text.</param>
        public X3270ifInternalException(string explanation)
            : base(explanation)
        {
        }
    }

    /// <summary>
    /// An emulator session.
    /// </summary>
    /// <remarks>This is an abstract class. To create a session, use a concrete class like <see cref="ProcessSession"/> or <see cref="PortSession"/>.</remarks>
    public abstract partial class Session : IDisposable
    {
        /// <summary>
        /// The limit on the number of commands to save.
        /// </summary>
        public const int MaxCommands = 5;

        /// <summary>
        /// The initialization object, if any.
        /// </summary>
        public readonly Config Config = new Config();

        /// <summary>
        /// The text encoding for talking to the emulator.
        /// </summary>
        private System.Text.Encoding encoding = System.Text.Encoding.UTF8;

        /// <summary>
        /// The list of recent commands.
        /// </summary>
        private LinkedList<IoResult> recentCommands = new LinkedList<IoResult>();

        /// <summary>
        /// Lock for <see cref="recentCommands"/>.
        /// </summary>
        private object recentCommandsLock = new object();

        /// <summary>
        /// Backing field for <see cref="StatusLine"/>.
        /// </summary>
        private string lastStatusLine;

        /// <summary>
        /// Initializes a new instance of the <see cref="Session"/> class.
        /// Constructor, given a configuration and a back end.
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <param name="emulatorConnection">Back end.</param>
        /// <exception cref="ArgumentNullException"><paramref name="emulatorConnection"/> is null.</exception>
        protected Session(Config config, IBackEnd emulatorConnection)
        {
            this.Config = config ?? new Config();

            if (emulatorConnection == null)
            {
                throw new ArgumentNullException("emulatorConnection");
            }

            this.BackEnd = emulatorConnection;
        }

        /// <summary>
        /// Gets a value indicating whether the emulator instance is running (started).
        /// </summary>
        public bool EmulatorRunning { get; private set; }

        /// <summary>
        /// Gets the last command.
        /// </summary>
        public IoResult LastCommand
        {
            get
            {
                lock (this.recentCommandsLock)
                {
                    if (this.recentCommands.Count() == 0)
                    {
                        return null;
                    }

                    return this.recentCommands.First();
                }
            }
        }

        /// <summary>
        /// Gets the set of recent commands.
        /// <para>These are the unmodified commands sent to and responses received from the emulator.
        /// The data reported by the <see cref="StatusLine"/> attribute, the <see cref="Query"/>
        /// method and the <see cref="StatusField"/> method may be translated to conform to the
        /// session's <see cref="X3270if.Config.Origin"/>.</para>
        /// </summary>
        public IoResult[] RecentCommands
        {
            get
            {
                lock (this.recentCommandsLock)
                {
                    return this.recentCommands.ToArray();
                }
            }
        }

        /// <summary>
        /// Gets the last status.
        /// <para>This may be different from the StatusLine in the recent history, because of <see cref="X3270if.Config.Origin"/> translation.</para>
        /// </summary>
        public string StatusLine
        {
            get { return this.lastStatusLine; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether failed commands will raise exceptions. Failures include the emulator
        /// scripting API returning bad status, the command fatally timing out (see
        /// <see cref="X3270if.Config.DefaultTimeoutMsec"/>) and the emulator closing
        /// the scripting API connection.
        /// </summary>
        public bool ExceptionMode { get; set; }

        /// <summary>
        /// Gets a value indicating whether the host was connected when the last command was run.
        /// </summary>
        public bool HostConnected
        {
            get { return this.EmulatorRunning && this.StatusField(StatusLineField.Connection)[0] == 'C'; }
        }

        /// <summary>
        /// Gets or sets the connection object, real or emulated.
        /// </summary>
        protected IBackEnd BackEnd { get; set; }

        /// <summary>
        /// Gets the socket for communicating with the emulator.
        /// </summary>
        private TcpClient Client
        {
            get { return this.BackEnd.GetClient(); }
        }

        /// <summary>
        /// Return a status field.
        /// <para>For cursor coordinates, the value may have been translated to conform to the session's <see cref="X3270if.Config.Origin"/>.</para>
        /// </summary>
        /// <param name="index">Zero-based index of field to return (<see cref="int"/> cast of StatusLineField).</param>
        /// <returns>Field value.</returns>
        public string StatusField(StatusLineField index)
        {
            if (!this.EmulatorRunning)
            {
                throw new InvalidOperationException("Not running");
            }

            return this.StatusLine.Split(' ')[(int)index];
        }

        /// <summary>
        /// Store a new command status.
        /// </summary>
        /// <param name="result">Result to store.</param>
        /// <returns>The <paramref name="result"/> parameter.</returns>
        private IoResult SaveRecentCommand(IoResult result)
        {
            lock (this.recentCommandsLock)
            {
                this.recentCommands.AddFirst(result);
                if (this.recentCommands.Count() > MaxCommands)
                {
                    this.recentCommands.RemoveLast();
                }
            }

            return result;
        }
    }
}
