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

namespace x3270if
{
    /// <summary>
    /// Variants of the Query method.
    /// </summary>
    public enum QueryType
	{
        /// <summary>
        /// The LU name reported by the BIND-DATA from the host.
        /// </summary>
        BindPluName,
        /// <summary>
        /// The state of the connection ("tn3270"|"tn3270e" "nvt"|"3270"|"sscp-lu").
        /// </summary>
        ConnectionState,
        /// <summary>
        /// The cursor position (row column). These will be translated to the session's <see cref="x3270if.Config.Origin"/>.
        /// </summary>
        Cursor,
        /// <summary>
        /// Screen formatting status ("formatted"|"unformatted").
        /// </summary>
        Formatted,
        /// <summary>
        /// Host information ("host" hostname port|"process" pathname).
        /// </summary>
        Host,
        /// <summary>
        /// Local encoding (Windows code page or "utf-8").
        /// </summary>
        LocalEncoding,
        /// <summary>
        /// LU name negotiated by TN3270E.
        /// </summary>
        LuName,
        /// <summary>
        /// Full model name, e.g., IBM-3278-4-E.
        /// </summary>
        Model,
        /// <summary>
        /// Current screen size (rows columns).
        /// </summary>
        ScreenCurSize,
        /// <summary>
        /// Maximum screen size (rows columns).
        /// </summary>
        ScreenMaxSize,
        /// <summary>
        /// SSL state ("not secure" | "secure" "host-verified"|"host-unverified").
        /// </summary>
        Ssl
	};

    public partial class Session
    {
        /// <summary>
        /// Run the emulator Query action, asynchronous version.
        /// </summary>
        /// <param name="queryType">Type of query.</param>
        /// <returns>Success/failure and failure text.</returns>
        public async Task<IoResult> QueryAsync(QueryType queryType)
        {
            var result = await IoAsync("Query(" + queryType.ToString() + ")").ConfigureAwait(continueOnCapturedContext: false);
            if (queryType == QueryType.Cursor && result.Success)
            {
                // Translate the cursor coordinates.
                // This violates the idea that the ioResult always contains the actual value returned by the emulator,
                // but because we clone the result here, the history will have a reference to the original value.
                var rowCol = result.Result[0].Split(' ');
                result = new IoResult(result);
                result.Result = new[] { string.Format("{0} {1}", int.Parse(rowCol[0]) + Config.Origin, int.Parse(rowCol[1]) + Config.Origin) };
            }
            return result;
        }

        /// <summary>
        /// Run the emulator Query action.
        /// </summary>
        /// <param name="queryType">Type of query.</param>
        /// <returns>Success/failure and failure text.</returns>
        public IoResult Query(QueryType queryType)
        {
            try
            {
                return QueryAsync(queryType).Result;
            }
            catch (AggregateException e)
            {
                throw e.InnerException;
            }
        }
    }
}
