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
    using System.Threading.Tasks;

    /// <summary>
    /// Session class.
    /// </summary>
    public partial class Session
    {
        /// <summary>
        /// Move cursor up. Asynchronous version.
        /// </summary>
        /// <returns>Success/failure and failure reason.</returns>
        /// <exception cref="InvalidOperationException">Session is not started.</exception>
        /// <exception cref="X3270ifCommandException"><see cref="ExceptionMode"/> is enabled and the command fails.</exception>
        public async Task<IoResult> UpAsync()
        {
            return await this.IoAsync("Up()", isModify: true).ConfigureAwait(continueOnCapturedContext: false);
        }

        /// <summary>
        /// Move cursor down. Asynchronous version.
        /// </summary>
        /// <returns>Success/failure and failure reason.</returns>
        /// <exception cref="InvalidOperationException">Session is not started.</exception>
        /// <exception cref="X3270ifCommandException"><see cref="ExceptionMode"/> is enabled and the command fails.</exception>
        public async Task<IoResult> DownAsync()
        {
            return await this.IoAsync("Down()", isModify: true).ConfigureAwait(continueOnCapturedContext: false);
        }

        /// <summary>
        /// Move cursor left. Asynchronous version.
        /// </summary>
        /// <returns>Success/failure and failure reason.</returns>
        /// <exception cref="InvalidOperationException">Session is not started.</exception>
        /// <exception cref="X3270ifCommandException"><see cref="ExceptionMode"/> is enabled and the command fails.</exception>
        public async Task<IoResult> LeftAsync()
        {
            return await this.IoAsync("Left()", isModify: true).ConfigureAwait(continueOnCapturedContext: false);
        }

        /// <summary>
        /// Move cursor right. Asynchronous version.
        /// </summary>
        /// <returns>Success/failure and failure reason.</returns>
        /// <exception cref="InvalidOperationException">Session is not started.</exception>
        /// <exception cref="X3270ifCommandException"><see cref="ExceptionMode"/> is enabled and the command fails.</exception>
        public async Task<IoResult> RightAsync()
        {
            return await this.IoAsync("Right()", isModify: true).ConfigureAwait(continueOnCapturedContext: false);
        }

        /// <summary>
        /// Move cursor to a particular row and column. Asynchronous version.
        /// </summary>
        /// <param name="row">Desired row, using the session's <see cref="X3270if.Config.Origin"/>.</param>
        /// <param name="column">Desired column, using the session's <see cref="X3270if.Config.Origin"/>.</param>
        /// <returns>Success/failure and failure reason.</returns>
        /// <exception cref="InvalidOperationException">Session is not started.</exception>
        /// <exception cref="X3270ifCommandException"><see cref="ExceptionMode"/> is enabled and the command fails.</exception>
        public async Task<IoResult> MoveCursorAsync(int row, int column)
        {
            if (row < this.Config.Origin)
            {
                throw new ArgumentOutOfRangeException("row");
            }

            if (column < this.Config.Origin)
            {
                throw new ArgumentOutOfRangeException("column");
            }

            return await this.IoAsync(
                string.Format("MoveCursor({0},{1})", row - this.Config.Origin, column - this.Config.Origin), isModify: true)
                .ConfigureAwait(continueOnCapturedContext: false);
        }

        /// <summary>
        /// Move the cursor to the next field. Asynchronous version.
        /// </summary>
        /// <returns>Success/failure and failure reason.</returns>
        /// <exception cref="InvalidOperationException">Session is not started.</exception>
        /// <exception cref="X3270ifCommandException"><see cref="ExceptionMode"/> is enabled and the command fails.</exception>
        public async Task<IoResult> TabAsync()
        {
            return await this.IoAsync("Tab()", isModify: true).ConfigureAwait(continueOnCapturedContext: false);
        }

        /// <summary>
        /// Move the cursor to the previous field. Asynchronous version.
        /// </summary>
        /// <returns>Success/failure and failure reason.</returns>
        /// <exception cref="InvalidOperationException">Session is not started.</exception>
        /// <exception cref="X3270ifCommandException"><see cref="ExceptionMode"/> is enabled and the command fails.</exception>
        public async Task<IoResult> BackTabAsync()
        {
            return await this.IoAsync("BackTab()", isModify: true).ConfigureAwait(continueOnCapturedContext: false);
        }

        /// <summary>
        /// Move cursor up.
        /// </summary>
        /// <returns>Success/failure, failure reason.</returns>
        /// <exception cref="InvalidOperationException">Session is not started.</exception>
        /// <exception cref="X3270ifCommandException"><see cref="ExceptionMode"/> is enabled and the command fails.</exception>
        public IoResult Up()
        {
            try
            {
                return this.UpAsync().Result;
            }
            catch (AggregateException e)
            {
                throw e.InnerException;
            }
        }

        /// <summary>
        /// Move cursor down.
        /// </summary>
        /// <returns>Success/failure, failure reason.</returns>
        /// <exception cref="InvalidOperationException">Session is not started.</exception>
        /// <exception cref="X3270ifCommandException"><see cref="ExceptionMode"/> is enabled and the command fails.</exception>
        public IoResult Down()
        {
            try
            {
                return this.DownAsync().Result;
            }
            catch (AggregateException e)
            {
                throw e.InnerException;
            }
        }

        /// <summary>
        /// Move cursor left.
        /// </summary>
        /// <returns>Success/failure, failure reason.</returns>
        /// <exception cref="InvalidOperationException">Session is not started.</exception>
        /// <exception cref="X3270ifCommandException"><see cref="ExceptionMode"/> is enabled and the command fails.</exception>
        public IoResult Left()
        {
            try
            {
                return this.LeftAsync().Result;
            }
            catch (AggregateException e)
            {
                throw e.InnerException;
            }
        }

        /// <summary>
        /// Move cursor right.
        /// </summary>
        /// <returns>Success/failure, failure reason.</returns>
        /// <exception cref="InvalidOperationException">Session is not started.</exception>
        /// <exception cref="X3270ifCommandException"><see cref="ExceptionMode"/> is enabled and the command fails.</exception>
        public IoResult Right()
        {
            try
            {
                return this.RightAsync().Result;
            }
            catch (AggregateException e)
            {
                throw e.InnerException;
            }
        }

        /// <summary>
        /// Move cursor to a specific location.
        /// </summary>
        /// <param name="row">Row, using the session's <see cref="X3270if.Config.Origin"/>.</param>
        /// <param name="column">Column, using the session's <see cref="X3270if.Config.Origin"/>.</param>
        /// <returns>Success/failure, failure reason.</returns>
        /// <exception cref="InvalidOperationException">Session is not started.</exception>
        /// <exception cref="X3270ifCommandException"><see cref="ExceptionMode"/> is enabled and the command fails.</exception>
        public IoResult MoveCursor(int row, int column)
        {
            try
            {
                return this.MoveCursorAsync(row, column).Result;
            }
            catch (AggregateException e)
            {
                throw e.InnerException;
            }
        }

        /// <summary>
        /// Tab to the next field.
        /// </summary>
        /// <returns>Success/failure, failure reason.</returns>
        /// <exception cref="InvalidOperationException">Session is not started.</exception>
        /// <exception cref="X3270ifCommandException"><see cref="ExceptionMode"/> is enabled and the command fails.</exception>
        public IoResult Tab()
        {
            try
            {
                return this.TabAsync().Result;
            }
            catch (AggregateException e)
            {
                throw e.InnerException;
            }
        }

        /// <summary>
        /// Tab to the previous field.
        /// </summary>
        /// <returns>Success/failure, failure reason.</returns>
        /// <exception cref="InvalidOperationException">Session is not started.</exception>
        /// <exception cref="X3270ifCommandException"><see cref="ExceptionMode"/> is enabled and the command fails.</exception>
        public IoResult BackTab()
        {
            try
            {
                return this.BackTabAsync().Result;
            }
            catch (AggregateException e)
            {
                throw e.InnerException;
            }
        }
    }
}
