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

namespace x3270if
{
    /// <summary>
    /// x3270if session configuration.
    /// </summary>
    public class Config
    {
        private int origin = 0;
        /// <summary>
        /// Coordinate (row and column) origin.
        /// <para>The default is 0 to conform to how the emulator represents coordinates, but it
        /// can be set to 1 to conform to how the on-screen indicator and emulator trace files display them.
        /// </para>
        /// </summary>
        /// <remarks>
        /// This parameter changes the behavior of all methods and attributes that refer to row and
        /// column coordinates. When set to 1, it causes all coordinates to be transparently translated from
        /// 1-origin (as supplied by and reported to the user of these classes) to 0-origin (as required by
        /// the emulator scripting API). This corrects the historical mismatch between the scripting API and
        /// the on-screen display and trace files.
        /// </remarks>
        public int Origin
        {
            get { return origin; }
            set
            {
                if (value < 0 || value > 1)
                {
                    throw new ArgumentOutOfRangeException("Origin");
                }
                origin = value;
            }
        }

        /// <summary>
        /// Default flags to use on every connection to this host.
        /// </summary>
        public ConnectFlags DefaultConnectFlags
        {
            get;
            set;
        }

        /// <summary>
        /// If nonzero, all I/O requests that don't specify a timeout use this value as a timeout.
        /// </summary>
        public int DefaultTimeoutMsec
        {
            get;
            set;
        }

        private int handshakeTimeoutMsec = 5000;
        /// <summary>
        /// Timeout for the initial handshake with the emulator.
        /// </summary>
        public int HandshakeTimeoutMsec
        {
            get
            {
                return handshakeTimeoutMsec;
            }
            set
            {
                handshakeTimeoutMsec = value;
            }
        }

        private int? connectRetryMsec = null;
        /// <summary>
        /// Connect retry delay.
        /// </summary>
        public int? ConnectRetryMsec
        {
            get { return connectRetryMsec; }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("Invalid ConnectRetryMsec");
                }
                connectRetryMsec = value;
            }
        }
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
}
