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

using System;
using System.Threading.Tasks;

namespace x3270if
{
    /// <summary>
    /// Result of an Ebcdic method.
    /// </summary>
    public class EbcdicIoResult : IoResult
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="r">IoResult from an Ebcdic call.</param>
        public EbcdicIoResult(IoResult r)
            : base(r)
        {
        }

        /// <summary>
        /// Translate the result of an Ebcdic action into a byte array.
        /// </summary>
        /// <returns>Two-dimensional array of bytes.</returns>
        public byte[,] ToByteArray()
        {
            if (!Success)
            {
                return null;
            }
            // The Result from the emulator is an array of space-separated hex strings: 00 0e f0 f1, etc.
            var result = new byte[Result.GetLength(0), (Result[0].Length + 1) / 3];
            for (int row = 0; row < Result.GetLength(0); row++)
            {
                int column = 0;
                foreach (var hex in Result[row].Split(' '))
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
            return new EbcdicIoResult(await IoAsync("Ebcdic()").ConfigureAwait(continueOnCapturedContext: false));
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
            return new EbcdicIoResult(await IoAsync("Ebcdic(" + length + ")").ConfigureAwait(continueOnCapturedContext: false));
        }

        /// <summary>
        /// Return the 3270 display buffer as EBCDIC data (hexadecimal values encoded in strings), starting at the specified coordinates. Async version.
        /// </summary>
        /// <param name="row">Starting row, using session <see cref="x3270if.Config.Origin"/>.</param>
        /// <param name="column">Starting column, using session <see cref="x3270if.Config.Origin"/>.</param>
        /// <param name="length">Number of characters.</param>
        /// <returns>Success/failure, one row of text.</returns>
        /// <exception cref="InvalidOperationException">Session is not started.</exception>
        /// <exception cref="X3270ifCommandException"><see cref="ExceptionMode"/> is enabled and the command fails.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="row"/> or <paramref name="column"/> is less than <see cref="x3270if.Config.Origin"/>.</exception>
        public async Task<EbcdicIoResult> EbcdicAsync(int row, int column, int length)
        {
            if (row < Config.Origin)
            {
                throw new ArgumentOutOfRangeException("row");
            }
            if (column < Config.Origin)
            {
                throw new ArgumentOutOfRangeException("column");
            }
            return new EbcdicIoResult(await IoAsync(
                string.Format("Ebcdic({0},{1},{2})", row - Config.Origin, column - Config.Origin, length))
                .ConfigureAwait(continueOnCapturedContext: false));
        }

        /// <summary>
        /// Return the 3270 display buffer as EBCDIC data (hexadecimal values encoded in strings) in an array. Async version.
        /// </summary>
        /// <param name="row">Starting row, using the session's <see cref="x3270if.Config.Origin"/>.</param>
        /// <param name="column">Starting column, using the session's <see cref="x3270if.Config.Origin"/>.</param>
        /// <param name="rows">Number of rows.</param>
        /// <param name="columns">Number of columns.</param>
        /// <returns>Success/failure, array of text.</returns>
        /// <exception cref="InvalidOperationException">Session is not started.</exception>
        /// <exception cref="X3270ifCommandException"><see cref="ExceptionMode"/> is enabled and the command fails.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="row"/> or <paramref name="column"/> is less than <see cref="x3270if.Config.Origin"/>.</exception>        
        public async Task<EbcdicIoResult> EbcdicAsync(int row, int column, int rows, int columns)
        {
            if (row < Config.Origin)
            {
                throw new ArgumentOutOfRangeException("row");
            }
            if (column < Config.Origin)
            {
                throw new ArgumentOutOfRangeException("column");
            }
            return new EbcdicIoResult(await IoAsync(
                string.Format("Ebcdic({0},{1},{2},{3})", row - Config.Origin, column - Config.Origin, rows, columns))
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
                return EbcdicAsync().Result;
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
                return EbcdicAsync(length).Result;
            }
            catch (AggregateException e)
            {
                throw e.InnerException;
            }
        }

        /// <summary>
        /// Return a region of the 3270 display buffer as EBCDIC data (hexadecimal values encoded in strings), starting at the specified coordinates.
        /// </summary>
        /// <param name="row">Starting row, using the session's <see cref="x3270if.Config.Origin"/>.</param>
        /// <param name="column">Starting column, using the session's <see cref="x3270if.Config.Origin"/>.</param>
        /// <param name="length">Number of characters to return.</param>
        /// <returns>Success/failure, one row of text.</returns>
        /// <exception cref="InvalidOperationException">Session is not started.</exception>
        /// <exception cref="X3270ifCommandException"><see cref="ExceptionMode"/> is enabled and the command fails.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="row"/> or <paramref name="column"/> is less than <see cref="x3270if.Config.Origin"/>.</exception>        
        public EbcdicIoResult Ebcdic(int row, int column, int length)
        {
            try
            {
                return EbcdicAsync(row, column, length).Result;
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
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="row"/> or <paramref name="column"/> is less than <see cref="x3270if.Config.Origin"/>.</exception>
        public EbcdicIoResult Ebcdic(int row, int column, int rows, int columns)
        {
            try
            {
                return EbcdicAsync(row, column, rows, columns).Result;
            }
            catch (AggregateException e)
            {
                throw e.InnerException;
            }
        }
    }
}