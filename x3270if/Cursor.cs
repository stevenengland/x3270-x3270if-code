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
    public partial class Session
    {
        /// <summary>
        /// Move cursor up, async version.
        /// </summary>
        /// <returns>success/failure and failure reason.</returns>
        public async Task<IoResult> UpAsync()
        {
            return await IoAsync("Up()").ConfigureAwait(continueOnCapturedContext: false);
        }

        /// <summary>
        /// Move cursor down, async version.
        /// </summary>
        /// <returns>success/failure and failure reason.</returns>
        public async Task<IoResult> DownAsync()
        {
            return await IoAsync("Down()").ConfigureAwait(continueOnCapturedContext: false);
        }

        /// <summary>
        /// Move cursor left, async version.
        /// </summary>
        /// <returns>success/failure and failure reason.</returns>
        public async Task<IoResult> LeftAsync()
        {
            return await IoAsync("Left()").ConfigureAwait(continueOnCapturedContext: false);
        }

        /// <summary>
        /// Move cursor right, async version.
        /// </summary>
        /// <returns>success/failure and failure reason.</returns>
        public async Task<IoResult> RightAsync()
        {
            return await IoAsync("Right()").ConfigureAwait(continueOnCapturedContext: false);
        }

        /// <summary>
        /// Move cursor to a particular row and column, async version.
        /// </summary>
        /// <param name="row">Desired row</param>
        /// <param name="column">Desired column</param>
        /// <returns>success/failure and failure reason.</returns>
        public async Task<IoResult> MoveCursorAsync(int row, int column)
        {
            if (row < Config.Origin)
            {
                throw new ArgumentOutOfRangeException("row");
            }
            if (column < Config.Origin)
            {
                throw new ArgumentOutOfRangeException("column");
            }
            return await IoAsync(
                string.Format("MoveCursor({0},{1})", row - Config.Origin, column - Config.Origin))
                .ConfigureAwait(continueOnCapturedContext: false);
        }

        /// <summary>
        /// Move the cursor to the next field, async version.
        /// </summary>
        /// <returns>success/failure and failure reason.</returns>
        public async Task<IoResult> TabAsync()
        {
            return await IoAsync("Tab()").ConfigureAwait(continueOnCapturedContext: false);
        }

        /// <summary>
        /// Move the cursor to the previous field, async version.
        /// </summary>
        /// <returns>success/failure and failure reason.</returns>
        public async Task<IoResult> BackTabAsync()
        {
            return await IoAsync("BackTab()");
        }

        /// <summary>
        /// Move cursor up.
        /// </summary>
        /// <returns>success/failure, failure reason</returns>
        public IoResult Up()
        {
            try
            {
                return UpAsync().Result;
            }
            catch (AggregateException e)
            {
                throw e.InnerException;
            }
        }

        /// <summary>
        /// Move cursor down.
        /// </summary>
        /// <returns>success/failure, failure reason</returns>
        public IoResult Down()
        {
            try
            {
                return DownAsync().Result;
            }
            catch (AggregateException e)
            {
                throw e.InnerException;
            }
        }

        /// <summary>
        /// Move cursor left.
        /// </summary>
        /// <returns>success/failure, failure reason</returns>
        public IoResult Left()
        {
            try
            {
                return LeftAsync().Result;
            }
            catch (AggregateException e)
            {
                throw e.InnerException;
            }
        }

        /// <summary>
        /// Move cursor right.
        /// </summary>
        /// <returns>success/failure, failure reason</returns>
        public IoResult Right()
        {
            try
            {
                return RightAsync().Result;
            }
            catch (AggregateException e)
            {
                throw e.InnerException;
            }
        }

        /// <summary>
        /// Move cursor to a specific location.
        /// </summary>
        /// <param name="row">Row</param>
        /// <param name="column">Column</param>
        /// <returns>success/failure, failure reason</returns>
        public IoResult MoveCursor(int row, int column)
        {
            try
            {
                return MoveCursorAsync(row, column).Result;
            }
            catch (AggregateException e)
            {
                throw e.InnerException;
            }
        }

        /// <summary>
        /// Tab to the next field.
        /// </summary>
        /// <returns>success/failure, failure reason</returns>
        public IoResult Tab()
        {
            try
            {
                return TabAsync().Result;
            }
            catch (AggregateException e)
            {
                throw e.InnerException;
            }
        }

        /// <summary>
        /// Tab to the previous field.
        /// </summary>
        /// <returns>success/failure, failure reason</returns>
        public IoResult BackTab()
        {
            try
            {
                return BackTabAsync().Result;
            }
            catch (AggregateException e)
            {
                throw e.InnerException;
            }
        }
    }
}
