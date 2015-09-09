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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace x3270if
{
    /// <summary>
    /// Coordinates and text passed in a list to <see cref="M:x3270if.Session.StringAtAsync(System.Collections.Generic.IEnumerable{x3270if.StringAtBlock},System.Boolean,System.Boolean)"/>.
    /// </summary>
    public class StringAtBlock
    {
        /// <summary>
        /// Row.
        /// </summary>
        public int Row;
        /// <summary>
        /// Column.
        /// </summary>
        public int Column;
        /// <summary>
        /// String to add.
        /// </summary>
        public string Text;
    }

    public partial class Session
    {
        /// <summary>
        /// Quote a string for transmission to the String action.
        /// </summary>
        /// <param name="text">Text to quote.</param>
        /// <param name="quoteBackslashes">If true, quote backslash characters.</param>
        /// <returns>Quoted string.</returns>
        public static string QuoteString(string text, bool quoteBackslashes = true)
        {
            const string metaChars = " ,\"()\\";
            const string bsChars = "\"\\";

            // Do quoting.
            var translatedText = text;
            if (metaChars.Any(c => text.Contains(c)))
            {
                // The string contains something that requires some sort of quoting.
                // At minimum, we put double quotes around everything.
                var outString = new StringBuilder("\"", 2 + (2 * text.Length));

                foreach (char c in text)
                {
                    // Put a backslash ahead of anything in bsChars.
                    // We only quote backslashes if we are asked to.
                    if (bsChars.Contains(c) &&
                        (c != '\\' || quoteBackslashes))
                    {
                        outString.Append("\\");
                    }
                    outString.Append(c);
                }
                outString.Append('"');
                translatedText = outString.ToString();
            }

            // Translate newline, carriage return, backspace, formfeed and tab to C escapes.
            var len = translatedText.Length;
            translatedText = string.Join("", translatedText.ToCharArray().Select(c =>
            {
                switch (c)
                {
                    case '\r':
                        return @"\r";
                    case '\n':
                        return @"\n";
                    case '\b':
                        return @"\b";
                    case '\f':
                        return @"\f";
                    case '\t':
                        return @"\t";
                    default:
                        return c.ToString();
                }
            }));
            // If anything was expanded, it needs double quotes.
            // Actually, it may not -- I need finer control over this method.
            if (translatedText.Length != len && !translatedText.StartsWith("\""))
            {
                translatedText = "\"" + translatedText + "\"";
            }

            // Any other control characters are verboten.
            if (translatedText.Any(c => char.IsControl(c)))
            {
                throw new ArgumentException("text contains control character(s)");
            }

            return translatedText;
        }

        /// <summary>
        /// Input text to the emulator at the current cursor position. Asynchronous version.
        /// </summary>
        /// <param name="text">Text to send. It will be quoted as necessary.</param>
        /// <param name="quoteBackslashes">If true, quote '\' characters.</param>
        /// <returns>Success indication.</returns>
        /// <exception cref="InvalidOperationException">Session is not started.</exception>
        /// <exception cref="X3270ifCommandException"><see cref="ExceptionMode"/> is enabled and the command fails.</exception>
        public async Task<IoResult>StringAsync(string text, bool quoteBackslashes = true)
        {
            return await IoAsync("String(" + QuoteString(text, quoteBackslashes) + ")").ConfigureAwait(continueOnCapturedContext: false);
        }

        /// <summary>
        /// Input text to the emulator at a specified position. Asynchronous version.
        /// </summary>
        /// <param name="row">Row, using the session's <see cref="x3270if.Config.Origin"/>.</param>
        /// <param name="column">Column, using the session's <see cref="x3270if.Config.Origin"/>.</param>
        /// <param name="text">Text to send. It will be quoted as necessary.</param>
        /// <param name="quoteBackslashes">If true, quote '\' characters.</param>
        /// <param name="eraseEof">If true, do EraseEOF before each string.</param>
        /// <returns>Success indication.</returns>
        /// <exception cref="InvalidOperationException">Session is not started.</exception>
        /// <exception cref="X3270ifCommandException"><see cref="ExceptionMode"/> is enabled and the command fails.</exception>
        public async Task<IoResult> StringAtAsync(int row, int column, string text, bool quoteBackslashes = true, bool eraseEof = false)
        {
            var strings = new [] { new StringAtBlock { Row = row, Column = column, Text = text } };
            return await StringAtAsync(strings, quoteBackslashes, eraseEof).ConfigureAwait(continueOnCapturedContext: false);
        }

        /// <summary>
        /// Input blocks of text to the emulator at specified positions. Asynchronous version.
        /// </summary>
        /// <param name="strings">Set strings to add.</param>
        /// <param name="quoteBackslashes">If true, quote '\' characters.</param>
        /// <param name="eraseEof">If true, do EraseEOF before each string.</param>
        /// <returns>Success indication.</returns>
        /// <exception cref="InvalidOperationException">Session is not started.</exception>
        /// <exception cref="X3270ifCommandException"><see cref="ExceptionMode"/> is enabled and the command fails.</exception>
        public async Task<IoResult> StringAtAsync(IEnumerable<StringAtBlock> strings, bool quoteBackslashes = true, bool eraseEof = false)
        {
            string command = string.Empty;

            foreach (var b in strings)
            {
                if (b.Row < Config.Origin)
                {
                    throw new ArgumentOutOfRangeException("Row");
                }
                if (b.Column < Config.Origin)
                {
                    throw new ArgumentOutOfRangeException("Column");
                }
                command += command.JoinNonEmpty(" ", string.Format(
                    "MoveCursor({0},{1}) {2}String({3})",
                    b.Row - Config.Origin,
                    b.Column - Config.Origin,
                    eraseEof ? "EraseEOF() " : string.Empty,
                    QuoteString(b.Text, quoteBackslashes)));
            }
            return await IoAsync(command).ConfigureAwait(continueOnCapturedContext: false);
        }

        /// <summary>
        /// Input text to the emulator.
        /// </summary>
        /// <param name="text">Text to send. It will be quoted as necessary.</param>
        /// <param name="quoteBackslashes">If true, quote '\' characters.</param>
        /// <returns>Success indication.</returns>
        /// <exception cref="InvalidOperationException">Session is not started.</exception>
        /// <exception cref="X3270ifCommandException"><see cref="ExceptionMode"/> is enabled and the command fails.</exception>
        public IoResult String(string text, bool quoteBackslashes = true)
        {
            try
            {
                return StringAsync(text, quoteBackslashes).Result;
            }
            catch (AggregateException e)
            {
                throw e.InnerException;
            }
        }

        /// <summary>
        /// Input text to the emulator at a particular location.
        /// </summary>
        /// <param name="row">Row, using the session's <see cref="x3270if.Config.Origin"/>.</param>
        /// <param name="column">Column, using the session's <see cref="x3270if.Config.Origin"/>.</param>
        /// <param name="text">Text to send. It will be quoted as necessary.</param>
        /// <param name="quoteBackslashes">If true, quote '\' characters.</param>
        /// <param name="eraseEof">If true, do EraseEOF before each string.</param>
        /// <returns>Success indication.</returns>
        /// <exception cref="InvalidOperationException">Session is not started.</exception>
        /// <exception cref="X3270ifCommandException"><see cref="ExceptionMode"/> is enabled and the command fails.</exception>
        public IoResult StringAt(int row, int column, string text, bool quoteBackslashes = true, bool eraseEof = false)
        {
            try
            {
                return StringAtAsync(row, column, text, quoteBackslashes, eraseEof).Result;
            }
            catch (AggregateException e)
            {
                throw e.InnerException;
            }
        }

        /// <summary>
        /// Input blocks of text to the emulator at specified locations.
        /// </summary>
        /// <param name="strings">Set of strings to add.</param>
        /// <param name="quoteBackslashes">If true, quote '\' characters.</param>
        /// <param name="eraseEof">If true, do EraseEOF before each string.</param>
        /// <returns>Success indication.</returns>
        /// <exception cref="InvalidOperationException">Session is not started.</exception>
        /// <exception cref="X3270ifCommandException"><see cref="ExceptionMode"/> is enabled and the command fails.</exception>
        public IoResult StringAt(IEnumerable<StringAtBlock> strings, bool quoteBackslashes = true, bool eraseEof = false)
        {
            try
            {
                return StringAtAsync(strings, quoteBackslashes, eraseEof).Result;
            }
            catch (AggregateException e)
            {
                throw e.InnerException;
            }
        }
    }
}