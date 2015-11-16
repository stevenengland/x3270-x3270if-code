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

    /// <summary>
    /// When to artificially fail screen-modifying commands.
    /// </summary>
    public enum ModifyFailType
    {
        /// <summary>
        /// When the host is not connected (the default).
        /// </summary>
        RequireConnection,

        /// <summary>
        /// When the host is not in 3270 mode.
        /// </summary>
        Require3270,

        /// <summary>
        /// Never fail.
        /// </summary>
        Never
    }

    /// <summary>
    /// Connection flags.
    /// These can be passed per connection, or set globally.
    /// </summary>
    [Flags]
    public enum ConnectFlags
    {
        /// <summary>
        /// No connect flags.
        /// </summary>
        None = 0,

        /// <summary>
        /// Do not run any login macro. (The C: option.)
        /// </summary>
        NoLogin = 0x1,

        /// <summary>
        /// Use an SSL tunnel. (The L: option.)
        /// </summary>
        Secure = 0x2,

        /// <summary>
        /// Do not negotiate TN3270E. (The N: option.)
        /// </summary>
        NonTN3270E = 0x4,

        /// <summary>
        /// Use the pass-through protocol. (The P: option.)
        /// </summary>
        Passthru = 0x8,

        /// <summary>
        /// Do not add "-E" to the terminal name, so the host will not think
        /// we support later 3270 features like WriteStructuredField.
        /// (The S: option.)
        /// </summary>
        StandardDataStream = 0x10,

        /// <summary>
        /// Do not automatically unlock the keyboard when a BIND arrives from the host.
        /// (The B: option.)
        /// </summary>
        BindLock = 0x20,

        /// <summary>
        /// All connect flags.
        /// </summary>
        All = NoLogin | Secure | NonTN3270E | Passthru | StandardDataStream | BindLock
    }

    /// <summary>
    /// x3270if session configuration.
    /// </summary>
    public class Config
    {
        /// <summary>
        /// Backing field for the Origin.
        /// </summary>
        private int origin = 0;

        /// <summary>
        /// Backing field for <see cref="HandshakeTimeoutMsec"/>
        /// </summary>
        private int handshakeTimeoutMsec = 5000;

        /// <summary>
        /// Backing field for <see cref="ConnectRetryMsec"/>.
        /// </summary>
        private int? connectRetryMsec = null;

        /// <summary>
        /// Gets or sets the coordinate (row and column) origin.
        /// <para>The default is 0 to conform to how the emulator represents coordinates, but it
        /// can be set to 1 to match how the on-screen indicator and emulator trace files display them.
        /// </para>
        /// </summary>
        /// <remarks>
        /// This parameter changes the behavior of all methods and attributes that refer to row and
        /// column coordinates. When set to 1, it causes all coordinates to be transparently translated between
        /// 1-origin (supplied by and reported to the user of these classes) and 0-origin (as required by
        /// the emulator scripting API). This corrects the historical mismatch between the scripting API and
        /// the on-screen display and trace files.
        /// </remarks>
        public int Origin
        {
            get
            {
                return this.origin;
            }

            set
            {
                if (value < 0 || value > 1)
                {
                    throw new ArgumentOutOfRangeException("Origin");
                }

                this.origin = value;
            }
        }

        /// <summary>
        /// Gets or sets default flags to use on every connection to this host.
        /// </summary>
        public ConnectFlags DefaultConnectFlags
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the default command timeout.
        /// If nonzero, all <see cref="Session.Io"/> requests that don't specify a timeout use this value as a timeout. In milliseconds.
        /// If a command fails due to this timer, the session will be stopped.
        /// </summary>
        public int DefaultTimeoutMsec
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the timeout for the initial handshake with the emulator, in milliseconds.
        /// </summary>
        public int HandshakeTimeoutMsec
        {
            get
            {
                return this.handshakeTimeoutMsec;
            }

            set
            {
                this.handshakeTimeoutMsec = value;
            }
        }

        /// <summary>
        /// Gets or sets the connect retry delay, in milliseconds.
        /// </summary>
        public int? ConnectRetryMsec
        {
            get
            {
                return this.connectRetryMsec;
            }

            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("Invalid ConnectRetryMsec");
                }

                this.connectRetryMsec = value;
            }
        }

        /// <summary>
        /// Gets or sets when to artificially fail screen modification operations.
        /// Failures in this case are generated in the library, not by the emulator.
        /// </summary>
        public ModifyFailType ModifyFail { get; set; }
    }
}
