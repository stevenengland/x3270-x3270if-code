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
    /// Common utility functions.
    /// </summary>
    public static class Util
    {
        /// <summary>
        /// Return a string by building up a result with a separator. An incremental form of <see cref="string.Join(string,string[])"/>.
        /// </summary>
        /// <param name="left">The original string.</param>
        /// <param name="separator">The separator.</param>
        /// <param name="right">The string to add, which might be null or empty.</param>
        /// <returns>Incremental string to concatenate to <paramref name="left"/>.</returns>
        /// <remarks>
        /// Start with an empty
        /// string 's' and use it like this:
        /// <code>
        /// String s = String.Empty;
        /// //...
        /// s += s.JoinNonEmpty(" ", newElement);
        /// </code>
        /// This lets you build up a result with only single separators between non-empty elements,
        /// and no separators on the ends.
        /// </remarks>
        public static string JoinNonEmpty(this string left, string separator, string right)
        {
            if (string.IsNullOrEmpty(right))
            {
                // Nothing on the right. Return nothing.
                return string.Empty;
            }
            if (string.IsNullOrEmpty(left))
            {
                // Something on the right, nothing on the left. Return the right.
                return right;
            }
            // Something on both the left and right. Return the separator plus the right.
            return separator + right;
        }

        /// <summary>
        /// If true, send debug output to the console.
        /// </summary>
        public static bool ConsoleDebug;

        /// <summary>
        /// Conditionally write debug output to the console.
        /// </summary>
        /// <param name="format"><see cref="String.Format(String,object)"/> specifier.</param>
        /// <param name="args">Format arguments.</param>
        public static void Log(string format, params object[] args)
        {
            if (ConsoleDebug)
            {
                var text = string.Format(format, args);
                var now = DateTime.Now;
                Console.WriteLine("{0}{1:D2}{2:D2}:{3:D2}{4:D2}{5:D2}.{6:D4} {7}",
                    now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second, now.Millisecond, text);
            }
        }

        /// <summary>
        /// The name of the port environment variable.
        /// </summary>
        public const string x3270Port = "X3270PORT";
    }
}
