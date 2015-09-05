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
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace x3270if
{
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
        };

        /// <summary>
        /// IO result from ReadBuffer and ReadBufferAsync.
        /// An ordinary IoResult, plus the type of ReadBuffer operation that was requested,
        /// and the coordinate origin, so DisplayBuffer operations can use the same mode as the
        /// Session that the ReadBufferIoResult was derived from.
        /// </summary>
        public class ReadBufferIoResult : IoResult
        {
            /// <summary>
            /// ASCII or EBCDIC mode.
            /// </summary>
            public ReadBufferType ReadBufferType;

            /// <summary>
            /// Coordinate origin.
            /// </summary>
            public int Origin;

            /// <summary>
            /// Empty constructor.
            /// </summary>
            public ReadBufferIoResult()
            {
            }

            /// <summary>
            /// Constructor, given a plan IoResult, plus type and origin.
            /// </summary>
            /// <param name="r">Plain IoResult.</param>
            /// <param name="type">ASCII or EBCDIC mode.</param>
            /// <param name="origin">Coordinate origin</param>
            public ReadBufferIoResult(IoResult r, ReadBufferType type, int origin) : base(r)
            {
                // Initialize the ReadBufferType.
                ReadBufferType = type;
                Origin = origin;
            }
        }

        /// <summary>
        /// Async version of ReadBuffer.
        /// </summary>
        /// <param name="type">See ReadBuffer.</param>
        /// <returns>See ReadBuffer.</returns>
        public async Task<ReadBufferIoResult> ReadBufferAsync(ReadBufferType type = ReadBufferType.Ascii)
        {
            var result = await IoAsync("ReadBuffer(" + type.ToString() + ")").ConfigureAwait(continueOnCapturedContext: false);
            return new ReadBufferIoResult(result, type, Config.Origin);
        }

        /// <summary>
        /// Read the 3270 display buffer.
        /// </summary>
        /// <param name="type">ASCII or EBCDIC mode.</param>
        /// <returns>success/failure and result</returns>
        public ReadBufferIoResult ReadBuffer(ReadBufferType type = ReadBufferType.Ascii)
        {
            try
            {
                return ReadBufferAsync(type).Result;
            }
            catch (AggregateException e)
            {
                throw e.InnerException;
            }
        }
    }
}