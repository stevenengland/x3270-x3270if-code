﻿// Copyright (c) 2015 Paul Mattes.
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
    using System.Threading.Tasks;

    /// <summary>
    /// Flavors of Wait().
    /// </summary>
    public enum WaitMode
    {
        /// <summary>
        /// Wait for the host to draw a screen containing a modifiable field.
        /// </summary>
        InputField,

        /// <summary>
        /// Wait for the host to transition to NVT (ASCII) mode.
        /// </summary>
        NVTmode,

        /// <summary>
        /// Wait for the host to transition to 3270 mode.
        /// </summary>
        Wait3270Mode,

        /// <summary>
        /// Wait for the host to change the screen.
        /// <para>See the caution under <see cref="Session.Wait"/> for restrictions.</para>
        /// </summary>
        Output,

        /// <summary>
        /// Wait for the specified number of seconds.
        /// </summary>
        Seconds,

        /// <summary>
        /// Wait for the host to disconnect.
        /// </summary>
        Disconnect,

        /// <summary>
        /// Wait for the keyboard to be unlocked by the host.
        /// This is useful only when the aidWait toggle (set by default) is clear, which means that methods
        /// that send the host an AID do not automatically wait for the keyboard to be unlocked.
        /// </summary>
        Unlock
    }

    /// <summary>
    /// Session class.
    /// </summary>
    public partial class Session
    {
        /// <summary>
        /// Block until an emulator event occurs, asynchronous version.
        /// </summary>
        /// <param name="waitMode">What to wait for.</param>
        /// <param name="timeoutSecs">Optional timeout. This is not destructive if it fails.</param>
        /// <returns>Success/failure and failure text.</returns>
        /// <exception cref="InvalidOperationException">Session is not started.</exception>
        /// <exception cref="X3270ifCommandException"><see cref="ExceptionMode"/> is enabled and the command fails.</exception>
        /// <remarks>
        /// <note type="note">
        /// If the specified condition has already been met, <see cref="WaitAsync"/> will return immediately.
        /// See the documentation under each value of <see cref="WaitMode"/> for details on the conditions for waiting.
        /// </note>
        /// <note type="caution">
        /// The <see cref="WaitMode.Output"/> flavor of <see cref="WaitAsync"/> is integrated with the calls that
        /// read data from the screen. You must call <see cref="ReadBuffer"/>, <see cref="Ascii()"/> or <see cref="Ebcdic()"/>
        /// before a WaitAsync(WaitMode.Output) will actually wait for anything. If you
        /// Wait(WaitMode.Output) immediately after a previous WaitAsync(Output), without any intervening call to <see cref="ReadBuffer"/>,
        /// <see cref="Ascii()"/> or <see cref="Ebcdic()"/>, <see cref="WaitAsync"/> will return immediately.
        /// </note>
        /// </remarks>
        public async Task<IoResult> WaitAsync(WaitMode waitMode, int? timeoutSecs = null)
        {
            string command = "Wait(";

            if (timeoutSecs != null)
            {
                command += timeoutSecs.ToString() + ",";
            }

            var modeName = waitMode.ToString();
            if (modeName.StartsWith("Wait"))
            {
                modeName = modeName.Substring(4);
            }

            command += modeName + ")";

            return await this.IoAsync(command, isModify: waitMode == WaitMode.Output).ConfigureAwait(continueOnCapturedContext: false);
        }

        /// <summary>
        /// Block until an emulator event occurs.
        /// </summary>
        /// <param name="waitMode">What to wait for.</param>
        /// <param name="timeoutSecs">Optional timeout. This is not destructive if it fails.</param>
        /// <returns>Success/failure and failure text.</returns>
        /// <exception cref="InvalidOperationException">Session is not started.</exception>
        /// <exception cref="X3270ifCommandException"><see cref="ExceptionMode"/> is enabled and the command fails.</exception>
        /// <remarks>
        /// <note type="note">
        /// If the specified condition has already been met, <see cref="WaitAsync"/> will return immediately.
        /// See the documentation under each value of <see cref="WaitMode"/> for details on the conditions for waiting.
        /// </note>
        /// <note type="caution">
        /// The <see cref="WaitMode.Output"/> flavor of <see cref="Wait"/> is integrated with the calls that
        /// read data from the screen. You must call <see cref="ReadBuffer"/>, <see cref="Ascii()"/> or <see cref="Ebcdic()"/>
        /// before a Wait(WaitMode.Output) will actually wait for anything. If you
        /// Wait(WaitMode.Output) immediately after a previous Wait(WaitMode.Output) without any intervening call to
        /// <see cref="ReadBuffer"/>, <see cref="Ascii()"/> or <see cref="Ebcdic()"/>, <see cref="Wait"/> will return immediately.
        /// </note>
        /// </remarks>
        public IoResult Wait(WaitMode waitMode, int? timeoutSecs = null)
        {
            try
            {
                return this.WaitAsync(waitMode, timeoutSecs).Result;
            }
            catch (AggregateException e)
            {
                throw e.InnerException;
            }
        }
    }
}
