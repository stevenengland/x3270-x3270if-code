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
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Session class.
    /// </summary>
    public partial class Session
    {
        /// <summary>
        /// Type for ReadBuffer -- ASCII or EBCDIC.
        /// </summary>
        public enum ReadBufferType
        {
            /// <summary>
            /// ASCII (text) mode.
            /// </summary>
            Ascii,

            /// <summary>
            /// EBCDIC (numeric character code) mode.
            /// </summary>
            Ebcdic
        }

        /// <summary>
        /// Async version of ReadBuffer.
        /// </summary>
        /// <param name="type">Type of operation (ASCII or EBCDIC).</param>
        /// <returns>I/O result.</returns>
        /// <exception cref="InvalidOperationException">Session is not started.</exception>
        /// <exception cref="X3270ifCommandException"><see cref="ExceptionMode"/> is enabled and the command fails.</exception>
        public async Task<ReadBufferIoResult> ReadBufferAsync(ReadBufferType type = ReadBufferType.Ascii)
        {
            var result = await this.IoAsync("ReadBuffer(" + type.ToString() + ")").ConfigureAwait(continueOnCapturedContext: false);
            return new ReadBufferIoResult(result, type, this.Config.Origin);
        }

        /// <summary>
        /// Read the 3270 display buffer.
        /// </summary>
        /// <param name="type">ASCII or EBCDIC mode.</param>
        /// <returns>Success/failure and result.</returns>
        /// <exception cref="InvalidOperationException">Session is not started.</exception>
        /// <exception cref="X3270ifCommandException"><see cref="ExceptionMode"/> is enabled and the command fails.</exception>
        public ReadBufferIoResult ReadBuffer(ReadBufferType type = ReadBufferType.Ascii)
        {
            try
            {
                return this.ReadBufferAsync(type).Result;
            }
            catch (AggregateException e)
            {
                throw e.InnerException;
            }
        }

        /// <summary>
        /// IO result from ReadBuffer and ReadBufferAsync.
        /// An ordinary IoResult, plus the type of ReadBuffer operation that was requested,
        /// and the coordinate <see cref="X3270if.Config.Origin"/>, so DisplayBuffer operations can use the same origin as the
        /// Session that the ReadBufferIoResult was derived from.
        /// </summary>
        public class ReadBufferIoResult : IoResult
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="ReadBufferIoResult"/> class.
            /// </summary>
            public ReadBufferIoResult()
            {
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="ReadBufferIoResult"/> class.
            /// Constructor, given a plain IoResult, plus type and origin.
            /// </summary>
            /// <param name="r">Plain IoResult.</param>
            /// <param name="type">ASCII or EBCDIC mode.</param>
            /// <param name="origin">Coordinate origin.</param>
            public ReadBufferIoResult(IoResult r, ReadBufferType type, int origin)
                : base(r)
            {
                // Initialize the ReadBufferType.
                this.ReadBufferType = type;
                this.Origin = origin;
            }

            /// <summary>
            /// Gets or sets ASCII or EBCDIC mode.
            /// </summary>
            public ReadBufferType ReadBufferType { get; set; }

            /// <summary>
            /// Gets or sets the coordinate origin.
            /// </summary>
            public int Origin { get; set; }
        }
    }
}