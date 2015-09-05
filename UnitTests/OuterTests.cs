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
using NUnit.Framework;
using x3270if;
using System.IO;

namespace UnitTests
{
    /// <summary>
    /// Tests for accessory methods (methods that help process the output of basic features).
    /// </summary>
    [TestFixture]
    public class OuterTests
    {
        public OuterTests()
        {
        }

        [TestFixtureSetUp]
        public void Setup()
        {
            Util.ConsoleDebug = false;
        }

        /// <summary>
        /// Call DumpAsciiConsole, redirecting the console to NUL: unless ConsoleDebug is set.
        /// </summary>
        /// <param name="b"></param>
        private static void WrappedDumpAsciiBuffer(DisplayBuffer b)
        {
            var oldOut = Console.Out;
            if (!Util.ConsoleDebug)
            {
                Console.SetOut(new StreamWriter(Stream.Null));
            }
            b.DumpAsciiConsole();
            if (!Util.ConsoleDebug)
            {
                Console.SetOut(oldOut);
            }
        }

        /// <summary>
        /// Test the displayBuffer class.
        /// </summary>
        [Test]
        public void TestDisplayBuffer()
        {
            // Fake result of a ReadBuffer(Ascii) command.
            var rb = new Session.ReadBufferIoResult
            {
                Success = true,
                Result = new string[] {
                    "SF(c0=e8) 41 4c 4c 20 20 20 20 20 20 43 48 41 52 53 20 20 20 20 41 31 20 20 46 20 38 30 20 20 54 72 75 6e 63 3d 38 30 20 53 69 7a 65 3d 31 37 20 4c 69 6e 65 3d 30 20 43 6f 6c 3d 31 20 41 6c 74 3d 30 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 SF(c0=e8,42=f2)",
                    "43 41 53 45 20 55 50 50 45 52 20 52 45 53 50 45 43 54 20 20 20 20 20 54 41 42 20 6b 65 79 20 69 73 20 50 46 34 20 6f 72 20 50 46 31 36 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 SF(c0=e8,42=f2)",
                    "00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 SF(c0=e0)",
                    "00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00",
                    "00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00",
                    "00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00",
                    "00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00",
                    "00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00",
                    "00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00",
                    "00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00",
                    "00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00",
                    "00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00",
                    "00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00",
                    "00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00",
                    "00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00",
                    "00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00",
                    "00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00",
                    "00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00",
                    "00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00",
                    "00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00",
                    "00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 SF(c0=c0)",
                    "3d 3d 3d 3d 3d SF(c0=e8) 2a 20 2a 20 2a 20 54 6f 70 20 6f 66 20 46 69 6c 65 20 2a 20 2a 20 2a 20 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 SF(c0=e0)",
                    "20 20 20 20 20 SF(c0=e8) 7c 2e 2e 2e 2b 2e 2e 2e 2e 31 2e 2e 2e 2e 2b 2e 2e 2e 2e 32 2e 2e 2e 2e 2b 2e 2e 2e 2e 33 2e 2e 2e 2e 2b 2e 2e 2e 2e 34 2e 2e 2e 2e 2b 2e 2e 2e 2e 35 2e 2e 2e 2e 2b 2e 2e 2e 2e 36 2e 2e 2e 2e 2b 2e 2e 2e 2e 37 2e 2e 2e SF(c0=c0)",
                    "3d 3d 3d 3d 3d SF(c0=c0) 20 20 20 30 2d 20 31 2d 20 32 2d 20 33 2d 20 34 2d 20 35 2d 20 36 2d 20 37 2d 20 38 2d 20 39 2d 20 41 2d 20 42 2d 20 43 2d 20 44 2d 20 45 2d 20 46 2d 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 SF(c0=c0)",
                    "3d 3d 3d 3d 3d SF(c0=c0) 2d 30 20 22 20 20 22 20 20 22 20 20 22 20 20 20 20 20 26 20 20 2d 20 20 20 20 20 20 20 20 20 20 20 7e 20 20 5e 20 20 7b 20 20 7d 20 20 24 20 20 30 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 SF(c0=c0)",
                    "3d 3d 3d 3d 3d SF(c0=c0) 2d 31 20 22 20 20 22 20 20 22 20 20 22 20 20 20 20 20 20 20 20 2f 20 20 20 20 20 61 20 20 6a 20 20 c2af 20 20 20 20 20 41 20 20 4a 20 20 20 20 20 31 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 SF(c0=c0)",
                    "3d 3d 3d 3d 3d SF(c0=c0) 2d 32 20 22 20 20 22 20 20 22 20 20 22 20 20 20 20 20 20 20 20 20 20 20 20 20 20 62 20 20 6b 20 20 73 20 20 5c 20 20 42 20 20 4b 20 20 53 20 20 32 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 SF(c0=c0)",
                    "3d 3d 3d 3d 3d SF(c0=c0) 2d 33 20 22 20 20 22 20 20 22 20 20 22 20 20 20 20 20 20 20 20 20 20 20 20 20 20 63 20 20 6c 20 20 74 20 20 20 20 20 43 20 20 4c 20 20 54 20 20 33 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 SF(c0=c0)",
                    "3d 3d 3d 3d 3d SF(c0=c0) 2d 34 20 22 20 20 22 20 20 22 20 20 22 20 20 20 20 20 20 20 20 20 20 20 20 20 20 64 20 20 6d 20 20 75 20 20 20 20 20 44 20 20 4d 20 20 55 20 20 34 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 SF(c0=c0)",
                    "3d 3d 3d 3d 3d SF(c0=c0) 2d 35 20 3b 20 20 22 20 20 22 20 20 22 20 20 20 20 20 20 20 20 20 20 20 20 20 20 65 20 20 6e 20 20 76 20 20 20 20 20 45 20 20 4e 20 20 56 20 20 35 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 SF(c0=c0)",
                    "3d 3d 3d 3d 3d SF(c0=c0) 2d 36 20 22 20 20 22 20 20 22 20 20 22 20 20 20 20 20 20 20 20 20 20 20 20 20 20 66 20 20 6f 20 20 77 20 20 20 20 20 46 20 20 4f 20 20 57 20 20 36 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 SF(c0=c0)",
                    "3d 3d 3d 3d 3d SF(c0=c0) 2d 37 20 22 20 20 22 20 20 22 20 20 22 20 20 20 20 20 20 20 20 20 20 20 20 20 20 67 20 20 70 20 20 78 20 20 20 20 20 47 20 20 50 20 20 58 20 20 37 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 SF(c0=c0)",
                    "3d 3d 3d 3d 3d SF(c0=c0) 2d 38 20 22 20 20 22 20 20 22 20 20 22 20 20 20 20 20 20 20 20 20 20 20 20 20 20 68 20 20 71 20 20 79 20 20 20 20 20 48 20 20 51 20 20 59 20 20 38 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 SF(c0=c0)",
                    "3d 3d 3d 3d 3d SF(c0=c0) 2d 39 20 22 20 20 22 20 20 22 20 20 22 20 20 20 20 20 20 20 20 20 20 20 60 20 20 69 20 20 72 20 20 7a 20 20 20 20 20 49 20 20 52 20 20 20 20 20 39 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 SF(c0=c0)",
                    "3d 3d 3d 3d 3d SF(c0=c0) 2d 41 20 22 20 20 22 20 20 22 20 20 22 20 20 c2a3 20 20 21 20 20 20 20 20 3a 20 20 20 20 20 20 20 20 20 20 20 5b 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 SF(c0=c0)",
                    "3d 3d 3d 3d 3d SF(c0=c0) 2d 42 20 22 20 20 22 20 20 22 20 20 22 20 20 2e 20 20 c2a5 20 20 2c 20 20 23 20 20 20 20 20 20 20 20 20 20 20 5d 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 SF(c0=c0)",
                    "3d 3d 3d 3d 3d SF(c0=c0) 2d 43 20 22 20 20 22 20 20 22 20 20 22 20 20 3c 20 20 2a 20 20 25 20 20 40 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 SF(c0=c0)",
                    "3d 3d 3d 3d 3d SF(c0=c0) 2d 44 20 22 20 20 22 20 20 22 20 20 22 20 20 28 20 20 29 20 20 5f 20 20 27 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 22 20 20 22 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 SF(c0=c0)",
                    "3d 3d 3d 3d 3d SF(c0=c0) 2d 45 20 22 20 20 22 20 20 22 20 20 22 20 20 2b 20 20 3b 20 20 3e 20 20 3d 20 20 20 20 20 20 20 20 20 20 20 20 20 20 22 20 20 22 20 20 22 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 SF(c0=c0)",
                    "3d 3d 3d 3d 3d SF(c0=c0) 2d 46 20 22 20 20 22 20 20 22 20 20 22 20 20 7c 20 20 5e 20 20 3f 20 20 22 20 20 20 20 20 20 20 20 20 20 20 20 20 20 22 20 20 22 20 20 20 20 20 22 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 SF(c0=c0)",
                    "3d 3d 3d 3d 3d SF(c0=e0) 2a 20 2a 20 2a 20 45 6e 64 20 6f 66 20 46 69 6c 65 20 2a 20 2a 20 2a 20 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 SF(c0=e8)",
                    "3d 3d 3d 3d 3e SF(c0=c0) 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00",
                    "00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 SF(c0=e8) 58 20 45 20 44 20 49 20 54 20 20 31 20 46 69 6c 65 20 00 SF(c0=e0)"
                },
                Command = "ReadBuffer(ascii)",
                StatusLine = "U F U C(host.mycompany.com) I 4 43 80 41 6 0x0 -",
                ReadBufferType = Session.ReadBufferType.Ascii,
                Encoding = Encoding.UTF8
            };
            var b = new DisplayBuffer(rb);

            // Check the basic dimensions.
            Assert.AreEqual(43, b.ContentsArray.GetLength(0));
            Assert.AreEqual(80, b.ContentsArray.GetLength(1));

            // Check the first location, which is a normal unprotected field.
            var c = b.Contents(0, 0);
            Assert.AreEqual(PositionType.FieldAttribute, c.Type);
            Assert.Throws<InvalidOperationException>(() => { var e = c.AsciiChar; });
            Assert.Throws<InvalidOperationException>(() => { var e = c.EbcdicChar; });
            Assert.AreEqual(FieldFlags.Protected, c.Attrs.Flags);
            Assert.AreEqual(FieldIntensity.HighlightedSelectable, c.Attrs.Intensity);
            Assert.AreEqual(FieldColor.Default, c.Attrs.Foreground);
            Assert.AreEqual(FieldColor.Default, c.Attrs.Background);
            Assert.AreEqual(CharacterSet.Default, c.Attrs.CharacterSet);
            Assert.AreEqual(Highlighting.Default, c.Attrs.Highlighting);
            Assert.AreEqual(Outlining.Default, c.Attrs.Outlining);
            Assert.AreEqual(InputControl.Default, c.Attrs.InputControl);

            // Check the last location, which is a red field.
            c = b.Contents(0, 79);
            Assert.AreEqual(FieldColor.Red, c.Attrs.Foreground);

            // Check some simple text.
            Assert.AreEqual('A', b.Contents(0, 1).AsciiChar);
            Assert.AreEqual('L', b.Contents(0, 2).AsciiChar);
            Assert.AreEqual('L', b.Contents(0, 3).AsciiChar);

            // Check a modifiable field.
            Assert.AreEqual(FieldIntensity.Normal, b.Contents(40, 5).Attrs.Intensity);

            // Check a non-ASCII-7 character.
            Assert.AreEqual('£', b.Contents(34, 21).AsciiChar);

            // Check some DBCS.
            rb = new Session.ReadBufferIoResult
            {
                Result = new string[] {
                    "SF(c0=e8) 4d 41 54 54 45 53 SF(c0=e0) 52 65 61 64 79 3b 20 54 3d 30 2e 30 35 2f 30 2e 30 35 20 30 31 3a 32 36 3a 33 37 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00",
                    "74 79 70 65 20 63 68 69 6e 65 73 65 20 66 69 6c 65 20 61 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00",
                    "20 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00",
                    "54 68 69 73 20 69 73 20 73 6f 6d 65 20 43 68 69 6e 65 73 65 3a 20 0e e6b581 - e585ad - e79599 - e58898 - e7a1ab - e69fb3 - e99986 - e581bb - e8928c - e798a4 - 0f 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00",
                    "20 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00",
                    "SF(c0=e8) 4d 41 54 54 45 53 SF(c0=e0) 52 65 61 64 79 3b 20 54 3d 30 2e 30 31 2f 30 2e 30 31 20 30 31 3a 32 36 3a 33 37 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00",
                    "00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00",
                    "00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00",
                    "00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00",
                    "00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00",
                    "00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00",
                    "00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00",
                    "00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00",
                    "00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00",
                    "00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00",
                    "00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00",
                    "00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00",
                    "00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00",
                    "00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00",
                    "00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00",
                    "00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00",
                    "00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 SF(c0=c1)",
                    "00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00",
                    "00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 SF(c0=e0) 52 55 4e 4e 49 4e 47 20 20 20 53 52 55 20 20 20 20 20 SF(c0=e0) SF(c0=e0)"
                },
                Command = "ReadBuffer(Ascii)",
                StatusLine = "U F U C(host.mycompany.com) I 2 24 80 22 0 0x0 -",
                ReadBufferType = Session.ReadBufferType.Ascii,
                Encoding = Encoding.UTF8
            };
            b = new DisplayBuffer(rb);
            Assert.AreEqual('\x0e', b.Contents(3, 22).AsciiChar); // Shift Out
            Assert.AreEqual('流', b.Contents(3, 23).AsciiChar);
            Assert.AreEqual(PositionType.DbcsRight, b.Contents(3, 24).Type);
            Assert.AreEqual('\x0f', b.Contents(3, 43).AsciiChar); // Shift In

            // Exercise DumpAscii().
            var asAscii = b.Ascii();

            // Verify basic ASCII-7 translation and translation of SFs to blanks.
            Assert.AreEqual(" MATTES ", asAscii[0].Substring(0, 8));
            // Verify DBCS.
            Assert.AreEqual("流六留", asAscii[3].Substring(23, 3));

            // Try DumpAscii on an EBCDIC buffer.
            rb = new Session.ReadBufferIoResult
            {
                Success = true,
                Result = new string[] {
                    "40 40 40 40 40 SF(c0=e0,45=f1) 40 40 48 49 50 SF(c0=cc,cf=cc,45=3d) 40 40 40 GE(F0) 40",
                },
                Command = "ReadBuffer(Ebcdic)",
                StatusLine = "U F U C(host.mycompany.com) I 2 1 17 0 0 0x0 -",
                ReadBufferType = Session.ReadBufferType.Ebcdic,
                Encoding = Encoding.UTF8
            };
            b = new DisplayBuffer(rb);
            Assert.Throws<InvalidOperationException>(() => { var da = b.Ascii(); });

            // Verify GE, which happens only on EBCDIC dumps.
            Assert.AreEqual(CharacterSet.Apl, b.Contents(0, 15).Attrs.CharacterSet);

            // Verify that DumpAsciiConsole() fails for EBCDIC dumps.
            Assert.Throws<InvalidOperationException>(() => { b.DumpAsciiConsole(); });

            // Exercise the Ascii and Ebcdic methods. The first also exercises zero intensity.
            Assert.AreEqual(0x40, b.Contents(0, 15).EbcdicChar);
            Assert.AreEqual(0x50, b.Contents(0, 10).EbcdicChar);
            Assert.Throws<InvalidOperationException>(() => { var ga = b.Contents(0, 15).AsciiChar; });

            // Test other displayBuffer methods with a completely contrived buffer.
            rb = new Session.ReadBufferIoResult
            {
                Success = true,
                Result = new string[] {
                    // First 5 positions are a wrapped field from the end of the buffer.
                    // Position 5 is an SFE, protected, blue background
                    // Position 7 has an SA changing the background to red
                    // Position 8 has an SA changing the background back to the default, which would be blue
                    // Position 11 is an SF, unprotected, zero intensity (like a password field).
                    // The rest of the row is returned as ASCII blanks, as is the beginning of the row due to the wrap.
                    // At position 16, there is an SA that sets Validation to Fill, Outlining to Underline, Highlighting to Blink, Character Set to Apl,
                    //  Transparency to Xor, and Input Control to Enabled. There is also an 'unknown' 66 attribute to exercise one more code path, and non-hex junk to exercise others.
                    "41 e585ad - 44 45 SF(c0=e0,45=f1) 46 SA(45=f2) 47 SA(45=00) 48 49 50 SF(c0=cc) 51 52 GE(53) 54 SA(c1=04,c2=01,41=f1,43=f1,46=f1,fe=01,66=f1,fe=gg,gg=ii) 55",
                },
                Command = "ReadBuffer(Ascii)",
                StatusLine = "U F U C(host.mycompany.com) I 2 1 17 0 0 0x0 -",
                ReadBufferType = Session.ReadBufferType.Ascii,
                Encoding = Encoding.UTF8
            };
            b = new DisplayBuffer(rb);

            // Verify attribute wrap-around.
            Assert.AreEqual(FieldFlags.None, b.Contents(0, 0).Attrs.Flags);
            Assert.AreEqual(FieldFlags.Protected, b.Contents(0, 6).Attrs.Flags);

            // Verify zero intensity, including field wrap.
            Assert.AreEqual(FieldIntensity.Zero, b.Contents(0, 13).Attrs.Intensity);
            Assert.AreEqual(' ', b.Contents(0, 13).AsciiChar);
            Assert.AreEqual(FieldIntensity.Zero, b.Contents(0, 0).Attrs.Intensity);
            Assert.AreEqual(' ', b.Contents(0, 0).AsciiChar);

            // Verify Background color and SA overriding FA.
            Assert.AreEqual(FieldColor.Blue, b.Contents(0, 6).Attrs.Background);
            Assert.AreEqual(FieldColor.Red, b.Contents(0, 7).Attrs.Background);
            Assert.AreEqual(FieldColor.Blue, b.Contents(0, 8).Attrs.Background);

            // Verify the other extended attributes.
            var a = b.Contents(0, 16).Attrs;
            Assert.AreEqual(Validation.Fill, a.Validation);
            Assert.AreEqual(Outlining.Underline, a.Outlining);
            Assert.AreEqual(Highlighting.Blink, a.Highlighting);
            Assert.AreEqual(CharacterSet.Apl, a.CharacterSet);
            Assert.AreEqual(Transparency.Xor, a.Transparency);
            Assert.AreEqual(InputControl.Enabled, a.InputControl);

            // Verify that the SA does not wrap.
            a = b.Contents(0, 0).Attrs;
            Assert.AreEqual(Validation.Default, a.Validation);
            Assert.AreEqual(Outlining.Default, a.Outlining);
            Assert.AreEqual(Highlighting.Default, a.Highlighting);
            Assert.AreEqual(CharacterSet.Default, a.CharacterSet);
            Assert.AreEqual(Transparency.Default, a.Transparency);
            Assert.AreEqual(InputControl.Default, a.InputControl);

            // Exercise DumpAscii's zero-intensity logic.
            asAscii = b.Ascii();
            Assert.AreEqual(" ", asAscii[0].Substring(0, 1));

            // Exercise DumpAsciiConsole().
            WrappedDumpAsciiBuffer(b);

            // Exercise all of the colors in DumpAsciiConsole.
            rb = new Session.ReadBufferIoResult
            {
                Success = true,
                Result = new string[] {
                    "SF(c0=e0) 42 SA(45=f0) 42 SA(45=f1) 42 SA(45=f2) 42 SA(45=f3) 42 SA(45=f4) 42 SA(45=f5) 42 SA(45=f6) 42 SA(45=f7) 42 SA(45=f8) 42 SA(45=f9) 42 SA(45=fa) 42 SA(45=fb) 42 SA(45=fc) 42 SA(45=fd) 42 SA(45=fe) 42 SA(45=ff) 42"
                },
                Command = "ReadBuffer(Ascii)",
                StatusLine = "U F U C(host.mycompany.com) I 2 1 18 0 0 0x0 -",
                ReadBufferType = Session.ReadBufferType.Ascii,
                Encoding = Encoding.UTF8
            };
            b = new DisplayBuffer(rb);
            WrappedDumpAsciiBuffer(b);

            // Verify that you can't read an EBCDIC character from an ASCII dump.
            Assert.Throws<InvalidOperationException>(() => { var ec = b.Contents(0, 1).EbcdicChar; });
        }

        /// <summary>
        /// Exercise DisplayBuffer.Ascii.
        /// </summary>
        [Test]
        public void TestDisplayBufferAscii()
        {
            var rb = new Session.ReadBufferIoResult
            {
                Success = true,
                Result = new string[] { "30 31 32 33 34 35 36" },
                Command = "ReadBuffer(Ascii)",
                StatusLine = "U F U C(host.mycompany.com) I 2 1 7 0 1 0x0 -",
                ReadBufferType = Session.ReadBufferType.Ascii,
                Encoding = Encoding.UTF8,
                Origin = 0
            };
            var b = new DisplayBuffer(rb);

            // Basic functionality.
            Assert.AreEqual("123456", b.Ascii(6));
            Assert.AreEqual("12", b.Ascii(0, 1, 2));
            Assert.AreEqual(new string[] { "12" }, b.Ascii(0, 1, 1, 2));
            Assert.AreEqual(string.Empty, b.Ascii(0));
            Assert.AreEqual(string.Empty, b.Ascii(0, 1, 0));

            // Handy shortcut functionality.
            Assert.AreEqual(true, b.AsciiEquals(0, 1, "123456"));
            Assert.AreEqual(true, b.AsciiMatches(0, 1, 6, "1.*6"));

            // Exceptions.
            Assert.Throws<ArgumentOutOfRangeException>(() => { var s = b.Ascii(99); });
            Assert.Throws<ArgumentOutOfRangeException>(() => { var s = b.Ascii(-1); });
            Assert.Throws<ArgumentOutOfRangeException>(() => { var s = b.Ascii(100, 1, 2); });
            Assert.Throws<ArgumentOutOfRangeException>(() => { var s = b.Ascii(-100, 1, 2); });
            Assert.Throws<ArgumentOutOfRangeException>(() => { var s = b.Ascii(0, 100, 2); });
            Assert.Throws<ArgumentOutOfRangeException>(() => { var s = b.Ascii(0, -100, 2); });
            Assert.Throws<ArgumentOutOfRangeException>(() => { var s = b.Ascii(0, 1, 999, 2); });
            Assert.Throws<ArgumentOutOfRangeException>(() => { var s = b.Ascii(0, 1, -1, 2); });
            Assert.Throws<ArgumentOutOfRangeException>(() => { var s = b.Ascii(0, 1, 1, 999); });
            Assert.Throws<ArgumentOutOfRangeException>(() => { var s = b.Ascii(0, 1, 1, -1); });
            Assert.Throws<ArgumentOutOfRangeException>(() => { var s = b.Ascii(-1, 1, 1, 2); });
            Assert.Throws<ArgumentOutOfRangeException>(() => { var s = b.Ascii(999, 1, 1, 2); });
            Assert.Throws<ArgumentOutOfRangeException>(() => { var s = b.Ascii(0, -1, 1, 2); });
            Assert.Throws<ArgumentOutOfRangeException>(() => { var s = b.Ascii(0, 999, 1, 2); });


            rb = new Session.ReadBufferIoResult
            {
                Success = true,
                Result = new string[] { "30 31 32 33 34 35 36" },
                Command = "ReadBuffer(Ebcdic)",
                StatusLine = "U F U C(host.mycompany.com) I 2 1 7 0 1 0x0 -",
                ReadBufferType = Session.ReadBufferType.Ebcdic,
                Encoding = Encoding.UTF8
            };
            b = new DisplayBuffer(rb);
            Assert.Throws<InvalidOperationException>(() => { var s = b.Ascii(1); });
            Assert.Throws<InvalidOperationException>(() => { var s = b.Ascii(0, 1, 2); });
            Assert.Throws<InvalidOperationException>(() => { var s = b.Ascii(0, 1, 1, 2); });
        }

        /// <summary>
        /// Exercise DisplayBuffer.Ascii in 1-origin mode.
        /// </summary>
        [Test]
        public void TestDisplayBufferAscii1()
        {
            var rb = new Session.ReadBufferIoResult
            {
                Success = true,
                Result = new string[] { "30 31 32 33 34 35 36" },
                Command = "ReadBuffer(Ascii)",
                StatusLine = "U F U C(host.mycompany.com) I 2 1 7 1 2 0x0 -",
                ReadBufferType = Session.ReadBufferType.Ascii,
                Encoding = Encoding.UTF8,
                Origin = 1
            };
            var b = new DisplayBuffer(rb);

            // Basic functionality.
            Assert.AreEqual("123456", b.Ascii(6));
            Assert.AreEqual("12", b.Ascii(1, 2, 2));
            Assert.AreEqual(new string[] { "12" }, b.Ascii(1, 2, 1, 2));
            Assert.AreEqual(string.Empty, b.Ascii(0));
            Assert.AreEqual(string.Empty, b.Ascii(1, 2, 0));

            // Handy shortcut functionality.
            Assert.AreEqual(true, b.AsciiEquals(1, 2, "123456"));
            Assert.AreEqual(true, b.AsciiMatches(1, 2, 6, "1.*6"));

            // Exceptions.
            Assert.Throws<ArgumentOutOfRangeException>(() => { var s = b.Ascii(100, 1, 2); });
            Assert.Throws<ArgumentOutOfRangeException>(() => { var s = b.Ascii(-100, 1, 2); });
            Assert.Throws<ArgumentOutOfRangeException>(() => { var s = b.Ascii(1, 100, 2); });
            Assert.Throws<ArgumentOutOfRangeException>(() => { var s = b.Ascii(1, -100, 2); });
            Assert.Throws<ArgumentOutOfRangeException>(() => { var s = b.Ascii(1, 1, 999, 2); });
            Assert.Throws<ArgumentOutOfRangeException>(() => { var s = b.Ascii(1, 1, -1, 2); });
            Assert.Throws<ArgumentOutOfRangeException>(() => { var s = b.Ascii(1, 1, 1, 999); });
            Assert.Throws<ArgumentOutOfRangeException>(() => { var s = b.Ascii(1, 1, 1, -1); });
            Assert.Throws<ArgumentOutOfRangeException>(() => { var s = b.Ascii(0, 1, 1, 2); });
            Assert.Throws<ArgumentOutOfRangeException>(() => { var s = b.Ascii(999, 1, 1, 2); });
            Assert.Throws<ArgumentOutOfRangeException>(() => { var s = b.Ascii(1, 0, 1, 2); });
            Assert.Throws<ArgumentOutOfRangeException>(() => { var s = b.Ascii(1, 999, 1, 2); });
        }

        // Test the Coordinate class.
        [Test]
        public void TestDisplayBufferCoordinate()
        {
            var rb = new Session.ReadBufferIoResult
            {
                Success = true,
                Result = new string[] {
                    "SF(c0=e0) 41 42 43 44 45 46 SF(c0=e0) 47 48 49 4a 4b",
                    "SF(c0=e0) 61 62 63 64 65 66 SF(c0=e0) 67 68 69 6a 6b"
                },
                Command = "ReadBuffer(Ascii)",
                StatusLine = "U F U C(host.mycompany.com) I 2 2 13 0 1 0x0 -",
                ReadBufferType = Session.ReadBufferType.Ascii,
                Encoding = Encoding.UTF8
            };
            var b = new DisplayBuffer(rb);

            // Exercise the Coordinate class.
            var c = new DisplayBuffer.Coordinates(b);
            Assert.Throws<ArgumentOutOfRangeException>(() => { c.Row = -1; });
            Assert.Throws<ArgumentOutOfRangeException>(() => { c.Row = 100; });
            Assert.Throws<ArgumentOutOfRangeException>(() => { c.Column = -1; });
            Assert.Throws<ArgumentOutOfRangeException>(() => { c.Column = 100; });
            var d = new DisplayBuffer.Coordinates(c);
            Assert.AreEqual(c, d);
            object o = d;
            Assert.AreEqual(c, o);
            Assert.AreNotEqual(c, b);
            object o2 = d;
            Assert.AreEqual(true, c.Equals(o2));
            Assert.AreEqual(false, c == null);
            Assert.AreEqual(false, null == c);
            Assert.AreEqual(0, c.GetHashCode());
            Assert.AreEqual("[0,0]", c.ToString());

            // Test wrapping back past [0,0].
            var e = --c;
            Assert.AreEqual(12, e.Column);
            Assert.AreEqual(1, e.Row);

            // Test operator overrides.
            var f = new DisplayBuffer.Coordinates(b);
            Assert.AreEqual(f.Row, 0);
            Assert.AreEqual(f.Column, 0);
            var g = new DisplayBuffer.Coordinates(b);
            Assert.AreEqual(true, f == g);
            Assert.AreEqual(false, f < g);
            Assert.AreEqual(false, f > g);
            f.Row = 1;
            Assert.AreEqual(true, f != g);
            Assert.AreEqual(true, f > g);
            Assert.AreEqual(true, g < f);
            Assert.Throws<ArgumentNullException>(() => { var t = f < null; });
            Assert.Throws<ArgumentNullException>(() => { var t = null < f; });
            Assert.Throws<ArgumentNullException>(() => { var t = f > null; });
            Assert.Throws<ArgumentNullException>(() => { var t = null > f; });

            // Test BufferAddress.
            Assert.AreEqual(0, g.BufferAddress);
            Assert.AreEqual(13, f.BufferAddress);
        }

        // Test the Coordinate class with 1-origin.
        [Test]
        public void TestDisplayBufferCoordinate1()
        {
            var rb = new Session.ReadBufferIoResult
            {
                Success = true,
                Result = new string[] {
                    "SF(c0=e0) 41 42 43 44 45 46 SF(c0=e0) 47 48 49 4a 4b",
                    "SF(c0=e0) 61 62 63 64 65 66 SF(c0=e0) 67 68 69 6a 6b"
                },
                Command = "ReadBuffer(Ascii)",
                StatusLine = "U F U C(host.mycompany.com) I 2 2 13 0 1 0x0 -",
                ReadBufferType = Session.ReadBufferType.Ascii,
                Encoding = Encoding.UTF8,
                Origin = 1
            };
            var b = new DisplayBuffer(rb);

            // Exercise the Coordinate class.
            var c = new DisplayBuffer.Coordinates(b);
            Assert.Throws<ArgumentOutOfRangeException>(() => { c.Row = 0; });
            Assert.Throws<ArgumentOutOfRangeException>(() => { c.Column = 0; });
            var d = new DisplayBuffer.Coordinates(c);

            Assert.AreEqual("[1,1]", c.ToString());

            // Test wrapping back past [1,1].
            var e = --c;
            Assert.AreEqual(13, e.Column);
            Assert.AreEqual(2, e.Row);
        }

        // Exercise the AsciiField method of a DisplayBuffer.
        [Test]
        public void TestDisplayBufferAsciiField()
        {
            var rb = new Session.ReadBufferIoResult
            {
                Success = true,
                Result = new string[] {
                    "SF(c0=e0) 41 42 43 44 45 46 SF(c0=e0) 47 48 49 4a 4b",
                    "SF(c0=e0) 61 62 63 64 65 66 SF(c0=e0) 67 68 69 6a 6b"
                },
                Command = "ReadBuffer(Ascii)",
                StatusLine = "U F U C(host.mycompany.com) I 2 2 13 0 1 0x0 -",
                ReadBufferType = Session.ReadBufferType.Ascii,
                Encoding = Encoding.UTF8
            };
            var b = new DisplayBuffer(rb);

            // Trivial success.
            Assert.AreEqual("ABCDEF", b.AsciiField());
            Assert.AreEqual("ghijk", b.AsciiField(1, 10));

            // Unformatted.
            rb = new Session.ReadBufferIoResult
            {
                Success = true,
                Result = new string[] {
                    "41 42 43 44 45 46 47 48 49 4a 4b",
                    "61 62 63 64 65 66 67 68 69 6a 6b"
                },
                Command = "ReadBuffer(Ascii)",
                StatusLine = "U F U C(host.mycompany.com) I 2 2 11 0 1 0x0 -",
                ReadBufferType = Session.ReadBufferType.Ascii,
                Encoding = Encoding.UTF8
            };
            var b2 = new DisplayBuffer(rb);
            Assert.AreEqual("ABCDEFGHIJKabcdefghijk", b2.AsciiField());

            // Test field length on an unformatted buffer.
            Assert.AreEqual(22, b2.FieldLength(new DisplayBuffer.Coordinates(b2, 0, 0)));
            Assert.AreEqual(22, b2.FieldLength(0, 0));
        }
    }
}
