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
    /// Result of an <c>Ebdcic</c> method call.
    /// </summary>
    public class EbcdicIoResult : IoResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EbcdicIoResult"/> class.
        /// </summary>
        /// <param name="r">Result from an <c>Ebcdic</c> call.</param>
        public EbcdicIoResult(IoResult r)
            : base(r)
        {
        }

        /// <summary>
        /// Translate the result of an <c>Ebcdic</c> call into a byte array.
        /// </summary>
        /// <returns>Two-dimensional array of bytes.</returns>
        public byte[,] ToByteArray()
        {
            if (!this.Success)
            {
                return null;
            }

            // The Result from the emulator is an array of space-separated hex strings: 00 0e f0 f1, etc.
            var result = new byte[this.Result.GetLength(0), (this.Result[0].Length + 1) / 3];
            for (int row = 0; row < this.Result.GetLength(0); row++)
            {
                int column = 0;
                foreach (var hex in this.Result[row].Split(' '))
                {
                    byte b;
                    if (!byte.TryParse(hex, System.Globalization.NumberStyles.HexNumber, null, out b))
                    {
                        throw new InvalidOperationException("Bad EBCDIC hex data from host");
                    }

                    result[row, column++] = b;
                }
            }

            return result;
        }
    }

    /// <summary>
    /// Session class.
    /// </summary>
    public partial class Session
    {
        /// <summary>
        /// Return the entire 3270 display buffer as EBCDIC data (hexadecimal values encoded in strings). Async version.
        /// </summary>
        /// <returns>Success/failure, array of text.</returns>
        /// <exception cref="InvalidOperationException">Session is not started.</exception>
        /// <exception cref="X3270ifCommandException"><see cref="ExceptionMode"/> is enabled and the command fails.</exception>
        public async Task<EbcdicIoResult> EbcdicAsync()
        {
            return new EbcdicIoResult(await this.IoAsync("Ebcdic()").ConfigureAwait(continueOnCapturedContext: false));
        }

        /// <summary>
        /// Return the 3270 display buffer as EBCDIC data (hexadecimal values encoded in strings), from the cursor location. Async version.
        /// </summary>
        /// <param name="length">Number of characters.</param>
        /// <returns>Success/failure, one row of text.</returns>
        /// <exception cref="InvalidOperationException">Session is not started.</exception>
        /// <exception cref="X3270ifCommandException"><see cref="ExceptionMode"/> is enabled and the command fails.</exception>
        public async Task<EbcdicIoResult> EbcdicAsync(int length)
        {
            return new EbcdicIoResult(await this.IoAsync("Ebcdic(" + length + ")").ConfigureAwait(continueOnCapturedContext: false));
        }

        /// <summary>
        /// Return the 3270 display buffer as EBCDIC data (hexadecimal values encoded in strings), starting at the specified coordinates. Async version.
        /// </summary>
        /// <param name="row">Starting row, using session <see cref="X3270if.Config.Origin"/>.</param>
        /// <param name="column">Starting column, using session <see cref="X3270if.Config.Origin"/>.</param>
        /// <param name="length">Number of characters.</param>
        /// <returns>Success/failure, one row of text.</returns>
        /// <exception cref="InvalidOperationException">Session is not started.</exception>
        /// <exception cref="X3270ifCommandException"><see cref="ExceptionMode"/> is enabled and the command fails.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="row"/> or <paramref name="column"/> is less than <see cref="X3270if.Config.Origin"/>.</exception>
        public async Task<EbcdicIoResult> EbcdicAsync(int row, int column, int length)
        {
            if (row < this.Config.Origin)
            {
                throw new ArgumentOutOfRangeException("row");
            }

            if (column < this.Config.Origin)
            {
                throw new ArgumentOutOfRangeException("column");
            }

            return new EbcdicIoResult(await this.IoAsync(
                string.Format("Ebcdic({0},{1},{2})", row - this.Config.Origin, column - this.Config.Origin, length))
                .ConfigureAwait(continueOnCapturedContext: false));
        }

        /// <summary>
        /// Return the 3270 display buffer as EBCDIC data (hexadecimal values encoded in strings) in an array. Async version.
        /// </summary>
        /// <param name="row">Starting row, using the session's <see cref="X3270if.Config.Origin"/>.</param>
        /// <param name="column">Starting column, using the session's <see cref="X3270if.Config.Origin"/>.</param>
        /// <param name="rows">Number of rows.</param>
        /// <param name="columns">Number of columns.</param>
        /// <returns>Success/failure, array of text.</returns>
        /// <exception cref="InvalidOperationException">Session is not started.</exception>
        /// <exception cref="X3270ifCommandException"><see cref="ExceptionMode"/> is enabled and the command fails.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="row"/> or <paramref name="column"/> is less than <see cref="X3270if.Config.Origin"/>.</exception>        
        public async Task<EbcdicIoResult> EbcdicAsync(int row, int column, int rows, int columns)
        {
            if (row < this.Config.Origin)
            {
                throw new ArgumentOutOfRangeException("row");
            }

            if (column < this.Config.Origin)
            {
                throw new ArgumentOutOfRangeException("column");
            }

            return new EbcdicIoResult(await this.IoAsync(
                string.Format("Ebcdic({0},{1},{2},{3})", row - this.Config.Origin, column - this.Config.Origin, rows, columns))
                .ConfigureAwait(continueOnCapturedContext: false));
        }

        /// <summary>
        /// Return the entire 3270 display buffer as EBCDIC data (hexadecimal values encoded in strings).
        /// </summary>
        /// <returns>Success/failure, entire display buffer in an array, one row per element.</returns>
        /// <exception cref="InvalidOperationException">Session is not started.</exception>
        /// <exception cref="X3270ifCommandException"><see cref="ExceptionMode"/> is enabled and the command fails.</exception>
        public EbcdicIoResult Ebcdic()
        {
            try
            {
                return this.EbcdicAsync().Result;
            }
            catch (AggregateException e)
            {
                throw e.InnerException;
            }
        }

        /// <summary>
        /// Return a region of the 3270 display buffer as EBCDIC data (hexadecimal values encoded in strings), starting at the cursor.
        /// </summary>
        /// <param name="length">Number of characters to return.</param>
        /// <returns>Success/failure, one row of text.</returns>
        /// <exception cref="InvalidOperationException">Session is not started.</exception>
        /// <exception cref="X3270ifCommandException"><see cref="ExceptionMode"/> is enabled and the command fails.</exception>
        public EbcdicIoResult Ebcdic(int length)
        {
            try
            {
                return this.EbcdicAsync(length).Result;
            }
            catch (AggregateException e)
            {
                throw e.InnerException;
            }
        }

        /// <summary>
        /// Return a region of the 3270 display buffer as EBCDIC data (hexadecimal values encoded in strings), starting at the specified coordinates.
        /// </summary>
        /// <param name="row">Starting row, using the session's <see cref="X3270if.Config.Origin"/>.</param>
        /// <param name="column">Starting column, using the session's <see cref="X3270if.Config.Origin"/>.</param>
        /// <param name="length">Number of characters to return.</param>
        /// <returns>Success/failure, one row of text.</returns>
        /// <exception cref="InvalidOperationException">Session is not started.</exception>
        /// <exception cref="X3270ifCommandException"><see cref="ExceptionMode"/> is enabled and the command fails.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="row"/> or <paramref name="column"/> is less than <see cref="X3270if.Config.Origin"/>.</exception>        
        public EbcdicIoResult Ebcdic(int row, int column, int length)
        {
            try
            {
                return this.EbcdicAsync(row, column, length).Result;
            }
            catch (AggregateException e)
            {
                throw e.InnerException;
            }
        }

        /// <summary>
        /// Return a rectangular region of the 3270 display buffer as EBCDIC data (hexadecimal values encoded in strings).
        /// </summary>
        /// <param name="row">Starting row.</param>
        /// <param name="column">Starting column.</param>
        /// <param name="rows">Number of rows.</param>
        /// <param name="columns">Number of columns.</param>
        /// <returns>Success/failure, array of text.</returns>
        /// <exception cref="InvalidOperationException">Session is not started.</exception>
        /// <exception cref="X3270ifCommandException"><see cref="ExceptionMode"/> is enabled and the command fails.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="row"/> or <paramref name="column"/> is less than <see cref="X3270if.Config.Origin"/>.</exception>
        public EbcdicIoResult Ebcdic(int row, int column, int rows, int columns)
        {
            try
            {
                return this.EbcdicAsync(row, column, rows, columns).Result;
            }
            catch (AggregateException e)
            {
                throw e.InnerException;
            }
        }
    }
}
